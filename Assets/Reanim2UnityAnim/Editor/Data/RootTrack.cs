using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	public class RootTrack : Track
	{
		public float startX { get; set; }
		public float startY { get; set; }

		public RootTrack(string name, List<Frame> transforms) : base(name, transforms) { }
	}
}