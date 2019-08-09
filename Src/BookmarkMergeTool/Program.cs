using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            var mergeDirectoryPath = ConfigurationManager.AppSettings["mergeDirectoryPath"];
            var mergeFilePath = ConfigurationManager.AppSettings["mergeFilePath"];
            var backupBasedFile = ConfigurationManager.AppSettings["backupBasedFile"].ToLower() == "true";

            if (backupBasedFile)
            {
                BackupFile(basedFilePath);
            }

            var based = BookmarkReader.ReadFile(basedFilePath);
            var otherFolders = Directory.GetFiles(mergeDirectoryPath).OrderBy(t => t).Select(t => BookmarkReader.ReadFile(t).Folder).ToList();

            Merge(based.Folder, otherFolders);
            BookmarkWriter.WriteFile(based, mergeFilePath);

            Console.WriteLine("合并完成，按任意键退出!");
            Console.ReadKey();
        }

        /// <summary>
        /// 备份文件
        /// </summary>
        /// <param name="filePath"></param>
        static void BackupFile(string filePath)
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);
            var newFilePath = filePath.Insert(filePath.LastIndexOf('.'), lastWriteTime.ToString("_yyyy_MM_dd_HH_mm_ss"));
            File.Copy(filePath, newFilePath, true);
        }

        /// <summary>
        /// 将<paramref name="others"/>合并到<paramref name="based"/>
        /// </summary>
        /// <param name="based"></param>
        /// <param name="others"></param>
        static void Merge(Folder based, List<Folder> others)
        {
            //查找based的文件夹和书签
            var basedFolder = based.ComponentList.OfType<Folder>().ToList();
            var basedBookmark = based.ComponentList.OfType<Bookmark>().ToList();

            List<List<Folder>> otherFolderList = new List<List<Folder>>();
            foreach (var other in others)
            {
                //将based的添加时间戳和修改时间戳更新到最新值
                if (other.AddDate > based.AddDate) based.AddDate = other.AddDate;
                if (other.LastModified > based.LastModified) based.LastModified = other.LastModified;

                //查找other的文件夹和书签
                var otherFolder = other.ComponentList.OfType<Folder>().ToList();
                var otherBookmark = other.ComponentList.OfType<Bookmark>().ToList();

                //other的文件夹添加到otherFolderList
                otherFolderList.Add(otherFolder);

                //标记删除的文件夹和书签
                basedFolder.Except(otherFolder, folderEquality).ForEach(t => t.Operation = Operation.Delete);
                basedBookmark.Except(otherBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Delete);

                //查找添加的文件夹和书签
                var addFolder = otherFolder.Except(basedFolder, folderEquality).ForEach(t => t.Operation = Operation.Add);
                var addBookmark = otherBookmark.Except(basedBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Add);

                //合并添加的文件夹和书签
                MergeAdd(based.ComponentList, basedFolder, basedBookmark, other.ComponentList, addFolder, addBookmark);
            }

            //合并操作类型未变过的文件夹
            foreach (var basedItem in basedFolder.Where(t => t.Operation == Operation.None))
            {
                var otherItem = otherFolderList.Select(list => list.First(t => t.Equals(basedItem))).ToList();
                Merge(basedItem, otherItem);
            }

            //删除标记为删除的文件夹和书签
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
        static void MergeAdd(List<Component> basedList, List<Folder> basedFolders, List<Bookmark> basedBookmarks, List<Component> otherList, IEnumerable<Folder> addFolders, IEnumerable<Bookmark> addBookmarks)
        {
            //合并添加项并按Order升序排序
            var addComponents = addFolders.Select(t => (Component)t).Union(addBookmarks).OrderBy(t => t.Order).ToList();
            if (addComponents.Count > 0)
            {
                //按Order的连续性进行分组，反转是为了foreach的时候从后往前插入，从而保证插入顺序的正确
                var group = GroupByOrder(addComponents).Reverse();
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
