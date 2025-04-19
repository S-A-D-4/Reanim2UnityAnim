#nullable enable
using System.Collections.Generic;

namespace Reanim2UnityAnim.Editor.Data
{
	/// <summary>
	/// 代表单个部件在某一帧内的状态
	/// </summary>
	public class Frame
	{
		public float?  X     { get; set; }
		public float?  Y     { get; set; }
		public float?  Kx    { get; set; } // Skew X
		public float?  Ky    { get; set; } // Skew Y
		public float?  Sx    { get; set; } // Scale X
		public float?  Sy    { get; set; } // Scale Y
		public string? Image { get; set; } // Image identifier
		public int?    F     { get; set; } // Figurable
		public float?  A     { get; set; } // Alpha

		public Frame GetClone()
		{
			return new Frame()
			{
			X = X,
			Y = Y,
			Kx = Kx,
			Ky = Ky,
			Sx = Sx,
			Sy = Sy,
			Image = Image,
			F     = F,
			A     = A,
			};
		}

		public override string ToString()
		{
			// 提供一个简单的字符串表示形式，方便调试
			List<string> parts = new List<string>();
			if (X.HasValue) parts.Add($"X={X.Value}");
			if (Y.HasValue) parts.Add($"Y={Y.Value}");
			if (Kx.HasValue) parts.Add($"Kx={Kx.Value}");
			if (Ky.HasValue) parts.Add($"Ky={Ky.Value}");
			if (Sx.HasValue) parts.Add($"Sx={Sx.Value}");
			if (Sy.HasValue) parts.Add($"Sy={Sy.Value}");
			if (!string.IsNullOrEmpty(Image)) parts.Add($"Image='{Image}'");
			if (F.HasValue) parts.Add($"F={F.Value}");
			if (A.HasValue) parts.Add($"A={A.Value}");
			return $"T({string.Join(", ", parts)})";
		}
	}
}

// 代表 <track> 元素