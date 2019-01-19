using System;
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
			based.ComponentList.AddRange(addHomeFolder);
			based.ComponentList.AddRange(addCompanyFolder);
			based.ComponentList.AddRange(addHomeBookmark);
			based.ComponentList.AddRange(addCompanyBookmark);

			//删除标记为删除的目录和书签
			based.ComponentList = based.ComponentList.Where(t => t.Operation != Operation.Delete).ToList();
		}
	}
}
