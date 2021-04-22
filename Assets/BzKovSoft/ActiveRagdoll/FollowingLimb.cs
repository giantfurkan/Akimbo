using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	class FollowingLimb
	{
		public Transform sklBeginTrans;
		public Transform sklEndTrans;
		public Transform ragBeginTrans;
		public Transform ragEndTrans;
		public Rigidbody ragBeginRigid;
		public Rigidbody ragEndRigid;
		public Joint curJoint;
		public ConfigurableJoint joint;
		public Vector3 prevPos;
		public Quaternion prevRot;
		public Vector3 velocity;
		public Vector3 prevMassPos;
		public Quaternion rotationDelta;
	}
}
