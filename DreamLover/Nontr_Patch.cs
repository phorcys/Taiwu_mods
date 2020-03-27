using GameData;
using Harmony12;
using System.Collections.Generic;
using UnityEngine;

namespace DreamLover
{
    public static class Nontr_Patch
    {
        public static void Debug(string info)
        {
            Main.Debug("<防绿> " + info);
        }

        public static PatchModuleInfo patchModuleInfo = new PatchModuleInfo(
            typeof(PeopleLifeAI), "AISetChildren",
            typeof(Nontr_Patch));

        public static bool Prefix(int fatherId, int motherId, int setFather, int setMother, ref bool __result)
        {
            if (!Main.enabled)
            {
                return true;
            }
            int mainActorId = DateFile.instance.MianActorID();

            // 启用功能并且不是主角
            if (Main.settings.nontr.Enabled && fatherId != mainActorId && motherId != mainActorId)
            {
                bool 是否拦截 = false;

                bool 人物是否需要拦截 = false;

                if (Main.settings.nontr.PreventAll)
                    人物是否需要拦截 = true;

                List<int> list1 = DateFile.instance.GetActorSocial(fatherId, 309);
                List<int> list2 = DateFile.instance.GetActorSocial(motherId, 309);
                bool 是太吾的关系人 = false;
                foreach (int i in list1)
                    if (!是太吾的关系人 && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, i))
                        是太吾的关系人 = true;
                foreach (int i in list2)
                    if (!是太吾的关系人 && DateFileHelper.HasAnySocial(mainActorId, Main.NoNtrSocialTypList, i))
                        是太吾的关系人 = true;

                bool 是否配偶关系 = DateFileHelper.HasSocial(fatherId, 309, motherId);

                if (是太吾的关系人)
                    人物是否需要拦截 = true;

                是否拦截 = 人物是否需要拦截;
                if (Main.settings.nontr.AllowCouple && 是否配偶关系) 是否拦截 = false;

                if (是否拦截)
                {
                    if (是太吾的关系人)
                    {
                        Debug(string.Format("检测到想要搞事情的 {0} {1} 与关系列表内的人有婚姻关系，已拦截", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                    else
                    {
                        Debug(string.Format("检测到想要搞事情的 {0} {1} 的人，已拦截", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
                    }
                }
                else
                {
                    if (是太吾的关系人)
                    {
                        Debug(string.Format("检测到想要搞事情的 {0} {1} 与关系列表内的人有婚姻关系，未拦截", DateFile.instance.GetActorName(fatherId), DateFile.instance.GetActorName(motherId)));
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
    }
}
