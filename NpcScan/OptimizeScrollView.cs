using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NpcScan
{
    /// <summary>
    /// 优化滚动条
    /// </summary>
    internal sealed class OptimizeScrollView
    {
        /// <summary>优化类型, ALL：全部, RowOnly：只有行, ColOnly：只有列</summary>
        public enum OptType { All, RowOnly, ColOnly };
        /// <summary>优化类实例</summary>
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
        private OptimizeScrollView()
        {
            colEndPosition = new List<float>();
            rowEndPosition = new List<float>();
            rowsHeight = new List<float>();
        }

        /// <summary>
        /// 根据设定的优化类型判断，该优化类是否可以已可用来获取可视区域大小
        /// </summary>
        /// <param name="optType">y优化类型</param>
        /// <returns>是否可以已可用来获取可视区域大小</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsReady(OptType optType = OptType.All)
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
        /// 记录列长度信息
        /// </summary>
        /// <param name="index">列序号</param>
        /// <param name="labelRect">列的位置信息</param>
        /// <param name="colLimit">列数限制</param>
        public void AddColRect(int index, Rect labelRect, int colLimit)
        {
            if (ColCount < colLimit && index == ColCount)
            {
                colEndPosition.Add(labelRect.x + labelRect.width);
            }
        }

        /// <summary>
        /// 当列和行被Box包住的时候，为行和列添加Box的大小以反应行和列的正确大小
        /// </summary>
        /// <param name="boxRect">外包的Box大小参数</param>
        public void AddOffset(Rect boxRect)
        {
            if (colEndPosition.Count != 0 && rowEndPosition.Count != 0)
            {
                colEndPosition[colEndPosition.Count - 1] = boxRect.x + boxRect.width;
                rowEndPosition[rowEndPosition.Count - 1] = boxRect.y + boxRect.height;
            }
        }

        /// <summary>
        /// 记录行高度信息
        /// </summary>
        /// <param name="index">行序号</param>
        /// <param name="rowRect">行位置信息</param>
        public void AddRowRect(int index, Rect rowRect)
        {
            if (index == RowCount)
            {
                rowEndPosition.Add(rowRect.y + rowRect.height);
                rowsHeight.Add(rowRect.height);
            }
        }

        /// <summary>
        /// 记录滚动条显示区域的大小
        /// </summary>
        /// <param name="viewRect">记录滚动条显示区域的位置信息</param>
        public void AddViewRect(Rect viewRect)
        {
            viewHeight = viewRect.height;
            viewWidth = viewRect.width;
        }

        /// <summary>
        /// 获取该列距离滚动条显示区域右边框的距离
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>该列距离滚动条显示区域右边框的距离</returns>
        public float GetColEndPosition(int i) => (i < colEndPosition.Count) ? colEndPosition[i] : 0;

        /// <summary>
        /// 获取该列距离滚动条显示区域上边框的距离
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>该列距离滚动条显示区域上边框的距离</returns>
        public float GetRowEndPosition(int i) => (i < rowEndPosition.Count) ? rowEndPosition[i] : 0;

        /// <summary>
        /// 获取行高
        /// </summary>
        /// <param name="i">行号</param>
        /// <returns>行高</returns>
        public float GetRowHeight(int i) => i < rowsHeight.Count ? rowsHeight[i] : 0;

        /// <summary>
        /// 清除记录
        /// </summary>
        public void ResetView()
        {
            viewHeight = -1;
            viewWidth = -1;
            rowEndPosition.Clear();
            rowsHeight.Clear();
            colEndPosition.Clear();
            xyPos[0] = xyPos[1] = -1;
        }

        /// <summary>
        /// 获取可显示的起始列和结束列
        /// </summary>
        /// <param name="scrollPosition">滚动轴位置向量</param>
        /// <param name="firstColVisible">起始列</param>
        /// <param name="lastColVisible">结束列</param>
        /// <param name="viewWidth">视图宽度</param>
        public bool GetFirstAndLastColVisible(Vector2 scrollPosition, out int firstColVisible, out int lastColVisible)
        {
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
        /// 获取可显示的起始行和结束行
        /// </summary>
        /// <param name="scrollPosition">滚动轴位置向量</param>
        /// <param name="firstRowVisible">起始行</param>
        /// <param name="lastRowVisible">结束行</param>
        public bool GetFirstAndLastRowVisible(Vector2 scrollPosition, out int firstRowVisible, out int lastRowVisible)
        {
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
        /// 根据名称获取滚动条优化的类实例
        /// </summary>
        /// <param name="name">实例名字</param>
        /// <returns>类实例</returns>
        public static OptimizeScrollView GetInstance(string name)
        {
            if (instances == null)
                instances = new Dictionary<string, OptimizeScrollView>();

            if (!instances.TryGetValue(name, out OptimizeScrollView instance))
            {
                instance = new OptimizeScrollView();
                instances[name] = instance;
            }
            return instance;
        }

        /// <summary>
        /// 清除记录
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
