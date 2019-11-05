using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LYGame
{
	[Serializable]
	public class GPUSkinSection
	{
		public int start_row;
		public int end_row;
	}

	public class GPUSkinController : MonoBehaviour
	{
		[ReadOnly]
		[LabelText("动作帧区间")]
		public List<GPUSkinSection> sections;

		private GPUSkinSection cur_section;
		private float time_offset;
		private Vector4 shader_value;
		private MaterialPropertyBlock property_block;
		private MeshRenderer mesh_renderer;

		private void Awake()
		{
			this.shader_value = Vector4.one;
			this.property_block = new MaterialPropertyBlock();
			this.mesh_renderer = this.GetComponent<MeshRenderer>();
		}

		/// <summary>
		/// 设置动作
		/// </summary>
		/// <param name="index">动作索引</param>
		/// <param name="offset">起始时间偏移</param>
		[Button("切换动作")]
		public void SetAnimation(int index, float time_offset = 0)
		{
			if (index < 0 || index >= this.sections.Count)
				return;

			this.cur_section = this.sections[index];
			this.time_offset = time_offset;

			this.shader_value.x = this.cur_section.start_row;
			this.shader_value.y = this.cur_section.end_row;
			this.shader_value.z = (int)(this.time_offset * (this.cur_section.end_row - this.cur_section.start_row + 1));
			this.UpdateMaterial();
		}

		[Button("设置速度")]
		public void SetSpeed(float speed)
		{
			this.shader_value.w = speed;
			this.UpdateMaterial();
		}

		private void UpdateMaterial()
		{
			this.property_block.SetVector("_Dynamic", this.shader_value);
			this.mesh_renderer.SetPropertyBlock(this.property_block);
		}
	}
}
