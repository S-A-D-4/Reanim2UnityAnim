using System;

namespace Reanim2UnityAnim.Editor.Data
{
	[Serializable]
	public class Link
	{
		public string parent;
		public string child;
		public float  correct_x;
		public float  correct_y;
		public Link() { }

		public Link(string parent, string child)
		{
			this.parent = parent;
			this.child  = child;
		}
	}
}