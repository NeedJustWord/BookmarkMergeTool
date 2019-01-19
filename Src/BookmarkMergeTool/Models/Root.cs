namespace BookmarkMergeTool.Models
{
	/// <summary>
	/// 书签文件信息
	/// </summary>
	class Root
	{
		/// <summary>
		/// 标题
		/// </summary>
		public string Title { get; set; }
		/// <summary>
		/// 书签列表
		/// </summary>
		public Folder Folder { get; set; }
	}
}
