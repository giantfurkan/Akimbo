using System;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.RagdollBehaviours
{
	/// <summary>
	/// Active ragdoll event handler. This component stabilize bone positions after it was hit.
	/// </summary>
	public class JointBounceStabilizerBehaviour : MonoBehaviour, IRagdollBehaviour
	{
		[SerializeField]
		float _suppressForce = 40f;
		[SerializeField]
		float _distTreshold = 0.2f;

		IBzRagdoll _ragdoll;
		BounceJoint[] _joints;

		Rigidbody _velocityCenter;

		void OnEnable()
		{
			_ragdoll = GetComponent<IBzRagdoll>();
		}

		public void OnIsRagdolledChanged(bool newValue)
		{
		}

		public void OnIsConnectedChanged(bool newValue)
		{
			if (newValue & isActiveAndEnabled)
			{
				_velocityCenter = _ragdoll.RagdollRigid;
				var joints = GetComponentsInChildren<ConfigurableJoint>();

				_joints = new BounceJoint[joints.Length];
				for (int i = 0; i < joints.Length; i++)
				{
					var bj = new BounceJoint();
					_joints[i] = bj;
					var joint = joints[i];
					bj.ragdollRigid = joint.gameObject.GetComponent<Rigidbody>();
					bj.skeleton =  _ragdoll.GetSkeletonTransform(joint.transform);
				}
			}
			else
			{
				_joints = null;
			}
		}

		private void FixedUpdate()
		{
			if (_joints == null || !_ragdoll.IsConnected)
				return;
			
			Vector3 mainVelocity = _velocityCenter.velocity;
			Vector3 mainAngularVelocity = _velocityCenter.angularVelocity;

			for (int i = 0; i < _joints.Length; i++)
			{
				var joint = _joints[i];

				Vector3 toVel = mainVelocity;

				Rigidbody fromRigid = joint.ragdollRigid;
				Vector3 fromVel = fromRigid.velocity;
				Transform from = fromRigid.transform;

				Vector3 dist = joint.skeleton.position - from.position;
				Vector3 dir = toVel - fromVel;
				float dot = Vector3.Dot(-dist.normalized, dir.normalized);
				bool needSuppressPos = dot > 0 & dist.magnitude < _distTreshold;

				if (needSuppressPos)
				{
					float suppressForce = _suppressForce * Time.deltaTime; 
					fromRigid.velocity = Vector3.Lerp(fromVel, mainVelocity, suppressForce);

					fromRigid.angularVelocity = Vector3.Lerp(fromRigid.angularVelocity, mainAngularVelocity, suppressForce);
				}
			}
		}

		class BounceJoint
		{
			public Transform skeleton;
			public Rigidbody ragdollRigid;
		}
	}
}