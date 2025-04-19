using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	public class Track
	{
		public string Name { get; set; }
		public List<Frame> Transforms { get; set; }

		public Track(string name, List<Frame> transforms)
		{
			Name = name;
			Transforms = new List<Frame>();
			foreach (Frame transform in transforms)
			{
				Transforms.Add(transform.GetClone());
			}
		}

		public override string ToString()
		{
			return $"Track(Name='{Name}', Transforms=[{Transforms.Count} items])";
		}
	}
}