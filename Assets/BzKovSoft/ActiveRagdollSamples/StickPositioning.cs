using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	public class StickPositioning : MonoBehaviour
	{
		Transform _camTrans;
		Vector3 _pos;
		Vector3 _posPrev;
		Quaternion _rot;
		Quaternion _rotPrev;

		[SerializeField]
		float _velocityMultiplyer = 2f;

		void Start()
		{
			_camTrans = Camera.main.transform;
			_pos = _posPrev = _camTrans.position;
			_rot = _rotPrev = _camTrans.rotation;
		}

		void Update()
		{
			_posPrev = _pos;
			_rotPrev = _rot;
			_pos = _camTrans.position;
			_rot = _camTrans.rotation;

			transform.position = _pos;
			transform.rotation = _rot;
		}

		void OnCollisionEnter(Collision collision)
		{
			OnCollisionMy(collision);
		}

		void OnCollisionStay(Collision collision)
		{
			OnCollisionMy(collision);
		}

		void OnCollisionMy(Collision collision)
		{
			if (collision.rigidbody == null)
				return;

			var contact = collision.contacts[0];

			var rotDelta = Quaternion.Inverse(_rotPrev) * _rot;
			Vector3 pointLoc2 = transform.InverseTransformPoint(contact.point);
			Vector3 pointLoc1 = Quaternion.Inverse(rotDelta) * pointLoc2;

			Vector3 objVel = _pos - _posPrev;
			Vector3 pointVel = transform.TransformVector(pointLoc2 - pointLoc1);
			Vector3 totalVel = objVel + pointVel;
			totalVel = totalVel / Time.deltaTime;

			collision.rigidbody.AddForce(totalVel * _velocityMultiplyer, ForceMode.VelocityChange);
		}
	}
}