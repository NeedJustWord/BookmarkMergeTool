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

		/// <summary>
		/// 获取输出信息
		/// </summary>
		/// <param name="spaceNumber">前置空格数量</param>
		/// <returns></returns>
		public override IEnumerable<string> GetWriteInfo(int spaceNumber)
		{
			switch (LabelName)
			{
				case "H1":
					yield return $"<H1>{LabelText}</H1>".AddLeftSpace(spaceNumber);
					break;
				case "H3":
					if (PersonalToolbarFolder == null)
						yield return $"<DT><H3 ADD_DATE=\"{AddDate}\" LAST_MODIFIED=\"{LastModified}\">{LabelText}</H3>".AddLeftSpace(spaceNumber);
					else
						yield return $"<DT><H3 ADD_DATE=\"{AddDate}\" LAST_MODIFIED=\"{LastModified}\" PERSONAL_TOOLBAR_FOLDER=\"{PersonalToolbarFolder.ToString().ToLower()}\">{LabelText}</H3>".AddLeftSpace(spaceNumber);
					break;
			}

			yield return "<DL><p>".AddLeftSpace(spaceNumber);

			foreach (var component in ComponentList)
			{
				foreach (var item in component.GetWriteInfo(spaceNumber + 4))
				{
					yield return item;
				}
			}

			yield return "</DL><p>".AddLeftSpace(spaceNumber);
		}
	}

	/// <summary>
	/// 文件夹信息比较类
	/// </summary>
	class FolderEqualityComparer : IEqualityComparer<Folder>
	{
		public bool Equals(Folder x, Folder y)
		{
			return x.LabelName == y.LabelName && x.LabelText == y.LabelText;
		}

		public int GetHashCode(Folder obj)
		{
			return obj.LabelName.GetHashCode() ^ obj.LabelText.GetHashCode();
		}
	}
}
