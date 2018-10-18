using Harmony12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using System.IO;
//通过对话获得新村的控制权和关键建筑图纸
namespace NewWorld
{
    public class Settings : UnityModManager.ModSettings
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }
    }

    public static class Main
    {
        public static bool enabled;
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
       
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("  <color=#FF0000FF>如果要删除本MOD，请在对应存档内按下清除并存档，"+
                            "  清除会删除所有额外开辟的村庄和建筑和太吾村卷轴\n为防止坏档，请务必" +
                            "在大地图界面点击此按钮！\n这数据结构太tm操蛋了，我不确定有没有少删或"+
                            "多删，不建议点。</color>");
            //检测存档
            DateFile tbl = DateFile.instance;
            if (tbl == null || tbl.actorsDate == null || !tbl.actorsDate.ContainsKey(tbl.mianActorId))
            {
                GUILayout.Label("  存档未载入!");
            }
            else
            {
                if(Main.enabled)
                {
                    GUILayout.Label("  关闭Mod后才能清除!");
                }
                else if (GUILayout.Button("清除"))
                {
                    DeleteAll();
                }
            }
            GUILayout.EndHorizontal();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static void deleteDict<TValue>(ref Dictionary<int, Dictionary<int, Dictionary<int, TValue>>> target,int x,int y,int buildingIndex)
        {
            if (target.ContainsKey(x)
                && target[x].ContainsKey(y)
                && target[x][y].ContainsKey(buildingIndex))
                {
                    target[x][y].Remove(buildingIndex);
                    if (target[x][y].Count <= 0)
                    {
                        target[x].Remove(y);
                        if (target[x].Count <= 0)
                            target.Remove(x);
                    }
                }
        }
        static void DeleteAll()//太tm难了
        {
            int actorId = DateFile.instance.MianActorID();
            //恢复信息
            DateFile.instance.resourceDate[7][99] = "人力来自于太吾村村民的数量以及太吾村中「居所」的规模和数量。\n<color=#E3C66DFF>（你最多可以调配(<color=#FBFBFBFF>50</color>)个人力。）</color>\n";//人力资源的描述信息
            //移除太吾村卷轴
            int itemId = 301;                    //太吾村=301
            if (DateFile.instance.itemsDate.ContainsKey(itemId)
                || DateFile.instance.presetitemDate.ContainsKey(itemId))
                if(DateFile.instance.actorItemsDate.ContainsKey(actorId))
                    if(DateFile.instance.actorItemsDate[actorId].ContainsKey(itemId))
                        DateFile.instance.LoseItem(actorId, itemId, DateFile.instance.actorItemsDate[actorId][itemId], true);
            //获得所有能靠图纸造出来的建筑id
            Dictionary<int, int> taiwuBuildingIds = new Dictionary<int, int>();
            foreach(var item_line in DateFile.instance.presetitemDate)
                if(int.Parse(DateFile.instance.GetItemDate(item_line.Key,5))==20)
                {
                    taiwuBuildingIds[int.Parse(DateFile.instance.GetItemDate(item_line.Key, 301))] = 0;//所造出的建筑id
                }
            foreach (var item_line in DateFile.instance.itemsDate)
                if (int.Parse(DateFile.instance.GetItemDate(item_line.Key, 5)) == 20)
                {
                    taiwuBuildingIds[int.Parse(DateFile.instance.GetItemDate(item_line.Key, 301))] = 0;//所造出的建筑id
                }
            //移除建筑
            List<KeyValuePair<int,int>> removeHomeList = new List<KeyValuePair<int, int>>();
            var x_list = DateFile.instance.baseHomeDate.Keys;
            foreach(var x in x_list)
            {
                var y_list = DateFile.instance.baseHomeDate[x].Keys;
                foreach (var y in y_list)
                    if(DateFile.instance.baseHomeDate[x][y]>0)
                        if (DateFile.instance.GetNewMapDate(x,y,0)!= "「太吾村」")
                    {
                        List<int> building_id_list = new List<int>(DateFile.instance.homeBuildingsDate[x][y].Keys);
                        Dictionary<int,int[]> new_building_id_list =new Dictionary<int, int[]>();
                        foreach (var buildingIndex in building_id_list)
                        {
                            //如果建筑的类型不是通过图纸能造出来的，说明不是自己村的，无视
                            if (!taiwuBuildingIds.ContainsKey(DateFile.instance.homeBuildingsDate[x][y][buildingIndex][0]))
                                {
                                new_building_id_list[buildingIndex] = DateFile.instance.homeBuildingsDate[x][y][buildingIndex];
                                continue;
                                }

                            //移除工作人员
                            deleteDict<int>(ref DateFile.instance.actorsWorkingDate, x, y, buildingIndex);
                            //移除战利品
                            deleteDict<List<int[]>>(ref DateFile.instance.homeShopBootysDate, x, y, buildingIndex);

                            //移除升级\拆除\建造人力
                            deleteDict<int>(ref DateFile.instance.manpowerHomeRemoveList, x, y, buildingIndex);
                            deleteDict<int>(ref DateFile.instance.manpowerHomeUpList, x, y, buildingIndex);
                            deleteDict<int>(ref DateFile.instance.manpowerHomeUseList, x, y, buildingIndex);
                        }
                        removeHomeList.Add(new KeyValuePair<int, int>(x, y));
                        //DateFile.instance.homeBuildingsDate[x][y] = new_building_id_list;
                    }
            }
            foreach(var pair in removeHomeList)
            {
                DateFile.instance.baseHomeDate[pair.Key][pair.Value] = 0;
            }
            //移除事件
            MassageWindow_SetMassageWindow_Patch.RemoveEvent();
        }
    }
    //令建筑上限至少为1以防止无法建设"太吾村"建筑
    [HarmonyPatch(typeof(HomeSystem), "GetMaxBuilding", new Type[] { typeof(int),typeof(int)})]
    public static class HomeSystem_GetMaxBuilding_Patch
    {
        static void Postfix(ref int __result)
        {
            if (!Main.enabled)
                return;
            if(__result <= 0)
                __result = 1;
        }
    }
    //根据村子数量提升人力上限
    [HarmonyPatch(typeof(UIDate), "GetMaxManpower", new Type[] {  })]
    public static class UIDate_GetMaxManpower_Patch
    {
        public static void Postfix(ref int __result)
        {
            if (!Main.enabled)
                return;
            int num = UIDate.instance.GetBaseMaxManpower();
            int up_limit = 40;
            foreach (int key in DateFile.instance.baseHomeDate.Keys)
            {
                Dictionary<int, int> dictionary = DateFile.instance.baseHomeDate[key];
                foreach (int key2 in dictionary.Keys)
                {
                    if (dictionary[key2] != 0)
                    {
                        up_limit += 10;
                        Dictionary<int, int[]> dictionary2 = DateFile.instance.homeBuildingsDate[key][key2];
                        foreach (int key3 in dictionary2.Keys)
                        {
                            int[] array = dictionary2[key3];
                            float num2 = float.Parse(DateFile.instance.basehomePlaceDate[array[0]][61]);
                            if (num2 > 0f)
                            {
                                num += 1 + Convert.ToInt32(num2 * (float)array[1]);
                            }
                        }
                    }
                }
            }
            __result= Mathf.Clamp(num, 0, up_limit);
        }
    }
    //注入事件
    [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow", new Type[] { typeof(int[]), typeof(int) })]
    public static class MassageWindow_SetMassageWindow_Patch
    {
        //事件的Value相关
        public const int EventNewWorldBegin = 901300099;//入口事件(分支)
        public const int EventNewWorldDo = 9399;//确认事件(对话)
        public const int EventNewWorldEnd = 901300097;//确认事件(分支)
        public const int EventNewWorldMagicNumber = 901300096;//EndEvent数，不是事件
        //EventDate索引，Event分两种：
        //1.总和事件，特征：短ID，自身文本显示在上方，在索引5上包含复数分支事件
        //2.分支事件，特征：长ID，自身文本显示在按钮上，在索引7上包含一个总和事件(可能不是有效的)
        public const int EventDateIndexRequire = 6;//事件可以被点击的条件
        public const int EventDateIndexEndEvent = 8;//事件结束时的处理
        //第1，2分别为0，-1似乎是"以对方为对象"的意思？
        public const int EventDateIndexConditionEvent = 5;//该事件所包含的分支
        public const int EventDateIndexCodeEvent = 7;//下一步的事件
        //presetGangGroupDate相关
        public static int[] GangGroupValue = {15,25,35,45};//15-45=商人，11-41=镇长
        public const int GangGroupMagic = 812;//身份提供的特殊交互

        //baseHomeDate相关
        public const int BaseHomeMagicNumber = 1;//读出来就变成1了，设成2也没卵用
        // Token: 0x0600000A RID: 10 RVA: 0x00002EA4 File Offset: 0x000010A4
        public static void Prefix(int[] baseEventDate, int chooseId)
        {
            if (!Main.enabled)
                return;
            //添加入口事件，如果已添加过就在这里return
            if (true)
            {
                bool needAddEvent = false;
                foreach (var id in GangGroupValue)
                {
                    List<string> gangDateEvents = new List<string>(DateFile.instance.presetGangGroupDateValue[id][GangGroupMagic].Split(new char[] { '|' }));
                    //向特定身份提供的事件添加我的入口事件
                    if (gangDateEvents.Contains(EventNewWorldBegin.ToString()))
                        continue;
                    gangDateEvents.Add(EventNewWorldBegin.ToString());
                    DateFile.instance.presetGangGroupDateValue[id][GangGroupMagic] = string.Join("|", gangDateEvents.ToArray());
                    needAddEvent = true;//只要有一个数据没添加就认为是第一次进入
                }
                if (needAddEvent)
                    { 
                    //修改人力资源的
                    DateFile.instance.resourceDate[7][99] = "人力来自于太吾村村民的数量以及太吾村中「居所」的规模和数量。\n<color=#E3C66DFF>（你最多可以调配(<color=#FBFBFBFF>50+分矿*10</color>)个人力。）</color>\n";//人力资源的描述信息
                    //入口事件，添加到身份所带来的事件当中，参考治疗伤病，
                    if (true)
                    {
                        Dictionary<int, string> beginEventDate = new Dictionary<int, string>{
                                    { 1,"0"},//
                                    { 2,"-1"},//
                                    { 3,"（想要穷极种田之奥义……）"},//文字
                                    { 4,"1"},//不知道啥
                                    { 5,""},//
                                    { 6,"TIME&20|FA&2"},//条件
                                    { 7,EventNewWorldDo.ToString()},//用于判断接下来显示的对话的事件ID
                                    { 8,""},//
                                    { 9,"0"},
                                    {10,""},
                                    {11,"0" }};
                        DateFile.instance.eventDate[EventNewWorldBegin] = beginEventDate;
                    }
                    //总和事件
                    if (true)
                    {
                        Dictionary<int, string> doEventDate = new Dictionary<int, string>{
                            { 0,"开启新世界" },//描述
                            { 1,"0"},//
                            { 2,"0"},//
                            { 3,"这会消耗大量威望，会对这个世界产生永久的改变，一旦决定了就无法后悔。"},//文字
                            { 4,"1"},//不知道啥
                            { 5,EventNewWorldEnd.ToString()+"|900700001"},//包含的分支事件ID
                            { 6,""},//条件
                            { 7,""},//
                            { 8,""},//结束处理
                            { 9,"0"},
                            {10,"" },
                            {11,"0" }};
                        DateFile.instance.eventDate[EventNewWorldDo] = doEventDate;
                    }
                    //确认的分支
                    if (true)
                    {
                        Dictionary<int, string> endEventDate = new Dictionary<int, string>{
                            { 0,"" },//描述
                            { 1,"0"},//
                            { 2,"-1"},//
                            { 3,"（我早有觉悟……）"},//文字
                            { 4,"1"},//不知道啥
                            { 5,""},//
                            { 6,"TIME&20|ATTMAX&407&500"},//条件
                            { 7,"-1"},//
                            { 8,"TIME&-20|RES&6&-500|END&"+EventNewWorldMagicNumber.ToString()},//结束处理
                            { 9,"0"},
                            {10,"" },
                            {11,"0" }};
                        DateFile.instance.eventDate[EventNewWorldEnd] = endEventDate;
                    }
                    }
            }
            //触发确认时更新所需威望值
            if (chooseId== EventNewWorldBegin)
            {
                int count = 0;
                foreach (var line in DateFile.instance.baseHomeDate)
                    foreach (var row in line.Value)
                        if (row.Value > 0)
                            count++;
                int cost = count * count*300;
                int choosePartId = WorldMapSystem.instance.choosePartId;
                int choosePlaceId = WorldMapSystem.instance.choosePlaceId;
                if (choosePartId >= 0 && choosePlaceId >= 0)
                    if (DateFile.instance.baseHomeDate[choosePartId][choosePlaceId] > 0)
                        cost = 2000000;

                Dictionary<int, string> endEventDate = new Dictionary<int, string>{
                        { 0,"" },//描述
                        { 1,"0"},//
                        { 2,"-1"},//
                        { 3,"（我早有觉悟……）"},//文字
                        { 4,"1"},//不知道啥
                        { 5,""},//
                        { 6,"TIME&20|ATTMAX&407&"+cost.ToString()},//条件
                        { 7,"-1"},//
                        { 8,"TIME&-20|RES&6&-"+cost.ToString()+"|END&"+EventNewWorldMagicNumber.ToString()},//结束处理
                        { 9,"0"},
                        {10,"" },
                        {11,"0" }};
                DateFile.instance.eventDate[EventNewWorldEnd] = endEventDate;
            }
        }
        public static void RemoveEvent()//只有入口事件需要删，剩下的关游戏就没了
        {
            foreach (var id in GangGroupValue)
            {
                List<string> gangDateEvents = new List<string>(DateFile.instance.presetGangGroupDateValue[id][GangGroupMagic].Split(new char[] { '|' }));
                if (!gangDateEvents.Contains(EventNewWorldBegin.ToString()))
                    continue;
                gangDateEvents.Remove(EventNewWorldBegin.ToString());
                DateFile.instance.presetGangGroupDateValue[id][GangGroupMagic] = string.Join("|", gangDateEvents.ToArray());
            }
        }
    }
    //处理新增村庄的结束时间：获得一个太吾村卷轴，将该地的baseHomeDate置1
    [HarmonyPatch(typeof(MassageWindow), "EndEvent", new Type[] {})]
    public static class MassageWindow_EndEvent_Patch
    {
        public static void Prefix(MassageWindow __instance)
        {
            if (__instance.eventValue.Count > 0 && __instance.eventValue[0] != 0)
                if(__instance.eventValue[0]== MassageWindow_SetMassageWindow_Patch.EventNewWorldMagicNumber)
                {
                    __instance.eventValue.Clear();
                    if (!Main.enabled)
                        return;
                    Main.Logger.Log("Done");
                    int actorId = DateFile.instance.MianActorID();
                    //添加卷轴
                    //for (int id = 101; id < 2011; ++id)
                    int id = 301;                    //太吾村=301
                    if (DateFile.instance.itemsDate.ContainsKey(id)
                        || DateFile.instance.presetitemDate.ContainsKey(id))
                            DateFile.instance.GetItem(actorId, id, 1, true);
                    //标记老家
                    int choosePartId = WorldMapSystem.instance.choosePartId;
                    int choosePlaceId = WorldMapSystem.instance.choosePlaceId;
                    if (choosePartId >= 0 && choosePlaceId >= 0)
                        DateFile.instance.baseHomeDate[choosePartId][choosePlaceId] = MassageWindow_SetMassageWindow_Patch.BaseHomeMagicNumber;
                }
        }
    }
}
