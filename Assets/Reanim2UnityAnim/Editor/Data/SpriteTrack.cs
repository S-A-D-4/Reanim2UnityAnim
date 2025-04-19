using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	public class SpriteTrack : Track
	{
		public string ParentPath => Parent?.Name;

		public string Path => ParentPath == null ? Name : $"{Parent.Name}/{Name}";
		
		public RootTrack Parent { get; set; }

		public float ParentX => Parent?.startX ?? 0;
		
		public float ParentY => Parent?.startY ?? 0;

		public SpriteTrack(string spriteName, List<Frame> transforms) : base(spriteName, transforms)
		{
			for (int index = 0; index < Transforms.Count; index++)
			{
				Frame transform = Transforms[index];
				if (transform.Image != null && transform.Image != Name)
				{
					transform.F = -1;
				}
				if (transform.Image == spriteName)
				{
					transform.F = 0;
				}
			}
		}
	}
}