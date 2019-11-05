using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LYGame
{
	public class GPUSkinSample : MonoBehaviour
	{
		public GameObject prefab;
		public int cnt_radius = 20;
		public float gap = 3;

		private void Start()
		{
			if (this.prefab == null)
				return;

			for (int i = -this.cnt_radius; i <= this.cnt_radius; ++i)
			{
				for (int j = -this.cnt_radius; j <= this.cnt_radius; ++j)
				{
					GameObject go = Object.Instantiate(this.prefab);
					go.transform.position = Vector3.right * i * gap + Vector3.forward * j * gap;
					GPUSkinController instance = go.GetComponent<GPUSkinController>();
					int index = Random.Range(0, 3);
					instance.SetAnimation(index, Random.Range(0.0f, 1.0f));
					instance.SetSpeed(1.0f);
				}
			}
		}
	}
}
