using System;
using System.Collections.Generic;

namespace BookmarkMergeTool.Models
{
	/// <summary>
	/// 书签信息
	/// </summary>
	class Bookmark : Component, IEquatable<Bookmark>
	{
		/// <summary>
		/// 地址
		/// </summary>
		public string Href { get; private set; }
		/// <summary>
		/// 图标
		/// </summary>
		public string Icon { get; private set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="labelName">标签名</param>
		/// <param name="labelText">标签文本</param>
		/// <param name="href">地址</param>
		/// <param name="addDate">添加时间戳</param>
		/// <param name="icon">图标</param>
		public Bookmark(string labelName, string labelText, string href, int addDate, string icon) : base(labelName, labelText, addDate)
		{
			Href = href;
			Icon = icon;
		}

		/// <summary>
		/// 空实现
		/// </summary>
		/// <param name="component"></param>
		public override void Add(Component component)
		{
		}

		/// <summary>
		/// 获取输出信息
		/// </summary>
		/// <param name="spaceNumber">前置空格数量</param>
		/// <returns></returns>
		public override IEnumerable<string> GetWriteInfo(int spaceNumber)
		{
			if (string.IsNullOrEmpty(Icon))
				yield return $"<DT><A HREF=\"{Href}\" ADD_DATE=\"{AddDate}\">{LabelText}</A>".AddLeftSpace(spaceNumber);
			else
				yield return $"<DT><A HREF=\"{Href}\" ADD_DATE=\"{AddDate}\" ICON=\"{Icon}\">{LabelText}</A>".AddLeftSpace(spaceNumber);
		}

		public bool Equals(Bookmark other)
		{
			return LabelName == other.LabelName && Href == other.Href;
		}

		public override int GetHashCode()
		{
			return LabelName.GetHashCode() ^ Href.GetHashCode();
		}
	}

	/// <summary>
	/// 书签信息比较类
	/// </summary>
	class BookmarkEqualityComparer : IEqualityComparer<Bookmark>
	{
		public bool Equals(Bookmark x, Bookmark y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(Bookmark obj)
		{
			return obj.GetHashCode();
		}
	}
}
