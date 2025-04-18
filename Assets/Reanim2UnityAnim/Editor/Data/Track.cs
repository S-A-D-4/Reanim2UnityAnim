using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	public class Track
	{
		public string Name { get; set; }
		public List<Frame> Transforms { get; set; }

		public Track()
		{
			Transforms = new List<Frame>();
			Name = string.Empty; // 初始化为空字符串
		}

		public override string ToString()
		{
			return $"Track(Name='{Name}', Transforms=[{Transforms.Count} items])";
		}
	}

	public class SpriteTrack : Track
	{
		public SpriteTrack(string spriteName, List<Frame> transforms)
		{
			Name = spriteName;
			Transforms = new List<Frame>();
			foreach (Frame transform in transforms)
			{
				Transforms.Add(transform.GetClone());
			}


			for (int index = 0; index < Transforms.Count; index++)
			{
				Frame transform = Transforms[index];
				if (transform.Image != null && transform.Image != Name)
				{
					transform.Image = null;
					transform.F = -1;
				}
			}
		}
	}
}