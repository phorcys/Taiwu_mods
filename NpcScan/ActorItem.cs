using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace NpcScan
{
    /// <summary>
    /// 角色信息类
    /// </summary>
    internal class ActorItem : IComparable<ActorItem>, IEquatable<ActorItem>
    {
        #region 域
        /// <summary>UI索引</summary>
        private readonly UI _ui;
        /// <summary>NPC的ID</summary>
        public readonly int npcId;
        /// <summary>是否获取基础值</summary>
        private readonly bool isGetReal;
        /// <summary>是否是排行榜模式</summary>
        private readonly bool isRank;
        /// <summary>是否精确特性</summary>
        private readonly bool isTarFeature;
        /// <summary>从属名称</summary>
        private string gangName;
        /// <summary>门派身份文字描述</summary>
        private string gangLevelText;
        /// <summary>商会名称</summary>
        private string shopName;
        /// <summary>排行模式下综合评价计算时的加权</summary>
        private int[] totalRankWeight;
        /// <summary>角色物品, 基础ID、促织ID和促织部位ID共同唯一确定任何物品的品阶和名称</summary>
        /// <remarks>使用<see cref="ValueTuple"/>作为Key比使用<see cref="Item"/>为Key速度快很多</remarks>
        private Dictionary<(int BaseId, int QuquId, int QuquPartId), int> actorItems;
        /// <summary>角色装备, 基础ID、促织ID和促织部位ID共同唯一确定任何物品的品阶和名称</summary>
        private Dictionary<(int BaseId, int QuquId, int QuquPartId), int> actorEquips;

        /*----------------------------------------
        以下缓存对应的数据计算方法无法多线程计算，只在UI
        渲染时才会按需计算，使用缓存可以减少排序时的计算量
        ------------------------------------------*/
        /// <summary>整型属性值缓存 0综合评分 1魅力 2立场 3婚姻 4-9六维 10-23技艺资质 24-39功法资质 40金钱</summary>
        /// <remarks>缓存一些计算量大的整型数值，如<see cref="DateFile.GetActorDate(int, int, bool)"/> bool为true时计算量较大</remarks>
        private readonly int[] intCache = new int[41];
        /// <summary>字符串属性值缓存 0 地点坐标 1技艺资质成长(无色) 2功法资质成长(无色)</summary>
        private readonly string[] stringCache = new string[3];
        /// <summary>健康值缓存</summary>
        private int[] healthCache;
        /// <summary>七元赋性缓存</summary>
        private int[] actorResourcesCache;
        /// <summary>可教技能最大等级缓存</summary>
        private List<int> skillMaxCache;
        /// <summary>可教功法数量缓存</summary>
        private int[] gongFaResultCache;
        /// <summary>物品搜索结果缓存</summary>
        private Item[] itemResultCache;
        /// <summary>搜索结果渲染缓存</summary>
        private string[] addedItemCache;
        /*----------------------------------------*/
        /// <summary>线程锁</summary>
        private int locker;
        #endregion

        #region 私有属性(只在需要时才执行其中的方法，提高搜索性能)
        /// <summary>是否需要添加(线程安全)</summary>
        public bool NeededAdd { get; private set; }
        // 注意:以下属性引用的大部分方法不保证线程安全
        /// <summary>姓名</summary>
        public string ActorName { get; private set; }
        /// <summary>魅力</summary>
        public int Charm => intCache[1] = intCache[1] > -1 ? intCache[1] : int.Parse(DateFile.instance.GetActorDate(npcId, 15, !isGetReal));
        /// <summary>立场数值</summary>
        public int Goodness => intCache[2] = intCache[2] > -1 ? intCache[2] : DateFile.instance.GetActorGoodness(npcId);
        /// <summary>膂力</summary>
        public int Str => intCache[4] = intCache[4] > -1 ? intCache[4] : int.Parse(DateFile.instance.GetActorDate(npcId, 61, !isGetReal));
        /// <summary>体质</summary>
        public int Con => intCache[5] = intCache[5] > -1 ? intCache[5] : int.Parse(DateFile.instance.GetActorDate(npcId, 62, !isGetReal));
        /// <summary>灵敏</summary>
        public int Agi => intCache[6] = intCache[6] > -1 ? intCache[6] : int.Parse(DateFile.instance.GetActorDate(npcId, 63, !isGetReal));
        /// <summary>根骨</summary>
        public int Bon => intCache[7] = intCache[7] > -1 ? intCache[7] : int.Parse(DateFile.instance.GetActorDate(npcId, 64, !isGetReal));
        /// <summary>悟性</summary>
        public int Inv => intCache[8] = intCache[8] > -1 ? intCache[8] : int.Parse(DateFile.instance.GetActorDate(npcId, 65, !isGetReal));
        /// <summary>定力</summary>
        public int Pat => intCache[9] = intCache[9] > -1 ? intCache[9] : int.Parse(DateFile.instance.GetActorDate(npcId, 66, !isGetReal));
        /// <summary>年龄</summary>
        public int Age => int.Parse(DateFile.instance.GetActorDate(npcId, 11, false));
        /// <summary>性别：1男 2女 3男生女相 4女生男相</summary>
        public int Gender => int.Parse(DateFile.instance.GetActorDate(npcId, 14, false))
                             + int.Parse(DateFile.instance.GetActorDate(npcId, 17, false)) * 2;
        /// <summary>性别文字</summary>
        public string GenderText => (Gender > 2)
                                    // 处理男生女相和女生男相
                                    ? DateFile.instance.SetColoer(20010, Gender - 2 == 1 ? "男" : "女")
                                    : Gender == 1 ? "男" : "女";

        /// <summary>健康</summary>
        public int Health => int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) == 0 ? DateFile.instance.Health(npcId) : 0;
        /// <summary>坐标</summary>
        public string Place => stringCache[0] = stringCache[0] ?? GetPlace();
        /// <summary>魅力</summary>
        public string CharmText => GetCharmText();
        /// <summary>轮回</summary>
        public int SamsaraCount { get; private set; }
        /// <summary>前世</summary>
        public string SamsaraNames { get; private set; }
        /// <summary>立场</summary>
        public string GoodnessText => DateFile.instance.massageDate[9][0].Split('|')[Goodness];
        /// <summary>从属ID</summary>
        public int Groupid => int.Parse(DateFile.instance.GetActorDate(npcId, 19, false));
        /// <summary>身份等级(负数时为因配偶身份获得的等级)</summary>
        public int GangLevel => int.Parse(DateFile.instance.GetActorDate(npcId, 20, false));
        /// <summary>金钱</summary>
        public int Money => intCache[40] = intCache[40] > -1 ? intCache[40] : DateFile.instance.ActorResource(npcId)[5];
        /// <summary>从属身份对应的数据ID</summary>
        public int GangValueId => DateFile.instance.GetGangValueId(Groupid, GangLevel);
        /// <summary>七元赋性</summary>
        public int[] ActorResources => actorResourcesCache = actorResourcesCache ?? DateFile.instance.GetActorResources(npcId);
        /// <summary>角色特性</summary>
        public List<int> ActorFeatures { get; private set; }
        /// <summary>综合评价</summary>
        public int Totalrank
        {
            get
            {
                if (intCache[0] > -1)
                    return intCache[0];
                if (totalRankWeight == null) return 0;
                var tmp = Str * totalRankWeight[0] + Con * totalRankWeight[1] + Agi * totalRankWeight[2] + Bon * totalRankWeight[3] + Inv * totalRankWeight[4] + Pat * totalRankWeight[5];
                tmp += Charm * totalRankWeight[6];
                for (int tmpi = 0; tmpi < 14; tmpi++)
                    tmp += GetLevelValue(tmpi, 1) * totalRankWeight[7 + tmpi];
                for (int tmpi = 0; tmpi < 16; tmpi++)
                    tmp += GetLevelValue(tmpi, 0) * totalRankWeight[21 + tmpi];
                intCache[0] = tmp;
                // 计算完毕清除权重
                totalRankWeight = null;
                return tmp;
            }
        }
        #endregion

        /// <summary>
        /// 角色信息类构造函数
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="ui"></param>
        /// <remarks>
        /// <see cref="DateFile.GetActorName(int, bool, bool)"/>目前线程安全
        /// <see cref="DateFile.GetActorDate(int, int, bool)"/>中当bool为false时线程安全
        /// <see cref="DateFile.GetGangDate(int, int)"/>线程安全
        /// <see cref="DateFile.GetActorFeature(int)"/><see cref="Patch.DateFile_GetActorFeature_Patch"/>中已经将该方法修改为线程安全
        /// </remarks>
        public ActorItem(int npcId, UI ui)
        {
            _ui = ui;
            this.npcId = npcId;
            isGetReal = ui.IsGetReal;
            isRank = ui.Rankmode ? true : false;
            isTarFeature = ui.TarFeature;
            ActorName = DateFile.instance.GetActorName(npcId);
            SamsaraNames = GetSamsaraNames(npcId);

            // 初始化缓存
            for (int i = 0; i < intCache.Length; i++)
                intCache[i] = -1;
            if (isRank)
            {
                // 初始化综合评分计算加权
                totalRankWeight = new int[37];
                totalRankWeight[0] = ui.StrValue;
                totalRankWeight[1] = ui.ConValue;
                totalRankWeight[2] = ui.AgiValue;
                totalRankWeight[3] = ui.BonValue;
                totalRankWeight[4] = ui.IntValue;
                totalRankWeight[5] = ui.PatValue;
                totalRankWeight[6] = ui.CharmValue;
                for (int tmpi = 0; tmpi < 14; tmpi++)
                    totalRankWeight[7 + tmpi] = ui.Gongfa[tmpi];
                for (int tmpi = 0; tmpi < 16; tmpi++)
                    totalRankWeight[21 + tmpi] = ui.Skill[tmpi];
            }

            AddCheck();
        }

        /// <summary>
        /// 获取搜索结果
        /// </summary>
        /// <returns></returns>
        public string[] GetAddedItem()
        {
            // 之前已经处理过则不需要再次处理
            if (addedItemCache != null)
                return addedItemCache;
            if (!NeededAdd)
                return null;
            int index = 0;
            // 使用array而非List减少copy次数提升渲染性能
            var additem = new string[isRank ? 63 : 62];
            //综合评分
            if (isRank)
                additem.Add(Totalrank.ToString(), ref index);
            //姓名
            additem.Add($"{ActorName}({npcId})", ref index);
            //年龄
            additem.Add(Age.ToString(), ref index);
            //性别
            additem.Add(GenderText, ref index);
            //坐标
            additem.Add(Place, ref index);
            //魅力
            additem.Add($"{Charm}({CharmText})", ref index);
            //从属gangText
            additem.Add(gangName, ref index);
            //身份gangLevelText
            additem.Add(gangLevelText, ref index);
            //商会
            additem.Add(shopName, ref index);
            //立场goodnessText
            additem.Add(GoodnessText, ref index);
            //婚姻
            additem.Add(GetMarriageText(), ref index);
            //技艺资质成长
            additem.Add(GetSkillDevelopText(true), ref index);
            //功法资质成长
            additem.Add(GetGongFaDevelopText(true), ref index);
            //健康
            additem.Add(GetHealth(), ref index);
            //膂力
            additem.Add(Str.ToString(), ref index);
            //体质
            additem.Add(Con.ToString(), ref index);
            //灵敏
            additem.Add(Agi.ToString(), ref index);
            //根骨
            additem.Add(Bon.ToString(), ref index);
            //悟性
            additem.Add(Inv.ToString(), ref index);
            //定力
            additem.Add(Pat.ToString(), ref index);
            //功法资质
            for (int i = 0; i < 14; i++)
            {
                additem.Add(GetLevel(i, 1), ref index);
            }
            //技艺资质
            for (int i = 0; i < 16; i++)
            {
                additem.Add(GetLevel(i, 0), ref index);
            }
            // 银钱
            additem.Add(DateFile.instance.ActorResource(npcId)[5].ToString(), ref index);
            // 细腻
            additem.Add(ActorResources[0].ToString(), ref index);
            // 聪颖
            additem.Add(ActorResources[1].ToString(), ref index);
            // 水性
            additem.Add(ActorResources[2].ToString(), ref index);
            // 勇壮
            additem.Add(ActorResources[3].ToString(), ref index);
            // 坚毅
            additem.Add(ActorResources[4].ToString(), ref index);
            // 冷静
            additem.Add(ActorResources[5].ToString(), ref index);
            // 福源
            additem.Add(ActorResources[6].ToString(), ref index);
            // 可学功法
            additem.Add(GetGongFaListText(), ref index);
            // 可学技艺
            additem.Add(GetMaxSkillLevelText(), ref index);
            // 物品
            additem.Add(GetItemsText(), ref index);
            // 前世
            additem.Add(SamsaraNames, ref index);
            // 特性
            additem.Add(GetActorFeatureNameText(isTarFeature), ref index);
            addedItemCache = additem;
            return additem;
        }

        /// <summary>
        /// 添加的检查：是否符合搜索条件(线程安全)
        /// </summary>
        private void AddCheck()
        {
            // 997为人物模板, 当大于100是特殊剧情人物，跳过不处理, 详见TextAsset中的PresetActor_Date
            // 如: 2001:莫女 2002:大岳瑶常 2003:九寒 2004:金凰儿 2005:衣以候 2006:卫起 2007:以向 2008:血枫 2009:术方
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 997, false)) > 100) return;
            // 健康
            if (_ui.HealthValue > 0)
            {
                bool result = false;
                Lock();
                try { result = Health < _ui.HealthValue; }
                finally { Unlock(); }
                if (result) return;
            }
            // 姓名
            if (_ui.AName != "" && (!(ActorName.Contains(_ui.AName) || SamsaraNames.Contains(_ui.AName))))
                return;

            // 轮回次数
            if (_ui.SamsaraCount > 0 && SamsaraCount < _ui.SamsaraCount) return;

            // 最低年龄
            if (_ui.Minage > 0)
            {
                bool result = false;
                Lock();
                try { result = Age < _ui.Minage; }
                finally { Unlock(); }
                if (result) return;
            }
            // 最高年龄
            if (_ui.Maxage > 0)
            {
                bool result = false;
                Lock();
                try { result = Age > _ui.Maxage; }
                finally { Unlock(); }
                if (result) return;
            }
            // 性别
            if (_ui.GenderValue > 0)
            {
                bool result = false;
                Lock();
                try { result = Gender != _ui.GenderValue; }
                finally { Unlock(); }
                if (result) return;
            }

            // 排行榜模式以下搜索条件无效
            // 我至今不知道排行榜模式有啥用？都能排序了都。
            if (!isRank)
            {
                if (_ui.StrValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Str < _ui.StrValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.ConValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Con < _ui.ConValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.AgiValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Agi < _ui.AgiValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.BonValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Bon < _ui.BonValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.IntValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Inv < _ui.IntValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.PatValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Pat < _ui.PatValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                if (_ui.CharmValue > 0)
                {
                    bool result = false;
                    Lock();
                    try { result = Charm < _ui.CharmValue; }
                    finally { Unlock(); }
                    if (result) return;
                }
                for (int i = 0; i < 14; i++)
                {
                    if (_ui.Gongfa[i] > 0)
                    {
                        bool result = false;
                        Lock();
                        try { result = GetLevelValue(i, 1) < _ui.Gongfa[i]; }
                        finally { Unlock(); }
                        if (result) return;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    if (_ui.Skill[i] > 0)
                    {
                        bool result = false;
                        Lock();
                        try { result = GetLevelValue(i, 0) < _ui.Skill[i]; }
                        finally { Unlock(); }
                        if (result) return;
                    }
                }
            }

            // gangLevel 门派地位 若为负 则地位由婚姻带来的。
            if (_ui.HighestLevel > 1)
            {
                bool result = false;
                Lock();
                try { result = Mathf.Abs(GangLevel) < _ui.HighestLevel; }
                finally { Unlock(); }
                if (result) return;
            }

            // 门派识别和搜门派
            // 如果未开启门派搜索 直接通过
            // (groupid >= 1 && groupid <= 15)结果为门派中人
            // _ui.isGang = true 仅搜索门派：(groupid > 15)=true 排除
            // _ui.isGang = false 仅搜索非门派：(groupid > 15)=false 排除
            if (_ui.TarIsGang)
            {
                bool result = false;
                Lock();
                try { result = _ui.IsGang == Groupid > 15; }
                finally { Unlock(); }
                if (result) return;
            }
            // 立场
            if (_ui.Goodness > -1)
            {
                bool result = false;
                Lock();
                try { result = Goodness != _ui.Goodness; }
                finally { Unlock(); }
                if (result) return;
            }
            // 婚姻
            if (_ui.Marriage > 0)
            {
                bool result = false;
                Lock();
                try { result = GetMarriage() != _ui.Marriage; }
                finally { Unlock(); }
                if (result) return;
            }

            // 从属名称
            gangName = DateFile.instance.GetGangDate(Groupid, 0);
            // 地位名称ID (注：因亲属关系可能获得相应地位改变)
            int gangLevelTextId = (GangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)));
            // 身份的文字描述
            gangLevelText = DateFile.instance.SetColoer((GangValueId != 0) ? (20011 - Mathf.Abs(GangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[GangValueId][gangLevelTextId], false);
            // 从属和地位
            if ((_ui.GangValue != "" && !gangName.Contains(_ui.GangValue)) || (_ui.GangLevelText != "" && !gangLevelText.Contains(_ui.GangLevelText)))
                return;

            //商会信息获取
            shopName = GetShopName(true);
            // 商会
            if (_ui.AShopName != "" && !shopName.Contains(_ui.AShopName))
                return;

            /// <see cref="Patch.DateFile_GetActorFeature_Patch"/>中已经将该方法修改为线程安全
            ActorFeatures = DateFile.instance.GetActorFeature(npcId, true);

            if (_ui.ActorFeatureText != "" && !ScanFeature(_ui.featureSearchSet, _ui.TarFeature, _ui.TarFeatureOr))
                return;
            if (_ui.ActorGongFaText != "" && !ScanGongFa(npcId, _ui.gongFaSearchList, _ui.TarGongFaOr))
                return;
            if (_ui.ActorSkillText != "" && !ScanSkills(_ui.skillSearchList, _ui.TarSkillOr))
                return;
            //初始化角色物品比较耗时间，仅在前面的条件都筛选完了才执行
            //是否搜索过世之人的物品
            bool isDead = int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) > 0;
            if (!isDead || _ui.ItemDead)
            {
                InitActorItems();
            }
            if (_ui.ActorItemText != "" && !ScanItems(_ui.itemSearchList, _ui.TarItemOr))
                return;

            NeededAdd = true;
        }

        /// <summary>
        /// 初始化角色物品
        /// </summary>
        private void InitActorItems()
        {
            if (DateFile.instance.actorItemsDate.TryGetValue(npcId, out var actorItemData))
            {
                actorItems = new Dictionary<(int baseId, int QuquId, int QuquPartId), int>();
                foreach (var itemId in actorItemData.Keys)
                {
                    if (itemId > 0 && DateFile.instance.GetItemNumber(npcId, itemId) > 0)
                    {
                        int baseId = int.Parse(DateFile.instance.GetItemDate(itemId, 999, false));
                        int ququId = int.Parse(DateFile.instance.GetItemDate(itemId, 2002, false));
                        int ququPartId = int.Parse(DateFile.instance.GetItemDate(itemId, 2003, false));
                        var item = (baseId, ququId, ququPartId);
                        if (!actorItems.ContainsKey(item))
                            actorItems.Add(item, itemId);
                    }
                }
            }

            for (int i = 0; i < 12; i++)
            {
                actorEquips = actorEquips ?? new Dictionary<(int baseId, int ququId, int ququPartId), int>();
                actorItems = actorItems ?? new Dictionary<(int baseId, int ququId, int ququPartId), int>();
                int equipId = int.Parse(DateFile.instance.GetActorDate(npcId, 301 + i, false));
                if (equipId > 0)
                {
                    int baseId = int.Parse(DateFile.instance.GetItemDate(equipId, 999, false));
                    int ququId = int.Parse(DateFile.instance.GetItemDate(equipId, 2002, false));
                    int ququPartId = int.Parse(DateFile.instance.GetItemDate(equipId, 2003, false));
                    var item = (baseId, ququId, ququPartId);
                    if (!actorEquips.ContainsKey(item))
                        actorEquips.Add(item, equipId);
                    if (!actorItems.ContainsKey(item))
                        actorItems.Add(item, equipId);
                }
            }
        }

        /// <summary>
        /// 获取婚姻状况
        /// </summary>
        /// <param name="color">是否带颜色</param>
        /// <returns>婚姻状况</returns>
        public int GetMarriage()
        {
            if (intCache[3] > -1)
                return intCache[3];

            List<int> actorSocial = DateFile.instance.GetActorSocial(npcId, 309, false, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(npcId, 309, true, false);
            int result;
            if (actorSocial.Count <= 0)
            {
                /// 判断是否可以说媒, 参考<see cref="MassageWindow.SetMassageWindow"/>中的
                /// case -9006中的Add("900600012")时满足的条件，900600012：男媒女约的事件ID
                // 同道及太吾本人
                List<int> familyList = DateFile.instance.GetFamily(false);
                // 同道里有对的人
                bool hasRightOne = false;
                foreach (var actorId in familyList)
                {
                    int mainActorId = DateFile.instance.MianActorID();
                    // 对方年龄太小或者太吾想给自己说媒
                    if (Age < ConstValue.actorMinAge || actorId == mainActorId) continue;
                    // 同道好感度不够
                    if (DateFile.instance.GetActorFavor(false, mainActorId, actorId, true) < 4) continue;
                    // 同道年龄太小
                    if (int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) <= ConstValue.actorMinAge) continue;
                    int gender = int.Parse(DateFile.instance.GetActorDate(actorId, 14, false));
                    // 同性
                    if (gender == int.Parse(DateFile.instance.GetActorDate(npcId, 14, false))) continue;
                    // 是否禁婚
                    bool abstention = int.Parse(DateFile.instance.presetGangGroupDateValue[GangValueId][803]) == 0;
                    if (abstention) continue;
                    if (int.Parse(DateFile.instance.GetActorDate(actorId, 2, false)) != 0) continue;
                    if (int.Parse(DateFile.instance.GetActorDate(npcId, 2, false)) != 0) continue;
                    if (DateFile.instance.GetLifeDate(actorId, 601, 0) == DateFile.instance.GetLifeDate(npcId, 601, 0))
                        continue;
                    if (DateFile.instance.GetLifeDate(actorId, 602, 0) == DateFile.instance.GetLifeDate(npcId, 602, 0))
                        continue;
                    // 对方是同道的义父母
                    if (DateFile.instance.GetActorSocial(actorId, 304).Contains(npcId)) continue;
                    // 对方与同道义结金兰
                    if (DateFile.instance.GetActorSocial(actorId, 308).Contains(npcId)) continue;
                    if (DateFile.instance.GetActorSocial(actorId, 311).Contains(npcId)) continue;
                    // 对方已经结婚了
                    if (DateFile.instance.GetActorSocial(actorId, 309).Count > 0) continue;

                    hasRightOne = true;
                }
                if (hasRightOne)
                {
                    result = 4; // 可以说媒
                }
                else if (actorSocial2.Count <= 0)
                {
                    result = 1; // 未婚
                }
                else
                {
                    result = 3; // 丧偶
                }
            }
            else
            {
                result = 2; // 已婚
            }
            intCache[3] = result;
            return result;
        }

        /// <summary>
        /// 获取婚姻状况
        /// </summary>
        /// <param name="color">是否带颜色</param>
        /// <returns>婚姻状况</returns>
        private string GetMarriageText()
        {
            int spouse = GetMarriage();

            switch (spouse)
            {
                default:
                    return DateFile.instance.SetColoer(20004, "未婚", false);
                case 2:
                    return DateFile.instance.SetColoer(20010, "已婚", false);
                case 3:
                    return DateFile.instance.SetColoer(20007, "丧偶", false);
                case 4:
                    return DateFile.instance.SetColoer(20009, "可媒", false);
            }
        }


        /// <summary>
        /// 技艺资质成长
        /// </summary>
        /// <param name="color">是否带颜色</param>
        /// <returns></returns>
        public string GetSkillDevelopText(bool color = false)
        {
            if (!color && stringCache[1] != null)
                return stringCache[1];

            DateFile instance = DateFile.instance;
            int skillDevelop = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 551, false)), 2, 4);
            int skillDevelopValue = int.Parse(instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 11, false)), 0, 100)][skillDevelop]);
            var text = instance.massageDate[2002][2].Split('|')[skillDevelop];

            if (skillDevelopValue > 0)
            {
                text += color ? instance.SetColoer(20005, "+" + skillDevelopValue, false) : "+" + skillDevelopValue;
            }
            else if (skillDevelopValue < 0)
            {
                text += color ? instance.SetColoer(20010, "-" + -skillDevelopValue, false) : "+" + skillDevelopValue;
            }
            else
            {
                text += color ? instance.SetColoer(20002, "+" + skillDevelopValue, false) : "+" + skillDevelopValue;
            }

            if (!color)
                stringCache[1] = text;
            return text;
        }

        /// <summary>
        /// 功法资质成长(线程安全)
        /// </summary>
        /// <param name="color">是否带颜色</param>
        /// <returns></returns>
        public string GetGongFaDevelopText(bool color = false)
        {
            if (!color && stringCache[2] != null)
                return stringCache[2];

            int gongFaDevelop = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 651, false)), 2, 4);
            int gongFaDevelopValue = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 11, false)), 0, 100)][gongFaDevelop + 3]);
            var text = DateFile.instance.massageDate[2002][2].Split('|')[gongFaDevelop];
            if (gongFaDevelopValue > 0)
            {
                text += color ? DateFile.instance.SetColoer(20005, "+" + gongFaDevelopValue, false) : "+" + gongFaDevelopValue;
            }
            else if (gongFaDevelopValue < 0)
            {
                text += color ? DateFile.instance.SetColoer(20010, "-" + -gongFaDevelopValue, false) : "+" + gongFaDevelopValue;
            }
            else
            {
                text += color ? DateFile.instance.SetColoer(20002, "+" + gongFaDevelopValue, false) : "+" + gongFaDevelopValue;
            }

            if (!color)
                stringCache[2] = text;
            return text;
        }

        /// <summary>
        /// 健康数值(当前/最大)
        /// </summary>
        /// <returns></returns>
        public int[] GetHealthValue()
        {
            if (healthCache != null)
                return healthCache;

            if (int.Parse(DateFile.instance.GetActorDate(npcId, 8, false)) != 1)
            {
                healthCache = new[] { -1, -1 };
                return null;
            }

            int maxHealth, currentHealth;
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) != 0)
            {
                maxHealth = currentHealth = 0;
            }
            else
            {
                maxHealth = DateFile.instance.MaxHealth(npcId);
                currentHealth = Mathf.Clamp(DateFile.instance.Health(npcId), 0, maxHealth);
            }
            healthCache = new[] { currentHealth, maxHealth };
            return healthCache;
        }

        /// <summary>
        /// 健康
        /// </summary>
        /// <returns></returns>
        private string GetHealth()
        {
            var healthValue = GetHealthValue();
            if (healthValue[0] == -1 || healthValue[1] == -1)
            {
                return "??? / ???";
            }
            return $"{DateFile.instance.Color3(healthValue[0], healthValue[1])}{healthValue[0]}</color> / {healthValue[1]}";
        }

        /// <summary>
        /// 检测是否符合功法搜索条件(线程安全)
        /// </summary>
        /// <param name="searchlist"></param>
        /// <param name="tarGongFaOr"></param>
        /// <returns></returns>
        private static bool ScanGongFa(int actorId, List<int> searchlist, bool tarGongFaOr)
        {
            if (searchlist.Count == 0 || !DateFile.instance.actorGongFas.TryGetValue(actorId, out var gongFas) || gongFas == null || gongFas.Count == 0)
            {
                return false;
            }
            if (!tarGongFaOr)   //与查找
            {
                foreach (int key in searchlist)
                {
                    if (!gongFas.ContainsKey(key))
                        return false;
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchlist)
                {
                    if (gongFas.ContainsKey(key))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 检测是否符合特性搜索条件(线程安全)
        /// </summary>
        /// <param name="searchSet"></param>
        /// <param name="tarFeature"></param>
        /// <param name="tarFeatureOr"></param>
        /// <returns></returns>
        private bool ScanFeature(HashSet<int> searchSet, bool tarFeature, bool tarFeatureOr)
        {
            if (searchSet.Count == 0 || ActorFeatures.Count == 0)
            {
                return false;
            }
            var actorFeatureRule = new HashSet<int>(ActorFeatures.Count);
            foreach (int key in ActorFeatures)
            {
                Features f = Main.featuresList[key];
                if (Main.multinameFeatureGroupIdSet.Contains(f.Group))
                {
                    actorFeatureRule.Add(f.Group);
                }
                else
                {
                    if (!tarFeature)
                    {
                        int category = f.Category;
                        if (category == 3 || category == 4)
                        {
                            //组查找 记录组ID
                            actorFeatureRule.Add(Main.featuresList[key].Group);
                            continue;
                        }
                    }
                    //精确查找记录特性
                    actorFeatureRule.Add(key);
                }
            }

            if (!tarFeatureOr)   //与查找
            {
                foreach (int key in searchSet)
                {
                    if (!actorFeatureRule.Contains(key))
                    {
                        return false;
                    }
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchSet)
                {
                    if (actorFeatureRule.Contains(key))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 检测是否符合技艺搜索条件(线程安全)
        /// </summary>
        /// <param name="searchlist"></param>
        /// <param name="tarSkillsOr"></param>
        /// <returns></returns>
        private bool ScanSkills(List<int> searchlist, bool tarSkillsOr)
        {
            if (searchlist.Count == 0)
            {
                return false;
            }

            if (!tarSkillsOr)   //与查找
            {
                foreach (int key in searchlist)
                {
                    if (!CanTeach(key)) return false;
                    Lock();
                    int maxLevel = 0;
                    try { maxLevel = MessageEventManager.Instance.GetSkillValue(npcId, key + 501); }
                    finally { Unlock(); }
                    if (maxLevel <= 0) return false;
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchlist)
                {
                    if (CanTeach(key))
                    {
                        Lock();
                        int maxLevel = 0;
                        try { maxLevel = MessageEventManager.Instance.GetSkillValue(npcId, key + 501); }
                        finally { Unlock(); }
                        if (maxLevel > 0) return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 检测是否符合物品搜索条件(线程安全)
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="tarItemsOr"></param>
        /// <returns></returns>
        private bool ScanItems(List<int> searchList, bool tarItemsOr)
        {
            if (searchList.Count == 0 || actorItems == null || actorItems.Count == 0)
            {
                return false;
            }

            if (!tarItemsOr)   //与查找
            {
                /// 需要处理同一名字对应多个ID的情况, 同名称对应多个ID在<see cref="UI.GetItemKey"/>中取反编为一组，每组ID用0结尾
                // 上一个baseId是否小于0
                bool lastIdSameGroup = false;
                // 上一组baseId小于零的物品的查找情况(其值只在lastIdNegative为真时有意义)
                bool lastStatusWhenNegative = false;
                foreach (var baseId in searchList)
                {
                    if (baseId < 0)
                    {
                        // 连续的基础ID小于零的物品拥有相同的名称，这部分用OR查找，有一个为真则最终结果为真
                        if (lastIdSameGroup && lastStatusWhenNegative) continue;
                        lastIdSameGroup = true;
                        lastStatusWhenNegative = actorItems.ContainsKey((-baseId, 0, 0));
                    }
                    else if (baseId == 0)
                    {
                        // 0为拥有相同名字的一组物品ID截止符，但是其主要作用还是隔开两组都小于零但是对于名称不同的ID
                        if (lastIdSameGroup && !lastStatusWhenNegative) return false;
                        lastIdSameGroup = false;
                    }
                    else
                    {
                        // 非零则AND查找，先判断上一组基础ID小于零的物品的最终查找结果是否为真，否则直接否决
                        if (!actorItems.ContainsKey((baseId, 0, 0)))
                            return false;
                    }
                }
                return true;
            }
            else                //或查找
            {
                foreach (var baseId in searchList)
                {
                    if (baseId == 0) continue;
                    if (actorItems.ContainsKey((Math.Abs(baseId), 0, 0)))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获得功法、技艺资质数值
        /// </summary>
        /// <param name="index">
        /// 功法 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// 技艺 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </param>
        /// <param name="gongfa">1为功法，0为技艺</param>
        /// <returns></returns>
        public int GetLevelValue(int index, int gongfa)
        {
            // 11-24技艺资质 25-40功法资质
            int cacheIndex = index + 10 + (gongfa == 0 ? 14 : 0);
            if (intCache[cacheIndex] > -1)
                return intCache[cacheIndex];

            int num;
            if (isGetReal)
            {
                num = int.Parse(DateFile.instance.GetActorDate(npcId, 501 + index + 100 * gongfa, false));
                int age = Age;
                if (age <= ConstValue.actorMinAge && age > 0)
                {
                    // 显示年幼者真实资质
                    num = num * (ConstValue.actorMinAge * 100 / age) / 100;
                }
            }
            else
            {
                num = int.Parse(DateFile.instance.GetActorDate(npcId, 501 + index + 100 * gongfa, true));
            }
            intCache[cacheIndex] = num;
            return num;
        }

        /// <summary>
        /// 获得功法、技艺资质文字
        /// </summary>
        /// <param name="index">
        /// 功法 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// 技艺 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </param>
        /// <param name="gongfa">1为功法，0为技艺</param>
        /// <returns></returns>
        private string GetLevel(int index, int gongfa)
        {
            int colorCorrect = 20;
            int levelValue = GetLevelValue(index, gongfa);
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((int)((levelValue - colorCorrect) * 0.1), 0, 8), levelValue.ToString(), false);
            return text;
        }

        /// <summary>
        /// 获取可学功法数量(线程安全)
        /// </summary>
        /// <returns></returns>
        public int[] GetGongFaList()
        {
            if (gongFaResultCache != null)
                return gongFaResultCache;

            int[] resultList = new int[9];
            // 没办法向过世之人请教功法，直接跳过
            bool isDead = int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) > 0;
            if (isDead || !DateFile.instance.actorGongFas.TryGetValue(npcId, out var gongFas) || gongFas == null || gongFas.Count == 0)
            {
                gongFaResultCache = resultList;
                return resultList;
            }

            var dateFile = DateFile.instance;
            // 主角已经会的功法就不算做"可学功法"了
            var myGongFas = new HashSet<int>(dateFile.actorGongFas[dateFile.MianActorID()].Keys);
            foreach (var key in gongFas.Keys)
            {
                if (!myGongFas.Contains(key)
                    && dateFile.gongFaDate.TryGetValue(key, out var gongFaInfo)
                    && gongFaInfo.TryGetValue(2, out var text) && int.TryParse(text, out var prestige))
                {
                    resultList[9 - prestige]++;
                }
            }
            gongFaResultCache = resultList;
            return resultList;
        }

        /// <summary>
        /// 获取可学功法数量的文字(线程安全)
        /// </summary>
        /// <returns></returns>
        private string GetGongFaListText()
        {
            var resultList = GetGongFaList();
            if (resultList == null && resultList.Length < 9)
                return "";

            var result = new string[9];

            for (int i = 0; i < 9; i++)
                result[i] = DateFile.instance.SetColoer(20010 - i, $"{resultList[i]:D2}");

            return string.Join(" | ", result);
        }

        /// <summary>
        /// 获取特性的文字(线程安全)
        /// </summary>
        /// <param name="tarFeature">是否精确特性</param>
        /// <returns></returns>
        private string GetActorFeatureNameText(bool tarFeature)
        {
            if (ActorFeatures.Count == 0)
                return "";

            var featureList = new string[ActorFeatures.Count];
            int index = 0;
            foreach (int key in ActorFeatures)
            {
                Features f = Main.featuresList[key];
                if (!tarFeature)
                {
                    if (f.Category == 3 || f.Category == 4)
                    {
                        // 不要求精确特性时，将与搜索条件中的特性的同类特性全部标识
                        featureList.Add((_ui.featureSearchSet.Contains(f.Group) ? f.TarColor : f.Color)
                            + f.Name + "(" + f.Level + ")</color>", ref index);
                        continue;
                    }
                }
                // 要求精确特性时，只标识将与搜索条件中的特性
                featureList.Add((_ui.featureSearchSet.Contains(key) ? f.TarColor : f.Color)
                    + f.Name + "(" + f.Level + ")</color>", ref index);
            }
            return string.Concat(featureList);
        }

        /// <summary>
        /// 获取前世(线程安全)
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        private string GetSamsaraNames(int npcId)
        {
            List<int> samaras = DateFile.instance.GetLifeDateList(npcId, 801, false);
            if (samaras.Count == 0)
                return "";
            SamsaraCount = samaras.Count;
            var samsaraNames = new string[samaras.Count];
            int index = 0;
            foreach (int samsaraId in samaras)
            {
                samsaraNames.Add(DateFile.instance.GetActorName(samsaraId), ref index);
            }
            return string.Join(" ", samsaraNames);
        }

        /// <summary>
        /// 获取魅力说明
        /// </summary>
        /// <returns></returns>
        private string GetCharmText()
        {
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 11, false)) > 14)
            {
                // 敌人组别，1为大大的良民
                int enemyTeamId = int.Parse(DateFile.instance.GetActorDate(npcId, 8, false));
                // 穿着衣服
                int equipedclothId = int.Parse(DateFile.instance.GetActorDate(npcId, 305, false));
                // 敌人的话就不用考虑是否衣不蔽体了
                if (enemyTeamId != 1 || equipedclothId != 0)
                {
                    // 不同性别有不同的魅力等级描述文字，0男1女
                    int genderIndex = int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)) - 1;
                    // 魅力等级
                    int charmLevel = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 15, true)) / 100, 0, 9);
                    return DateFile.instance.massageDate[25][genderIndex].Split('|')[charmLevel];
                }
                else
                {
                    // 衣不蔽体
                    return DateFile.instance.massageDate[25][5].Split('|')[1];
                }
            }
            else
            {
                // 年幼
                return DateFile.instance.massageDate[25][5].Split('|')[0];
            }
        }

        /// <summary>
        /// 获取地点
        /// </summary>
        /// <returns>地点</returns>
        private string GetPlace()
        {
            string place;
            // 敌人组别，1为大大的良民
            int enemyTeamId = int.Parse(DateFile.instance.GetActorDate(npcId, 8, false));
            if (enemyTeamId != 1)
            {
                // "身处未知之地"
                place = DateFile.instance.massageDate[8010][3].Split(new char[] { '|' })[1];
            }
            else
            {
                List<int> list = DateFile.instance.GetActorAtPlace(npcId);
                if (list != null && list.Count >= 2)
                {
                    int num = int.Parse(DateFile.instance.partWorldMapDate[list[0]][98]);
                    place = string.Format("{0}{1}({2},{3})",
                            // 地块大类
                            DateFile.instance.GetNewMapDate(list[0], list[1], 98),
                            // 地块小类
                            DateFile.instance.GetNewMapDate(list[0], list[1], 0),
                            // 坐标
                            list[1] % num,
                            list[1] / num);
                }
                else
                {
                    place = "未知地点";
                }
            }
            return place;
        }

        /// <summary>
        /// 商会信息获取(线程安全)
        /// </summary>
        /// <param name="gangLevelTextId">身份描述文字索引</param>
        /// <param name="color">是否带颜色</param>
        /// <returns></returns>
        public string GetShopName(bool color = false)
        {
            int gangId = int.Parse(DateFile.instance.GetActorDate(npcId, 9, false));
            int shopType = int.Parse(DateFile.instance.GetGangDate(gangId, 16));
            string shopName = DateFile.instance.storyShopDate[shopType][0];
            if (!color)
                return shopName;
            int gangLevelTextId = (GangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)));
            // 如果不是真正的商人, 将隐藏的商会类型名称标为灰色
            shopName = DateFile.instance.presetGangGroupDateValue[GangValueId][gangLevelTextId] == "商人"
                ? DateFile.instance.SetColoer(20006, shopName, false)
                : DateFile.instance.SetColoer(20002, shopName, false);
            return shopName;
        }

        /// <summary>
        /// 获取可学技艺等级加权评级
        /// </summary>
        /// <returns></returns>
        public int GetMaxSkillGrade()
        {
            int result = 0;
            if (skillMaxCache != null)
            {
                foreach (var value in skillMaxCache)
                {
                    result += value << (value - 1);
                }
            }
            else
            {
                skillMaxCache = new List<int>();
                foreach (int key in DateFile.instance.baseSkillDate.Keys)
                {
                    if (key < 100 && CanTeach(key))
                    {
                        int typ = key + 501;
                        int maxLevel = Mathf.Min(MessageEventManager.Instance.GetSkillValue(npcId, typ), 8);
                        if (maxLevel > 0)
                        {
                            skillMaxCache.Add(maxLevel);
                            result += maxLevel << (maxLevel - 1);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取可学技艺名称及其等级的文字
        /// </summary>
        /// <returns></returns>
        private string GetMaxSkillLevelText()
        {
            skillMaxCache = new List<int>();
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) > 0)
                return "";

            var result = new List<string>();
            foreach (var pair in DateFile.instance.baseSkillDate)
            {
                if (pair.Key < 100 && CanTeach(pair.Key))
                {
                    int typ = pair.Key + 501;
                    int maxLevel = MessageEventManager.Instance.GetSkillValue(npcId, typ);
                    if (maxLevel > 0)
                    {
                        maxLevel = Mathf.Min(maxLevel, 9);
                        skillMaxCache.Add(maxLevel);
                        result.Add(DateFile.instance.SetColoer(20001 + maxLevel, $"{pair.Value[0]}{10 - maxLevel}"));
                    }
                }
            }
            return string.Join(" ", result);
        }

        /// <summary>
        /// 根据NPC的门派或职业,判断能否传授你这生活艺能(线程安全)
        /// </summary>
        /// <param name="skillId">技艺ID(对应资质ID减去501)</param>
        /// <returns></returns>
        /// 借鉴自人物浮动信息MOD
        public bool CanTeach(int skillId)
        {
            string taughtId = DateFile.instance.presetGangGroupDateValue[GangValueId][818];

            if (taughtId == "0")
                return false;

            string eventID;
            string[] eventIDs;

            if (skillId >= 0 && skillId <= 5)
                eventID = (931900001 + skillId).ToString();
            else if (skillId == 6 || skillId == 7)
                eventID = (932200001 + skillId - 6).ToString();
            else if (skillId == 8 || skillId == 9)
                eventID = (932900001 + skillId - 8).ToString();
            else if (skillId == 10 || skillId == 11)
                eventID = (932200003 + skillId - 10).ToString();
            else if (skillId >= 12 && skillId <= 15)
                eventID = (932300001 + skillId - 12).ToString();
            else
                return false;

            switch (taughtId)
            {
                case "901000006": // 学习复合技艺
                    eventIDs = DateFile.instance.presetGangGroupDateValue[GangValueId][813].Split('|');
                    break;
                case "901300004": // 请教才艺
                case "901300007": // 请教手艺
                case "901300008": // 请教杂艺
                    int messageID = int.Parse(DateFile.instance.eventDate[int.Parse(taughtId)][7]);
                    eventIDs = DateFile.instance.eventDate[messageID][5].Split('|');
                    break;
                default: // 学习單一技艺 or 大夫
                    eventIDs = taughtId.Split('|');
                    break;
            }
            return eventIDs.Contains(eventID);
        }

        /// <summary>
        /// 人物身上的最佳物品获取+装备+搜索的物品
        /// </summary>
        /// <returns></returns>
        public Item[] GetItems()
        {
            if (itemResultCache != null)
                return itemResultCache;

            // 使用hashset防止加入相同名称的物品(基础ID、促织ID和促织部位ID共同唯一确定任何物品的品阶和名称)
            var items = new HashSet<(int BaseId, int QuquId, int QuquPartId)>();
            if (actorItems != null && actorItems.Count > 0)
            {
                int bestGrade = 0;
                // 添加最佳物品
                foreach (var item in actorItems)
                {
                    if (int.Parse(DateFile.instance.GetItemDate(item.Value, 98, false)) == 86)
                        continue; //跳过伏虞剑及其碎片
                    int grade = int.Parse(DateFile.instance.GetItemDate(item.Value, 8, false));
                    if (grade > bestGrade)
                    {
                        bestGrade = grade;
                        items.Clear();
                        items.Add(item.Key);
                    }
                    else if (grade == bestGrade)
                    {
                        items.Add(item.Key);
                    }
                }
            }

            // 添加装备
            if (actorEquips != null && actorEquips.Count > 0)
                items.UnionWith(actorEquips.Keys);

            // 添加要搜索的物品
            if (_ui.itemSearchSet.Count > 0)
            {
                foreach (var baseIds in _ui.itemSearchSet)
                {
                    if (actorItems.ContainsKey((baseIds, 0, 0)))
                        items.Add((baseIds, 0, 0));
                }
            }
            itemResultCache = new Item[items.Count];
            int index = 0;
            foreach (var item in items)
            {
                var newitem = new Item(actorItems[item]);
                itemResultCache.Add(newitem, ref index);
            }
            // 逆序排
            Array.Sort(itemResultCache, (a, b) => -a.CompareTo(b));
            // 使命完成，释放内存
            actorItems = actorEquips = null;
            return itemResultCache;
        }

        /// <summary>
        /// 人物身上的最佳物品获取+装备+搜索的物品的文字
        /// </summary>
        /// <returns></returns>
        private string GetItemsText()
        {
            var items = GetItems();
            return items.Length == 0 ? "" : string.Join(", ", items.Select(GetItemColorName));
        }

        /// <summary>
        /// 获得带品级颜色的物品名称
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        private string GetItemColorName(Item item)
        {
            if (_ui.itemSearchSet.Contains(item.BaseId))
                return $"<color=red>{item.Name}</color>";
            return DateFile.instance.SetColoer(20001 + item.Grade, item.Name);
        }

        /// <summary>
        /// 线程锁加锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Lock()
        {
            while (Interlocked.CompareExchange(ref locker, 1, 0) == 1) ;
        }

        /// <summary>
        /// 线程锁解锁
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unlock() => Interlocked.Exchange(ref locker, 0);

        /// <summary>
        /// 比较大小
        /// </summary>
        /// <param name="other">要比较的另一个<see cref="ActorItem"/>实例</param>
        /// <returns></returns>
        public int CompareTo(ActorItem other)
        {
            if (other == null)
                return 1;
            return npcId.CompareTo(other.npcId);
        }

        /// <summary>
        /// 是否相同
        /// </summary>
        /// <param name="other">要比较的另一个<see cref="ActorItem"/>实例</param>
        /// <returns></returns>
        public bool Equals(ActorItem other)
        {
            if (other == null)
                return false;
            return npcId.Equals(other.npcId);
        }

        public override int GetHashCode() => npcId.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is ActorItem actorItem)
            {
                return Equals(actorItem);
            }
            return false;
        }
    }
}
