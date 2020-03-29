using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
	[HarmonyPatch(typeof(PeopleLifeAI), "AISetChildren")]
	public static class SetChildren_Patch
    {
        private static void Debug(string str)
        {
            Main.Debug("<造孩子> " + str);
        }

        public static PatchModuleInfo patchModuleInfo = new PatchModuleInfo(
            typeof(PeopleLifeAI), "AISetChildren",
            typeof(SetChildren_Patch));

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
                Debug("主角尝试发生关系");
                int num2 = int.Parse(DateFile.instance.GetActorDate(fatherId, 24));
                int num3 = int.Parse(DateFile.instance.GetActorDate(motherId, 24));
                DateFile.instance.ChangeActorFeature(fatherId, 4001, 4002);
                DateFile.instance.ChangeActorFeature(motherId, 4001, 4002);
                GEvent.OnEvent(eEvents.Copulate, fatherId, motherId);
                if (Main.settings.pregnant.fecundity.setFather)
                    num2 = Main.settings.pregnant.fecundity.valueFather;
                if (Main.settings.pregnant.fecundity.setMother)
                    num3 = Main.settings.pregnant.fecundity.valueMother;
                Debug("父方生育能力: " + num2 + " 母方生育能力: " + num3);

                int num4 = num2 * num3; // 生育能力的结果
                if (Main.settings.pregnant.fecundity.setAll)
                    num4 = Main.settings.pregnant.fecundity.valueAll;
                
                // 生育能力判定
                if (num4 <= 0)
                {
                    Debug("不可能生育，因为父方或母方生育能力不为正数");
                    __result = false;
                    return false;
                }
                if (int.Parse(DateFile.instance.GetActorDate(motherId, 14, applyBonus: false)) != 2)
                {
                    Debug("母方不是女性，终止怀孕尝试");
                    __result = false;
                    return false;
                }
                int var1 = Random.Range(0, 15000);
                if (!DateFile.instance.HaveLifeDate(motherId, 901) && var1 < num4)
                {
                    Debug("生育能力判定通过，检定结果：" + num4 + "/" + var1);

                    int 怀孕概率 = 100;
                    int existChildWeight = 20; // 现存的子嗣对剩余能力的影响
                    怀孕概率 -= DateFile.instance.GetActorSocial(fatherId, 310).Count * existChildWeight;
                    怀孕概率 -= DateFile.instance.GetActorSocial(motherId, 310).Count * existChildWeight;
                    int var2 = Random.Range(0, 100);

                    int num5 = 怀孕概率;
                    if (Main.settings.pregnant.SpecifiedPossibility)
                        num5 = Main.settings.pregnant.Possibility;
                    // 怀孕概率判定
                    if (var2 < num5)
                    {
                        Debug("怀孕判定通过，检定结果：" + num5 + "/" + var2);
                        DateFile.instance.ChangeActorFeature(motherId, 4002, 4003);
                        int var3 = Random.Range(0, 100);
                        int num6 = (((DateFile.instance.getQuquTrun - 100) / 10)); // 异胎概率
                        if (Main.settings.pregnant.SpecifiedQuQu)
                            num6 = Main.settings.pregnant.QuQu;

                        if (var3 < num6)
                        {
                            Debug("异胎判定通过，检定结果："+ num6 + "/" + var3);
                            DateFile.instance.getQuquTrun = 0;
                            DateFile.instance.actorLife[motherId].Add(901, new List<int> { 1042, fatherId, motherId, setFather, setMother});
                        }
                        else
                        {
                            Debug("正常胎儿判定通过，异胎检定结果：" + num6 + "/" + var3);
                            DateFile.instance.actorLife[motherId].Add(901, new List<int> { Random.Range(7, 10), fatherId, motherId, setFather, setMother });
                            DateFile.instance.pregnantFeature.Add(motherId, new string[2] {
                                Characters.GetCharProperty(fatherId, 101),
                                Characters.GetCharProperty(motherId, 101)
                            });
                        }
                        __result = true;
                    }
                    else // 怀孕概率判定失败
                    {
                        Debug("怀孕判定未通过，检定结果：" + num5 + "/" + var2);
                    }
                }
                else // 生育能力判定失败
                {
                    if (!DateFile.instance.HaveLifeDate(motherId, 901))
                    {
                        Debug("生育能力判定未通过，因为母方已怀孕。");
                    }
                    else
                    {
                        Debug("生育能力判定未通过，检定结果：" + num4 + "/" + var1);
                    }
                }
                return false;
            }
            else
            {
                // 不拦截
                return true;
            }
		}

	}
}
