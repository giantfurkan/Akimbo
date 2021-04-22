using System;
using System.Linq;
using BzKovSoft.ActiveRagdoll;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	[DisallowMultipleComponent]
	public class BzThirdPersonActiveRagdollAndIK : MonoBehaviour
	{
		[Range(0.5f, 1.5f)]
		public float _castOffset = 0.5f;
		[Range(-0.5f, 0.5f)]
		public float _footOffset = -0.11f;
		[Range(0f, 1f)]
		public float _footUpperLimit = 0.4f;
		[Range(0f, 1f)]
		public float _footLowerLimit = 0.4f;
		[Range(0.01f, 0.2f)]
		public float _footWeightDistance = 0.05f;
		[SerializeField]
		float _jumpForce = 4f;

		readonly int _animatorVelocityX = Animator.StringToHash("VelocityX");
		readonly int _animatorVelocityY = Animator.StringToHash("VelocityY");
		readonly int _animatorJump = Animator.StringToHash("Jump");
		readonly int _animatorJumpLeg = Animator.StringToHash("JumpLeg");
		readonly int _animatorJumpProgress = Animator.StringToHash("JumpProgress");
		const float RunCycleLegOffset = 0.2f;   // animation cycle offset (0-1) used for determining correct leg to jump off
		CharacterController _characterController;
		Animator _animator;
		IBzRagdoll _ragdoll;
		IBzBalancer _balancer;
		float _connectionTime;
		Vector3 _velocity;
		float _yCorrectionVelocity = 4f;
		bool _grounded;

		FootHitResult _leftFootHit;
		FootHitResult _rightFootHit;

		void OnEnable()
		{
			_leftFootHit = new FootHitResult();
			_rightFootHit = new FootHitResult();

			_characterController = GetComponent<CharacterController>();
			_animator = GetComponent<Animator>();
			_ragdoll = GetComponent<IBzRagdoll>();
			_balancer = GetComponent<IBzBalancer>();

			if (_balancer == null)
				throw new InvalidOperationException("No balancer defined on the character");

			_ragdoll.IsConnectedChanged += ConnectionChanged;
		}

		void OnDisable()
		{
			_ragdoll.IsConnectedChanged -= ConnectionChanged;
		}

		private void ConnectionChanged()
		{
			if (_ragdoll.IsConnected)
			{
				_connectionTime = Time.time;
			}
		}

		private void LateUpdate()
		{
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis("Vertical");

			if (Input.GetMouseButton(1))
			{
				x = 0;
				y = 0;
			}

			if (_ragdoll.IsConnected)
			{
				Vector3 balVel = _balancer.BalanceVelocity / 30f;
				float magnitude = balVel.magnitude;
				if (magnitude > 1.5f & _connectionTime + 2f < Time.time)
				{
					_ragdoll.IsConnected = false;
					return;
				}

				balVel *= 2f;

				if (magnitude > 0.1f)
				{
					balVel = transform.InverseTransformDirection(balVel);
					x += balVel.x;
					y += balVel.z;
				}
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_velocity = _animator.velocity;
				_velocity.y += _jumpForce;

				// calculate which leg is behind, so as to leave that leg trailing in the jump animation
				// (This code is reliant on the specific run cycle offset in our animations,
				// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
				float runCycle = Mathf.Repeat(
						_animator.GetCurrentAnimatorStateInfo(0).normalizedTime + RunCycleLegOffset, 1);

				float jumpLeg = (runCycle < 0.5f ? 1 : -1) * y;
				_animator.SetFloat(_animatorJumpLeg, jumpLeg);
			}

			bool inJump = !_grounded | _velocity.y > 0f;
			if (inJump)
			{
				_animator.SetFloat(_animatorJumpProgress, _velocity.y);
			}

			// update the animator parameters
			_animator.SetBool(_animatorJump, inJump);
			_animator.SetFloat(_animatorVelocityX, x, 0.2f, Time.deltaTime);
			_animator.SetFloat(_animatorVelocityY, y, 0.2f, Time.deltaTime);
		}

		private void FixedUpdate()
		{
			if (!_ragdoll.IsRagdolled)
			{
				return;
			}

			var pos = transform.position;

			Vector3 shift = Vector3.zero;
			if (_ragdoll.IsRagdolled)
			{
				var rag = _ragdoll.RagdollRigid.transform;
				var skl = _ragdoll.GetSkeletonTransform(rag);
				shift = rag.position - skl.position;
				shift.y = 0f;
			}

			pos += shift;

			RaycastHit rootHit;
			Vector3 drawStart = pos - Vector3.down * 0.5f;
			Vector3 drawDir = Vector3.down * 0.6f;
			Debug.DrawRay(drawStart, drawDir, Color.blue);
			bool rootHitSuccess = HitGround(drawStart, drawDir, out rootHit);

			bool groundHitSuccess = _leftFootHit.Success | _rightFootHit.Success | rootHitSuccess;
			if (_grounded != groundHitSuccess)
			{
				_grounded = groundHitSuccess;
			}

			_velocity += Physics.gravity * Time.deltaTime;

			if (groundHitSuccess & _velocity.y <= 0f)
			{
				_velocity = Vector3.zero;

				float lY_lower    = _leftFootHit.Success  ? _leftFootHit.Position.y  : float.MaxValue;
				float rY_lower    = _rightFootHit.Success ? _rightFootHit.Position.y : float.MaxValue;
				float rootY_lower = rootHitSuccess        ? rootHit.point.y          : float.MaxValue;

				float y_min = Mathf.Min(lY_lower, rY_lower, rootY_lower);

				float lY_upper    = _leftFootHit.Success  ? _leftFootHit.Position.y  : float.MinValue;
				float rY_upper    = _rightFootHit.Success ? _rightFootHit.Position.y : float.MinValue;
				float rootY_upper = rootHitSuccess        ? rootHit.point.y          : float.MinValue;

				float y_max = Mathf.Max(lY_upper, rY_upper, rootY_upper);

				float y = Mathf.Max(y_min, y_max - _footUpperLimit);
				if (pos.y < y)
				{
					pos.y += _yCorrectionVelocity * Time.deltaTime;
					if (pos.y > y)
						pos.y = y;
				}
				else
				{
					pos.y -= _yCorrectionVelocity * Time.deltaTime;
					if (pos.y < y)
						pos.y = y;
				}
			}
			else
			{
				pos += _velocity * Time.deltaTime;
			}

			pos -= shift;
			transform.position = pos;
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (!_ragdoll.IsRagdolled)
			{
				return;
			}

			if (layerIndex != 0)
				return;

			Vector3 sklRootShift = Vector3.zero;

			if (_ragdoll.IsRagdolled)
			{
				var ragRoot = _ragdoll.RagdollRigid.transform;
				var sklRoot = _ragdoll.GetSkeletonTransform(ragRoot);
				sklRootShift = sklRoot.position - ragRoot.position;
			}

			ProcessFoot(HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot, _leftFootHit, sklRootShift);
			ProcessFoot(HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot, _rightFootHit, sklRootShift);
		}

		private void ProcessFoot(HumanBodyBones foot, AvatarIKGoal goal, FootHitResult footResult, Vector3 toSklShift)
		{
			RaycastHit hit;
			var sklFoot = _animator.GetBoneTransform(foot);
			Vector3 ragFootPos = sklFoot.position - toSklShift;

			var footCastBegin = new Vector3(ragFootPos.x, transform.position.y + _footUpperLimit + _castOffset, ragFootPos.z);
			var castVector = new Vector3(0f, -_castOffset - _footLowerLimit - _footUpperLimit, 0f);
			Debug.DrawRay(footCastBegin, new Vector3(0, -_castOffset, 0), Color.yellow);
			Debug.DrawRay(footCastBegin + new Vector3(0, -_castOffset, 0), new Vector3(0, -_footLowerLimit - _footUpperLimit, 0), Color.blue);
			footResult.Success = HitGround(footCastBegin, castVector, out hit);

			if (footResult.Success)
			{
				footResult.Position = hit.point;
				SetFootIK(goal, ragFootPos, hit, toSklShift);
			}
		}

		private void SetFootIK(AvatarIKGoal goal, Vector3 originalPos, RaycastHit hit, Vector3 sklRootShift)
		{
			var hitPos = hit.point;
			var shiftPos = new Vector3(0f, _footOffset, 0f);

			var rotDelta = Quaternion.FromToRotation(Vector3.up, hit.normal);
			var posDelta = rotDelta * shiftPos;
			originalPos += shiftPos;

			DrawRectangle(originalPos, Vector3.up, Color.yellow);
			DrawRectangle(hitPos, hit.normal, Color.blue);

			float ikFootWeight;
			float posYDist = hitPos.y - originalPos.y;
			if (posYDist > 0f)
			{
				ikFootWeight = 1f;
			}
			else
			{
				ikFootWeight = (_footWeightDistance + posYDist) / _footWeightDistance;
				ikFootWeight = Mathf.Clamp01(ikFootWeight);
				DrawWeight(originalPos, ikFootWeight);
			}

			_animator.SetIKPosition(goal, hitPos - posDelta + sklRootShift);
			_animator.SetIKPositionWeight(goal, ikFootWeight);

			var rot = transform.rotation * rotDelta;

			_animator.SetIKRotation(goal, rot);
			_animator.SetIKRotationWeight(goal, ikFootWeight);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void DrawWeight(Vector3 originalPos, float ikFootWeight)
		{
			if (ikFootWeight > 0f)
			{
				Vector3 endPos = originalPos + new Vector3(0f, -_footWeightDistance, 0f);
				Vector3 forward = transform.forward * 0.1f;
				Debug.DrawLine(originalPos + forward, endPos, Color.red);
				Debug.DrawLine(originalPos - forward, endPos, Color.red);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void DrawRectangle(Vector3 hitPos, Vector3 up, Color color)
		{
			var left = Vector3.Cross(transform.forward, up).normalized;
			var forward = Vector3.Cross(up, left).normalized;
			left *= 0.1f;
			forward *= 0.15f;
			Debug.DrawRay(hitPos + forward - left, left * 2, color);
			Debug.DrawRay(hitPos - forward - left, left * 2, color);
			Debug.DrawRay(hitPos - forward - left, forward * 2, color);
			Debug.DrawRay(hitPos - forward + left, forward * 2, color);
		}

		private bool HitGround(Vector3 castBegin, Vector3 dir, out RaycastHit hit)
		{
			var hits = Physics.RaycastAll(castBegin, dir, dir.magnitude);
			int hitGroundIndex = -1;
			float distance = float.MaxValue;

			for (int i = 0; i < hits.Length; i++)
			{
				var hit2 = hits[i];
				if (hit2.transform.IsChildOf(gameObject.transform))
					continue;

				if (hitGroundIndex == -1 || hit2.distance < distance)
				{
					hitGroundIndex = i;
					distance = hit2.distance;
				}
			}

			if (hitGroundIndex == -1)
			{
				hit = default(RaycastHit);
				return false;
			}
			else
			{
				hit = hits[hitGroundIndex];
				return true;
			}
		}

		void OnAnimatorMove()
		{
			var pos = transform.position + _animator.deltaPosition;
			var rot = transform.rotation * _animator.deltaRotation;
			transform.SetPositionAndRotation(pos, rot);
		}

		class FootHitResult
		{
			public bool Success;
			public Vector3 Position;
		}
	}
}