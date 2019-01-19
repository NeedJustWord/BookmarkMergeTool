using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using BookmarkMergeTool.Models;

namespace BookmarkMergeTool
{
	class Program
	{
		static FolderEqualityComparer folderEquality = new FolderEqualityComparer();
		static BookmarkEqualityComparer bookmarkEquality = new BookmarkEqualityComparer();

		static void Main(string[] args)
		{
			var basedFilePath = ConfigurationManager.AppSettings["basedFilePath"];
			var homeFilePath = ConfigurationManager.AppSettings["homeFilePath"];
			var companyFilePath = ConfigurationManager.AppSettings["companyFilePath"];
			var mergeFilePath = ConfigurationManager.AppSettings["mergeFilePath"];

			var based = BookmarkReader.ReadFile(basedFilePath);
			var home = BookmarkReader.ReadFile(homeFilePath);
			var company = BookmarkReader.ReadFile(companyFilePath);

			Merge(based.Folder, home.Folder, company.Folder);
			BookmarkWriter.WriteFile(based, mergeFilePath);

			Console.WriteLine("合并完成，按任意键退出!");
			Console.ReadKey();
		}

		/// <summary>
		/// 将<paramref name="home"/>和<paramref name="company"/>合并到<paramref name="based"/>
		/// </summary>
		/// <param name="based"></param>
		/// <param name="home"></param>
		/// <param name="company"></param>
		static void Merge(Folder based, Folder home, Folder company)
		{
			//查找目录
			var basedFolder = based.ComponentList.OfType<Folder>();
			var homeFolder = home.ComponentList.OfType<Folder>();
			var companyFolder = company.ComponentList.OfType<Folder>();

			//查找书签
			var basedBookmark = based.ComponentList.OfType<Bookmark>();
			var homeBookmark = home.ComponentList.OfType<Bookmark>();
			var companyBookmark = company.ComponentList.OfType<Bookmark>();

			//标记删除的目录和书签
			basedFolder.Except(homeFolder, folderEquality).ForEach(t => t.Operation = Operation.Delete);
			basedFolder.Except(companyFolder, folderEquality).ForEach(t => t.Operation = Operation.Delete);
			basedBookmark.Except(homeBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Delete);
			basedBookmark.Except(companyBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Delete);

			//查找添加的目录和书签
			var addHomeFolder = homeFolder.Except(basedFolder, folderEquality).ForEach(t => t.Operation = Operation.Add);
			var addCompanyFolder = companyFolder.Except(basedFolder, folderEquality).ForEach(t => t.Operation = Operation.Add);
			var addHomeBookmark = homeBookmark.Except(basedBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Add);
			var addCompanyBookmark = companyBookmark.Except(basedBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Add);

			//合并未删除的目录
			foreach (var basedItem in basedFolder.Where(t => t.Operation == Operation.None))
			{
				var homeItem = homeFolder.First(t => t.Equals(basedItem));
				var companyItem = companyFolder.First(t => t.Equals(basedItem));
				Merge(basedItem, homeItem, companyItem);
			}

			//添加新增的目录和书签
			MergeAdd(based.ComponentList, basedFolder, basedBookmark, home.ComponentList, addHomeFolder, addHomeBookmark);
			MergeAdd(based.ComponentList, basedFolder, basedBookmark, company.ComponentList, addCompanyFolder, addCompanyBookmark);

			//删除标记为删除的目录和书签
			based.ComponentList = based.ComponentList.Where(t => t.Operation != Operation.Delete).ToList();
		}

		/// <summary>
		/// 保持有序的合并添加项
		/// </summary>
		/// <param name="basedList">based集合</param>
		/// <param name="basedFolders">based里的文件夹集合</param>
		/// <param name="basedBookmarks">based里的书签集合</param>
		/// <param name="otherList">添加项所在集合</param>
		/// <param name="addFolders">添加的文件夹</param>
		/// <param name="addBookmarks">添加的书签</param>
		static void MergeAdd(List<Component> basedList, IEnumerable<Folder> basedFolders, IEnumerable<Bookmark> basedBookmarks, List<Component> otherList, IEnumerable<Folder> addFolders, IEnumerable<Bookmark> addBookmarks)
		{
			//合并添加项并按Order升序排序
			var addComponents = addFolders.Select(t => (Component)t).Union(addBookmarks).OrderBy(t => t.Order).ToList();
			if (addComponents.Count > 0)
			{
				//按Order的连续性进行分组
				var group = GroupByOrder(addComponents);
				foreach (var item in group)
				{
					//插入索引
					int insertIndex = 0;
					//获取此次插入项集合
					var insertRange = otherList.GetRange(item.Item1, item.Item2);

					if (item.Item1 != 0)
					{
						//在此项后面插入
						var previous = otherList[item.Item1 - 1];

						if (previous is Folder)
						{
							insertIndex = basedFolders.First(t => t.Equals((Folder)previous)).Order + 1;
						}
						else if (previous is Bookmark)
						{
							insertIndex = basedBookmarks.First(t => t.Equals((Bookmark)previous)).Order + 1;
						}
					}

					basedList.InsertRange(insertIndex, insertRange);
				}
			}
		}

		/// <summary>
		/// 将<paramref name="components"/>按Order的连续性进行分组，对于每个<see cref="Tuple{T1, T2}"/>，第一项是Order，第二项是连续的个数
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="components"></param>
		/// <returns></returns>
		static IEnumerable<Tuple<int, int>> GroupByOrder(List<Component> components)
		{
			int startValue = components[0].Order;
			int startIndex = 0;
			int i;
			for (i = 1; i < components.Count; i++)
			{
				if (startValue + i - startIndex != components[i].Order)
				{
					yield return new Tuple<int, int>(startValue, i - startIndex);
					startValue = components[i].Order;
					startIndex = i;
				}
			}
			yield return new Tuple<int, int>(startValue, i - startIndex);
		}
	}
}
