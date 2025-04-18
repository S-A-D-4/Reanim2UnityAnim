using System.IO;
using UnityEngine;

namespace Reanim2UnityAnim.Editor
{
	public static class FileUtil
	{
		public static void CreateDirectoryAtRelativePath(string relativePath)
		{
			string absolutePath = Path.Combine(Application.dataPath, relativePath);

			if (!Directory.Exists(absolutePath))
			{
				Directory.CreateDirectory(absolutePath);
				Debug.Log($"已创建文件夹: {absolutePath}");
			}
		}
	}
}