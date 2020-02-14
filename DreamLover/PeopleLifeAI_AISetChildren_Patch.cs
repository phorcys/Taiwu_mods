using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
	public static class PeopleLifeAI_AISetChildren_Patch
	{
        private static void Debug(string str) 
        {
            Main.Debug("<造孩子> " + str);
        }
		public static bool Prefix(int fatherId, int motherId, int setFather, int setMother, ref bool __result)
		{
			if (!Main.enabled)
			{
				return true;
            }
            int mainActorId = DateFile.instance.MianActorID();

            // 启用功能并且是主角造孩子
            if (Main.settings.启用指定怀孕功能 && (fatherId == mainActorId || motherId == mainActorId))
            {
                Debug("主角尝试发生关系");
                int num2 = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
                int num3 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
                DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
                DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);
                GEvent.OnEvent(eEvents.Copulate, fatherId, motherId);
                Debug("父方生育可能性: " + num2 + " 母方生育可能性: " + num3);
                if (Main.settings.指定生育可能性 ? (Main.settings.生育可能性 <= 0) : (num2 <= 0 || num3 <= 0))
                {
                    Debug("生育不可能，终止");
                    __result = false;
                    return false;
                }
                if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, applyBonus: false)) != 2)
                {
                    Debug("阿拉，母方不是女性，终止");
                    __result = false;
                    return false;
                }
                int var1 = Random.Range(0, 15000);
                if (!DateFile.instance.HaveLifeDate(motherId, 901) && var1 < (Main.settings.指定生育可能性 ? Main.settings.生育可能性 : (num2 * num3)))
                {
                    Debug("生育可能性判定通过，需求 " + var1 + "，结果 " + (Main.settings.指定生育可能性 ? Main.settings.生育可能性 : (num2 * num3)));
                    bool flag = fatherId == mainActorId || motherId == mainActorId;
                    int num4 = 100;
                    int num5 = flag ? 20 : 50;
                    num4 -= DateFile.instance.GetActorSocial(fatherId, 310).Count * num5;
                    num4 -= DateFile.instance.GetActorSocial(motherId, 310).Count * num5;
                    int var2 = Random.Range(0, 100);
                    if (var2 < (Main.settings.指定怀孕概率 ? Main.settings.怀孕概率 : num4))
                    {
                        Debug("怀孕判定通过，需求 " + var2 + "，结果 " + (Main.settings.指定怀孕概率 ? Main.settings.怀孕概率 : num4));
                        DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                        int var3 = Random.Range(0, 100);
                        if (var3 < (Main.settings.指定蛐蛐概率 ? Main.settings.蛐蛐概率 : ((DateFile.instance.getQuquTrun - 100) / 10)))
                        {
                            Debug("异胎判定通过，需求 " + var3 + "，结果 " + (Main.settings.指定蛐蛐概率 ? Main.settings.蛐蛐概率 : ((DateFile.instance.getQuquTrun - 100) / 10)));
                            DateFile.instance.getQuquTrun = 0;
                            DateFile.instance.actorLife[motherId].Add(901, new List<int> { 1042, fatherId, motherId, setFather, setMother});
                        }
                        else
                        {
                            Debug("正常胎儿判定通过");
                            DateFile.instance.actorLife[motherId].Add(901, new List<int> { Random.Range(7, 10), fatherId, motherId, setFather, setMother });
                            DateFile.instance.pregnantFeature.Add(motherId, new string[2] {
                                Characters.GetCharProperty(fatherId, 101),
                                Characters.GetCharProperty(motherId, 101)
                            });
                        }
                        __result = true;
                    }
                    else
                    {
                        Debug("怀孕判定未通过，需求 " + var2 + "，结果 " + (Main.settings.指定怀孕概率 ? Main.settings.怀孕概率 : num4));
                    }
                }
                Debug("生育可能性判定未通过，需求 " + var1 + "，结果 " + (Main.settings.指定生育可能性 ? Main.settings.生育可能性 : (num2 * num3)));
                return false;
            }
            else if(Main.settings.启用防绿功能)
            {
                bool 是否拦截 = false; 
                if (!是否拦截 && Main.settings.和太吾之外的人都不能发生关系)
                {
                    是否拦截 = (fatherId == mainActorId || motherId == mainActorId);
                    if(是否拦截)
                        Debug("拦截发生关系，父方 " + DateFile.instance.GetActorName(fatherId) +
                            "，母方 " + DateFile.instance.GetActorName(motherId));
                }
                if (!是否拦截 && Main.settings.太吾爱慕的人不能与他人发生关系)
                {
                    List<int> 太吾爱慕的人 = new List<int>();
                    foreach(int k in Main.settings.爱慕关系.Keys)
                    {
                        if(Main.settings.爱慕关系[k])
                            太吾爱慕的人.AddRange(DateFile.instance.GetActorSocial(mainActorId, k));
                    }
                    是否拦截 = (太吾爱慕的人.Contains(fatherId) || 太吾爱慕的人.Contains(motherId));
                    if (是否拦截)
                        Debug("拦截太吾爱慕的人发生关系，父方 " + DateFile.instance.GetActorName(fatherId) +
                            "，母方 " + DateFile.instance.GetActorName(motherId));
                }
                if (!是否拦截 && Main.settings.爱慕太吾的人不能与他人发生关系)
                {
                    List<int> 这两人爱慕的人们 = new List<int>();
                    foreach (int k in Main.settings.爱慕关系.Keys)
                    {
                        if (Main.settings.爱慕关系[k])
                        {
                            这两人爱慕的人们.AddRange(DateFile.instance.GetActorSocial(fatherId, k));
                            这两人爱慕的人们.AddRange(DateFile.instance.GetActorSocial(motherId, k));
                        }
                    }
                    是否拦截 = (这两人爱慕的人们.Contains(mainActorId));
                    if (是否拦截)
                        Debug("拦截爱慕太吾的人发生关系，父方 " + DateFile.instance.GetActorName(fatherId) +
                            "，母方 " + DateFile.instance.GetActorName(motherId));
                }
                return !是否拦截;
            }
            else
            {
                // 不拦截
                return true;
            }
		}
	}
}
