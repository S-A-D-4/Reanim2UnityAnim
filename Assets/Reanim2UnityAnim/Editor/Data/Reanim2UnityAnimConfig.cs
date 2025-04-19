using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reanim2UnityAnim.Editor.Data
{
	[CreateAssetMenu(fileName = "Reanim2UnityAnimConfig", menuName = "Reanim2UnityAnimConfig", order = 1)]
	[Serializable]
	public class Reanim2UnityAnimConfig : ScriptableObject
	{
		public string            filePath;
		public List<Root2Childs> root2Childs      = new List<Root2Childs>();
		public List<Partition>   customPartitions = new List<Partition>();
	}
}