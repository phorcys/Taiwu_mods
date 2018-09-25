using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SelectBattleReward
{

 

    public static class Main
    {
        public static bool enabled;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

    }


    /// <summary>
    ///  鼠标事件处理
    /// </summary>
    public class ClickAction : MonoBehaviour, IPointerClickHandler
    {
        int _itemid;
        int _count;
        GameObject _icon;
        public void OnPointerClick(PointerEventData eventData)
        {
            //删掉不要的物品
            DateFile.instance.LoseItem(DateFile.instance.MianActorID(), _itemid, _count, true);
            //销毁图标
            GameObject.Destroy(_icon);
        }

        public void setParam(int itemid,int count , GameObject icon)
        {
            _itemid = itemid;
            _count = count;
            _icon = icon;
        }
    }

    /// <summary>
    ///  选择战利品，战利品展示界面注入鼠标响应事件
    /// </summary>
    [HarmonyPatch(typeof(BattleSystem), "ShowBattleBooty")]
    public static class BattleSystem_ShowBattleBooty_Patch
    {

        private static void Postfix(BattleSystem __instance, ref List<int[]> ___battleBooty, ref Transform ___battleBootyHolder)
        {
            if (!Main.enabled)
            {
                return;
            }

            //遍历所有战利品
            for (int i = 0; i < ___battleBootyHolder.childCount; i++)
            {
                var gameobj = ___battleBootyHolder.GetChild(i);
                var iconobj = gameobj.Find(gameobj.name).gameObject;

                int itemid = int.Parse(iconobj.name.Split(',')[1]);
                int count = -1;
                //便利战利品信息列表
                foreach (var val in ___battleBooty)
                {
                    if (val[0] == itemid)
                    {
                        count = val[1];
                    }
                    
                }
                
                if(count == -1)
                {
                    //错误不处理
                    return;
                }

                //添加相应处理Component,注入参数
                var actionstub = iconobj.AddComponent<ClickAction>();
                actionstub.setParam(itemid, count, gameobj.gameObject);

            }
            return;
        }
    }
}