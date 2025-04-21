using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	/// <summary>
	/// 代表动画中的单个父部件的各种信息
	/// </summary>
	public class RootTrack : Track
	{
		public float startX { get; set; }
		public float startY { get; set; }

		public RootTrack(string name, List<Frame> transforms) : base(name, transforms)
		{
			if (name == "_ground")
			{
				Name = "";
				foreach (Frame transform in Transforms)
				{
					transform.X = -transform.X;
				}
			}
		}
	}
}