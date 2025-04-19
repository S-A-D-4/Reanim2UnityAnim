using System;
using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	/// <summary>
	/// 代表已添加的部件间的父子关系
	/// </summary>
	[Serializable]
	public class Root2Childs
	{
		public string root = "";

		public List<string> childs = new List<string>();
	}
}