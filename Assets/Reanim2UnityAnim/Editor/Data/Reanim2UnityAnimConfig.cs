using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reanim2UnityAnim.Editor.Data
{
	[CreateAssetMenu(fileName = "Reanim2UnityAnimConfig", menuName = "Reanim2UnityAnimConfig", order = 1)]
	[Serializable]
	public class Reanim2UnityAnimConfig : ScriptableObject
	{
		public string filePath;
		public Vector2 center = new Vector2(0.4f, 0.7f);
		public List<Partition> customPartitions = new List<Partition>();
		public Mode mode = Mode.平滑模式;
	}
}