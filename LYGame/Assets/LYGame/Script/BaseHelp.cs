using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace LYGame
{
	public class BaseHelp
	{
		/// <summary>
		/// 获取工程目录绝对路径
		/// </summary>
		/// <returns></returns>
		public static string GetProjectAbsolutePath()
		{
			string project_root = BaseHelp.GetParentDir(Application.dataPath);
			project_root = project_root.Replace("\\", "/");
			return project_root;
		}

		/// <summary>
		/// 资源相对工程目录的路径
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <returns></returns>
		public static string GetProjectRelativePath(string absolute_path)
		{
			string project_root = BaseHelp.GetParentDir(Application.dataPath);
			absolute_path = absolute_path.Substring(project_root.Length + 1);
			absolute_path = absolute_path.Replace("\\", "/");
			return absolute_path;
		}

		/// <summary>
		/// 获取相对于Resources目录的路径
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <returns></returns>
		public static string GetResourcesRelativePath(string absolute_path)
		{
			string resource_root = string.Format("{0}/Resources", Application.dataPath);
			absolute_path = absolute_path.Substring(resource_root.Length + 1);
			absolute_path = absolute_path.Replace("\\", "/");
			return absolute_path;
		}

		/// <summary>
		/// 获取父级目录
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public static string GetParentDir(string absolute_path, int loop = 1)
		{
			while (loop > 0)
			{
				absolute_path = absolute_path.Substring(0, absolute_path.LastIndexOf('/'));
				--loop;
			}
			return absolute_path;
		}

		/// <summary>
		/// 获取子目录
		/// </summary>
		/// <param name="path"></param>
		/// <param name="loop"></param>
		/// <returns></returns>
		public static string GetSubDir(string path, int loop = 1)
		{
			while (loop > 0)
			{
				path = path.Substring(path.IndexOf('/') + 1);
				--loop;
			}
			return path;
		}

		/// <summary>
		/// 清空目录
		/// </summary>
		/// <param name="absolute_path"></param>
		public static void ClearDir(string absolute_path)
		{
			if (Directory.Exists(absolute_path))
				Directory.Delete(absolute_path, true);
			Directory.CreateDirectory(absolute_path);
		}

		/// <summary>
		/// 获取文件的MD5
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <returns></returns>
		public static string GetFileMD5(string absolute_path)
		{
			FileStream fs = new FileStream(absolute_path, FileMode.Open);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] cur_md5_bytes = md5.ComputeHash(fs);
			string cur_md5 = System.BitConverter.ToString(cur_md5_bytes);
			fs.Close();
			return cur_md5.Replace("-", "").ToLower();
		}

		/// <summary>
		/// 获取字节流的MD5
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string GetBytesMD5(byte[] bytes)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] cur_md5_bytes = md5.ComputeHash(bytes);
			string cur_md5 = System.BitConverter.ToString(cur_md5_bytes);
			return cur_md5.Replace("-", "").ToLower();
		}

		/// <summary>
		/// 清理构建AB生成的manifest汇总信息
		/// </summary>
		/// <param name="root_absolute_path"></param>
		public static void ClearBuildManifest(string root_absolute_path)
		{
			string name = root_absolute_path.Substring(root_absolute_path.LastIndexOf('/') + 1);
			string src = string.Format("{0}/{1}", root_absolute_path, name);
			File.Delete(src);
			File.Delete(string.Format("{0}.manifest", src));

			BaseHelp.ScanPath(
				root_absolute_path,
				delegate (string file_path)
				{
					if (!file_path.EndsWith(".manifest"))
						return ScanPathResult.none;

					File.Delete(file_path);
					return ScanPathResult.none;
				},
				null
			);
		}

		/// <summary>
		/// 扫描路径的返回值枚举
		/// </summary>
		public enum ScanPathResult
		{
			none,
			file_return,
			dir_continue,
			dir_return,
		}

		/// <summary>
		/// 扫描路径委托
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <returns></returns>
		public delegate ScanPathResult ScanPathHandler(string absolute_path);

		/// <summary>
		/// 扫描路径
		/// </summary>
		/// <param name="absolute_path"></param>
		/// <param name="file_handler"></param>
		/// <param name="dir_handler"></param>
		public static void ScanPath(string absolute_path, ScanPathHandler file_handler, ScanPathHandler dir_handler)
		{
			string[] files = Directory.GetFiles(absolute_path);
			int count = files.Length;
			for (int i = 0; i < count; ++i)
			{
				string one = files[i].Replace("\\", "/");
				ScanPathResult result = file_handler.Invoke(one);
				// 通常用于
				if (result == ScanPathResult.file_return)
					return;
			}

			string[] sub_dirs = Directory.GetDirectories(absolute_path);
			count = sub_dirs.Length;
			for (int i = 0; i < count; ++i)
			{
				string one = sub_dirs[i].Replace("\\", "/");
				ScanPathResult result = dir_handler.Invoke(one);
				if (result == ScanPathResult.dir_continue)
					continue;
				else if (result == ScanPathResult.dir_return)
					return;
				BaseHelp.ScanPath(one, file_handler, dir_handler);
			}
		}

		/// <summary>
		/// 递归复制目录和文件
		/// </summary>
		/// <param name="src_root">源目录</param>
		/// <param name="des_root">目标目录</param>
		public static void CopyDirectory(string src_root, string des_root)
		{
			// 清空目录
			BaseHelp.ClearDir(des_root);

			// 递归复制
			BaseHelp.ScanPath(
				src_root,
				delegate (string absolute_path)
				{
					string des_file = absolute_path.Replace(src_root, des_root);
					File.Copy(absolute_path, des_file, true);
					return BaseHelp.ScanPathResult.none;
				},
				delegate (string absolute_path)
				{
					string des_dir = absolute_path.Replace(src_root, des_root);
					if (!Directory.Exists(des_dir))
						Directory.CreateDirectory(des_dir);
					return BaseHelp.ScanPathResult.none;
				}
			);
		}

		public static string GetBuildTargetString()
		{
#if UNITY_STANDALONE_WIN
			return "StandaloneWindows64";
#elif UNITY_ANDROID
			return "Android";
#elif UNITY_IOS
			return "iOS";
#endif
		}

		public static Component EnsureHasComponent<T>(GameObject go)
		{
			Component com = go.GetComponent(typeof(T));
			if (com == null)
				com = go.AddComponent(typeof(T));
			return com;
		}

		public static void ExportPNG(Texture2D texture, string absolute_path)
		{
			byte[] bytes = texture.EncodeToPNG();
			using (FileStream fs = new FileStream(absolute_path, FileMode.Create))
			{
				using (BinaryWriter writer = new BinaryWriter(fs))
				{
					writer.Write(bytes);
				}
			}
		}
	}
}