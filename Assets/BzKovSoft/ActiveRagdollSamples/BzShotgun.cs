using System;
using System.Collections;
using BzKovSoft.ActiveRagdoll;
using UnityEngine;

namespace BzKovSoft.ActiveRagdollSamples
{
	public class BzShotgun : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
		GameObject _bulletPrefab;
		[SerializeField]
		float _mass = 10f;
		[SerializeField]
		float _velocity = 30f;
#pragma warning restore 0649

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				StartCoroutine(ThrowBullet());
			}
		}

		private IEnumerator ThrowBullet()
		{
			var bullet = Instantiate(_bulletPrefab);

			var rigid = bullet.AddComponent<Rigidbody>();
			rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			rigid.mass = _mass;

			bullet.transform.position = Camera.main.transform.position;
			rigid.velocity = Camera.main.transform.rotation * Vector3.forward * _velocity;

			yield return new WaitForSeconds(10f);

			Destroy(bullet);
		}
	}
}