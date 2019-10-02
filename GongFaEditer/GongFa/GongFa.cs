using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GongFa
{
    /// <summary>
    /// 功法id
    /// </summary>
    public int GongFaId { get; set; }
    /// <summary>
    /// 功法名称
    /// </summary>
    [GongFa(0, "功法名称")]
    public string Name { get; set; }
    /// <summary>
    /// 品级
    /// </summary>
    [GongFa(2, "品级", Max = 9,Min = 1)]
    public int Level { get; set; }
    /// <summary>
    /// 品级
    /// </summary>
    [GongFa(61, "功法类型", Enable = false)]
    public int GongFaType { get; set; }
    /// <summary>
    /// 资质加成
    /// </summary>
    [GongFa(62, "加成")]
    public int Addition { get; set; }
    /// <summary>
    /// 造诣
    /// </summary>
    [GongFa(63, "基础资质需求")]
    public int RequireValue { get; set; }
    /// <summary>
    /// 升级资质要求
    /// </summary>
    [GongFa(64, "每修习1%需要的额外资质")]
    public float ReadDifficulty { get; set; }
    /// <summary>
    /// 升级所需历练点
    /// </summary>
    [GongFa(66, "基础历练消耗")]
    public int Experience { get; set; }
    /// <summary>
    /// 伤害类型
    /// </summary>
    [GongFa(67, "走火入魔伤害类型", Tips = "0.内伤,1.外伤", Max = 1, Min = 0)]
    public int DamageType { get; set; }
    /// <summary>
    /// 伤害阈值
    /// </summary>
    [GongFa(68, "允许走火入魔次数", Tips = "超过多少为致命伤", Max = 5, Min = 0)]
    public int DamageMax { get; set; }
    /// <summary>
    /// 走火部位
    /// </summary>
    [GongFa(69, "走火入魔伤害部位", Tips = "0 上身,1 下身,2 头部,3 左臂,4 右臂,5 左腿,6 右腿,7 感知,8 中毒,9 全身", Max = 9, Min = 0)]//5|6
    public string CollapsePart { get; set; }
    /// <summary>
    /// 起始位置
    /// </summary>
    [GongFa(70, "突破起点", Enable = false)]
    public string StartPostion { get; set; }
    /// <summary>
    /// 终点位置
    /// </summary>
    [GongFa(71, "突破终点", Enable = false)]
    public string EndPostion { get; set; }
    /// <summary>
    /// 修炼类型
    /// </summary>
    [GongFa(72, "突破部位", Enable = false)]
    public string LearnType { get; set; }
    /// <summary>
    /// 突破所需历练点
    /// </summary>
    [GongFa(73, "阅读难度")]
    public int MyProperty { get; set; }
    /// <summary>
    /// 正练
    /// </summary>
    [GongFa(103, "正练")]
    public string DirectLearn { get; set; }
    /// <summary>
    /// 逆练
    /// </summary>
    [GongFa(104, "逆练")]
    public string NegativeLearn { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    [GongFa(1, "功法类型", Enable = false)]
    public int TypeValue { get; set; }
    /// <summary>
    /// 运功栏位1摧破2轻灵3护体4奇窍
    /// </summary>
    [GongFa(6, "真气类型", Tips = "1摧破,2轻灵,3护体,4奇窍", Max = 4, Min = 1)]
    public int UsePosition { get; set; }
    /// <summary>
    /// 运功占格
    /// </summary>
    [GongFa(6, "运功占格", Max = 3, Min = 1)]
    public int UseWidth { get; set; }
    /// <summary>
    /// 门派
    /// </summary>
    [GongFa(3, "门派", Enable = false)]
    public int MenPai { get; set; }
    /// <summary>
    /// 是否可以在奇遇中获得
    /// </summary>
    [GongFa(15, "是否可以在奇遇中获得", Max = 1, Min = 0)]
    public int CanGetInStory { get; set; }
    /// <summary>
    /// 不可为外人道
    /// </summary>
    [GongFa(16, "不可为外人道", Max = 1, Min = 0)]
    public int ShowOrHide { get; set; }
    /// <summary>
    /// 五行属性
    /// </summary>
    [GongFa(4, "五行属性", Max = 5, Min = 0)]
    public int FiveElement { get; set; }
    /// <summary>
    /// 五行属性
    /// </summary>
    [GongFa(5, "造诣", Max = 9, Min = 0)]
    public int LevelBase { get; set; }
    /// <summary>
    /// 金刚
    /// </summary>
    [GongFa(701, "金刚属性成长")]
    public decimal KingKong { get; set; }
    /// <summary>
    /// 紫霞
    /// </summary>
    [GongFa(702, "紫霞属性成长")]
    public decimal ZiXia { get; set; }
    /// <summary>
    /// 玄阴
    /// </summary>
    [GongFa(703, "玄阴属性成长")]
    public decimal XuanYing { get; set; }
    /// <summary>
    /// 纯阳
    /// </summary>
    [GongFa(704, "纯阳属性成长")]
    public decimal ChunYang { get; set; }
    /// <summary>
    /// 归元
    /// </summary>
    [GongFa(705, "归元属性成长")]
    public decimal GuiYuan { get; set; }
    [GongFa(999, "大成")]
    public int Max { get; set; }
    [GongFa(710, "使用需求", Enable = false)]
    public string BaseRequire { get; set; }
    [GongFa(711, "发挥上限")]
    public string MaxUsePercentage { get; set; }
    [GongFa(901, "10%万用")]
    public string AllUse1 { get; set; }
    [GongFa(902, "20%万用")]
    public string AllUse2 { get; set; }
    [GongFa(903, "30%万用")]
    public string AllUse3 { get; set; }
    [GongFa(904, "40%万用")]
    public string AllUse4 { get; set; }
    [GongFa(905, "50%万用")]
    public string AllUse5 { get; set; }
    [GongFa(906, "60%万用")]
    public string AllUse6 { get; set; }
    [GongFa(907, "70%万用")]
    public string AllUse7 { get; set; }
    [GongFa(908, "80%万用")]
    public string AllUse8 { get; set; }
    [GongFa(909, "90%万用")]
    public string AllUse9 { get; set; }
    [GongFa(910, "100%万用")]
    public string AllUse10 { get; set; }
    [GongFa(921, "摧破")]
    public string CuiPo { get; set; }
    [GongFa(922, "轻灵")]
    public string QingLing { get; set; }
    [GongFa(923, "护体")]
    public string HuTi { get; set; }
    [GongFa(924, "奇窍")]
    public string QiQiao { get; set; }
    [GongFa(8, "耗气比例", Tips = "气势消耗—提气% 架势%=总气势-提气%")]
    public int NeiGong { get; set; }
    [GongFa(9, "总消耗", Max = 100, Min = 0)]
    public int TotalQi { get; set; }
    [GongFa(31, "范围", Tips = "实际值=当前值/100")]
    public decimal Distance { get; set; }

    [GongFa(501, "摧破1")]
    public decimal CP1 { get; set; }
    [GongFa(502, "摧破2")]
    public decimal CP2 { get; set; }
    [GongFa(503, "摧破3")]
    public decimal CP3 { get; set; }
    [GongFa(504, "摧破4")]
    public decimal CP4 { get; set; }

    [GongFa(38, "暗器耐久消耗")]
    public decimal AQXH { get; set; }

    [GongFa(39, "蛊虫消耗")]
    public decimal GCXH { get; set; }

    [GongFa(40, "移动消耗")]
    public decimal YDXH { get; set; }

    [GongFa(10, "施放时间")]
    public decimal SFSJ { get; set; }

    [GongFa(601, "力道")]
    public decimal LD { get; set; }

    [GongFa(602, "精妙")]
    public decimal JM { get; set; }

    [GongFa(603, "迅疾")]
    public decimal XJ { get; set; }

    [GongFa(611, "力道占比")]
    public decimal LDZB { get; set; }

    [GongFa(612, "精妙占比")]
    public decimal JMZB { get; set; }

    [GongFa(613, "迅疾占比")]
    public decimal XJZB { get; set; }

    [GongFa(614, "破体")]
    public decimal PT { get; set; }

    [GongFa(615, "破气")]
    public decimal PQ { get; set; }

    [GongFa(604, "伤害")]
    public decimal SH { get; set; }

    [GongFa(41, "护体")]
    public decimal HT { get; set; }

    [GongFa(42, "御气")]
    public decimal YQ { get; set; }

    [GongFa(43, "护体发挥")]
    public decimal HTFH { get; set; }

    [GongFa(44, "御气发挥")]
    public decimal YQFH { get; set; }

    [GongFa(45, "卸力")]
    public decimal XL { get; set; }

    [GongFa(46, "拆招")]
    public decimal CZ { get; set; }

    [GongFa(47, "闪避")]
    public decimal SB { get; set; }

    [GongFa(48, "卸力发挥")]
    public decimal XLFH { get; set; }

    [GongFa(49, "拆招发挥")]
    public decimal CZFH { get; set; }

    [GongFa(50, "闪避发挥")]
    public decimal SBFH { get; set; }

    [GongFa(51, "反普比例")]
    public decimal FPBL { get; set; }

    [GongFa(52, "反外比例")]
    public decimal FWBL { get; set; }

    [GongFa(53, "反内比例")]
    public decimal FNBL { get; set; }

    [GongFa(54, "持续时间")]
    public decimal CXSJ { get; set; }

    [GongFa(55, "反击范围")]
    public decimal FJFW { get; set; }

    [GongFa(32, "移动速度")]
    public decimal YDSD { get; set; }

    [GongFa(33, "移速发挥")]
    public decimal YSFH { get; set; }

    [GongFa(34, "移动距离")]
    public decimal YDJL { get; set; }

    [GongFa(35, "移距发挥")]
    public decimal YJFH { get; set; }

    [GongFa(36, "生效次数")]
    public decimal SXCS { get; set; }

    [GongFa(81, "烈毒")]
    public decimal lD { get; set; }

    [GongFa(82, "郁毒")]
    public decimal YD { get; set; }

    [GongFa(83, "寒毒")]
    public decimal HD { get; set; }

    [GongFa(84, "赤毒")]
    public decimal CD { get; set; }

    [GongFa(85, "腐毒")]
    public decimal FD { get; set; }

    [GongFa(86, "幻毒")]
    public decimal hD { get; set; }

    [GongFa(50041, "烈毒抵抗")]
    public decimal LDDK { get; set; }

    [GongFa(50042, "郁毒抵抗")]
    public decimal YDDK { get; set; }

    [GongFa(50043, "寒毒抵抗")]
    public decimal HDDK { get; set; }

    [GongFa(50044, "赤毒抵抗")]
    public decimal CDDK { get; set; }

    [GongFa(50045, "腐毒抵抗")]
    public decimal FDDK { get; set; }

    [GongFa(50046, "幻毒抵抗")]
    public decimal hDDK { get; set; }

    [GongFa(50032, "外伤上限")]
    public decimal WSSX { get; set; }

    [GongFa(50033, "内伤上限")]
    public decimal NSSX { get; set; }

    [GongFa(50022, "守御效率")]
    public decimal SYXL { get; set; }

    [GongFa(50023, "疗伤效率")]
    public decimal LSXL { get; set; }

    [GongFa(51110, "驱毒效率")]
    public decimal QDXL { get; set; }

    [GongFa(51101, "提气速度")]
    public decimal TQSD { get; set; }

    [GongFa(51102, "架势速度")]
    public decimal JSSD { get; set; }

    [GongFa(51103, "提气消耗")]
    public decimal TQXH { get; set; }

    [GongFa(51104, "架势消耗")]
    public decimal JSXH { get; set; }

    [GongFa(51105, "内功发挥")]
    public decimal NGFH { get; set; }

    [GongFa(51106, "施展速度")]
    public decimal SZSD { get; set; }

    [GongFa(51107, "攻击速度")]
    public decimal GJSD { get; set; }

    [GongFa(51108, "武器切换")]
    public decimal WQQH { get; set; }

    [GongFa(51109, "移动速度(内)")]
    public decimal yDSD { get; set; }

    [GongFa(51111, "移动速度(内)")]
    public decimal yYDJL { get; set; }

    /// <summary>
    /// 功法说明
    /// </summary>
    [GongFa(99, "说明")]
    public string Description { get; set; }

    /// <summary>
    /// 是否是自定义功法
    /// </summary>
    public bool IsCustom { get; set; }
}
