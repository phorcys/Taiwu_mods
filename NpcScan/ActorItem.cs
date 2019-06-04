using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NpcScan
{
    public class ActorItem
    {
        private NpcScan.UI _ui;
        public bool isNeedAdd;

        /// <summary>
        /// 是否是排行榜模式
        /// </summary>
        private bool isRank;

        /// <summary>
        /// NPC的ID
        /// </summary>
        private int npcId;

        /// <summary>
        /// 综合评价
        /// </summary>
        private int totalrank;

        private int str;
        private int con;
        private int agi;
        private int bon;
        private int inv;
        private int pat;

        private int age;
        private int gender;
        private string genderText;
        private int charm;
        private int samsara;
        private int health;
        //private int cv;

        private string place;
        private string actorName;
        private string charmText;
        private string samsaraNames;

        private string gn;
        private int groupid;
        private int gangLevel;
        private int gangValueId;
        private string gangLevelText;

        private string shopName;

        private int[] actorResources;

        public ActorItem(int npcId, NpcScan.UI ui)
        {
            this._ui = ui;
            this.npcId = npcId;

            this.isRank = _ui.rankmode ? true : false;

            this.str = int.Parse(DateFile.instance.GetActorDate(npcId, 61, !_ui.getreal));
            this.con = int.Parse(DateFile.instance.GetActorDate(npcId, 62, !_ui.getreal));
            this.agi = int.Parse(DateFile.instance.GetActorDate(npcId, 63, !_ui.getreal));
            this.bon = int.Parse(DateFile.instance.GetActorDate(npcId, 64, !_ui.getreal));
            this.inv = int.Parse(DateFile.instance.GetActorDate(npcId, 65, !_ui.getreal));
            this.pat = int.Parse(DateFile.instance.GetActorDate(npcId, 66, !_ui.getreal));

            this.age = int.Parse(DateFile.instance.GetActorDate(npcId, 11, false));
            this.gender = int.Parse(DateFile.instance.GetActorDate(npcId, 14, false));
            this.genderText = this.gender == 1 ? "男" : "女";
            this.charm = int.Parse(DateFile.instance.GetActorDate(npcId, 15, !_ui.getreal));
            this.samsara = DateFile.instance.GetLifeDateList(npcId, 801, false).Count;
            this.health = int.Parse(DateFile.instance.GetActorDate(npcId, 26, false)) == 0 ? ActorMenu.instance.Health(npcId) : 0;
            //this.cv = ui.charmValue == 0 ? 0 : -999;

            this.place = _getPlace(npcId);
            this.actorName = DateFile.instance.GetActorName(npcId);
            this.charmText = _getCharmText(npcId);
            this.samsaraNames = _getSamsaraNames(npcId);

            this.gn = DateFile.instance.massageDate[9][0].Split(new char[] { '|' })[DateFile.instance.GetActorGoodness(npcId)];
            //门派ID
            this.groupid = int.Parse(DateFile.instance.GetActorDate(npcId, 19, false));
            //身份等级
            this.gangLevel = int.Parse(DateFile.instance.GetActorDate(npcId, 20, false));
            this.gangValueId = DateFile.instance.GetGangValueId(groupid, gangLevel);
            //性别标识
            int key2 = (gangLevel >= 0) ? 1001 : (1001 + int.Parse(DateFile.instance.GetActorDate(npcId, 14, false)));
            //身份gangLevelText
            this.gangLevelText = DateFile.instance.SetColoer((gangValueId != 0) ? (20011 - Mathf.Abs(gangLevel)) : 20002, DateFile.instance.presetGangGroupDateValue[gangValueId][key2], false);

            //商会信息获取
            this.shopName = _getShopName(npcId,key2);

            actorResources = ActorMenu.instance.GetActorResources(npcId);

            if (isRank)
            {
                int totalrank = str * _ui.strValue + con * _ui.conValue + agi * _ui.agiValue + bon * _ui.bonValue + inv * _ui.intValue + pat * _ui.patValue;
                totalrank += charm * _ui.charmValue;
                for (int tmpi = 0; tmpi < 14; tmpi++)
                    totalrank += GetLevelValue(npcId, tmpi, 1) * _ui.gongfa[tmpi];
                for (int tmpi = 0; tmpi < 16; tmpi++)
                    totalrank += GetLevelValue(npcId, tmpi, 0) * _ui.life[tmpi];
            }

            AddCheck();
        }

        public string[] GetAddItem()
        {
            if (!isNeedAdd) return null;

            List<string> additem = new List<string>();

            if (isRank) { additem.Add(totalrank.ToString()); }
            additem.Add(actorName+ "(" + npcId + ")");
            additem.Add(age.ToString());
            additem.Add(genderText);
            additem.Add(place);

            //魅力
            additem.Add(charm + "(" + charmText + ")");
            //从属gangText
            additem.Add(DateFile.instance.GetGangDate(groupid, 0));
            //身份gangLevelText
            additem.Add(gangLevelText);
            //商会
            additem.Add(shopName);
            //立场goodnessText
            additem.Add(gn);
            additem.Add(GetSpouse(npcId));
            additem.Add(GetSkillDevelopText(npcId));
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
            // 前世
            additem.Add(samsaraNames);
            // 特性
            additem.Add(GetActorFeatureNameText(npcId, _ui.tarFeature));

            return additem.ToArray();
        }

        /// <summary>
        /// 添加的检查：是否符合搜索条件
        /// </summary>
        private void AddCheck()
        {
            // 997真实值判断。 如果是boss（相枢分身）直接返回
            // 真实ID为200开头 则为boss PS: 2001:莫女 2002:大岳瑶常 2003:九寒 2004:金凰儿 2005:衣以候 2006:卫起 2007:以向 2008:血枫 2009:术方
            if (DateFile.instance.actorsDate[this.npcId][997].StartsWith("200")) return;

            if (this.age < _ui.minage) return;
            if (this.health < _ui.healthValue) return;
            if (this.samsara < _ui.samsaraCount) return;
            if (_ui.maxage != 0 && this.age > _ui.maxage) return;
            if (_ui.genderValue != 0 && this.gender != _ui.genderValue) return;

            // 排行榜模式以下搜索条件无效
            // 我至今不知道排行榜模式有啥用？都能排序了都。
            if (!isRank)
            {
                if (this.inv < _ui.intValue) return;
                if (this.str < _ui.strValue) return;
                if (this.con < _ui.conValue) return;
                if (this.agi < _ui.agiValue) return;
                if (this.bon < _ui.bonValue) return;
                if (this.pat < _ui.patValue) return;
                //if (this.charm < this.cv) return;
                if (this.charm < _ui.charmValue && _ui.charmValue != 0) return;

                if (GetLevelValue(this.npcId, 0, 1) < _ui.gongfa[0]) return;
                if (GetLevelValue(this.npcId, 1, 1) < _ui.gongfa[1]) return;
                if (GetLevelValue(this.npcId, 1, 1) < _ui.gongfa[1]) return;
                if (GetLevelValue(this.npcId, 2, 1) < _ui.gongfa[2]) return;
                if (GetLevelValue(this.npcId, 3, 1) < _ui.gongfa[3]) return;
                if (GetLevelValue(this.npcId, 4, 1) < _ui.gongfa[4]) return;
                if (GetLevelValue(this.npcId, 5, 1) < _ui.gongfa[5]) return;
                if (GetLevelValue(this.npcId, 6, 1) < _ui.gongfa[6]) return;
                if (GetLevelValue(this.npcId, 7, 1) < _ui.gongfa[7]) return;
                if (GetLevelValue(this.npcId, 8, 1) < _ui.gongfa[8]) return;
                if (GetLevelValue(this.npcId, 9, 1) < _ui.gongfa[9]) return;
                if (GetLevelValue(this.npcId, 10, 1) < _ui.gongfa[10]) return;
                if (GetLevelValue(this.npcId, 11, 1) < _ui.gongfa[11]) return;
                if (GetLevelValue(this.npcId, 12, 1) < _ui.gongfa[12]) return;
                if (GetLevelValue(this.npcId, 13, 1) < _ui.gongfa[13]) return;
                if (GetLevelValue(this.npcId, 0, 0) < _ui.life[0]) return;
                if (GetLevelValue(this.npcId, 1, 0) < _ui.life[1]) return;
                if (GetLevelValue(this.npcId, 2, 0) < _ui.life[2]) return;
                if (GetLevelValue(this.npcId, 3, 0) < _ui.life[3]) return;
                if (GetLevelValue(this.npcId, 4, 0) < _ui.life[4]) return;
                if (GetLevelValue(this.npcId, 5, 0) < _ui.life[5]) return;
                if (GetLevelValue(this.npcId, 6, 0) < _ui.life[6]) return;
                if (GetLevelValue(this.npcId, 7, 0) < _ui.life[7]) return;
                if (GetLevelValue(this.npcId, 8, 0) < _ui.life[8]) return;
                if (GetLevelValue(this.npcId, 9, 0) < _ui.life[9]) return;
                if (GetLevelValue(this.npcId, 10, 0) < _ui.life[10]) return;
                if (GetLevelValue(this.npcId, 11, 0) < _ui.life[11]) return;
                if (GetLevelValue(this.npcId, 12, 0) < _ui.life[12]) return;
                if (GetLevelValue(this.npcId, 13, 0) < _ui.life[13]) return;
                if (GetLevelValue(this.npcId, 14, 0) < _ui.life[14]) return;
                if (GetLevelValue(this.npcId, 15, 0) < _ui.life[15]) return;
            }

            if (_ui.actorFeatureText != "" && !ScanFeature(this.npcId, Main.findList, _ui.tarFeature, _ui.tarFeatureOr))
                return;
            if (_ui.actorGongFaText != "" && !ScanGongFa(this.npcId, Main.GongFaList, _ui.tarGongFaOr))
                return;

            // gangLevel 门派地位 若为负 则地位由婚姻带来的。
            if (Mathf.Abs(gangLevel) < _ui.highestLevel)
                return;

            // 如果未开启门派搜索 直接通过
            // (groupid >= 1 && groupid <= 15)结果为门派中人
            // _ui.isGang = true 仅搜索门派：(groupid >= 1 && groupid <= 15)=false 排除
            // _ui.isGang = false 仅搜索非门派：(groupid >= 1 && groupid <= 15)=true 排除
            if (_ui.tarIsGang && (_ui.isGang != (groupid > 0 && groupid < 16)))
                return;

            if (!_ui.goodnessText.Equals("全部") && !gn.Contains(_ui.goodnessText))
                return;

            // 懒
            if (!((actorName.Contains(_ui.aName) || samsaraNames.Contains(_ui.aName)) && (DateFile.instance.GetGangDate(groupid, 0).Contains(_ui.gangValue)) && (gangLevelText.Contains(_ui.gangLevelValue))))
                return;

            // 商会
            if (_ui.aShopName != "" && !shopName.Contains(_ui.aShopName))
                return;

            this.isNeedAdd = true;
        }

        /// <summary>
        /// 获取婚姻状况
        /// </summary>
        /// <param name="id">NpcId</param>
        /// <returns>婚姻状况</returns>
        public string GetSpouse(int id)
        {
            List<int> actorSocial = DateFile.instance.GetActorSocial(id, 309, false, false);
            List<int> actorSocial2 = DateFile.instance.GetActorSocial(id, 309, true, false);
            bool flag = actorSocial2.Count == 0;
            string result;
            if (flag)
            {
                result = DateFile.instance.SetColoer(20004, "未婚", false);
            }
            else
            {
                bool flag2 = actorSocial.Count == 0;
                if (flag2)
                {
                    result = DateFile.instance.SetColoer(20007, "丧偶", false);
                }
                else
                {
                    result = DateFile.instance.SetColoer(20010, "已婚", false);
                }
            }

            return result;
        }
        private string GetSkillDevelopText(int key)
        {
            int num = DateFile.instance.MianActorID();
            int num2 = Mathf.Max(int.Parse(DateFile.instance.GetActorDate(key, 551, false)), 2);
            int num3 = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 11, false)), 0, 100)][num2]);
            string text = ((num2 == 0) ? DateFile.instance.massageDate[7006][0] : string.Format("{0} {1}", DateFile.instance.massageDate[2002][2].Split(new char[]
            {
            '|'
            })[num2], (num3 <= 0) ? ((num3 != 0) ? DateFile.instance.SetColoer(20010, "-" + Mathf.Abs(num3), false) : DateFile.instance.SetColoer(20002, "+" + num3, false)) : DateFile.instance.SetColoer(20005, "+" + num3, false)));
            return text;
        }
        private string GetGongFaDevelopText(int key)
        {
            int num = DateFile.instance.MianActorID();
            int num2 = Mathf.Max(int.Parse(DateFile.instance.GetActorDate(key, 651, false)), 2);
            int num3 = int.Parse(DateFile.instance.ageDate[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 11, false)), 0, 100)][num2 + 3]);
            string text = ((num2 == 0) ? DateFile.instance.massageDate[7006][0] : string.Format("{0} {1}", DateFile.instance.massageDate[2002][2].Split(new char[]
            {
            '|'
            })[num2], (num3 <= 0) ? ((num3 != 0) ? DateFile.instance.SetColoer(20010, "-" + Mathf.Abs(num3), false) : DateFile.instance.SetColoer(20002, "+" + num3, false)) : DateFile.instance.SetColoer(20005, "+" + num3, false)));
            return text;
        }
        private string GetHealth(int key, int value = 0)
        {
            int num = ActorMenu.instance.Health(key);
            int num2 = ActorMenu.instance.MaxHealth(key);
            if (value > 0 && num2 - num > value)
            {
                value = Mathf.Max(value / 5, 1);
            }
            int num3 = Mathf.Clamp(num + value, 0, num2);
            if (int.Parse(DateFile.instance.GetActorDate(key, 26, false)) != 0)
            {
                num2 = num3 = 0;
            }
            DateFile.instance.actorsDate[key][12] = num3.ToString();
            if (int.Parse(DateFile.instance.GetActorDate(key, 8, false)) != 1)
            {
                return "??? / ???";
            }
            else
            {
                return string.Format("{0}{1}</color> / {2}", ActorMenu.instance.Color3(num3, num2), num3, num2);
            }
        }
        private bool ScanGongFa(int key, List<int> slist, bool tarGongFaOr)
        {
            if (!DateFile.instance.actorGongFas.ContainsKey(key))
            {
                return false;
            }

            List<int> list = new List<int>(DateFile.instance.actorGongFas[key].Keys);
            bool result = false;
            if (slist.Count == 0)
            {
                return true;
            }

            if (!tarGongFaOr)   //与查找
            {
                if (slist.All(t => list.Any(b => b == t)))
                {
                    result = true;
                }
            }
            else                //或查找
            {
                foreach (int i in list)
                {
                    if (slist.Contains(i))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        private bool ScanFeature(int key, List<Features> slist, bool tarFeature, bool tarFeatureOr)
        {
            List<int> list = new List<int>(DateFile.instance.GetActorFeature(key));
            bool result = false;
            if (slist.Count == 0)
            {
                return result;
            }
            List<Features> actorFeature = new List<Features>();
            foreach (int i in list)
            {
                if (tarFeature)  //精确查找记录特性
                {
                    actorFeature.Add(Main.featuresList[i]);
                }
                else            //组查找 记录组ID
                {
                    Features f = Main.featuresList[i];
                    int j = f.Group;
                    actorFeature.Add(Main.featuresList[j]);
                }
            }

            if (!tarFeatureOr)   //与查找
            {
                if (slist.All(t => actorFeature.Any(b => b.Key == t.Key)))
                {
                    result = true;
                }
            }
            else                //或查找
            {
                foreach (Features f in actorFeature)
                {
                    if (slist.Contains(f))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        private int GetLevelValue(int id, int index, int gongfa)
        {
            int num;
            if (_ui.getreal)
            {
                num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, false));
                int age = int.Parse(DateFile.instance.GetActorDate(id, 11, false));
                if (age < 14 && age > 0)
                {
                    num = num * (1400 / age) / 100;
                }
            }
            else
            {
                num = int.Parse(DateFile.instance.GetActorDate(id, 501 + index + 100 * gongfa, true));
            }
            return num;
        }
        private string GetLevel(int id, int index, int gongfa)
        {
            int colorCorrect = 40;
            int num = GetLevelValue(id, index, gongfa);
            string text = DateFile.instance.SetColoer(20002 + Mathf.Clamp((num - colorCorrect) / 10, 0, 8), num.ToString(), false);
            return text;
        }
        private string GetGongFaListText(int key)
        {
            string result = "";
            List<int> myList = DateFile.instance.actorGongFas[DateFile.instance.MianActorID()].Keys.ToList();
            List<int> list = new List<int>();
            if (DateFile.instance.actorGongFas.ContainsKey(key))
            {
                list.AddRange(DateFile.instance.actorGongFas[key].Keys);
            }
            int[] resultList = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (int i in list)
            {
                if (myList.All(t => t != i))
                {
                    if (DateFile.instance.gongFaDate.ContainsKey(i) && DateFile.instance.gongFaDate[i].ContainsKey(2))
                        resultList[9 - int.Parse(DateFile.instance.gongFaDate[i][2])]++;
                }
            }
            for (int i = 0; i < 9; i++)
            {
                result += DateFile.instance.SetColoer(20010 - i, string.Format("{0:D2}", resultList[i]));
                if (i != 8)
                    result += " | ";
            }
            return result;
        }
        private string GetActorFeatureNameText(int key, bool tarFeature)
        {
            List<int> list = new List<int>(DateFile.instance.GetActorFeature(key));
            string text = "";
            for (int i = 0; i < list.Count; i++)
            {
                int j = list[i];
                Features f = Main.featuresList[j];
                if (f.Group == 2001 || f.Group == 3024) continue;
                string s = f.Level.ToString();
                if (tarFeature)
                {
                    text += (Main.findList.Contains(f) ? f.tarColor : f.Color) + f.Name + "(" + s + ")</color>";
                }
                else
                {
                    text += (Main.findList.Contains(Main.featuresList[f.Group]) ? f.tarColor : f.Color) + f.Name + "(" + s + ")</color>";
                }
            }
            //Main.Logger.Log(text);
            return text;
        }

        private string _getSamsaraNames(int index)
        {
            string samsaraNames = string.Empty;
            foreach (int samsaraId in DateFile.instance.GetLifeDateList(index, 801, false))
            {
                samsaraNames += " " + DateFile.instance.GetActorName(samsaraId);
            }
            return samsaraNames;
        }

        private string _getCharmText(int index)
        {
            return ((int.Parse(DateFile.instance.GetActorDate(index, 11, false)) > 14) ? ((int.Parse(DateFile.instance.GetActorDate(index, 8, false)) != 1 || int.Parse(DateFile.instance.GetActorDate(index, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(index, 14, false)) - 1].Split(new char[]
             {
                            '|'
             })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(index, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
             {
                            '|'
             })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
             {
                            '|'
             })[0]);
        }

        /// <summary>
        /// 获取地点
        /// </summary>
        /// <param name="index">人物ID</param>
        /// <returns>地点</returns>
        private string _getPlace(int index)
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
                    place = string.Format("{0}{1}({2},{3})", new object[]
                    {
                            DateFile.instance.GetNewMapDate(list[0], list[1], 98),
                            DateFile.instance.GetNewMapDate(list[0], list[1], 0),
                            list[1]% num,
                            list[1]/ num
                    });
                }
                else
                {
                    place = "未知地点";
                }
            }
            return place;
        }

        private string _getShopName(int index,int key2)
        {
            int key = int.Parse(DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate(index, 9, false)), 16));
            string shopName = DateFile.instance.storyShopDate[key][0];
            bool flag6 = DateFile.instance.presetGangGroupDateValue[gangValueId][key2] == "商人";
            if (flag6)
            {
                shopName = DateFile.instance.SetColoer(20006, shopName, false);
            }
            else
            {
                shopName = DateFile.instance.SetColoer(20002, shopName, false);
            }

            return shopName;
        }
    }
}
