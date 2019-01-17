using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BookmarkMergeTool.Models;

namespace BookmarkMergeTool
{
	/// <summary>
	/// 书签文件读取类
	/// </summary>
	static class BookmarkReader
	{
		/// <summary>
		/// 读取谷歌书签文件
		/// </summary>
		/// <param name="filePath">文件路径</param>
		/// <returns></returns>
		public static Root ReadFile(string filePath)
		{
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
				{
					Stack<Folder> stack = new Stack<Folder>();
					Root result = new Root();
					Folder current = null;
					int order = 0;
					string line;

					while ((line = streamReader.ReadLine()) != null)
					{
						line = line.TrimStart();

						if (line.StartsWith("<TITLE>"))
						{
							result.Title = GetLabelText(line);
							continue;
						}

						if (line.StartsWith("<H1>"))
						{
							current = CreateFolder("H1", line);
							current.Order = order;
							order = 0;
							continue;
						}

						if (line.StartsWith("<DT><H3"))
						{
							stack.Push(current);
							current = CreateFolder("H3", line);
							current.Order = order;
							order = 0;
							continue;
						}

						if (line.StartsWith("<DT><A"))
						{
							var bookmark = CreateBookmark(line);
							bookmark.Order = order;
							order++;
							current.Add(bookmark);
							continue;
						}

						if (line.StartsWith("</DL><p>") && stack.Count > 0)
						{
							order = current.Order + 1;
							var temp = stack.Pop();
							temp.Add(current);
							current = temp;
							continue;
						}
					}

					result.Folder = current;
					return result;
				}
			}
		}

		/// <summary>
		/// 生成文件夹信息
		/// </summary>
		/// <param name="labelName"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		private static Folder CreateFolder(string labelName, string line)
		{
			string labelText = "";
			int addDate = 0;
			int lastModified = 0;
			bool? personalToolbarFolder = null;

			switch (labelName)
			{
				case "H1":
					{
						labelText = GetLabelText(line);
					}
					break;
				case "H3":
					{
						labelText = GetLabelText(line);
						addDate = GetAddDate(line);
						lastModified = GetLastModified(line);
						personalToolbarFolder = GetPersonalToolbarFolder(line);
					}
					break;
				default:
					throw new NotSupportedException($"不匹配的{labelName}");
			}

			return new Folder(labelName, labelText, addDate, lastModified, personalToolbarFolder);
		}

		/// <summary>
		/// 生成书签信息
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static Bookmark CreateBookmark(string line)
		{
			string labelText = GetLabelText(line);
			string href = GetHref(line);
			int addDate = GetAddDate(line);
			string icon = GetIcon(line);

			return new Bookmark("A", labelText, href, addDate, icon);
		}

		/// <summary>
		/// 获取标签文本
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static string GetLabelText(string line)
		{
			return GetMatchText(line, ">(.*?)<", RegexOptions.RightToLeft);
		}

		/// <summary>
		/// 获取ADD_DATE属性
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static int GetAddDate(string line)
		{
			return int.Parse(GetMatchText(line, "ADD_DATE=\"(.*?)\"", defaultValue: "0"));
		}

		/// <summary>
		/// 获取LAST_MODIFIED属性
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static int GetLastModified(string line)
		{
			return int.Parse(GetMatchText(line, "LAST_MODIFIED=\"(.*?)\"", defaultValue: "0"));
		}

		/// <summary>
		/// 获取PERSONAL_TOOLBAR_FOLDER属性
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static bool? GetPersonalToolbarFolder(string line)
		{
			var text = GetMatchText(line, "PERSONAL_TOOLBAR_FOLDER=\"(.*?)\"");
			return (text == "") ? (bool?)null : bool.Parse(text);
		}

		/// <summary>
		/// 获取HREF属性
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static string GetHref(string line)
		{
			return GetMatchText(line, "HREF=\"(.*?)\"");
		}

		/// <summary>
		/// 获取ICON属性
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private static string GetIcon(string line)
		{
			return GetMatchText(line, "ICON=\"(.*?)\"");
		}

		/// <summary>
		/// 获取匹配指定正则表达式的文本，不匹配则返回<paramref name="defaultValue"/>
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <param name="regexOptions"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private static string GetMatchText(string input, string pattern, RegexOptions regexOptions = RegexOptions.None, string defaultValue = "")
		{
			var match = Regex.Match(input, pattern, regexOptions);
			return (match.Success && match.Groups.Count > 1) ? match.Groups[1].Value : defaultValue;
		}
	}
}
