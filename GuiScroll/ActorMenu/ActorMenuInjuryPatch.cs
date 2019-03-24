
using Harmony12;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GuiScroll
{
    public static class ActorMenuInjuryPatch
    {
        [HarmonyPatch(typeof(ActorMenu), "SetInjuryIcon")]
        public static class ActorMenu_SetInjuryIcon_Patch
        {
            public static bool Prefix(int typ, int injuryId, int injuryPower)
            {
                if (!Main.enabled)
                    return true;
                Main.Logger.Log(typ + " 设置伤口图标 " + injuryId+" : "+ injuryPower);
                ActorMenu _this = ActorMenu.instance;
                GameObject gameObject = UnityEngine.Object.Instantiate(_this.injuryBack, Vector3.zero, Quaternion.identity);
                gameObject.name = "injury," + injuryId;
                gameObject.transform.Find("InjuryIcon").GetComponent<Image>().sprite = GetSprites.instance.injuryIcon[DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][98])];
                gameObject.transform.SetParent(_this.actorInjuryHolder[Mathf.Min(typ, 7)], worldPositionStays: false);
                int injuryTyp = (DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][1]) > 0) ? 1 : 2;
                string value = Mathf.Max(1, DateFile.instance.ParseInt(DateFile.instance.injuryDate[injuryId][1]) * injuryPower / 100).ToString();
                gameObject.transform.Find("HpSpText").GetComponent<Text>().text = injuryTyp == 1 ? DateFile.instance.SetColoer(20010, value) : DateFile.instance.SetColoer(20007, value);
                Button btn = gameObject.GetComponent<Button>();
                if(!btn)
                    btn = gameObject.AddComponent<Button>();
                var onclick = btn.onClick;
                onclick.RemoveAllListeners();
                onclick.AddListener(delegate { OnClickInjury(injuryId, injuryTyp); });
                return false;
            }
        }
        /// <summary>
        /// 点击伤口 进行自动治疗
        /// </summary>
        /// <param name="injuryId">伤口id</param>
        /// <param name="typ">1是外伤 2是内伤</param>
        public static void OnClickInjury(int injuryId,int typ)
        {
            Main.Logger.Log(injuryId + " 点击伤口 退出 " + typ);
            int actorId = ActorMenu.instance.acotrId;
            int mainActorId = DateFile.instance.MianActorID();
            int useActorId = actorId;
            //  如果不是主角并且按了Ctrl 使用太吾传人身上的药进行疗伤
            if (actorId!= mainActorId && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                useActorId = mainActorId;
                Main.Logger.Log(actorId + " 从太吾传人包里拿药 " + useActorId);
            }
            bool is_use = false; // 是否有吃了药
            while (true)// 实现从背包获取疗伤药进行疗伤，直到伤口治愈或者没有疗伤药了
            {
                if(!DateFile.instance.actorInjuryDate[actorId].TryGetValue(injuryId, out  int value)) // 没有这个伤口id 退出
                {
                    Main.Logger.Log(actorId+" 没有这个伤口id 退出 " + injuryId);
                    break;
                }
                int itemId = GetActorHealingMedicine(actorId, typ, injuryId, useActorId); // 获取可用疗伤药物品ID
                if (itemId == -1) // 没有可用疗伤药 退出
                {
                    Main.Logger.Log(actorId + " 没有可用疗伤药 退出 " + injuryId);
                    break;
                }
                is_use = true;
                int cureValue = typ == 1 ? (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11)))) : (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))));
                if ((cureValue * 3 < DateFile.instance.actorInjuryDate[actorId][injuryId]))// 判断是否能百分百发挥疗伤药药效
                {
                    cureValue /= 5;
                }
                DateFile.instance.RemoveInjury(actorId, injuryId, -cureValue);
                for (int m = 0; m < 6; m++)
                {
                    int num23 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + m));
                    if (num23 != 0)
                    {
                        ActorMenu.instance.ChangePoison(actorId, m, num23 * 10);
                    }
                }
                DateFile.instance.ChangeItemHp(useActorId, itemId, -1); // 消耗物品耐久度
            }
            if (is_use)
            {
                DateFile.instance.PlayeSE(8); // 音效
                WindowManage.instance.WindowSwitch(on: false);
                ActorMenu.instance.GetActorInjury(actorId, ActorMenu.instance.injuryTyp);
                if (DateFile.instance.battleStart)
                {
                    StartBattle.instance.UpdateActorHSQP();
                }
            }
            else
            {
                YesOrNoWindow.instance.SetYesOrNoWindow(-1, "没药了", "包包里找不到可以用来治疗伤口的药了哦！", false, true);
            }
        }
        /// <summary>
        /// 获得人物身上的疗伤药 优先寻找能百分百发挥药效的疗伤药
        /// </summary>
        /// <param name="actorId">人物ID</param>
        /// <param name="typ">1是外伤 2是内伤</param>
        /// <param name="injuryId">伤口id</param>
        /// <param name="useActorId">提供疗伤药的人物ID</param>
        /// <returns>疗伤药的物品id</returns>
        public static int GetActorHealingMedicine(int actorId,int typ, int injuryId,int useActorId)
        {
            Main.Logger.Log(actorId + " 寻找疗伤药 " + typ);
            ActorMenu _this = ActorMenu.instance;
            List<int> itemSort = DateFile.instance.GetItemSort(new List<int>(_this.GetActorItems(useActorId).Keys));
            int result = -1;
            foreach (int itemId in itemSort)
            {
                int cureValue = typ == 1 ? (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 11)))) : (Mathf.Abs(DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 12))));
                Main.Logger.Log(itemId + " 物品疗伤效果 " + cureValue);
                if (cureValue>0)//判断是否要的疗伤药
                {
                    if ((cureValue * 3 >= DateFile.instance.actorInjuryDate[actorId][injuryId]))// 判断是否能百分百发挥疗伤药药效
                    {
                        Main.Logger.Log("获得能百分百发挥药效的疗伤药");
                        return itemId;
                    }
                    else if(result==-1)
                    {
                        Main.Logger.Log("记录疗伤药");
                        result = itemId;
                    }
                }
            }
            return result;
        }
        // 增加健康
        public static void AddHealth()
        {
            int actorId = ActorMenu.instance.acotrId;
            int mainActorId = DateFile.instance.MianActorID();
            int useActorId = actorId;
            //  如果不是主角并且按了Ctrl 使用太吾传人身上的药进行疗伤
            if (actorId != mainActorId && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                useActorId = mainActorId;
                Main.Logger.Log(actorId + " 从太吾传人包里拿药 " + useActorId);
            }
            bool is_use = false; // 是否有吃了药
            while (true)
            {
                int hp = ActorMenu.instance.Health(actorId);
                int maxHp = ActorMenu.instance.MaxHealth(actorId);
                if (hp >= maxHp)
                {
                    if (!is_use)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "你狠健康", "你都这么健康了怎么还想吃药！", false, true);
                    }
                    break;
                }
                int itemId = GetActorHealthMedicine(actorId, useActorId, maxHp - hp); // 获取可用疗伤药物品ID
                if (itemId == -1) // 没有可用疗伤药 退出
                {
                    if (!is_use)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "你太穷了", "包里没有可以吃的药哦！", false, true);
                    }
                    break;
                }
                ActorMenu.instance.ChangeHealth(actorId, DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 13)));
                for (int num33 = 0; num33 < 6; num33++)
                {
                    int num34 = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 71 + num33));
                    if (num34 != 0)
                    {
                        ActorMenu.instance.ChangePoison(actorId, num33, num34 * 10);
                    }
                }
                DateFile.instance.ChangeItemHp(actorId, itemId, -1);

            }
            if (is_use)
            {

                DateFile.instance.PlayeSE(8); // 音效
                WindowManage.instance.WindowSwitch(on: false);

                ActorMenu.instance.UpdateItems(actorId, ActorMenu.instance.itemTyp);
            }
        }

        public static int GetActorHealthMedicine(int actorId, int useActorId, int lack)
        {
            ActorMenu _this = ActorMenu.instance;
            List<int> itemSort = DateFile.instance.GetItemSort(new List<int>(_this.GetActorItems(useActorId).Keys));
            int result = -1;
            foreach (int itemId in itemSort)
            {
                int cureValue = DateFile.instance.ParseInt(DateFile.instance.GetItemDate(itemId, 13));
                if (cureValue > 0)//判断是否要的寿命药
                {
                    if (cureValue * 3 >= lack)// 判断是否能百分百发挥疗伤药药效
                    {
                        return itemId;
                    }
                    else if (result == -1)
                    {
                        result = itemId;
                    }
                }
            }
            return result;
        }
    }
}