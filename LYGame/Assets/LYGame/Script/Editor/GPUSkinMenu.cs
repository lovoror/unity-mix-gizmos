using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace LYGame.Editor
{
	public class GPUSkinMenu
	{
		[MenuItem("Assets/LYGame/烘焙GPUSkin", false, 2)]
		static void BakeGPUSkin()
		{
			EditorBaseHelp.ForeachSelection(GPUSkinMenu.BakeGPUSkinImpl);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		static ForeachSelectionResult BakeGPUSkinImpl(UnityEngine.Object obj)
		{
			string project_relative_path = AssetDatabase.GetAssetPath(obj);
			string folder = BaseHelp.GetParentDir(project_relative_path);
			string file_name = Path.GetFileNameWithoutExtension(project_relative_path);

			Object template = AssetDatabase.LoadAssetAtPath<Object>(project_relative_path) as Object;
			GameObject go = Object.Instantiate(template) as GameObject;
			Animator animator = go.GetComponent<Animator>();
			if (animator == null)
				return ForeachSelectionResult.result_continue;

			SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
			if (smr == null)
				return ForeachSelectionResult.result_continue;

			AnimatorController animator_controller = (AnimatorController)animator.runtimeAnimatorController;
			if (animator_controller == null)
				return ForeachSelectionResult.result_continue;

			Mesh baked_mesh = new Mesh();
			int vertices_count = smr.sharedMesh.vertexCount;

			// 贴图横坐标为顶点索引，纵坐标为采样帧索引
			int texture_width = Mathf.NextPowerOfTwo(vertices_count);

			// 动作区段配置
			List<GPUSkinSection> sections = new List<GPUSkinSection>();

			// 先计算贴图的高并创建贴图
			int texture_height = 0;
			float total_time = 0;
			AnimationClip[] clips = animator_controller.animationClips;
			int clip_count = clips.Length;
			for (int i = 0; i < clip_count; ++i)
			{
				AnimationClip clip = clips[i];
				int cur_clip_frame = (int)(clip.frameRate * clip.length);
				texture_height += cur_clip_frame;
				total_time += clip.length;
			}

			Texture2D sample_texture = new Texture2D(texture_width, texture_height, TextureFormat.RGBAHalf, false);
			int texture_cur_y = 0;
			for (int i = 0; i < clip_count; ++i)
			{
				AnimationClip clip = clips[i];

				int cur_clip_frame = (int)(clip.frameRate * clip.length);
				float per_frame_time = clip.length / cur_clip_frame;

				GPUSkinSection gpu_skin_section = new GPUSkinSection();
				gpu_skin_section.start_row = texture_cur_y;
				gpu_skin_section.end_row = texture_cur_y + cur_clip_frame - 1;
				sections.Add(gpu_skin_section);

				float sample_time = 0;
				for (int j = 0; j < cur_clip_frame; ++j)
				{
					// 设置采样时间并烘焙
					clip.SampleAnimation(go, sample_time);
					smr.BakeMesh(baked_mesh);

					// 写入顶点信息
					for (int k = 0; k < vertices_count; ++k)
					{
						Vector3 vertex = baked_mesh.vertices[k];
						sample_texture.SetPixel(k, texture_cur_y + j, new Color(vertex.x, vertex.y, vertex.z));
					}

					sample_time += per_frame_time;
				}

				texture_cur_y += cur_clip_frame;
			}

			sample_texture.Apply();

			// 输出贴图
			string texture_output = string.Format("{0}/{1}_gpu_skin.asset", folder, file_name);
			AssetDatabase.CreateAsset(sample_texture, texture_output);

			// 创建材质
			Material mat = new Material(Shader.Find("LYGame/GPUSkin"));
			mat.SetTexture("_MainTex", smr.sharedMaterial.mainTexture);
			mat.SetTexture("_AnimationMap", sample_texture);
			mat.SetFloat("_TotalTime", total_time);
			// 默认开启GPUInstancing
			mat.enableInstancing = true;
			string mat_output = string.Format("{0}/{1}_gpu_skin.mat", folder, file_name);
			AssetDatabase.CreateAsset(mat, mat_output);

			// 创建预设
			GameObject go_prefab = new GameObject();
			go_prefab.AddComponent<MeshFilter>().sharedMesh = smr.sharedMesh;
			go_prefab.AddComponent<MeshRenderer>().sharedMaterial = mat;
			GPUSkinController gpu_skin_controller = go_prefab.AddComponent<GPUSkinController>();
			gpu_skin_controller.sections = sections;
			string prefab_output = string.Format("{0}/{1}_gpu_skin.prefab", folder, file_name);
			PrefabUtility.SaveAsPrefabAsset(go_prefab, prefab_output);

			// 销毁临时创建的GameObject
			Object.DestroyImmediate(go);
			Object.DestroyImmediate(go_prefab);

			return ForeachSelectionResult.result_none;
		}
	}
}
