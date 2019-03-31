namespace Xzy.EmbeddedApp.Model
{
    public enum SimulateTaskType
    {
        /// <summary>
        /// 模糊查找
        /// </summary>
        FuzzySearch = 1,
        /// <summary>
        /// 精确查找
        /// </summary>
        ExactSearch = 2,
        /// <summary>
        /// 模糊查找并点击
        /// </summary>
        FuzzySearchAndClick = 3,
        /// <summary>
        /// 精确查找并点击
        /// </summary>
        ExactSearchAndClick = 4,
        /// <summary>
        /// 精确查找并清除
        /// </summary>
        ExactSearchAndClear = 5,
        /// <summary>
        /// 模糊查找并清除
        /// </summary>
        FuzzySearchAndClear = 6,

        /// <summary>
        /// 精确查找并赋值
        /// </summary>
        ExactSearchAndAssign = 7,

        /// <summary>
        /// 点击坐标
        /// </summary>
        ClickCoordinate = 8,

        /// <summary>
        /// 滑动
        /// </summary>
        Swipe = 9,
    }
}
