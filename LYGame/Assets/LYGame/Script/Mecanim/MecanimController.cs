using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LYGame
{
	public class MecanimController : MonoBehaviour
	{
		public const string STATE_PARAMETER = "state";

		public delegate void AnimationEndHandler(int context);
		public AnimationEndHandler animation_end_handler;

		[ReadOnly]
		[LabelText("动作列表")]
		public List<string> states;

		private Animator animator;
		private int current_index;

		private void Awake()
		{
			this.animator = this.GetComponent<Animator>();
		}

		public void SetState(string state)
		{
			int index = this.states.IndexOf(state);
			if (index == -1)
				return;

			this.SetStateIndex(index);
		}

		public void SetStateIndex(int index)
		{
			if (this.animator == null)
				return;

			this.current_index = index;
			this.animator.SetInteger(MecanimController.STATE_PARAMETER, index);
		}

		public void OnAnimationEnd(int context)
		{
			if (this.animation_end_handler == null)
				return;

			this.animation_end_handler.Invoke(context);
		}

		public static bool IsPlaying()
		{
			return Application.isPlaying;
		}

		[EnableIf("IsPlaying")]
		[Button("切换动作")]
		private void SetStateParameter(int index)
		{
			if (index < 0 || index >= this.states.Count)
				return;
			this.SetState(this.states[index]);
		}

		[EnableIf("IsPlaying")]
		[Button("重播当前动作", ButtonSizes.Medium)]
		[GUIColor(0.4f, 0.8f, 1)]
		private void ReplayCurrent()
		{
			string state = this.states[this.current_index];
			this.animator.Play(state, -1, 0.0f);
		}
	}
}


