using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Reanim2UnityAnim.Editor.Data;
using UnityEngine;

namespace Reanim2UnityAnim.Editor
{
	/// <summary>
	/// 用于将reanim文件解析为List<Track>
	/// </summary>
	public class TrackParser
	{
		// 辅助方法，用于安全地将字符串解析为 nullable float
		public static List<Track> ParseTracksFromFile(string filePath)
		{
			try
			{
				// 从文件加载 XML
				// 假设文件包含一个根元素包裹所有的 <track> 元素
				// 如果文件直接以 <track> 开始，没有单一根元素，XML 会无效
				// 你可能需要先读取文件内容，然后手动添加一个根元素
				string xmlContent = File.ReadAllText(filePath);

				// 检查文件是否包含有效的根元素。如果没有，添加一个临时的根。
				// 这是一个常见的处理方式，用于处理片段化的XML文件。
				if (!xmlContent.TrimStart().StartsWith("<")) throw new FormatException("File does not appear to be XML.");
				// 尝试解析，如果失败（例如，多个根元素），则包装它
				XDocument doc;
				try
				{
					doc = XDocument.Parse(xmlContent);
				}
				catch (XmlException ex) when (ex.Message.Contains("multiple root elements"))
				{
					Debug.Log("Warning: XML file has multiple root elements. Wrapping in <root>.");
					string wrappedXml = $"<root>{xmlContent}</root>";
					doc = XDocument.Parse(wrappedXml);
				}


				// 选择所有的 <track> 元素
				// 如果我们包装了XML，需要从包装的 <root> 开始查找
				IEnumerable<XElement> trackElements = doc.Root?.Elements("track") ?? Enumerable.Empty<XElement>();
				// 如果没有包装，并且 <track> 是根元素（虽然不规范），可以尝试 doc.Elements("track")
				// 但更健壮的方式是处理包装的情况
				if (!trackElements.Any() && doc.Root?.Name == "track") // 处理单个 track 作为根的情况
					trackElements = new[]
					{ doc.Root };
				else if (!trackElements.Any() && doc.Root?.Name != "root") // 如果根不是我们添加的root也不是track，尝试直接找
					trackElements = doc.Elements("track");


				List<Track> tracks = trackElements.Select(trackElement =>
				{
					// 获取 <name> 元素的值，如果不存在则为 null 或空
					string name = trackElement.Element("name")?.Value ?? string.Empty;

					// 选择当前 <track> 下的所有 <t> 元素
					List<Frame> transforms = trackElement.Elements("t").Select(tElement => new Frame
					{ // 使用辅助方法安全地解析每个可选元素
					  X = TryParseFloat(tElement.Element("x")?.Value),
					  Y = TryParseFloat(tElement.Element("y")?.Value),
					  Kx = TryParseFloat(tElement.Element("kx")?.Value),
					  Ky = TryParseFloat(tElement.Element("ky")?.Value),
					  Sx = TryParseFloat(tElement.Element("sx")?.Value),
					  Sy = TryParseFloat(tElement.Element("sy")?.Value),
					  Image = tElement.Element("i")?.Value.Substring(13), // 字符串可以直接赋值，null if missing
					  F = TryParseInt(tElement.Element("f")?.Value),
					  A = TryParseFloat(tElement.Element("a")?.Value), }).ToList(); // 将结果转换为 List<Frame>

					return new Track(name, transforms);
				}).ToList(); // 将所有解析的 Track 对象收集到 List<Track>
				
				// 检查名字重复
				// 1. 按 name 分组
				var groupedItems = tracks.GroupBy(item => item.Name);

				// 2. 遍历分组
				foreach (var group in groupedItems)
				{
					// 3. 筛选出包含重复项的组 (元素个数 >= 2)
					//    使用 ToList() 可以避免多次迭代 group 来获取 Count 和进行遍历
					var itemList = group.ToList();
					if (itemList.Count >= 2)
					{
						// 获取原始名称 (分组的 Key 就是原始名称)
						string originalName = group.Key;
						int counter = 1; // 初始化计数器

						// 4. 重命名组内的每个 Item
						foreach (var itemInGroup in itemList)
						{
							// 直接修改原始列表中对象的 name 属性
							itemInGroup.Name = $"{originalName}_part{counter}";
							counter++; // 递增计数器
						}
					}
					// 如果组内只有一个元素 (Count < 2)，则名称是唯一的，无需处理
				}

				return tracks;
			}
			catch (FileNotFoundException)
			{
				Debug.Log($"Error: File not found at '{filePath}'");
				return new List<Track>(); // 返回空列表
			}
			catch (XmlException ex)
			{
				Debug.Log($"Error: Failed to parse XML file. Details: {ex.Message}");
				return new List<Track>(); // 返回空列表
			}
			catch (Exception ex) // 捕获其他潜在错误
			{
				Debug.Log($"An unexpected error occurred: {ex.Message}");
				return new List<Track>(); // 返回空列表
			}
		}

		private static float? TryParseFloat(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;
			// 使用 InvariantCulture 确保小数点是 '.'
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)) return result;
			// 可选：添加日志记录或错误处理来处理无法解析的值
			Debug.Log($"Warning: Could not parse float value: '{value}'");
			return null;
		}

		// 辅助方法，用于安全地将字符串解析为 nullable int
		private static int? TryParseInt(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;
			if (int.TryParse(value, out int result)) return result;
			// 可选：添加日志记录或错误处理
			Debug.Log($"Warning: Could not parse int value: '{value}'");
			return null;
		}
	}
}