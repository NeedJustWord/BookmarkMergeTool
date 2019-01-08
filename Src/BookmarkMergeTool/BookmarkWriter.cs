using System.IO;
using System.Text;
using BookmarkMergeTool.Models;

namespace BookmarkMergeTool
{
	/// <summary>
	/// 书签文件写入类
	/// </summary>
	static class BookmarkWriter
	{
		/// <summary>
		/// 写入谷歌书签文件
		/// </summary>
		/// <param name="root">书签文件信息</param>
		/// <param name="filePath">文件路径</param>
		public static void WriteFile(Root root, string filePath)
		{
			using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false)))
				{
					streamWriter.WriteHeader();
					streamWriter.WriteTitle(root.Title);

					foreach (var item in root.Folder.GetWriteInfo(0))
					{
						streamWriter.WriteLine(item);
					}
				}
			}
		}

		/// <summary>
		/// 写文件头
		/// </summary>
		/// <param name="streamWriter"></param>
		private static void WriteHeader(this StreamWriter streamWriter)
		{
			string[] headers = new string[]
			{
				"<!DOCTYPE NETSCAPE-Bookmark-file-1>",
				"<!-- This is an automatically generated file.",
				"     It will be read and overwritten.",
				"     DO NOT EDIT! -->",
				"<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=UTF-8\">",
			};

			foreach (var header in headers)
			{
				streamWriter.WriteLine(header);
			}
		}

		/// <summary>
		/// 写Title
		/// </summary>
		/// <param name="streamWriter"></param>
		/// <param name="title"></param>
		private static void WriteTitle(this StreamWriter streamWriter, string title)
		{
			streamWriter.WriteLine($"<TITLE>{title}</TITLE>");
		}
	}
}
