using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LYGame
{
	public class LYGameAssetPostprocessor : AssetPostprocessor
	{
		/// <summary>
		/// 模型导入预处理
		/// </summary>
		void OnPreprocessModel()
		{
			string project_relative_path = this.assetPath;
			string folder = BaseHelp.GetParentDir(project_relative_path);
			string clip_file_path = string.Format("{0}/MecanimConfig.asset", folder);

			// 如果目录下包含MecanimConfig配置，才进行预处理
			MecanimConfig config = AssetDatabase.LoadAssetAtPath(clip_file_path, typeof(MecanimConfig)) as MecanimConfig;
			if (!config)
				return;

			ModelImporter importer = (ModelImporter)this.assetImporter;
			// 默认不导入材质
			importer.importMaterials = false;
			importer.importBlendShapes = false;
			importer.isReadable = false;
			importer.optimizeMeshPolygons = true;
			importer.optimizeMeshVertices = true;
			importer.weldVertices = true;
			importer.swapUVChannels = false;
			importer.generateSecondaryUV = false;
			// 设置旋转校正为0，避免一定程度的抖动
			importer.animationRotationError = 0;

			// 覆盖默认配置
			importer.isReadable = config.enable_read_write;

			List<ModelImporterClipAnimation> reset_list = new List<ModelImporterClipAnimation>();
			int clip_count = config.clips.Count;
			for (int i = 0; i < clip_count; ++i)
			{
				MecanimClipConfig clip_config = config.clips[i];
				ModelImporterClipAnimation reset = new ModelImporterClipAnimation();
				reset.name = clip_config.name;
				reset.firstFrame = clip_config.start_frame;
				reset.lastFrame = clip_config.end_frame;
				reset.loopTime = clip_config.loop;
				reset.wrapMode = clip_config.loop ? WrapMode.Loop : WrapMode.Default;

				// 事件
				List<AnimationEvent> evts = new List<AnimationEvent>();
				int evt_count = clip_config.evts.Count;
				for (int j = 0; j <  evt_count; ++j)
				{
					MecanimClipEvent mecanim_evt = clip_config.evts[j];
					AnimationEvent evt = new AnimationEvent();
					evt.time = mecanim_evt.time;
					// 枚举即是函数名称
					evt.functionName = mecanim_evt.evt_type.ToString();
					evt.stringParameter = mecanim_evt.str_param;
					evt.intParameter = mecanim_evt.int_param;
					evts.Add(evt);
				}
				reset.events = evts.ToArray();

				reset_list.Add(reset);
			}

			// 重新赋值
			importer.clipAnimations = reset_list.ToArray();
		}
	}
}
