using BzKovSoft.ActiveRagdoll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	[DisallowMultipleComponent]
	public class BzControllerSwitcher : MonoBehaviour
	{
		IBzRagdoll _ragdoll;
		BzThirdPersonActiveRagdollAndIK _ragdollController;
		BzThirdPersonPhysXController _physXController;
		CharacterController _characterController;
		Animator _animator;

		void OnEnable()
		{
			_ragdoll = GetComponent<IBzRagdoll>();
			_ragdollController = GetComponent<BzThirdPersonActiveRagdollAndIK>();
			_physXController = GetComponent<BzThirdPersonPhysXController>();
			_characterController = GetComponent<CharacterController>();
			_animator = GetComponent<Animator>();
			_ragdoll.IsConnectedChanged += RefreshLayout;
			_ragdoll.IsRagdolledChanged += RefreshLayout;

		}

		void OnDisable()
		{
			_ragdoll.IsConnectedChanged -= RefreshLayout;
			_ragdoll.IsRagdolledChanged -= RefreshLayout;
		}

		private void Start()
		{
			ConvertToRagdoll(true);
		}

		private void ConvertToRagdoll(bool toRagdoll)
		{
			_characterController.enabled = !toRagdoll;
			_physXController.enabled = !toRagdoll;
			_ragdollController.enabled = toRagdoll;

			if (toRagdoll)
			{
				_ragdoll.IsRagdolled = true;
				_ragdoll.IsConnected = true;
			}
			else
			{
				_ragdoll.IsConnected = false;
				_ragdoll.IsRagdolled = false;
				_animator.enabled = true;
			}
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Keypad1) | Input.GetKeyDown(KeyCode.Alpha1))
			{
				ConvertToRagdoll(!_ragdoll.IsRagdolled);
			}
			if (Input.GetKeyDown(KeyCode.Keypad2) | Input.GetKeyDown(KeyCode.Alpha2))
			{
				_ragdoll.IsConnected = !_ragdoll.IsConnected;
			}
		}

		public void RefreshLayout()
		{
			drawText =
@"press 1 - switch IsRagdolled
press 2 - switch IsConnected
IsRagdolled == " + _ragdoll.IsRagdolled.ToString() + @"
IsConnected == " + _ragdoll.IsConnected.ToString() + @"
";
		}

		static string drawText = null;

		void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 2000, 2000), drawText);
		}
	}
}
