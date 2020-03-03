using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
	public static class SetChildren_Patch
    {
        public static PatchModuleInfo patchModuleInfo = new PatchModuleInfo(
            typeof(PeopleLifeAI), "AISetChildren",
            typeof(SetChildren_Patch));
        private static void DebugPregnant(string str) 
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
            if (Main.settings.pregnant.Enabled && (fatherId == mainActorId || motherId == mainActorId))
            {
                DebugPregnant("主角尝试发生关系");
                int num2 = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
                int num3 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
                DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
                DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);
                GEvent.OnEvent(eEvents.Copulate, fatherId, motherId);
                DebugPregnant("父方生育能力: " + num2 + " 母方生育能力: " + num3);
                if (Main.settings.pregnant.SpecifiedFecundity ? (Main.settings.pregnant.Fecundity <= 0) : (num2 <= 0 || num3 <= 0))
                {
                    DebugPregnant("生育不可能，终止");
                    __result = false;
                    return false;
                }
                if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, applyBonus: false)) != 2)
                {
                    DebugPregnant("阿拉，母方不是女性，终止");
                    __result = false;
                    return false;
                }
                int var1 = Random.Range(0, 15000);
                if (!DateFile.instance.HaveLifeDate(motherId, 901) && var1 < (Main.settings.pregnant.SpecifiedFecundity ? Main.settings.pregnant.Fecundity : (num2 * num3)))
                {
                    DebugPregnant("生育能力判定通过，需求 " + var1 + "，结果 " + (Main.settings.pregnant.SpecifiedFecundity ? Main.settings.pregnant.Fecundity : (num2 * num3)));
                    bool flag = fatherId == mainActorId || motherId == mainActorId;
                    int num4 = 100;
                    int num5 = flag ? 20 : 50;
                    num4 -= DateFile.instance.GetActorSocial(fatherId, 310).Count * num5;
                    num4 -= DateFile.instance.GetActorSocial(motherId, 310).Count * num5;
                    int var2 = Random.Range(0, 100);
                    if (var2 < (Main.settings.pregnant.SpecifiedPossibility ? Main.settings.pregnant.Possibility : num4))
                    {
                        DebugPregnant("怀孕判定通过，需求 " + var2 + "，结果 " + (Main.settings.pregnant.SpecifiedPossibility ? Main.settings.pregnant.Possibility : num4));
                        DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                        int var3 = Random.Range(0, 100);
                        if (var3 < (Main.settings.pregnant.SpecifiedQuQu ? Main.settings.pregnant.QuQu : ((DateFile.instance.getQuquTrun - 100) / 10)))
                        {
                            DebugPregnant("异胎判定通过，需求 " + var3 + "，结果 " + (Main.settings.pregnant.SpecifiedQuQu ? Main.settings.pregnant.QuQu : ((DateFile.instance.getQuquTrun - 100) / 10)));
                            DateFile.instance.getQuquTrun = 0;
                            DateFile.instance.actorLife[motherId].Add(901, new List<int> { 1042, fatherId, motherId, setFather, setMother});
                        }
                        else
                        {
                            DebugPregnant("正常胎儿判定通过");
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
                        DebugPregnant("怀孕判定未通过，需求 " + var2 + "，结果 " + (Main.settings.pregnant.SpecifiedPossibility ? Main.settings.pregnant.Possibility : num4));
                    }
                }
                DebugPregnant("生育能力判定未通过，需求 " + var1 + "，结果 " + (Main.settings.pregnant.SpecifiedFecundity ? Main.settings.pregnant.Fecundity : (num2 * num3)));
                return false;
            }
            else if(Main.settings.nontr.Enabled)
            {
                bool 是否拦截 = false; 

                if(Main.settings.nontr.PreventAll)
                {
                    是否拦截 = true;
                    // 允许配偶关系，并且确实是配偶，就不拦截
                    if (Main.settings.nontr.AllowCouple && DateFileHelper.HasSocial(fatherId, 309, motherId))
                    {
                        是否拦截 = false;
                    }

                    if (是否拦截)
                        DebugNontr(string.Format("拦截了 {0} 与 {1} 试图发生关系的行为", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                }
                else
                {
                    bool 在关系列表内 = false;

                    if(!在关系列表内 && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, fatherId))
                    {
                        DebugNontr(string.Format("父方 {0} 在关系列表内", DateFile.instance.GetActorName(fatherId)));
                        在关系列表内 = true;
                    }

                    if (!在关系列表内 && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, motherId))
                    {
                        DebugNontr(string.Format("母方 {0} 在关系列表内", DateFile.instance.GetActorName(motherId)));
                        在关系列表内 = true;
                    }

                    if(在关系列表内)
                    {
                        是否拦截 = true;
                        if (Main.settings.nontr.AllowCouple && DateFileHelper.HasSocial(fatherId, 309, motherId))
                        {
                            是否拦截 = false;
                        }

                        if (是否拦截)
                            DebugNontr(string.Format("拦截了 {0} 与 {1} 试图发生关系的行为", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                }
                return !是否拦截;
            }
            else
            {
                // 不拦截
                return true;
            }
		}
        public static void DebugNontr(string info)
        {
            Main.Debug("<防绿> " + info);
        }
	}
}
