using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace NpcScan
{
    /// <summary>
    /// 角色信息类
    /// </summary>
    internal class ActorItem
    {
        #region 私有域
        /// <summary>UI索引</summary>
        private readonly UI _ui;
        /// <summary>NPC的ID</summary>
        private readonly int npcId;
        /// <summary>是否获取基础值</summary>
        private readonly bool isGetReal;
        /// <summary>是否是排行榜模式</summary>
        private readonly bool isRank;
        /// <summary>门派名称</summary>
        private readonly string gangName;
        /// <summary>门派身份等级文字</summary>
        private readonly string gangLevelText;
        /// <summary>商会</summary>
        private readonly string shopName;
        /*----------------------------------------
        以下缓存对应的数据计算方法无法多线程计算，只在UI
        渲染时才会按需计算，使用缓存可以减少排序时的计算量
        ------------------------------------------*/
        /// <summary>整型属性值缓存 0-5六维 6魅力 7立场 8金钱 9综合评分 10-25功法资质 26-39技艺资质</summary>
        /// <remarks>缓存一些计算量大的整型数值，如<see cref="DateFile.GetActorDate(int, int, bool)"/> bool为true时计算量较大</remarks>
        private readonly int[] intCache = new int[40];
        /// <summary>字符串属性值缓存 0 地点坐标 1婚姻(无色) 2技艺资质成长(无色) 3功法资质成长(无色)</summary>
        private readonly string[] stringCache = new string[4];
        /// <summary>健康值缓存</summary>
        private int[] healthCache;
        /// <summary>七元赋性缓存</summary>
        private int[] actorResourcesCache;
        /// <summary>可教技能最大等级缓存</summary>
        private List<int> skillMaxCache;
        /// <summary>可教功法数量缓存</summary>
        private int[] gongFaResultCache;
        /// <summary>搜索结果渲染缓存</summary>
        private string[] addItemCache;
        /*----------------------------------------*/
        /// <summary>线程锁</summary>
        private int locker;
        /// <summary>是否需要添加</summary>
        private bool isNeededAdd;
        #endregion

        #region 私有属性(只在需要时才执行其中的方法，提高搜索性能)
        /// <summary>姓名</summary>
        public string ActorName { get; private set; }
        /// <summary>膂力</summary>
        public int Str => intCache[0] = intCache[0] > -1 ? intCache[0] : int.Parse(DateFile.instance.GetActorDate(npcId, 61, !isGetReal));

        /// <summary>体质</summary>
        public int Con => intCache[1] = intCache[1] > -1 ? intCache[1] : int.Parse(DateFile.instance.GetActorDate(npcId, 62, !isGetReal));

        /// <summary>灵敏</summary>
        public int Agi => intCache[2] = intCache[2] > -1 ? intCache[2] : int.Parse(DateFile.instance.GetActorDate(npcId, 63, !isGetReal));
        /// <summary>根骨</summary>
        public int Bon => intCache[3] = intCache[3] > -1 ? intCache[3] : int.Parse(DateFile.instance.GetActorDate(npcId, 64, !isGetReal));
        /// <summary>悟性</summary>
        public int Inv => intCache[4] = intCache[4] > -1 ? intCache[4] : int.Parse(DateFile.instance.GetActorDate(npcId, 65, !isGetReal));
        /// <summary>定力</summary>
        public int Pat => intCache[5] = intCache[5] > -1 ? intCache[5] : int.Parse(DateFile.instance.GetActorDate(npcId, 66, !isGetReal));
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
        /// <summary>魅力</summary>
        public int Charm => intCache[6] = intCache[6] > -1 ? intCache[6] : int.Parse(DateFile.instance.GetActorDate(npcId, 15, !isGetReal));
        /// <summary>轮回</summary>
        public int SamsaraCount => DateFile.instance.GetLifeDateList(npcId, 801, false).Count;
        /// <summary>健康</summary>
        public int Health => int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) == 0 ? ActorMenu.instance.Health(npcId) : 0;
        /// <summary>坐标</summary>
        public string Place => stringCache[0] = stringCache[0] ?? GetPlace();
        /// <summary>魅力</summary>
        public string CharmText => GetCharmText();
        /// <summary>前世</summary>
        public string SamsaraNames { get; private set; }
        /// <summary>立场数值</summary>
        public int Goodness => intCache[7] = intCache[7] > -1 ? intCache[7] : DateFile.instance.GetActorGoodness(npcId);
        /// <summary>立场</summary>
        public string GoodnessText => DateFile.instance.massageDate[9][0].Split('|')[Goodness];
        /// <summary>门派ID</summary>
        public int Groupid => int.Parse(DateFile.instance.GetActorDate(npcId, 19, false));
        /// <summary>身份等级(负数时为因配偶身份获得的等级)</summary>
        public int GangLevel => int.Parse(DateFile.instance.GetActorDate(npcId, 20, false));
        /// <summary>金钱</summary>
        public int Money => intCache[8] = intCache[8] > -1 ? intCache[8] : ActorMenu.instance.ActorResource(npcId)[5];
        /// <summary>身份等级对应的数据ID</summary>
        public int GangValueId => DateFile.instance.GetGangValueId(Groupid, GangLevel);
        /// <summary>七元赋性</summary>
        public int[] ActorResources => actorResourcesCache = actorResourcesCache ?? ActorMenu.instance.GetActorResources(npcId);

        /// <summary>角色特性</summary>
        public List<int> ActorFeatures { get; private set; }
        /// <summary>综合评价</summary>
        public int Totalrank
        {
            get
            {
                if (intCache[9] > -1)
                    return intCache[9];
                var tmp = Str * _ui.StrValue + Con * _ui.ConValue + Agi * _ui.AgiValue + Bon * _ui.BonValue + Inv * _ui.IntValue + Pat * _ui.PatValue;
                tmp += Charm * _ui.CharmValue;
                for (int tmpi = 0; tmpi < 14; tmpi++)
                    tmp += GetLevelValue(tmpi, 1) * _ui.Gongfa[tmpi];
                for (int tmpi = 0; tmpi < 16; tmpi++)
                    tmp += GetLevelValue(tmpi, 0) * _ui.Skill[tmpi];
                intCache[9] = tmp;
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
            isRank = _ui.Rankmode ? true : false;
            ActorName = DateFile.instance.GetActorName(npcId);
            SamsaraNames = GetSamsaraNames();

            gangName = DateFile.instance.GetGangDate(Groupid, 0);
            // 地位名称ID (注：因亲属关系可能获得相应地位改变)
            int key2 = (GangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)));
            // 身份gangLevelText
            gangLevelText = DateFile.instance.SetColoer((GangValueId != 0) ? (20011 - Mathf.Abs(GangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[GangValueId][key2], false);

            //商会信息获取
            shopName = GetShopName(key2, true);
            /// <see cref="Patch.DateFile_GetActorFeature_Patch"/>中已经将该方法修改为线程安全
            ActorFeatures = DateFile.instance.GetActorFeature(npcId);
            // 初始化缓存
            for (int i = 0; i < intCache.Length; i++)
                intCache[i] = -1;
        }

        /// <summary>
        /// 获取每行搜索结果
        /// </summary>
        /// <returns></returns>
        public string[] GetAddItem()
        {
            // 之前已经处理过则不需要再次处理
            if (addItemCache != null)
                return addItemCache;
            if (!isNeededAdd)
                return null;
            int index = 0;
            // 使用array而非List减少copy次数提升渲染性能
            var additem = new string[isRank ? 62 : 61];
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
            additem.Add(GetSpouse(true), ref index);
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
            additem.Add(ActorMenu.instance.ActorResource(npcId)[5].ToString(), ref index);
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
            // 前世
            additem.Add(SamsaraNames, ref index);
            // 特性
            additem.Add(GetActorFeatureNameText(_ui.TarFeature), ref index);
            addItemCache = additem;
            return additem;
        }

        /// <summary>
        /// 添加的检查：是否符合搜索条件(线程安全)
        /// </summary>
        public bool AddCheck()
        {
            // 997为人物模板, 当大于100是特殊剧情人物，跳过不处理, 详见TextAsset中的PresetActor_Date
            // 如: 2001:莫女 2002:大岳瑶常 2003:九寒 2004:金凰儿 2005:衣以候 2006:卫起 2007:以向 2008:血枫 2009:术方
            if (int.Parse(DateFile.instance.actorsDate[npcId][997]) > 100) return false;

            if (_ui.Minage > 0)
            {
                Lock();
                var result = Age < _ui.Minage;
                Unlock();
                if (result) return false;
            }
            if (_ui.HealthValue > 0)
            {
                Lock();
                var result = Health < _ui.HealthValue;
                Unlock();
                if (result) return false;
            }
            if (_ui.SamsaraCount > 0)
            {
                Lock();
                var result = SamsaraCount < _ui.SamsaraCount;
                Unlock();
                if (result) return false;
            }
            if (_ui.Maxage > 0)
            {
                Lock();
                var result = Age > _ui.Maxage;
                Unlock();
                if (result) return false;
            }
            if (_ui.GenderValue > 0)
            {
                Lock();
                var result = Gender != _ui.GenderValue;
                Unlock();
                if (result) return false;
            }

            // 排行榜模式以下搜索条件无效
            // 我至今不知道排行榜模式有啥用？都能排序了都。
            if (!isRank)
            {
                if (_ui.IntValue > 0)
                {
                    Lock();
                    var result = Inv < _ui.IntValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.StrValue > 0)
                {
                    Lock();
                    var result = Str < _ui.StrValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.ConValue > 0)
                {
                    Lock();
                    var result = Con < _ui.ConValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.AgiValue > 0)
                {
                    Lock();
                    var result = Agi < _ui.AgiValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.BonValue > 0)
                {
                    Lock();
                    var result = Bon < _ui.BonValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.PatValue > 0)
                {
                    Lock();
                    var result = Pat < _ui.PatValue;
                    Unlock();
                    if (result) return false;
                }
                if (_ui.CharmValue > 0)
                {
                    Lock();
                    var result = Charm < _ui.CharmValue;
                    Unlock();
                    if (result) return false;
                }
                for (int i = 0; i < 14; i++)
                {
                    if (_ui.Gongfa[i] > 0)
                    {
                        Lock();
                        var result = GetLevelValue(i, 1) < _ui.Gongfa[i];
                        Unlock();
                        if (result) return false;

                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    if (_ui.Skill[i] > 0)
                    {
                        Lock();
                        var result = GetLevelValue(i, 0) < _ui.Skill[i];
                        Unlock();
                        if (result) return false;
                    }
                }
            }

            if (_ui.ActorFeatureText != "" && !ScanFeature(Main.findSet, _ui.TarFeature, _ui.TarFeatureOr))
                return false;
            if (_ui.ActorGongFaText != "" && !ScanGongFa(Main.gongFaList, _ui.TarGongFaOr))
                return false;
            if (_ui.ActorSkillText != "" && !ScanSkills(Main.skillList, _ui.TarSkillOr))
                return false;

            // gangLevel 门派地位 若为负 则地位由婚姻带来的。
            if (_ui.HighestLevel > 1)
            {
                Lock();
                var result = Mathf.Abs(GangLevel) < _ui.HighestLevel;
                Unlock();
                if (result) return false;
            }

            // 如果未开启门派搜索 直接通过
            if (_ui.TarIsGang && _ui.IsGang)
            {
                Lock();
                var result = Groupid > 15;
                Unlock();
                if (result) return false;
            }

            if (_ui.Goodness > -1)
            {
                Lock();
                var result = Goodness != _ui.Goodness;
                Unlock();
                if (result) return false;
            }

            // 姓名
            if (!(ActorName.Contains(_ui.AName) || SamsaraNames.Contains(_ui.AName)))
                return false;

            // 从属和地位
            if (!gangName.Contains(_ui.GangValue) || !gangLevelText.Contains(_ui.GangLevelValue))
                return false;

            // 商会
            if (_ui.AShopName != "" && !shopName.Contains(_ui.AShopName))
                return false;

            isNeededAdd = true;
            return true;
        }


        /// <summary>
        /// 获取婚姻状况
        /// </summary>
        /// <param name="actorId">NpcId</param>
        /// <returns>婚姻状况</returns>
        public string GetSpouse(bool color = false)
        {
            if (!color && stringCache[1] != null)
                return stringCache[1];

            List<int> actorSocial = DateFile.instance.GetActorSocial(npcId, 309, false, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(npcId, 309, true, false);
            string result;
            if (actorSocial2.Count == 0)
            {
                result = color ? DateFile.instance.SetColoer(20004, "未婚", false) : "未婚";
            }
            else if (actorSocial.Count == 0)
            {
                result = color ? DateFile.instance.SetColoer(20007, "丧偶", false) : "丧偶";
            }
            else
            {
                result = color ? DateFile.instance.SetColoer(20010, "已婚", false) : "已婚";
            }
            if (!color)
                stringCache[1] = result;
            return result;
        }

        /// <summary>
        /// 技艺资质成长
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public string GetSkillDevelopText(bool color = false)
        {
            if (!color && stringCache[2] != null)
                return stringCache[2];

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
                stringCache[2] = text;
            return text;
        }

        /// <summary>
        /// 功法资质成长(线程安全)
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public string GetGongFaDevelopText(bool color = false)
        {
            if (!color && stringCache[3] != null)
                return stringCache[3];

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
                stringCache[3] = text;
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
                maxHealth = ActorMenu.instance.MaxHealth(npcId);
                currentHealth = Mathf.Clamp(ActorMenu.instance.Health(npcId), 0, maxHealth);
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
            return $"{ActorMenu.instance.Color3(healthValue[0], healthValue[1])}{healthValue[0]}</color> / {healthValue[1]}";
        }

        /// <summary>
        /// 检测是否符合功法搜索条件(线程安全)
        /// </summary>
        /// <param name="searchlist"></param>
        /// <param name="tarGongFaOr"></param>
        /// <returns></returns>
        private bool ScanGongFa(List<int> searchlist, bool tarGongFaOr)
        {
            if (searchlist.Count == 0 || !DateFile.instance.actorGongFas.TryGetValue(npcId, out var gongFas) || gongFas == null || gongFas.Count == 0)
            {
                return false;
            }
            var actorGongFas = new HashSet<int>(gongFas.Keys);
            if (!tarGongFaOr)   //与查找
            {
                foreach (int key in searchlist)
                {
                    if (!actorGongFas.Contains(key))
                        return false;
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchlist)
                {
                    if (actorGongFas.Contains(key))
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
            var actorFeature = new HashSet<int>(ActorFeatures.Count);
            foreach (int key in ActorFeatures)
            {
                if (!tarFeature)
                {
                    int plus = Main.featuresList[key].Plus;
                    if (plus == 3 || plus == 4)
                    {
                        //组查找 记录组ID
                        actorFeature.Add(Main.featuresList[key].Group);
                        continue;
                    }
                }
                //精确查找记录特性
                actorFeature.Add(key);
            }

            if (!tarFeatureOr)   //与查找
            {
                foreach (int key in searchSet)
                {
                    if (!actorFeature.Contains(key))
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
                    if (actorFeature.Contains(key))
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
                    if (!CanTeach(key))
                        return false;
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchlist)
                {
                    if (CanTeach(key))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获得功法、技艺资质数值
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="index">
        /// 功法 0:内功;1:身法;2:绝技;3:拳掌;4:指法;5:腿法;6:暗器;7:剑法;8:刀法;9:长兵;10:奇门;11:软兵;12:御射;13:乐器;
        /// 技艺 0:音律;1:弈棋;2:诗书;3:绘画;4:术数;5:品鉴;6:锻造;7:制木;8:医术;9:毒术;10:织锦;11:巧匠;12:道法;13:佛学;14:厨艺;15:杂学;
        /// </param>
        /// <param name="gongfa">1为功法，0为技艺</param>
        /// <returns></returns>
        public int GetLevelValue(int index, int gongfa)
        {
            if (intCache[index + 9 + 16 * gongfa] > -1)
                return intCache[index + 9 + 16 * gongfa];

            int num;
            if (isGetReal)
            {
                num = int.Parse(DateFile.instance.GetActorDate(npcId, 501 + index + 100 * gongfa, false));
                int age = int.Parse(DateFile.instance.GetActorDate(npcId, 11, false));
                if (age <= 14 && age > 0)
                {
                    num = num * (1400 / age) / 100;
                }
            }
            else
            {
                num = int.Parse(DateFile.instance.GetActorDate(npcId, 501 + index + 100 * gongfa, true));
            }
            intCache[index + 9 + 16 * gongfa] = num;
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

            if (!DateFile.instance.actorGongFas.TryGetValue(npcId, out var gongFas) || gongFas == null || gongFas.Count == 0)
                return null;

            int[] resultList = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var instance = DateFile.instance;

            var myGongFas = new HashSet<int>(instance.actorGongFas[instance.MianActorID()].Keys);
            foreach (var key in gongFas.Keys)
            {
                if (!myGongFas.Contains(key)
                    && instance.gongFaDate.TryGetValue(key, out var gongFaInfo)
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
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetGongFaListText()
        {
            var resultList = GetGongFaList();
            if (resultList == null)
                return "";

            var result = new StringBuilder();

            for (int i = 0; i < 9; i++)
            {
                result.Append(DateFile.instance.SetColoer(20010 - i, $"{resultList[i]:D2}"));
                if (i != 8)
                    result.Append(" | ");
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取特性的文字(线程安全)
        /// </summary>
        /// <param name="tarFeature"></param>
        /// <returns></returns>
        private string GetActorFeatureNameText(bool tarFeature)
        {
            if (ActorFeatures.Count == 0)
                return "";

            var text = new StringBuilder();
            foreach (int key in ActorFeatures)
            {
                Features f = Main.featuresList[key];
                string s = f.Level.ToString();
                if (!tarFeature)
                {
                    if (f.Plus == 3 || f.Plus == 4)
                    {
                        text.Append(Main.findSet.Contains(f.Group) ? f.TarColor : f.Color)
                            .Append(f.Name + "(" + s + ")</color>");
                        continue;
                    }
                }

                text.Append(Main.findSet.Contains(key) ? f.TarColor : f.Color)
                    .Append(f.Name + "(" + s + ")</color>");
            }
            return text.ToString();
        }

        /// <summary>
        /// 获取前世(线程安全)
        /// </summary>
        /// <returns></returns>
        private string GetSamsaraNames()
        {
            var samaras = DateFile.instance.GetLifeDateList(npcId, 801, false);
            if (samaras.Count == 0)
                return "";

            var samsaraNames = new StringBuilder();
            foreach (int samsaraId in samaras)
            {
                samsaraNames.Append(" ").Append(DateFile.instance.GetActorName(samsaraId));
            }
            return samsaraNames.ToString(); ;
        }

        /// <summary>
        /// 获取魅力说明
        /// </summary>
        /// <returns></returns>
        private string GetCharmText()
        {
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 11, false)) > 14)
            {
                if (int.Parse(DateFile.instance.GetActorDate(npcId, 8, false)) != 1
                    || int.Parse(DateFile.instance.GetActorDate(npcId, 305, false)) != 0)
                {
                    return DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)) - 1]
                        .Split('|')[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(npcId, 15, true)) / 100, 0, 9)];
                }
                else
                {
                    return DateFile.instance.massageDate[25][5].Split('|')[1];
                }
            }
            else
            {
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
            if (int.Parse(DateFile.instance.GetActorDate(npcId, 8, false)) != 1)
            {
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
        /// <param name="key2"></param>
        /// <returns></returns>
        public string GetShopName(int key2 = -1, bool color = false)
        {
            int key = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(npcId, 9, false)), 16));
            string shopName = DateFile.instance.storyShopDate[key][0];
            if (!color)
                return shopName;
            if (key2 < 0)
                key2 = (GangLevel >= 0) ? 1001 : (1001 + Gender);
            shopName = DateFile.instance.presetGangGroupDateValue[GangValueId][key2] == "商人"
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
                    result += value << value;
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
                        int maxLevel = Mathf.Min(MassageWindow.instance.GetSkillValue(npcId, typ), 8);
                        skillMaxCache.Add(maxLevel);
                        result += maxLevel << maxLevel;
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
            var result = new StringBuilder();
            skillMaxCache = new List<int>();
            foreach (var pair in DateFile.instance.baseSkillDate)
            {
                if (pair.Key < 100 && CanTeach(pair.Key))
                {
                    int typ = pair.Key + 501;
                    int maxLevel = Mathf.Min(MassageWindow.instance.GetSkillValue(npcId, typ), 8);
                    skillMaxCache.Add(maxLevel);
                    var name = pair.Value[0][0];
                    if (result.Length != 0)
                    {
                        result.Append(" ");
                    }
                    result.Append(DateFile.instance.SetColoer(20002 + maxLevel, $"{name}{9 - maxLevel}"));
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 根据NPC的门派或职业,判断能否传授你这生活艺能(线程安全)
        /// </summary>
        /// <param name="skillId"></param>
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
        /// 线程锁加锁
        /// </summary>
        private void Lock()
        {
            while (Interlocked.CompareExchange(ref locker, 1, 0) == 1) ;
        }

        /// <summary>
        /// 线程锁解锁
        /// </summary>
        private void Unlock() => Interlocked.Exchange(ref locker, 0);
    }
}
