using System.Collections.Generic;

namespace BookmarkMergeTool.Models
{
	/// <summary>
	/// 文件夹信息
	/// </summary>
	class Folder : Component
	{
		/// <summary>
		/// 修改时间戳
		/// </summary>
		public int LastModified { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		public bool? PersonalToolbarFolder { get; private set; }
		/// <summary>
		/// 子项集合
		/// </summary>
		public List<Component> ComponentList { get; private set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="labelName">标签名</param>
		/// <param name="labelText">标签文本</param>
		/// <param name="addDate">添加时间戳</param>
		/// <param name="lastModified">修改时间戳</param>
		public Folder(string labelName, string labelText, int addDate, int lastModified, bool? personalToolbarFolder = null) : base(labelName, labelText, addDate)
		{
			LastModified = lastModified;
			PersonalToolbarFolder = personalToolbarFolder;
			ComponentList = new List<Component>();
		}

		/// <summary>
		/// 添加子项
		/// </summary>
		/// <param name="component"></param>
		public override void Add(Component component)
		{
			ComponentList.Add(component);
		}
	}
}
