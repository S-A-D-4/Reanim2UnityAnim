using System;
using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	/// <summary>
	/// 代表完整动画的一个分段，含AnimationClip的基本信息
	/// </summary>
	[Serializable]
	public class Partition
	{
		public string name;
		public int    startIndexInclude;
		public int    endIndexExclude;

		public Partition(string name, int startIndexInclude, int endIndexExclude)
		{
			this.name              = name;
			this.startIndexInclude = startIndexInclude;
			this.endIndexExclude   = endIndexExclude;
		}

		public Partition(Track track)
		{
			List<Frame> frames     = track.Transforms;
			int         lastF      = 0;
			int?        startFrame = null;
			int?        endFrame   = null;
			for (int i = 0; i < frames.Count; i++)
			{
				int currentF = frames[i].F == null ? lastF : frames[i].F.Value;
				if (startFrame == null && currentF == 0)
				{
					startFrame = i;
				}
				if (startFrame != null && currentF == -1)
				{
					endFrame = i;
					break;
				}
				lastF = currentF;
			}
			endFrame ??= frames.Count;
			
			name = track.Name;
			if (startFrame == null)
			{
				startIndexInclude = 0;
				endIndexExclude   = 0;
			}
			else
			{
				startIndexInclude = startFrame.Value;
				endIndexExclude   = endFrame.Value;
			}
		}
	}
}