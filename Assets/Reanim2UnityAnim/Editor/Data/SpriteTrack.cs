﻿using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	/// <summary>
	/// 代表动画中的单个Sprite部件的各种信息
	/// </summary>
	public class SpriteTrack : Track
	{
		public string ParentPath => Parent?.Name;

		public string Path => ParentPath == null ? Name : $"{Parent.Name}/{Name}";

		public RootTrack Parent { get; set; }

		private string imageName;

		public string ImageName => imageName ?? Name;

		public int Order { get; set; }

		public SpriteTrack(string childName, List<Frame> transforms, int order, string imageName = null) : base(childName, transforms)
		{
			Order = order;
			if (imageName != null)
			{
				this.imageName = imageName;
			}
			for (int index = 0; index < Transforms.Count; index++)
			{
				Frame transform = Transforms[index];
				if (transform.Image != null && transform.Image != ImageName)
				{
					transform.F = -1;
				}
				if (transform.Image == ImageName)
				{
					transform.F = 0;
				}
			}
		}
	}
}