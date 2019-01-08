namespace BookmarkMergeTool.Models
{
	/// <summary>
	/// 抽象类
	/// </summary>
	abstract class Component
	{
		/// <summary>
		/// 标签名
		/// </summary>
		public string LabelName { get; protected set; }
		/// <summary>
		/// 标签文本
		/// </summary>
		public string LabelText { get; protected set; }
		/// <summary>
		/// 添加时间戳
		/// </summary>
		public int AddDate { get; protected set; }
		/// <summary>
		/// 操作类型
		/// </summary>
		public Operation Operation { get; set; } = Operation.None;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="labelName">标签名</param>
		/// <param name="labelText">标签文本</param>
		/// <param name="addDate">添加时间戳</param>
		public Component(string labelName, string labelText, int addDate)
		{
			LabelName = labelName;
			LabelText = labelText;
			AddDate = addDate;
		}

		/// <summary>
		/// 添加子项
		/// </summary>
		/// <param name="component"></param>
		public abstract void Add(Component component);
	}
}
