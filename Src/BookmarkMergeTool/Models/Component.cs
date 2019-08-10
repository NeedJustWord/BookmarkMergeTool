using System.Collections.Generic;

namespace BookmarkMergeTool.Models
{
    /// <summary>
    /// 抽象类
    /// </summary>
    abstract class Component
    {
        /// <summary>
        /// 标签名
        /// </summary>
        public string LabelName { get; private set; }
        /// <summary>
        /// 标签文本
        /// </summary>
        public string LabelText { get; private set; }
        /// <summary>
        /// 添加时间戳
        /// </summary>
        public int AddDate
        {
            get
            {
                return addDate;
            }
            set
            {
                if (value > addDate)
                {
                    addDate = value;
                }
            }
        }
        private int addDate;
        /// <summary>
        /// 排序序号
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public Operation Operation { get; set; } = Operation.None;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="labelName">标签名</param>
        /// <param name="labelText">标签文本</param>
        /// <param name="addDate">添加时间戳</param>
        public Component(string labelName, string labelText, int addDate)
        {
            LabelName = labelName;
            LabelText = labelText;
            this.addDate = addDate;
        }

        /// <summary>
        /// 添加子项
        /// </summary>
        /// <param name="component"></param>
        public abstract void Add(Component component);

        /// <summary>
        /// 获取输出信息
        /// </summary>
        /// <param name="spaceNumber">前置空格数量</param>
        /// <returns></returns>
        public abstract IEnumerable<string> GetWriteInfo(int spaceNumber);
    }
}
