using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NpcScan
{
    /// <summary>
    /// 优化滚动视图
    /// </summary>
    internal sealed class OptimizeScrollView
    {
        /// <summary>优化类型, ALL：全部, RowOnly：只有行, ColOnly：只有列</summary>
        [Flags]
        public enum OptType { All = 3, RowOnly = 1, ColOnly = 2 };
        private readonly string name;
        private readonly OptType optType;
        /// <summary>优化滚动视图类实例</summary>
        private static Dictionary<string, OptimizeScrollView> instances;
        /// <summary>每列右边框的位置</summary>
        private readonly List<float> colEndPosition;
        /// <summary>每行下边框的位置</summary>
        private readonly List<float> rowEndPosition;
        /// <summary>每行高度</summary>
        private readonly List<float> rowsHeight;
        /// <summary>滑动条可视区域高度</summary>
        private float viewHeight = -1;
        /// <summary>滑动条可视区域宽度</summary>
        private float viewWidth = -1;
        /// <summary>首末行或首末列缓存记录对应的滑动条的位置, index: 1, x; 2, y</summary>
        private readonly float[] xyPos = new float[] { -1, -1 };
        /// <summary>首末行缓存</summary>
        private readonly int[] firstLastRowCache = new int[2];
        /// <summary>首末列缓存</summary>
        private readonly int[] firstLastColCache = new int[2];

        /// <summary>列记录数</summary>
        public int ColCount => colEndPosition.Count;
        /// <summary>行记录数</summary>
        public int RowCount => rowEndPosition.Count;

        /// <summary>
        /// 优化滚动条显示类
        /// </summary>
        private OptimizeScrollView(OptType optType, string name)
        {
            this.name = name;
            this.optType = optType;
            if ((optType & OptType.ColOnly) == OptType.ColOnly)
            {
                colEndPosition = new List<float>();
            }
            if ((optType & OptType.RowOnly) == OptType.RowOnly)
            {
                rowEndPosition = new List<float>();
                rowsHeight = new List<float>();
            }
        }

        /// <summary>
        /// 根据设定的优化类型判断，该优化类实例是否可以已可用来获取滚动视图可视区域大小
        /// </summary>
        /// <param name="optType">y优化类型</param>
        /// <returns>是否可以已可用来获取滚动视图可视区域大小</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady()
        {
            switch (optType)
            {
                case OptType.ColOnly:
                    return viewWidth != -1 && ColCount != 0;
                case OptType.RowOnly:
                    return viewHeight != -1 && RowCount != 0;
                default:
                    return viewHeight != -1 && viewWidth != -1 && RowCount != 0 && ColCount != 0;
            }

        }

        /// <summary>
        /// 记录滚动视图中列长度信息
        /// </summary>
        /// <param name="index">列序号</param>
        /// <param name="colRect">列的位置信息</param>
        /// <param name="colLimit">列数限制</param>
        public void AddColRect(int index, Rect colRect, int colLimit)
        {
            if ((optType & OptType.ColOnly) != OptType.ColOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.ColOnly} For AddColRect");

            if (ColCount < colLimit && index == ColCount)
            {
                colEndPosition.Add(colRect.x + colRect.width);
            }
        }

        /// <summary>
        /// 记录滚动视图中行高度信息
        /// </summary>
        /// <param name="index">行序号</param>
        /// <param name="rowRect">行位置信息</param>
        public void AddRowRect(int index, Rect rowRect)
        {
            if ((optType & OptType.RowOnly) != OptType.RowOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: {optType} For AddRowRect");

            if (index == RowCount)
            {
                rowEndPosition.Add(rowRect.y + rowRect.height);
                rowsHeight.Add(rowRect.height);
            }
        }

        /// <summary>
        /// 记录滚动视图可显示区域的大小
        /// </summary>
        /// <param name="viewRect">记录滚动条显示区域的位置信息</param>
        public void AddViewRect(Rect viewRect)
        {
            viewHeight = viewRect.height;
            viewWidth = viewRect.width;
        }

        /// <summary>
        /// 获取该列距离滚动视图区域右边框的距离
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>该列距离滚动条显示区域右边框的距离</returns>
        public float GetColEndPosition(int i)
        {
            if ((optType & OptType.ColOnly) != OptType.ColOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.ColOnly} for GetColEndPosition");
            if (i >= colEndPosition.Count)
                throw new ArgumentException($"{name}: Index out of Range: colNum: {i} for GetColEndPosition");

            return colEndPosition[i];
        }

        /// <summary>
        /// 获取该列距离滚动视图区域上边框的距离
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>该列距离滚动条显示区域上边框的距离</returns>
        public float GetRowEndPosition(int i)
        {
            if ((optType & OptType.RowOnly) != OptType.RowOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.RowOnly} for GetRowEndPosition");

            if (i >= rowEndPosition.Count)
                throw new ArgumentException($"{name}: Index out of Range: rowNum: {i} for GetRowEndPosition");

            return rowEndPosition[i];
        }

        /// <summary>
        /// 获取滚动视图中行高
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>行高</returns>
        public float GetRowHeight(int i)
        {
            if ((optType & OptType.RowOnly) != OptType.RowOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.RowOnly} for GetRowHeight");
            if (i >= rowsHeight.Count)
                throw new ArgumentException($"{name}: Index out of Range: rowNum: {i} for GetRowHeight");

            return rowsHeight[i];
        }

        /// <summary>
        /// 清除记录
        /// </summary>
        public void ResetView()
        {
            viewHeight = -1;
            viewWidth = -1;
            rowEndPosition?.Clear();
            rowsHeight?.Clear();
            colEndPosition?.Clear();
            xyPos[0] = xyPos[1] = -1;
        }

        /// <summary>
        /// 获取滚动视图中可显示的起始列和结束列
        /// </summary>
        /// <param name="scrollPosition">滚动轴位置向量</param>
        /// <param name="firstColVisible">起始列</param>
        /// <param name="lastColVisible">结束列</param>
        /// <param name="viewWidth">视图宽度</param>
        public bool GetFirstAndLastColVisible(Vector2 scrollPosition, out int firstColVisible, out int lastColVisible)
        {
            if ((optType & OptType.ColOnly) != OptType.ColOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.ColOnly} for GetFirstAndLastColVisible");

            if (colEndPosition.Count == 0 || viewWidth == -1)
            {
                firstColVisible = lastColVisible = -1;
                return false;
            }
            // 滚动条位置没有改变时从缓存中读取数据
            if (xyPos[0] == scrollPosition.x)
            {
                firstColVisible = firstLastColCache[0];
                lastColVisible = firstLastColCache[1];
                return true;
            }
            // 限定滚动条位置不能超过记录数
            scrollPosition.x = Math.Min(scrollPosition.x, colEndPosition[colEndPosition.Count - 1]);
            int i = colEndPosition.BinarySearch(scrollPosition.x);
            firstColVisible = (i < 0) ? ~i : i;
            i = colEndPosition.BinarySearch(scrollPosition.x + viewWidth);
            lastColVisible = (i < 0) ? ~i : i + 1;
            lastColVisible = Math.Min(lastColVisible, colEndPosition.Count - 1);
            xyPos[0] = scrollPosition.x;
            firstLastColCache[0] = firstColVisible;
            firstLastColCache[1] = lastColVisible;
            return true;
        }

        /// <summary>
        /// 获取滚动视图中可显示的起始行和结束行
        /// </summary>
        /// <param name="scrollPosition">滚动轴位置向量</param>
        /// <param name="firstRowVisible">起始行</param>
        /// <param name="lastRowVisible">结束行</param>
        public bool GetFirstAndLastRowVisible(Vector2 scrollPosition, out int firstRowVisible, out int lastRowVisible)
        {
            if ((optType & OptType.RowOnly) != OptType.RowOnly)
                throw new InvalidOperationException($"{name}: Incorrect OptType: Should Use {OptType.RowOnly} for GetFirstAndLastRowVisible");

            if (rowEndPosition.Count == 0 || viewHeight == -1)
            {
                firstRowVisible = lastRowVisible = -1;
                return false;
            }
            // 滚动条位置没有改变时从缓存中读取数据
            if (xyPos[1] == scrollPosition.y)
            {
                firstRowVisible = firstLastRowCache[0];
                lastRowVisible = firstLastRowCache[1];
                return true;
            }
            // 限定滚动条位置不能超过记录数
            scrollPosition.y = Math.Min(scrollPosition.y, rowEndPosition[rowEndPosition.Count - 1]);
            int i = rowEndPosition.BinarySearch(scrollPosition.y);
            firstRowVisible = (i < 0) ? ~i : i;
            i = rowEndPosition.BinarySearch(scrollPosition.y + viewHeight);
            lastRowVisible = (i < 0) ? ~i : i + 1;
            lastRowVisible = Math.Min(lastRowVisible, rowEndPosition.Count - 1);
            xyPos[1] = scrollPosition.y;
            firstLastRowCache[0] = firstRowVisible;
            firstLastRowCache[1] = lastRowVisible;
            return true;
        }

        /// <summary>
        /// 根据名称获取滚动视图优化类的实例
        /// </summary>
        /// <param name="name">实例名字</param>
        /// <returns>类实例</returns>
        public static OptimizeScrollView GetInstance(string name, OptType optType = OptType.All)
        {
            if (instances == null)
                instances = new Dictionary<string, OptimizeScrollView>();

            if (!instances.TryGetValue(name, out OptimizeScrollView instance))
            {
                instance = new OptimizeScrollView(optType, name);
                instances[name] = instance;
            }
            if (instance.optType != optType)
                throw new InvalidOperationException($"{name}: Inconsistent Instance OptType {optType} : Maybe It Should Be {instance.optType}?");

            return instance;
        }

        /// <summary>
        /// 清除滚动视图优化类实例
        /// </summary>
        /// <param name="name">清除滚动视图优化类实例名称</param>
        public static void ClearInstance(string name) => instances.Remove(name);

        /// <summary>
        /// 清除所有滚动视图优化类实例
        /// </summary>
        public static void ClearAllInstances() => instances.Clear();

        /// <summary>
        /// 清除所有类的记录
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ResetAllView()
        {
            if (instances != null)
            {
                foreach (KeyValuePair<string, OptimizeScrollView> instance in instances)
                {
                    instance.Value.ResetView();
                }
            }
        }
    }
}
