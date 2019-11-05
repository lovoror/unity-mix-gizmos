using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LYGame
{
	public enum MecanimAnimationEventType
	{
		SetState,
		SetStateIndex,
		OnAnimationEnd,
	}

	[Serializable]
	public class MecanimClipEvent
	{
		[LabelText("时间百分比[0, 1]")]
		public float time;

		[LabelText("函数类型")]
		[EnumPaging]
		public MecanimAnimationEventType evt_type;

		[LabelText("str参数")]
		public string str_param;

		[LabelText("int参数")]
		public int int_param;
	}

	[Serializable]
	public class MecanimClipConfig
	{
		[LabelText("动作名")]
		public string name;

		[LabelText("起始帧")]
		public int start_frame;

		[LabelText("结束帧")]
		public int end_frame;

		[LabelText("循环播放")]
		public bool loop;

		[LabelText("事件")]
		public List<MecanimClipEvent> evts;
	}

	[Serializable]
	[CreateAssetMenu(fileName = "MecanimConfig", menuName = "LYGame/MecanimConfig")]
	public class MecanimConfig : ScriptableObject
	{
		[BoxGroup("动作导入设置")]
		[LabelText("允许读写")]
		public bool enable_read_write;

		[BoxGroup("材质")]
		public Material material;

		[BoxGroup("动作导入设置")]
		public List<MecanimClipConfig> clips;
	}
}
