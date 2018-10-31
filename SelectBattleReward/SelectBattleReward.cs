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

    public class Settings : UnityModManager.ModSettings
    {
        public int destroy_grade = 2;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static List<string> ITEMGRADE = new List<string>() {
            "无",
            "<color=#8E8E8EFF>灰</color>",
            "<color=#FBFBFBFF>白</color>",
            "<color=#6DB75FFF>绿</color>",
            "<color=#8FBAE7FF>蓝</color>",
            "<color=#63CED0FF>青</color>",
            "<color=#AE5AC8FF>紫</color>",
            "<color=#E3C66DFF>黄</color>",
            "<color=#F28234FF>橙</color>",
            "<color=#E4504DFF>红</color>" };
        

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = Settings.Load<Settings>(modEntry);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
                return false;

            enabled = value;

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            GUILayout.Label("开始保留的物品稀有度：");
            settings.destroy_grade = GUILayout.SelectionGrid(settings.destroy_grade, ITEMGRADE.ToArray(), 5);
            GUILayout.Label("说明： 设定点击战利品时可以丢弃的物品稀有度， 小于此稀有度的物品都可以丢弃。");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
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
            DateFile.instance.LoseItem(DateFile.instance.MianActorID(), _itemid, _count, true, true);
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

                //大分类为6的物品 为任务物品，不可丢弃
                string maincate = DateFile.instance.GetItemDate(itemid, 4, true);
                if ( maincate == null || maincate == "" || maincate == "6")
                {
                    Main.Logger.Log(" Ignore Quest Items " + itemid);
                    continue;
                }

                // 稀有度超过设定等级的物品，不可丢弃
                string grade = DateFile.instance.GetItemDate(itemid, 8, true);
                if (maincate == null || maincate == "" ||  int.Parse(grade) >= Main.settings.destroy_grade)
                {
                    Main.Logger.Log(" Ignore Rare Items" + itemid  + " grade:" +grade);
                    continue;
                }

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
                    Main.Logger.Log("count == -1 " + itemid);
                    return;
                }

                //添加相应处理Component,注入参数
                var actionstub = iconobj.AddComponent<ClickAction>();
                actionstub.setParam(itemid, count, gameobj.gameObject);

                Main.Logger.Log("Finish add actionstub " + itemid);

            }
            return;
        }
    }
}