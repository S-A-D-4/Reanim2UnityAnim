using Reanim2UnityAnim.Editor.Data;
using UnityEditor;
using UnityEngine;

namespace Reanim2UnityAnim.Editor
{
	public class Reanim2UnityAnimToolbar : EditorWindow
	{
		private Reanim2UnityAnimConfig data;

		[MenuItem("Window/Reanim2UnityAnim")]
		public static void ShowWindow()
		{
			GetWindow<Reanim2UnityAnimToolbar>("Reanim2UnityAnimToolbar");
		}

		private void OnEnable()
		{
			data = AssetDatabase.LoadAssetAtPath<Reanim2UnityAnimConfig>(
				"Assets/Reanim2UnityAnim/Editor/Reanim2UnityAnimConfig.asset");
			if (data == null)
			{
				data = CreateInstance<Reanim2UnityAnimConfig>();
				AssetDatabase.CreateAsset(data, "Assets/Reanim2UnityAnim/Editor/Reanim2UnityAnimConfig.asset");
				SaveData();
			}
		}


		private void SaveData()
		{
			EditorUtility.SetDirty(data);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private void OnGUI()
		{
			data = (Reanim2UnityAnimConfig)EditorGUILayout.ObjectField(data, typeof(Reanim2UnityAnimConfig), false);

			GUILayout.Label($"当前选择的文件: \n{data.filePath}");

			// 文件夹选择按钮
			if (GUILayout.Button("选择文件"))
			{
				string path = EditorUtility.OpenFilePanelWithFilters("选择.reanim文件", Application.dataPath + "/Reanim2UnityAnim/reanim_all/", new[]
				{ "reanim", "Reanim" });
				if (!string.IsNullOrEmpty(path) && path.EndsWith(".reanim"))
				{
					data.filePath = path;
				}
				else
				{
					Debug.Log("未选择.reanim文件");
				}
			}

			// 增加输入栏按钮
			if (GUILayout.Button("添加Root"))
			{
				data.root2Childs.Add(new Root2Childs());
			}

			// 显示父子
			for (int i = data.root2Childs.Count - 1; i >= 0; i--)
			{
				GUILayout.BeginHorizontal();
				data.root2Childs[i].root = EditorGUILayout.TextField($"父项 {(i + 1)}", data.root2Childs[i].root);

				// 添加子项按钮
				if (GUILayout.Button("添加子项"))
				{
					data.root2Childs[i].childs.Add("");
				}

				// 删除输入栏按钮
				if (GUILayout.Button("关闭"))
				{
					data.root2Childs.RemoveAt(i);
					GUILayout.EndHorizontal();
					continue;
				}
				GUILayout.EndHorizontal();

				// 显示关联的最终输入栏
				for (int j = data.root2Childs[i].childs.Count - 1; j >= 0; j--)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Space(20); // 缩进子项
					data.root2Childs[i].childs[j]
						= EditorGUILayout.TextField($"子项 {(i + 1)}.{(j + 1)}", data.root2Childs[i].childs[j]);

					// 删除子项按钮
					if (GUILayout.Button("关闭"))
					{
						data.root2Childs[i].childs.RemoveAt(j);
						GUILayout.EndHorizontal();
						continue;
					}
					GUILayout.EndHorizontal();
				}
			}

			if (GUILayout.Button("添加链接"))
			{
				data.links.Add(new Link());
			}

			// 显示链接
			for (int i = data.links.Count - 1; i >= 0; i--)
			{
				GUILayout.BeginHorizontal();
				data.links[i].parent = EditorGUILayout.TextField($"目标节点 {(i + 1)}", data.links[i].parent);
				data.links[i].child = EditorGUILayout.TextField($"附属节点 {(i + 1)}", data.links[i].child);
				data.links[i].correct_x = EditorGUILayout.FloatField("修正x", data.links[i].correct_x);
				data.links[i].correct_y = EditorGUILayout.FloatField("修正y", data.links[i].correct_y);
				// 删除链接按钮
				if (GUILayout.Button("关闭"))
				{
					data.links.RemoveAt(i);
					GUILayout.EndHorizontal();
					continue;
				}
				GUILayout.EndHorizontal();
			}

			// 自定义轨道

			if (GUILayout.Button("添加自定义分割"))
			{
				data.customPartitions.Add(new Partition("UnNamedPartition", 0, 0));
			}

			for (int i = 0; i < data.customPartitions.Count; i++)
			{
				GUILayout.BeginHorizontal();
				data.customPartitions[i].name = EditorGUILayout.TextField($"分割 {(i + 1)}", data.customPartitions[i].name);
				data.customPartitions[i].startIndexInclude
					= EditorGUILayout.IntField("开始帧（包含）", data.customPartitions[i].startIndexInclude);
				data.customPartitions[i].endIndexExclude
					= EditorGUILayout.IntField("结束帧（不包含）", data.customPartitions[i].endIndexExclude);
				// 删除轨道按钮
				if (GUILayout.Button("关闭"))
				{
					data.customPartitions.RemoveAt(i);
					GUILayout.EndHorizontal();
					continue;
				}
				GUILayout.EndHorizontal();
			}

			// 操作按钮
			if (GUILayout.Button("生成"))
			{
				UnitBuilder.Create(data);
			}

			if (GUILayout.Button("保存配置"))
			{
				SaveData();
			}
		}
	}
}