using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GongFaAttribute : Attribute
{
    /// <summary>
    /// 字典对应Id
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 字段描述
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 提示
    /// </summary>
    public string Tips { get; set; }

    /// <summary>
    /// 当前字段最大值
    /// </summary>
    public int Max { get; set; } = int.MaxValue;

    /// <summary>
    /// 当前字段最小值
    /// </summary>
    public int Min { get; set; } = int.MinValue;

    public GongFaAttribute(int id, string displayName, string tips = "暂无说明")
    {
        Index = id;
        DisplayName = displayName;
    }
}
