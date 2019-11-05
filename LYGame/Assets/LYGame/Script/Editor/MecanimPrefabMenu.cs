using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace LYGame.Editor
{
	public class MecanimPrefab : MonoBehaviour
	{
		[MenuItem("Assets/LYGame/生成动作预设", false, 1)]
		static void CreateAnimatorPrefab()
		{
			EditorBaseHelp.ForeachSelection(MecanimPrefab.CreateAnimatorPrefabImpl);
			AssetDatabase.SaveAssets();
		}

		static ForeachSelectionResult CreateAnimatorPrefabImpl(UnityEngine.Object obj)
		{
			string project_relative_path = AssetDatabase.GetAssetPath(obj);
			if (!project_relative_path.EndsWith(".fbx") && !project_relative_path.EndsWith(".FBX"))
			{
				Debug.LogError("所选对象不是模型 !!!");
				return ForeachSelectionResult.result_continue;
			}

			// 确保有配置文件
			string folder = BaseHelp.GetParentDir(project_relative_path);
			string clip_file_path = string.Format("{0}/MecanimConfig.asset", folder);
			MecanimConfig mecanim_config = AssetDatabase.LoadAssetAtPath(clip_file_path, typeof(MecanimConfig)) as MecanimConfig;
			if (!mecanim_config)
				return ForeachSelectionResult.result_continue;

			// 创建Animator，添加一个名为"state"的变量
			string name = Path.GetFileNameWithoutExtension(project_relative_path);
			string animator_file_path = string.Format("{0}/{1}.controller", folder, name);
			AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animator_file_path);
			controller.AddParameter(MecanimController.STATE_PARAMETER, AnimatorControllerParameterType.Int);

			List<AnimationClip> clips = new List<AnimationClip>();
			AnimatorStateMachine sm = controller.layers[0].stateMachine;
			UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(project_relative_path);
			int count = objects.Length;
			for (int i = 0; i < count; ++i)
			{
				UnityEngine.Object one = objects[i];
				AnimationClip clip = one as AnimationClip;
				if (!clip)
					continue;

				// 排除原始的"__preview__"动作
				if (clip.name.StartsWith("__"))
					continue;

				clips.Add(clip);
			}

			// 调整排序和配置一致
			clips.Sort
			(
				delegate (AnimationClip c1, AnimationClip c2)
				{
					int index_1 = -1;
					int index_2 = -1;
					int index_count = mecanim_config.clips.Count;
					for (int i = 0; i < index_count; ++i)
					{
						MecanimClipConfig clip = mecanim_config.clips[i];
						if (c1.name == clip.name)
							index_1 = i;
						else if (c2.name == clip.name)
							index_2 = i;
						if (index_1 != -1 && index_2 != -1)
							break;
					}
					return (index_1 < index_2) ? -1 : 1;
				}
			);

			// 创建状态
			List<string> states = new List<string>();
			count = clips.Count;
			for (int i = 0; i < count; ++i)
			{
				// 绑定clip
				AnimationClip clip = clips[i];
				AnimatorState state = sm.AddState(clip.name);
				state.motion = clip;

				// 动作切换
				AnimatorStateTransition transition = sm.AddAnyStateTransition(state);
				transition.AddCondition(AnimatorConditionMode.Equals, states.Count, MecanimController.STATE_PARAMETER);
				transition.canTransitionToSelf = false;
				transition.duration = 0;

				states.Add(clip.name);
			}

			// 创建预设，添加MecanimController
			Object template = AssetDatabase.LoadAssetAtPath(project_relative_path, typeof(GameObject));
			GameObject instance = Object.Instantiate(template) as GameObject;

			Animator animator = BaseHelp.EnsureHasComponent<Animator>(instance) as Animator;
			animator.runtimeAnimatorController = controller;

			MecanimController mecanim = BaseHelp.EnsureHasComponent<MecanimController>(instance) as MecanimController;
			mecanim.states = states;

			// 如果有材质则绑定材质
			if (mecanim_config.material != null)
			{
				Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
				if (renderers != null)
				{
					foreach (Renderer one in renderers)
						one.material = mecanim_config.material;
				}
			}

			string prefab_path = string.Format("{0}/{1}.prefab", folder, name);
			PrefabUtility.SaveAsPrefabAsset(instance, prefab_path);
			Object.DestroyImmediate(instance, true);

			return ForeachSelectionResult.result_none;
		}
	}
}
