namespace BookmarkMergeTool.Models
{
	/// <summary>
	/// 书签信息
	/// </summary>
	class Bookmark : Component
	{
		/// <summary>
		/// 地址
		/// </summary>
		public string Href { get; set; }
		/// <summary>
		/// 图标
		/// </summary>
		public string Icon { get; set; }

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
	}
}
