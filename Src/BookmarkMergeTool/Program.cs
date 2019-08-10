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
            Console.Title = "谷歌浏览器书签合并工具";

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

            //将重复的书签标记成删除项
            DistinctBookmark(basedBookmark);

            List<List<Folder>> otherFolderList = new List<List<Folder>>();
            foreach (var other in others)
            {
                //将based的添加时间戳和修改时间戳更新到最新值
                based.AddDate = other.AddDate;
                based.LastModified = other.LastModified;

                //查找other的文件夹和书签
                var otherFolder = other.ComponentList.OfType<Folder>().ToList();
                var otherBookmark = other.ComponentList.OfType<Bookmark>().ToList();

                //将重复的书签标记成删除项
                DistinctBookmark(otherBookmark);

                //标记删除的文件夹和书签
                basedFolder.Except(otherFolder, folderEquality).ForEach(t => t.Operation = Operation.Delete);
                basedBookmark.Except(otherBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Delete);

                //查找添加的文件夹和书签
                var addFolder = otherFolder.Except(basedFolder, folderEquality).ForEach(t => t.Operation = Operation.Add);
                var addBookmark = otherBookmark.Except(basedBookmark, bookmarkEquality).ForEach(t => t.Operation = Operation.Add);

                //合并添加的文件夹和书签
                MergeAdd(based.ComponentList, basedFolder, basedBookmark, other.ComponentList, addFolder, addBookmark);

                //other的文件夹添加到otherFolderList
                otherFolderList.Add(otherFolder);
                //根据other的书签更新based的书签
                UpdateBookmark(basedBookmark, otherBookmark);

                //确保based集合的顺序和other集合的一致
                MakeSameOrder(based.ComponentList, other.ComponentList);
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
        /// 将重复的书签标记成删除项
        /// </summary>
        /// <param name="BookmarkList"></param>
        static void DistinctBookmark(List<Bookmark> BookmarkList)
        {
            //将重复的书签标记成删除项
            var repeatingBookmark = BookmarkList.GroupBy(t => t.Href).Where(t => t.Count() > 1);
            foreach (var item in repeatingBookmark)
            {
                bool isFirst = true;
                foreach (var bookmark in item)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        bookmark.Operation = Operation.Delete;
                    }
                }
            }
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

        /// <summary>
        /// 根据<paramref name="otherList"/>的书签更新<paramref name="basedList"/>的书签
        /// </summary>
        /// <param name="basedList"></param>
        /// <param name="otherList"></param>
        static void UpdateBookmark(List<Bookmark> basedList, List<Bookmark> otherList)
        {
            //添加的书签和删除的书签不需要更新
            var updateList = basedList.Where(t => t.Operation == Operation.None);

            foreach (var item in updateList)
            {
                var updateItem = otherList.FirstOrDefault(t => t.Href == item.Href);
                if (updateItem != null)
                {
                    item.AddDate = updateItem.AddDate;

                    //todo:在合并多个文件时，书签图标会更新成最后那个文件的样子
                    if (updateItem.Icon != item.Icon)
                    {
                        item.Icon = updateItem.Icon;
                    }
                }
            }
        }

        /// <summary>
        /// 确保<paramref name="based"/>的顺序和<paramref name="other"/>一致
        /// </summary>
        /// <param name="based"></param>
        /// <param name="other"></param>
        static void MakeSameOrder(List<Component> based, List<Component> other)
        {
            //排除删除项
            foreach (var otherComponent in other.Where(t => t.Operation != Operation.Delete))
            {
                //查找与otherComponent相等的未删除项
                Component basedComponent = based.FirstOrDefault(t => t.Operation != Operation.Delete && t.Equals(otherComponent));
                if (basedComponent != null)
                {
                    if (IsComponentEquals(basedComponent.PrevComponent, otherComponent.PrevComponent) == false)
                    {
                        //basedComponent和otherComponent的PrevComponent不同，则要调整顺序

                        //如果otherComponent.PrevComponent为null，则basedComponent在首位
                        //否则，basedComponent的位置在otherComponent.PrevComponent在based里的位置的后面
                        var newIndex = otherComponent.PrevComponent == null ? 0 : based.FindIndex(t => t.Equals(otherComponent.PrevComponent)) + 1;

                        based.Remove(basedComponent);
                        if (newIndex > based.Count)
                        {
                            newIndex = based.Count;
                        }
                        based.Insert(newIndex, basedComponent);
                    }
                }
            }
        }

        /// <summary>
        /// 判断<paramref name="based"/>和<paramref name="other"/>是否相等
        /// </summary>
        /// <param name="based"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        static bool IsComponentEquals(Component based, Component other)
        {
            if (based != null)
            {
                return based.Equals(other);
            }
            return other == null;
        }
    }
}
