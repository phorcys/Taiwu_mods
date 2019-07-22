using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NpcScan
{
    internal class ActorItem
    {
        private readonly UI _ui;
        public bool isNeedAdd;

        /// <summary>是否是排行榜模式</summary>
        private readonly bool isRank;

        /// <summary>NPC的ID</summary>
        private readonly int npcId;

        /// <summary>综合评价</summary>
        private readonly int totalrank;
        /// <summary>膂力</summary>
        private readonly int str;
        /// <summary>体质</summary>
        private readonly int con;
        /// <summary>灵敏</summary>
        private readonly int agi;
        /// <summary>根骨</summary>
        private readonly int bon;
        /// <summary>悟性</summary>
        private readonly int inv;
        /// <summary>定力</summary>
        private readonly int pat;
        /// <summary>年龄</summary>
        private readonly int age;
        /// <summary>性别</summary>
        private readonly int gender;
        /// <summary>性别文字</summary>
        private readonly string genderText;
        /// <summary>魅力</summary>
        private readonly int charm;
        /// <summary>轮回</summary>
        private readonly int samsara;
        /// <summary>健康</summary>
        private readonly int health;
        //private int cv;

        /// <summary></summary>
        private readonly string place;
        /// <summary></summary>
        private readonly string actorName;
        /// <summary></summary>
        private readonly string charmText;
        /// <summary>前世</summary>
        private readonly string samsaraNames;

        /// <summary>立场</summary>
        private readonly string gn;
        /// <summary>门派ID</summary>
        private readonly int groupid;
        /// <summary>身份等级</summary>
        private readonly int gangLevel;
        /// <summary>门派身份数值ID</summary>
        private readonly int gangValueId;
        /// <summary></summary>
        private readonly string gangLevelText;

        /// <summary></summary>
        private readonly string shopName;

        /// <summary></summary>
        private readonly int[] actorResources;

        public ActorItem(int npcId, UI ui)
        {
            _ui = ui;
            this.npcId = npcId;

            isRank = _ui.Rankmode ? true : false;

            str = int.Parse(DateFile.instance.GetActorDate(npcId, 61, !_ui.IsGetReal));
            con = int.Parse(DateFile.instance.GetActorDate(npcId, 62, !_ui.IsGetReal));
            agi = int.Parse(DateFile.instance.GetActorDate(npcId, 63, !_ui.IsGetReal));
            bon = int.Parse(DateFile.instance.GetActorDate(npcId, 64, !_ui.IsGetReal));
            inv = int.Parse(DateFile.instance.GetActorDate(npcId, 65, !_ui.IsGetReal));
            pat = int.Parse(DateFile.instance.GetActorDate(npcId, 66, !_ui.IsGetReal));

            age = int.Parse(DateFile.instance.GetActorDate(npcId, 11, false));
            gender = int.Parse(DateFile.instance.GetActorDate(npcId, 14, false));
            genderText = gender == 1 ? "男" : "女";
            charm = int.Parse(DateFile.instance.GetActorDate(npcId, 15, !_ui.IsGetReal));
            samsara = DateFile.instance.GetLifeDateList(npcId, 801, false).Count;
            health = int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) == 0 ? ActorMenu.instance.Health(npcId) : 0;
            //this.cv = ui.charmValue == 0 ? 0 : -999;

            place = GetPlace(npcId);
            actorName = DateFile.instance.GetActorName(npcId);
            charmText = GetCharmText(npcId);
            samsaraNames = GetSamsaraNames(npcId);

            gn = DateFile.instance.massageDate[9][0].Split('|')[DateFile.instance.GetActorGoodness(npcId)];
            groupid = int.Parse(DateFile.instance.GetActorDate(npcId, 19, false));
            gangLevel = int.Parse(DateFile.instance.GetActorDate(npcId, 20, false));
            gangValueId = DateFile.instance.GetGangValueId(groupid, gangLevel);
            // 地位名称ID (注：因亲属关系可能获得相应地位改变)
            int key2 = (gangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)));
            // 身份gangLevelText
            gangLevelText = DateFile.instance.SetColoer((gangValueId != 0) ? (20011 - Mathf.Abs(gangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[gangValueId][key2], false);

            //商会信息获取
            shopName = GetShopName(npcId, key2);

            actorResources = ActorMenu.instance.GetActorResources(npcId);

            if (isRank)
            {
                totalrank = str * _ui.StrValue + con * _ui.ConValue + agi * _ui.AgiValue + bon * _ui.BonValue + inv * _ui.IntValue + pat * _ui.PatValue;
                totalrank += charm * _ui.CharmValue;
                for (int tmpi = 0; tmpi < 14; tmpi++)
                    totalrank += GetLevelValue(npcId, tmpi, 1) * _ui.Gongfa[tmpi];
                for (int tmpi = 0; tmpi < 16; tmpi++)
                    totalrank += GetLevelValue(npcId, tmpi, 0) * _ui.Life[tmpi];
            }

            AddCheck();
        }

        /// <summary>
        /// 获取每行搜索结果
        /// </summary>
        /// <returns></returns>
        public string[] GetAddItem()
        {
            if (!isNeedAdd)
                return null;

            var additem = new List<string>();

            if (isRank)
                additem.Add(totalrank.ToString());

            additem.Add($"{actorName}({npcId})");
            additem.Add(age.ToString());
            additem.Add(genderText);
            additem.Add(place);

            //魅力
            additem.Add($"{charm}({charmText})");
            //从属gangText
            additem.Add(DateFile.instance.GetGangDate(groupid, 0));
            //身份gangLevelText
            additem.Add(gangLevelText);
            //商会
            additem.Add(shopName);
            //立场goodnessText
            additem.Add(gn);
            //婚姻
            additem.Add(GetSpouse(npcId));
            //技艺资质成长
            additem.Add(GetSkillDevelopText(npcId));
            //功法资质成长
            additem.Add(GetGongFaDevelopText(npcId));
            //健康
            additem.Add(GetHealth(npcId));

            additem.Add(str.ToString());
            additem.Add(con.ToString());
            additem.Add(agi.ToString());
            additem.Add(bon.ToString());
            additem.Add(inv.ToString());
            additem.Add(pat.ToString());
            additem.Add(GetLevel(npcId, 0, 1));
            additem.Add(GetLevel(npcId, 1, 1));
            additem.Add(GetLevel(npcId, 2, 1));
            additem.Add(GetLevel(npcId, 3, 1));
            additem.Add(GetLevel(npcId, 4, 1));
            additem.Add(GetLevel(npcId, 5, 1));
            additem.Add(GetLevel(npcId, 6, 1));
            additem.Add(GetLevel(npcId, 7, 1));
            additem.Add(GetLevel(npcId, 8, 1));
            additem.Add(GetLevel(npcId, 9, 1));
            additem.Add(GetLevel(npcId, 10, 1));
            additem.Add(GetLevel(npcId, 11, 1));
            additem.Add(GetLevel(npcId, 12, 1));
            additem.Add(GetLevel(npcId, 13, 1));
            additem.Add(GetLevel(npcId, 0, 0));
            additem.Add(GetLevel(npcId, 1, 0));
            additem.Add(GetLevel(npcId, 2, 0));
            additem.Add(GetLevel(npcId, 3, 0));
            additem.Add(GetLevel(npcId, 4, 0));
            additem.Add(GetLevel(npcId, 5, 0));
            additem.Add(GetLevel(npcId, 6, 0));
            additem.Add(GetLevel(npcId, 7, 0));
            additem.Add(GetLevel(npcId, 8, 0));
            additem.Add(GetLevel(npcId, 9, 0));
            additem.Add(GetLevel(npcId, 10, 0));
            additem.Add(GetLevel(npcId, 11, 0));
            additem.Add(GetLevel(npcId, 12, 0));
            additem.Add(GetLevel(npcId, 13, 0));
            additem.Add(GetLevel(npcId, 14, 0));
            additem.Add(GetLevel(npcId, 15, 0));
            // 银钱
            additem.Add(ActorMenu.instance.ActorResource(npcId)[5].ToString());
            // 细腻
            additem.Add(actorResources[0].ToString());
            // 聪颖
            additem.Add(actorResources[1].ToString());
            // 水性
            additem.Add(actorResources[2].ToString());
            // 勇壮
            additem.Add(actorResources[3].ToString());
            // 坚毅
            additem.Add(actorResources[4].ToString());
            // 冷静
            additem.Add(actorResources[5].ToString());
            // 福源
            additem.Add(actorResources[6].ToString());
            // 可学功法
            additem.Add(GetGongFaListText(npcId));
            // 可学技艺
            additem.Add(GetMaxSkillLevel(npcId));
            // 前世
            additem.Add(samsaraNames);
            // 特性
            additem.Add(GetActorFeatureNameText(npcId, _ui.TarFeature));
            return additem.ToArray();
        }

        /// <summary>
        /// 添加的检查：是否符合搜索条件
        /// </summary>
        private void AddCheck()
        {
            // 997真实值判断。 如果是boss（相枢分身）直接返回
            // 真实ID为200开头 则为boss PS: 2001:莫女 2002:大岳瑶常 2003:九寒 2004:金凰儿 2005:衣以候 2006:卫起 2007:以向 2008:血枫 2009:术方
            if (DateFile.instance.actorsDate[npcId][997].StartsWith("200")) return;

            if (age < _ui.Minage) return;
            if (health < _ui.HealthValue) return;
            if (samsara < _ui.SamsaraCount) return;
            if (_ui.Maxage != 0 && age > _ui.Maxage) return;
            if (_ui.GenderValue != 0 && gender != _ui.GenderValue) return;

            // 排行榜模式以下搜索条件无效
            // 我至今不知道排行榜模式有啥用？都能排序了都。
            if (!isRank)
            {
                if (inv < _ui.IntValue) return;
                if (str < _ui.StrValue) return;
                if (con < _ui.ConValue) return;
                if (agi < _ui.AgiValue) return;
                if (bon < _ui.BonValue) return;
                if (pat < _ui.PatValue) return;
                //if (this.charm < this.cv) return;
                if (charm < _ui.CharmValue && _ui.CharmValue != 0) return;

                if (GetLevelValue(npcId, 0, 1) < _ui.Gongfa[0]) return;
                if (GetLevelValue(npcId, 1, 1) < _ui.Gongfa[1]) return;
                if (GetLevelValue(npcId, 1, 1) < _ui.Gongfa[1]) return;
                if (GetLevelValue(npcId, 2, 1) < _ui.Gongfa[2]) return;
                if (GetLevelValue(npcId, 3, 1) < _ui.Gongfa[3]) return;
                if (GetLevelValue(npcId, 4, 1) < _ui.Gongfa[4]) return;
                if (GetLevelValue(npcId, 5, 1) < _ui.Gongfa[5]) return;
                if (GetLevelValue(npcId, 6, 1) < _ui.Gongfa[6]) return;
                if (GetLevelValue(npcId, 7, 1) < _ui.Gongfa[7]) return;
                if (GetLevelValue(npcId, 8, 1) < _ui.Gongfa[8]) return;
                if (GetLevelValue(npcId, 9, 1) < _ui.Gongfa[9]) return;
                if (GetLevelValue(npcId, 10, 1) < _ui.Gongfa[10]) return;
                if (GetLevelValue(npcId, 11, 1) < _ui.Gongfa[11]) return;
                if (GetLevelValue(npcId, 12, 1) < _ui.Gongfa[12]) return;
                if (GetLevelValue(npcId, 13, 1) < _ui.Gongfa[13]) return;
                if (GetLevelValue(npcId, 0, 0) < _ui.Life[0]) return;
                if (GetLevelValue(npcId, 1, 0) < _ui.Life[1]) return;
                if (GetLevelValue(npcId, 2, 0) < _ui.Life[2]) return;
                if (GetLevelValue(npcId, 3, 0) < _ui.Life[3]) return;
                if (GetLevelValue(npcId, 4, 0) < _ui.Life[4]) return;
                if (GetLevelValue(npcId, 5, 0) < _ui.Life[5]) return;
                if (GetLevelValue(npcId, 6, 0) < _ui.Life[6]) return;
                if (GetLevelValue(npcId, 7, 0) < _ui.Life[7]) return;
                if (GetLevelValue(npcId, 8, 0) < _ui.Life[8]) return;
                if (GetLevelValue(npcId, 9, 0) < _ui.Life[9]) return;
                if (GetLevelValue(npcId, 10, 0) < _ui.Life[10]) return;
                if (GetLevelValue(npcId, 11, 0) < _ui.Life[11]) return;
                if (GetLevelValue(npcId, 12, 0) < _ui.Life[12]) return;
                if (GetLevelValue(npcId, 13, 0) < _ui.Life[13]) return;
                if (GetLevelValue(npcId, 14, 0) < _ui.Life[14]) return;
                if (GetLevelValue(npcId, 15, 0) < _ui.Life[15]) return;
            }

            if (_ui.ActorFeatureText != "" && !ScanFeature(npcId, Main.findSet, _ui.TarFeature, _ui.TarFeatureOr))
                return;
            if (_ui.ActorGongFaText != "" && !ScanGongFa(npcId, Main.gongFaList, _ui.TarGongFaOr))
                return;
            if (_ui.ActorSkillText != "" && !ScanSkills(npcId, Main.skillList, _ui.TarSkillOr))
                return;

            // gangLevel 门派地位 若为负 则地位由婚姻带来的。
            if (Mathf.Abs(gangLevel) < _ui.HighestLevel)
                return;

            // 如果未开启门派搜索 直接通过
            // (groupid >= 1 && groupid <= 15)结果为门派中人
            // _ui.isGang = true 仅搜索门派：(groupid >= 1 && groupid <= 15)=false 排除
            // _ui.isGang = false 仅搜索非门派：(groupid >= 1 && groupid <= 15)=true 排除
            if (_ui.TarIsGang && (_ui.IsGang != (groupid > 0 && groupid < 16)))
                return;

            if (!_ui.GoodnessText.Equals("全部") && !gn.Contains(_ui.GoodnessText))
                return;

            // 姓名
            if (!(actorName.Contains(_ui.AName) || samsaraNames.Contains(_ui.AName)))
                return;

            // 从属和地位
            if (!DateFile.instance.GetGangDate(groupid, 0).Contains(_ui.GangValue) || !gangLevelText.Contains(_ui.GangLevelValue))
                return;

            // 商会
            if (_ui.AShopName != "" && !shopName.Contains(_ui.AShopName))
                return;

            isNeedAdd = true;
        }

        /// <summary>
        /// 获取婚姻状况
        /// </summary>
        /// <param name="actorId">NpcId</param>
        /// <returns>婚姻状况</returns>
        public string GetSpouse(int actorId)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(actorId, 309, false, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(actorId, 309, true, false);
            string result;
            if (actorSocial2.Count == 0)
            {
                result = DateFile.instance.SetColoer(20004, "未婚", false);
            }
            else if (actorSocial.Count == 0)
            {
                result = DateFile.instance.SetColoer(20007, "丧偶", false);
            }
            else
            {
                result = DateFile.instance.SetColoer(20010, "已婚", false);
            }

            return result;
        }

        /// <summary>
        /// 技艺资质成长
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetSkillDevelopText(int actorId)
        {
            DateFile instance = DateFile.instance;
            int skillDevelop = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 551, false)), 2, 4);
            int skillDevelopValue = int.Parse(instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)), 0, 100)][skillDevelop]);
            var text = instance.massageDate[2002][2].Split('|')[skillDevelop];

            if (skillDevelopValue > 0)
            {
                text += instance.SetColoer(20005, "+" + skillDevelopValue, false);
            }
            else if (skillDevelopValue < 0)
            {
                text += instance.SetColoer(20010, "-" + -skillDevelopValue, false);
            }
            else
            {
                text += instance.SetColoer(20002, "+" + skillDevelopValue, false);
            }
            return text;
        }

        /// <summary>
        /// 功法资质成长
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetGongFaDevelopText(int actorId)
        {
            int gongFaDevelop = Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 651, false)), 2, 4);
            int gongFaDevelopValue = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)), 0, 100)][gongFaDevelop + 3]);
            var text = DateFile.instance.massageDate[2002][2].Split('|')[gongFaDevelop];
            if (gongFaDevelopValue > 0)
            {
                text += DateFile.instance.SetColoer(20005, "+" + gongFaDevelopValue, false);
            }
            else if (gongFaDevelopValue < 0)
            {
                text += DateFile.instance.SetColoer(20010, "-" + -gongFaDevelopValue, false);
            }
            else
            {
                text += DateFile.instance.SetColoer(20002, "+" + gongFaDevelopValue, false);
            }

            return text;
        }

        /// <summary>
        /// 健康
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetHealth(int actorId)
        {
            int currentHealth = ActorMenu.instance.Health(actorId);
            int maxHealth = ActorMenu.instance.MaxHealth(actorId);
            int num3 = Mathf.Clamp(currentHealth, 0, maxHealth);
            if (int.Parse(DateFile.instance.GetActorDate(actorId, 26, false)) != 0)
            {
                maxHealth = num3 = 0;
            }
            if (int.Parse(DateFile.instance.GetActorDate(actorId, 8, false)) != 1)
            {
                return "??? / ???";
            }
            else
            {
                return $"{ActorMenu.instance.Color3(num3, maxHealth)}{num3}</color> / {maxHealth}";
            }
        }

        /// <summary>
        /// 检测是否符合功法搜索条件
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="searchlist"></param>
        /// <param name="tarGongFaOr"></param>
        /// <returns></returns>
        private bool ScanGongFa(int actorId, List<int> searchlist, bool tarGongFaOr)
        {
            if (!DateFile.instance.actorGongFas.TryGetValue(actorId, out SortedDictionary<int, int[]> gongFa))
            {
                return false;
            }

            if (gongFa.Keys.Count == 0 || searchlist.Count == 0)
            {
                return false;
            }

            var actorGongFas = new HashSet<int>(gongFa.Keys);

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
        /// 检测是否符合特性搜索条件
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="searchSet"></param>
        /// <param name="tarFeature"></param>
        /// <param name="tarFeatureOr"></param>
        /// <returns></returns>
        private bool ScanFeature(int actorId, HashSet<int> searchSet, bool tarFeature, bool tarFeatureOr)
        {
            List<int> list = DateFile.instance.GetActorFeature(actorId);
            if (list.Count == 0 || searchSet.Count == 0)
            {
                return false;
            }
            var actorFeature = new HashSet<int>(list.Count);
            foreach (int key in list)
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

        private bool ScanSkills(int actorId, List<int> searchlist, bool tarSkillsOr)
        {
            if (searchlist.Count == 0)
            {
                return false;
            }

            if (!tarSkillsOr)   //与查找
            {
                foreach (int key in searchlist)
                {
                    if (!CanTeach(actorId, key))
                        return false;
                }
                return true;
            }
            else                //或查找
            {
                foreach (int key in searchlist)
                {
                    if (CanTeach(actorId, key))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获得功法、技艺资质数值
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="index"></param>
        /// <param name="gongfa">1为功法，0为技艺</param>
        /// <returns></returns>
        private int GetLevelValue(int actorId, int index, int gongfa)
        {
            int num;
            if (_ui.IsGetReal)
            {
                num = int.Parse(DateFile.instance.GetActorDate(actorId, 501 + index + 100 * gongfa, false));
                int age = int.Parse(DateFile.instance.GetActorDate(actorId, 11, false));
                if (age <= 14 && age > 0)
                {
                    num = num * (1400 / age) / 100;
                }
            }
            else
            {
                num = int.Parse(DateFile.instance.GetActorDate(actorId, 501 + index + 100 * gongfa, true));
            }
            return num;
        }

        /// <summary>
        /// 获得功法、技艺资质文字
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <param name="gongfa"></param>
        /// <returns></returns>
        private string GetLevel(int id, int index, int gongfa)
        {
            int colorCorrect = 20;
            int levelValue = GetLevelValue(id, index, gongfa);
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((int)((levelValue - colorCorrect) * 0.1), 0, 8), levelValue.ToString(), false);
            return text;
        }

        /// <summary>
        /// 获取可学功法数量的文字
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetGongFaListText(int actorId)
        {
            var result = new StringBuilder();
            int[] resultList = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var instance = DateFile.instance;
            if (instance.actorGongFas.TryGetValue(actorId, out var gongFas))
            {
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
            }

            for (int i = 0; i < 9; i++)
            {
                result.Append(DateFile.instance.SetColoer(20010 - i, $"{resultList[i]:D2}"));
                if (i != 8)
                    result.Append(" | ");
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取特性的文字
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="tarFeature"></param>
        /// <returns></returns>
        private string GetActorFeatureNameText(int actorId, bool tarFeature)
        {
            List<int> list = DateFile.instance.GetActorFeature(actorId);
            if (list.Count == 0)
                return "";

            var text = new StringBuilder();
            foreach (int key in list)
            {
                Features f = Main.featuresList[key];
                string s = f.Level.ToString();
                if (!tarFeature)
                {
                    if (f.Plus == 3 || f.Plus == 4)
                    {
                        text.Append(Main.findSet.Contains(f.Group) ? f.TarColor : f.Color)
                            .Append(f.Name).Append("(").Append(s).Append(")</color>");
                        continue;
                    }
                }

                text.Append(Main.findSet.Contains(key) ? f.TarColor : f.Color)
                    .Append(f.Name).Append("(").Append(s).Append(")</color>");
            }
            return text.ToString();
        }

        /// <summary>
        /// 获取前世
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetSamsaraNames(int actorId)
        {
            var samaras = DateFile.instance.GetLifeDateList(actorId, 801, false);
            if (samaras.Count == 0)
                return "";

            var samsaraNames = new StringBuilder();
            foreach (int samsaraId in samaras)
            {
                samsaraNames.Append(" ").Append(DateFile.instance.GetActorName(samsaraId));
            }
            return samsaraNames.ToString();
        }

        /// <summary>
        /// 获取魅力说明
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetCharmText(int actorId)
        {
            if (int.Parse(DateFile.instance.GetActorDate(actorId, 11, false)) > 14)
            {
                if (int.Parse(DateFile.instance.GetActorDate(actorId, 8, false)) != 1
                    || int.Parse(DateFile.instance.GetActorDate(actorId, 305, false)) != 0)
                {
                    return DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(actorId, 14, false)) - 1]
                        .Split('|')[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(actorId, 15, true)) / 100, 0, 9)];
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
        /// <param name="index">人物ID</param>
        /// <returns>地点</returns>
        private string GetPlace(int index)
        {
            string place;
            if (int.Parse(DateFile.instance.GetActorDate(index, 8, false)) != 1)
            {
                place = DateFile.instance.massageDate[8010][3].Split(new char[] { '|' })[1];
            }
            else
            {
                List<int> list = DateFile.instance.GetActorAtPlace(index);
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
        /// 商会信息获取
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="key2"></param>
        /// <returns></returns>
        private string GetShopName(int actorId, int key2)
        {
            int key = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(actorId, 9, false)), 16));
            string shopName = DateFile.instance.storyShopDate[key][0];
            if (DateFile.instance.presetGangGroupDateValue[gangValueId][key2] == "商人")
            {
                shopName = DateFile.instance.SetColoer(20006, shopName, false);
            }
            else
            {
                shopName = DateFile.instance.SetColoer(20002, shopName, false);
            }
            return shopName;
        }

        /// <summary>
        /// 获取可学技艺的文字
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        private string GetMaxSkillLevel(int actorId)
        {
            var result = new StringBuilder();
            foreach (int key in DateFile.instance.baseSkillDate.Keys)
            {
                if (key < 100 && CanTeach(actorId, key))
                {
                    int typ = key + 501;
                    int maxLevel = Mathf.Min(MassageWindow.instance.GetSkillValue(actorId, typ), 8);
                    var name = DateFile.instance.baseSkillDate[key][0][0];
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
        /// 根据NPC的门派或职业,判断能否传授你这生活艺能, 
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        /// 借鉴自人物浮动信息MOD
        public static bool CanTeach(int actorId, int skillId)
        {
            int gangValueId = DateFile.instance.GetGangValueId(int.Parse(DateFile.instance.GetActorDate(actorId, 19, false)),
                                                               int.Parse(DateFile.instance.GetActorDate(actorId, 20, false)));
            string taughtId = DateFile.instance.presetGangGroupDateValue[gangValueId][818];

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
                    eventIDs = DateFile.instance.presetGangGroupDateValue[gangValueId][813].Split('|');
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
            return Array.FindIndex(eventIDs, s => s == eventID) > -1;
        }
    }
}
