using System;
using System.Linq;
using BzKovSoft.ActiveRagdoll;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	public class BzThirdPersonActiveRagdoll : MonoBehaviour
	{
		readonly int _animatorVelocityX = Animator.StringToHash("VelocityX");
		readonly int _animatorVelocityY = Animator.StringToHash("VelocityY");

		Animator _animator;
		IBzRagdoll _ragdoll;
		IBzBalancer _balancer;
		float _connectionTime;

		void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_ragdoll = GetComponent<IBzRagdoll>();
			_balancer = GetComponent<IBzBalancer>();

			if (_balancer == null)
				throw new InvalidOperationException("No balancer defined on the character");

			RefreshLayout();
		}

		private void Update()
		{
			ProcessKeyPressings();

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
					RefreshLayout();
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

			// update the animator parameters
			_animator.SetFloat(_animatorVelocityX, x, 0.2f, Time.deltaTime);
			_animator.SetFloat(_animatorVelocityY, y, 0.2f, Time.deltaTime);
		}

		private void ProcessKeyPressings()
		{
			if (Input.GetKeyDown(KeyCode.Keypad1) | Input.GetKeyDown(KeyCode.Alpha1))
			{
				_ragdoll.IsConnected = false;
				RefreshLayout();
			}
			if (Input.GetKeyDown(KeyCode.Keypad2) | Input.GetKeyDown(KeyCode.Alpha2))
			{
				_ragdoll.IsConnected = true;
				_connectionTime = Time.time;
				RefreshLayout();
			}
			if (Input.GetKeyDown(KeyCode.Keypad3) | Input.GetKeyDown(KeyCode.Alpha3))
			{
				_ragdoll.IsRagdolled = false;
				RefreshLayout();
			}
			if (Input.GetKeyDown(KeyCode.Keypad4) | Input.GetKeyDown(KeyCode.Alpha4))
			{
				_ragdoll.IsRagdolled = true;
				RefreshLayout();
			}
			if (Input.GetKeyDown(KeyCode.Q))
			{
				if (_ragdoll.IsRagdolled)
				{
					_ragdoll.IsConnected = false;
					_ragdoll.IsRagdolled = false;
				}
				else
				{
					_ragdoll.IsRagdolled = true;
					_ragdoll.IsConnected = true;
					_connectionTime = Time.time;
				}
				RefreshLayout();
			}
		}

		public void RefreshLayout()
		{
			drawText =
@"Q - Convert to/from Ragdoll+Connected
1 - Connected = false
2 - Connected = true
3 - IsRagdoll = false
4 - IsRagdoll = true
ConnectedToSkeleton == " + _ragdoll.IsConnected.ToString() + @"
IsRagdolled == " + _ragdoll.IsRagdolled.ToString();
		}

		static string drawText = null;

		void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 2000, 2000), drawText);
		}
	}
}