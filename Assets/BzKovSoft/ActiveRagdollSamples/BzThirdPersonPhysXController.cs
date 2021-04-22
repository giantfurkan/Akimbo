using System;
using System.Linq;
using BzKovSoft.ActiveRagdoll;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	[DisallowMultipleComponent]
	public class BzThirdPersonPhysXController : MonoBehaviour
	{
		float _jumpForce = 4f;

		readonly int _animatorVelocityX = Animator.StringToHash("VelocityX");
		readonly int _animatorVelocityY = Animator.StringToHash("VelocityY");
		readonly int _animatorJump = Animator.StringToHash("Jump");
		readonly int _animatorJumpLeg = Animator.StringToHash("JumpLeg");
		readonly int _animatorJumpProgress = Animator.StringToHash("JumpProgress");
		const float RunCycleLegOffset = 0.2f;   // animation cycle offset (0-1) used for determining correct leg to jump off
		CharacterController _characterController;
		Animator _animator;
		float _velocityY;

		void OnEnable()
		{
			_characterController = GetComponent<CharacterController>();
			_animator = GetComponent<Animator>();
		}

		private void Update()
		{
			float x = Input.GetAxis("Horizontal");
			float y = Input.GetAxis("Vertical");

			if (Input.GetMouseButton(1))
			{
				x = 0;
				y = 0;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				_velocityY += _jumpForce;

				// calculate which leg is behind, so as to leave that leg trailing in the jump animation
				// (This code is reliant on the specific run cycle offset in our animations,
				// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
				float runCycle = Mathf.Repeat(
						_animator.GetCurrentAnimatorStateInfo(0).normalizedTime + RunCycleLegOffset, 1);

				float jumpLeg = (runCycle < 0.5f ? 1 : -1) * y;
				_animator.SetFloat(_animatorJumpLeg, jumpLeg);
			}

			bool isGrounded = _characterController.isGrounded;
			bool inJump = !isGrounded | _velocityY > 0f;
			if (inJump)
			{
				_animator.SetFloat(_animatorJumpProgress, _velocityY);
			}

			// update the animator parameters
			_animator.SetBool(_animatorJump, !isGrounded);
			_animator.SetFloat(_animatorVelocityX, x, 0.2f, Time.deltaTime);
			_animator.SetFloat(_animatorVelocityY, y, 0.2f, Time.deltaTime);
		}

		void OnAnimatorMove()
		{
			if (Time.deltaTime < Mathf.Epsilon)
				return;

			_velocityY += Physics.gravity.y * Time.deltaTime;
			var delta = _animator.deltaPosition;
			delta.y += _velocityY * Time.deltaTime;
			_characterController.Move(delta);

			if (_characterController.isGrounded)
			{
				_velocityY = 0f;
			}
		}
	}
}