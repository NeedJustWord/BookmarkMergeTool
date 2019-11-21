using System;
using System.Collections.Generic;
using System.Text;

namespace BookmarkMergeTool
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// 遍历执行<paramref name="action"/>操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null && action != null)
            {
                foreach (var item in source)
                {
                    action(item);
                }
            }
            return source;
        }

        /// <summary>
        /// 在<paramref name="text"/>左边补<paramref name="spaceNumber"/>个空格
        /// </summary>
        /// <param name="text"></param>
        /// <param name="spaceNumber"></param>
        /// <returns></returns>
        public static string AddLeftSpace(this string text, int spaceNumber)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < spaceNumber; i++)
            {
                sb.Append(' ');
            }
            sb.Append(text);

            return sb.ToString();
        }
    }
}
