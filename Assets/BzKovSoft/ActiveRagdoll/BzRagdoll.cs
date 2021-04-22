using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BzKovSoft.ActiveRagdoll.RagdollBehaviours;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	/// <summary>
	/// Active ragdoll component
	/// </summary>
	[DisallowMultipleComponent]
	public class BzRagdoll : MonoBehaviour, IBzRagdoll
	{
#pragma warning disable 0649
		[SerializeField]
		bool _convertOnStart;
		[SerializeField]
		[Range(0.1f, 1f)]
		float _massReduction = 0.3f;
		[SerializeField]
		Rigidbody _ragdollRigid;
		[SerializeField]
		Rigidbody[] _AttachedLimbs;

		[SerializeField]
		ControllerMoveType _controllerMoveType;
		[SerializeField]
		RigidbodyTurnoffType _rigidbodyTurnoffType;
		public RigidbodyTurnoffType RigidbodyTurnoffType { get { return _rigidbodyTurnoffType; } set { _rigidbodyTurnoffType = value; } }
		[SerializeField]
		float _jointSpringRoot = 1000f;
		[SerializeField]
		float _jointSpringAttach = 1000f;
		[SerializeField]
		float _jointSpring = 50;
		[SerializeField]
		bool _deleteJoints;
		[SerializeField]
		bool _deleteColliders;
#pragma warning restore 0649


		Animator _animator;
		Dictionary<Transform, Transform> _tranToSkelet;
		Dictionary<Transform, Transform> _tranToRagdoll;
		Transform _skeletonObject;
		FixedLimb[] _fixedLimbs;
		FollowingLimb[] _followingLimbs;
		FollowingLimb[] _attachedLimbs;
		FollowingLimb _rootLimb;
		bool _ragdolled;
		float _springRate = 1f;
		bool _isConnected = false;

		public event Propertychanged IsConnectedChanged;
		public event Propertychanged IsRagdolledChanged;

		public bool IsRagdolled
		{
			get { return _ragdolled;}
			set
			{
				if (_ragdolled == value)
					return;

				if (IsConnected)
					throw new InvalidOperationException("You cannot change IsRagdoll property while it is connected to skeleton");

				_ragdolled = value;

				if (value)
				{
					//_animator.enabled = false;
					ConvertToRagdoll();
					EnableRigids(true);
				}
				else
				{
					EnableRigids(false);
					ConvertFromRagdoll();
					//_animator.enabled = true;
				}

				OnIsRagdolledChanged();
			}
		}

		public Rigidbody RagdollRigid { get { return _ragdollRigid; } }

		/// <summary>
		/// is ragdoll connected to skeleton
		/// </summary>
		public bool IsConnected
		{
			get { return _isConnected; }
			set
			{
				if (!gameObject.activeInHierarchy)
				{
					throw new InvalidOperationException("GameObject must be active");
				}

				if (_isConnected == value)
					return;

				if (!IsRagdolled)
				{
					throw new InvalidOperationException("You cannot change this property while it is not ragdolled");
				}

				_isConnected = value;

				SetGravity(!_isConnected);
				_animator.enabled = _isConnected;

				float massFactor = _isConnected ? _massReduction : 1f / _massReduction;
				ApplyMassFactor(massFactor);

				if (_isConnected)
				{
					_rootLimb.joint = CreateRootFollowingJoint(_rootLimb.ragEndTrans);
					CreateFollowingJoints();
					CreateAttachedLimbs();
					UpdateSpring();

					ResetLimbs();
				}
				else
				{
					for (int i = 0; i < _followingLimbs.Length; i++)
					{
						var limb = _followingLimbs[i];
						Destroy(limb.joint);
						ApplyVelocity(limb);
					}

					for (int i = 0; i < _attachedLimbs.Length; i++)
					{
						var limb = _attachedLimbs[i];
						Destroy(limb.joint.connectedBody);
						Destroy(limb.joint);
						ApplyVelocity(limb);
					}

					Destroy(_rootLimb.joint);
					ApplyVelocity(_rootLimb);
				}

				OnIsConnectedChanged();
			}
		}

		public float SpringRate
		{
			set
			{
				_springRate = value;
				UpdateSpring();
			}
		}

		void SetGravity(bool useGravity)
		{
			_rootLimb.ragEndRigid.useGravity = useGravity;
			
			for (int i = 0; i < _attachedLimbs.Length; i++)
			{
				var limb = _attachedLimbs[i];
				limb.ragEndRigid.useGravity = useGravity;
			}

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				limb.ragEndRigid.useGravity = useGravity;
			}
		}

		private void OnIsRagdolledChanged()
		{
			var bahaviours = GetComponents<IRagdollBehaviour>();
			for (int i = 0; i < bahaviours.Length; i++)
			{
				var behaviour = bahaviours[i];
				behaviour.OnIsRagdolledChanged(IsRagdolled);
			}
			IsRagdolledChanged?.Invoke();
		}

		private void OnIsConnectedChanged()
		{
			var behaviours = GetComponents<IRagdollBehaviour>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				var behaviour = behaviours[i];
				behaviour.OnIsConnectedChanged(IsConnected);
			}

			IsConnectedChanged?.Invoke();
		}

		private void ApplyMassFactor(float massFactor)
		{
			_rootLimb.ragEndRigid.mass *= massFactor;
			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				limb.ragEndRigid.mass *= massFactor;
			}
		}

		private static void ApplyVelocity(FollowingLimb limb)
		{
			// force
			limb.ragEndRigid.AddForce(limb.velocity, ForceMode.VelocityChange);

			// torque
			if (limb.ragEndRigid.maxAngularVelocity < 360f)
				limb.ragEndRigid.maxAngularVelocity = 360f;

			Vector3 rotDelta = limb.rotationDelta.eulerAngles;
			rotDelta.x = rotDelta.x > 180f ? rotDelta.x - 360f : rotDelta.x < -180f ? rotDelta.x + 360f : rotDelta.x;
			rotDelta.y = rotDelta.y > 180f ? rotDelta.y - 360f : rotDelta.y < -180f ? rotDelta.y + 360f : rotDelta.y;
			rotDelta.z = rotDelta.z > 180f ? rotDelta.z - 360f : rotDelta.z < -180f ? rotDelta.z + 360f : rotDelta.z;

			var angVel = rotDelta * Mathf.Deg2Rad / Time.deltaTime;

			limb.ragEndRigid.AddRelativeTorque(angVel, ForceMode.VelocityChange);
		}

		public Transform GetRagdollTransform(Transform skeletonTransform)
		{
			if (!IsRagdolled)
			{
				throw new InvalidOperationException("Ragdoll must be ragdolled");
			}

			Transform result;
			if (_tranToRagdoll.TryGetValue(skeletonTransform, out result))
			{
				return result;
			}

			throw new KeyNotFoundException("Key '" + skeletonTransform.ToString() + "' not found");
		}

		public Transform GetSkeletonTransform(Transform ragdollTransform)
		{
			if (!IsRagdolled)
			{
				throw new InvalidOperationException("Ragdoll must be ragdolled");
			}

			Transform result;
			if (_tranToSkelet.TryGetValue(ragdollTransform, out result))
			{
				return result;
			}

			throw new KeyNotFoundException("Key '" + ragdollTransform.ToString() + "' not found");
		}

		private void Awake()
		{
			_animator = GetComponent<Animator>();

			EnableRigids(false);
			ResolveMainColliders(true);
		}

		private void Start()
		{
			if (_convertOnStart)
			{
				IsRagdolled = true;
				IsConnected = true;
			}
		}

		private void EnableRigids(bool enable)
		{
			var rigids = _ragdollRigid.GetComponentsInChildren<Rigidbody>();
			
			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				if (rigid.transform == transform)
					continue;

				switch (_rigidbodyTurnoffType)
				{
					case RigidbodyTurnoffType.IsKinimatic:
						rigid.isKinematic = !enable;
						break;
					case RigidbodyTurnoffType.DetectCollisions:
						rigid.detectCollisions = enable;
						break;
					case RigidbodyTurnoffType.Full:
						rigid.isKinematic = !enable;
						rigid.detectCollisions = enable;
						break;
					default: throw new InvalidOperationException();
				}
			}
		}

		void LateUpdate()
		{
			if (_isConnected)
			{
				UpdateLimbs();
			}
		}

		void ConvertToRagdoll()
		{
			Validate();

			var ragdollTrans = _ragdollRigid.transform;
			_skeletonObject = new GameObject(ragdollTrans.name).transform;
			_skeletonObject.SetParent(ragdollTrans.parent, false);

			var ragdollRoot = new GameObject("ragdollRoot");
			ragdollRoot.transform.SetParent(ragdollTrans.parent, false);
			ragdollTrans.SetParent(ragdollRoot.transform, false);

			CloneSkeleton(ragdollTrans, _skeletonObject);
			FindLimbs(ragdollRoot);

			AnimatorRebind();

			SomeDebugOperationsStart();
		}

		void ConvertFromRagdoll()
		{
			Destroy(_rootLimb.joint);
			foreach (var limb in _followingLimbs)
			{
				Destroy(limb.joint);
			}

			_ragdollRigid.transform.SetParent(_skeletonObject.transform.parent, false);
			_skeletonObject.gameObject.name = "For deletion";
			Destroy(_skeletonObject.gameObject);
			Destroy(_rootLimb.ragBeginTrans.gameObject);

			SomeDebugOperationsEnd();

			AnimatorRebind();

			_skeletonObject = null;
			_rootLimb = null;
			_fixedLimbs = null;
			_followingLimbs = null;
			_tranToSkelet = null;
			_tranToRagdoll = null;
			_attachedLimbs = null;
		}

		private void AnimatorRebind()
		{
			var transs = GetComponentsInChildren<Transform>();
			var transPos = new Vector3[transs.Length];
			var transRot = new Quaternion[transs.Length];
			for (int i = 0; i < transs.Length; i++)
			{
				var trans = transs[i];
				transPos[i] = trans.localPosition;
				transRot[i] = trans.localRotation;
			}

			var clipPositions = new AnimatorStateInfo[_animator.layerCount];
			for (int i = 0; i < _animator.layerCount; i++)
			{
				clipPositions[i] = _animator.GetCurrentAnimatorStateInfo(i);
			}

			var parameters = _animator.parameters;
			foreach (var parameter in parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						parameter.defaultBool = _animator.GetBool(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Float:
						parameter.defaultFloat = _animator.GetFloat(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Int:
						parameter.defaultInt = _animator.GetInteger(parameter.nameHash);
						break;
				}
			}

			_animator.Rebind();

			foreach (var parameter in parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						_animator.SetBool(parameter.nameHash, parameter.defaultBool);
						break;
					case AnimatorControllerParameterType.Float:
						_animator.SetFloat(parameter.nameHash, parameter.defaultFloat, 0, 0);
						break;
					case AnimatorControllerParameterType.Int:
						_animator.SetInteger(parameter.nameHash, parameter.defaultInt);
						break;
				}
			}

			for (int i = 0; i < transs.Length; i++)
			{
				var trans = transs[i];
				trans.localPosition = transPos[i];
				trans.localRotation = transRot[i];
			}

			for (int i = 0; i < _animator.layerCount; i++)
			{
				var state = clipPositions[i];
				_animator.Play(state.shortNameHash, i, state.normalizedTime);
			}
		}

		private void ResolveMainColliders(bool offCollision)
		{
			var mainCollider = GetComponent<Collider>();
			if (mainCollider == null)
				return;

			var colliders = GetComponentsInChildren<Collider>();
			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];
				if (collider == mainCollider)
					continue;

				Physics.IgnoreCollision(mainCollider, collider, offCollision);
				collider.isTrigger = !offCollision;
			}
		}

		private void CreateAttachedLimbs()
		{
			for (int i = 0; i < _attachedLimbs.Length; i++)
			{
				var limb = _attachedLimbs[i];

				var connectedBody = limb.sklEndTrans.gameObject.AddComponent<Rigidbody>();
				connectedBody.isKinematic = true;
				limb.joint = CreateAttachedFollowingJoint(limb.ragEndTrans, connectedBody);
			}
		}

		[Conditional("DEBUG")]
		private void Validate()
		{
			if (_ragdollRigid == null)
			{
				throw new ArgumentNullException("'Ragdoll Rigid' must have value");
			}

			if (_ragdollRigid.transform == this.transform)
			{
				throw new ArgumentException("'Ragdoll Rigid' cannot be on the character object");
			}

			for (var tr = _ragdollRigid.transform.parent; tr != this.transform; tr = tr.parent)
			{
				if (tr.GetComponent<Rigidbody>() != null)
				{
					throw new ArgumentException("'Ragdoll Rigid' have to be set to the top most Rigidbody object");
				}
			}

			for (int i = 0; i < _AttachedLimbs.Length; i++)
			{
				var limb = _AttachedLimbs[i];
				if (limb == null || !limb.transform.IsChildOf(transform))
				{
					throw new ArgumentException("Invalid Attached item " + i.ToString());
				}
			}
		}

		private void CloneSkeleton(Transform from, Transform to)
		{
			_tranToSkelet = new Dictionary<Transform, Transform>();
			_tranToRagdoll = new Dictionary<Transform, Transform>();
			CloneSkeletonRec(from, to);
		}

		public void ResetLimbs()
		{
			// For some reason I'm not able to rid of some movement after animation was changed.
			// So I need to remember position to restore it later
			var pos = transform.localPosition;
			var rot = transform.localRotation;
			_animator.Update(0); // update skeleton bone positions and rotations according to animation
			transform.localPosition = pos;
			transform.localRotation = rot;

			Transform endTr = _rootLimb.sklEndTrans;
			switch(_controllerMoveType)
			{
				case ControllerMoveType.Transform:
					_rootLimb.prevPos = endTr.localPosition;
					_rootLimb.prevRot = endTr.localRotation;
					break;
				case ControllerMoveType.Rigidbody:
					_rootLimb.prevPos = endTr.position;
					_rootLimb.prevRot = endTr.rotation;
					break;
				default: throw new InvalidOperationException();
			}

			for (int i = 0; i < _attachedLimbs.Length; i++)
			{
				var limb = _attachedLimbs[i];
				endTr = limb.sklEndTrans;
				limb.prevPos = endTr.localPosition;
				limb.prevRot = endTr.localRotation;
			}

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				endTr = limb.sklEndTrans;
				limb.prevPos = endTr.localPosition;
				limb.prevRot = endTr.localRotation;
			}

			UpdateJoints();
		}

		[Conditional("DEBUG")]
		private void SomeDebugOperationsStart()
		{
			var ragdollGO = _ragdollRigid.gameObject;
			if (ragdollGO.GetComponent<SkeletonDrawer>() == null)
			{
				ragdollGO.AddComponent<SkeletonDrawer>().Color = Color.blue;
			}
			if (_skeletonObject.gameObject.GetComponent<SkeletonDrawer>() == null)
			{
				_skeletonObject.gameObject.AddComponent<SkeletonDrawer>().Color = Color.green;
			}

			if (_deleteColliders)
			{
				foreach (var item in ragdollGO.GetComponentsInChildren<Collider>())
				{
					Destroy(item);
				}
			}

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];

				if (_deleteJoints)
					Destroy(limb.curJoint);
			}
		}

		[Conditional("DEBUG")]
		private void SomeDebugOperationsEnd()
		{
			var ragdollGO = _ragdollRigid.gameObject;
			Destroy(ragdollGO.GetComponent<SkeletonDrawer>());
			Destroy(_skeletonObject.gameObject.AddComponent<SkeletonDrawer>());
		}

		private void FindLimbs(GameObject ragdollRoot)
		{
			var followingLimbs = new List<FollowingLimb>();
			var attachedLimbs = new List<FollowingLimb>();
			var fixedLimbs = new List<FixedLimb>();

			foreach (var item in _tranToSkelet)
			{
				var ragTransFrom = item.Key;
				var sklTransFrom = item.Value;
				var ragRigidFrom = ragTransFrom.GetComponent<Rigidbody>();
				var curJoint = ragTransFrom.GetComponent<Joint>();

				if (ragRigidFrom == null)
				{
					FixedLimb fixedLimb;
					fixedLimb.sklTransFrom = sklTransFrom;
					fixedLimb.ragTransFrom = ragTransFrom;
					fixedLimbs.Add(fixedLimb);
					continue;
				}

				if (curJoint == null)
				{
					if (_ragdollRigid == null)
					{
						_ragdollRigid = ragRigidFrom;
					}
					else if (ragRigidFrom.transform != _ragdollRigid.transform)
					{
						throw new InvalidOperationException(
							"You must have only one rigid without joint attached. " +
							"And it must be a root bone");
					}

					var ragTransTo = ragdollRoot.transform;

					FollowingLimb followingLimb = new FollowingLimb();

					followingLimb.sklBeginTrans = this.transform;
					followingLimb.sklEndTrans = sklTransFrom;

					followingLimb.ragBeginTrans = ragTransTo;
					followingLimb.ragEndTrans = ragTransFrom;
					followingLimb.ragEndRigid = ragRigidFrom;

					_rootLimb = followingLimb;

					continue;
				}
				
				{
					var ragRigTo = curJoint.connectedBody;
					var ragTransTo = ragRigTo.transform;

					FollowingLimb followingLimb = new FollowingLimb();

					followingLimb.sklBeginTrans = GetSkeletonTransform(ragTransTo);
					followingLimb.sklEndTrans = sklTransFrom;

					followingLimb.ragBeginTrans = ragTransTo;
					followingLimb.ragBeginRigid = ragRigTo;
					followingLimb.ragEndTrans = ragTransFrom;
					followingLimb.ragEndRigid = ragRigidFrom;

					followingLimb.curJoint = curJoint;

					bool isAttached = Array.Exists(_AttachedLimbs, l => l == ragRigidFrom);
					if (isAttached)
					{
						attachedLimbs.Add(followingLimb);
					}
					else
					{
						followingLimbs.Add(followingLimb);
					}
				}
			}

			_followingLimbs = followingLimbs.ToArray();
			_attachedLimbs = attachedLimbs.ToArray();
			_fixedLimbs = fixedLimbs.ToArray();
		}

		private void CreateFollowingJoints()
		{
			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				
				limb.joint = CreateFollowingJoint(limb.ragEndTrans, limb.ragBeginRigid);
			}
		}

		private static ConfigurableJoint CreateAttachedFollowingJoint(Transform transFrom, Rigidbody connectedBody)
		{
			// Work around for joint position & rotation bug
			var tmpLocPosition = transFrom.localPosition;
			var tmpLocRotation = transFrom.localRotation;
			transFrom.position = connectedBody.transform.position;
			transFrom.rotation = connectedBody.transform.rotation;

			var joint = transFrom.gameObject.AddComponent<ConfigurableJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;
			joint.connectedBody = connectedBody;
			joint.rotationDriveMode = RotationDriveMode.Slerp;

			transFrom.localPosition = tmpLocPosition;
			transFrom.localRotation = tmpLocRotation;

			return joint;
		}

		private static ConfigurableJoint CreateRootFollowingJoint(Transform transFrom)
		{
			// Work around for joint position & rotation bug
			var tmpLocPosition = transFrom.localPosition;
			var tmpLocRotation = transFrom.localRotation;
			transFrom.position = Vector3.zero;
			transFrom.rotation = Quaternion.identity;

			var joint = transFrom.gameObject.AddComponent<ConfigurableJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;
			//joint.connectedBody = connectedBody;
			joint.rotationDriveMode = RotationDriveMode.Slerp;
			joint.swapBodies = true;
			joint.configuredInWorldSpace = true;

			// revert position back
			transFrom.localPosition = tmpLocPosition;
			transFrom.localRotation = tmpLocRotation;

			return joint;
		}

		private void UpdateSpring()
		{
			var drive = new JointDrive()
			{
				positionDamper = 0f,
				maximumForce = float.MaxValue
			};

			// Create Attached Following Joint
			drive.positionSpring = _jointSpringAttach * _springRate;

			for (int i = 0; i < _attachedLimbs.Length; i++)
			{
				var joint = _attachedLimbs[i].joint;
				joint.slerpDrive = drive;
				joint.xDrive = drive;
				joint.yDrive = drive;
				joint.zDrive = drive;
			}

			// Create Root Following Joint
			drive.positionSpring = _jointSpringRoot * _springRate;

			{
				var joint = _rootLimb.joint;
				joint.slerpDrive = drive;
				joint.xDrive = drive;
				joint.yDrive = drive;
				joint.zDrive = drive;
			}

			// Create Following Joint
			drive.positionSpring = _jointSpring * _springRate;

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var joint = _followingLimbs[i].joint;
				joint.slerpDrive = drive;
				joint.xDrive = drive;
				joint.yDrive = drive;
				joint.zDrive = drive;
			}
		}

		private static ConfigurableJoint CreateFollowingJoint(Transform transFrom, Rigidbody connectedBody)
		{
			// Work around for joint position & rotation bug
			var tmpLocPosition = transFrom.localPosition;
			var tmpLocRotation = transFrom.localRotation;
			transFrom.position = connectedBody.transform.position;
			transFrom.rotation = connectedBody.transform.rotation;

			var joint = transFrom.gameObject.AddComponent<ConfigurableJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.anchor = Vector3.zero;
			joint.connectedAnchor = Vector3.zero;
			joint.connectedBody = connectedBody;
			joint.rotationDriveMode = RotationDriveMode.Slerp;
			joint.swapBodies = true;

			// revert position back
			transFrom.localPosition = tmpLocPosition;
			transFrom.localRotation = tmpLocRotation;

			return joint;
		}

		private void UpdateLimbs()
		{
			UpdateJoints();

			switch(_controllerMoveType)
			{
				case ControllerMoveType.Transform:
					AddLimbMovement(_rootLimb);
					break;
				case ControllerMoveType.Rigidbody:
					AddLimbMovementWithRootMove(_rootLimb);
					break;
				default: throw new InvalidOperationException();
			}

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				AddLimbMovement(limb);
			}

			for (int i = 0; i < _attachedLimbs.Length; i++)
			{
				var limb = _attachedLimbs[i];
				AddLimbMovement(limb);
			}
		}

		private void UpdateJoints()
		{
			for (int i = 0; i < _fixedLimbs.Length; i++)
			{
				var limb = _fixedLimbs[i];
				limb.ragTransFrom.localPosition = limb.sklTransFrom.localPosition;
				limb.ragTransFrom.localRotation = limb.sklTransFrom.localRotation;
			}

			UpdateRootJoint(_rootLimb);

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				UpdateLimbJoint(limb);
			}
		}

		private void UpdateRootJoint(FollowingLimb limb)
		{
			// set target rotation
			limb.joint.targetRotation = limb.sklEndTrans.rotation;

			// set target position
			limb.joint.targetPosition = limb.sklEndTrans.position;
		}

		private void UpdateLimbJoint(FollowingLimb limb)
		{
			// set target rotation
			limb.joint.targetRotation = Quaternion.Inverse(limb.sklBeginTrans.rotation) * limb.sklEndTrans.rotation;

			// set target position
			Vector3 posDistance = limb.sklEndTrans.position - limb.sklBeginTrans.position;
			limb.joint.targetPosition = limb.sklBeginTrans.InverseTransformDirection(posDistance);
		}

		private void AddLimbMovementWithRootMove(FollowingLimb limb)
		{
			// add position delta
			Vector3 positionDelta = limb.sklEndTrans.position - limb.prevPos;
			limb.prevPos = limb.sklEndTrans.position;
			limb.ragEndTrans.position += positionDelta;

			// add rotation delta
			limb.rotationDelta = Quaternion.Inverse(limb.prevRot) * limb.sklEndTrans.rotation;
			limb.prevRot = limb.sklEndTrans.rotation;
			limb.ragEndTrans.rotation *= limb.rotationDelta;

			// save velocity
			Vector3 sklCenterOfMassPos = limb.sklEndTrans.TransformPoint(limb.ragEndRigid.centerOfMass);
			limb.velocity = (sklCenterOfMassPos - limb.prevMassPos) / Time.deltaTime;
			limb.prevMassPos = sklCenterOfMassPos;
		}

		private void AddLimbMovement(FollowingLimb limb)
		{
			// add position delta
			Vector3 positionDelta = limb.sklEndTrans.localPosition - limb.prevPos;
			limb.prevPos = limb.sklEndTrans.localPosition;
			limb.ragEndTrans.localPosition += positionDelta;

			// add rotation delta
			limb.rotationDelta = Quaternion.Inverse(limb.prevRot) * limb.sklEndTrans.localRotation;
			limb.prevRot = limb.sklEndTrans.localRotation;
			limb.ragEndTrans.localRotation *= limb.rotationDelta;

			// save velocity
			Vector3 sklCenterOfMassPos = limb.sklEndTrans.TransformPoint(limb.ragEndRigid.centerOfMass);
			limb.velocity = (sklCenterOfMassPos - limb.prevMassPos) / Time.deltaTime;
			limb.prevMassPos = sklCenterOfMassPos;
		}

		private void CloneSkeletonRec(Transform tr, Transform newTr)
		{
			_tranToSkelet.Add(tr, newTr);
			_tranToRagdoll.Add(newTr, tr);
			ReflectMatrix(tr, newTr);

			for (int i = 0; i < tr.childCount; i++)
			{
				var child = tr.GetChild(i);
				var newChild = new GameObject(child.name).transform;

				newChild.SetParent(newTr, false);

				CloneSkeletonRec(child, newChild);
			}
		}

		private static void ReflectMatrix(Transform from, Transform to)
		{
			to.localPosition = from.localPosition;
			to.localRotation = from.localRotation;
			to.localScale = from.localScale;
		}
	}
}
