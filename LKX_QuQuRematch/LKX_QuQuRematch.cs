using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Threading;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;

/// <summary>
/// 促织探测器助手
/// </summary>
namespace LKX_QuQuRematch
{
    /// <summary>
    /// 设置文件
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        /// 选择的蛐蛐列表
        /// </summary>
        public List<int> ququList = new List<int>();

        /// <summary>
        /// 是否跳过选择框
        /// </summary>
        public bool skipMessageBox;

        /// <summary>
        /// 默认选择yes
        /// </summary>
        public bool isYes;

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    public class Main
    {
        /// <summary>
        /// umm日志
        /// </summary>
        public static UnityModManager.ModEntry.ModLogger logger;

        /// <summary>
        /// mod设置
        /// </summary>
        public static Settings settings;

        /// <summary>
        /// 是否开启mod
        /// </summary>
        public static bool enabled;

        /// <summary>
        /// 载入系统内蛐蛐的列表
        /// </summary>
        public static Dictionary<int, string> quQuGuiList = new Dictionary<int, string>();

        /// <summary>
        /// 用于显示在设置里的信息
        /// </summary>
        public static string messageLabel = "";

        /// <summary>
        /// 载入mod。
        /// </summary>
        /// <param name="modEntry">mod管理器对象</param>
        /// <returns></returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Main.logger = modEntry.Logger;
            Main.settings = Settings.Load<Settings>(modEntry);

            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnToggle = Main.OnToggle;
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            
            return true;
        }

        /// <summary>
        /// 确定是否激活mod
        /// </summary>
        /// <param name="modEntry">umm</param>
        /// <param name="value">是否激活</param>
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        /// <summary>
        /// 展示mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUIStyle redLabelStyle = new GUIStyle();
            redLabelStyle.normal.textColor = new Color(159f / 256f, 20f / 256f, 29f / 256f);
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            Main.settings.skipMessageBox = GUILayout.Toggle(Main.settings.skipMessageBox, "跳过对话框选择");
            if (Main.settings.skipMessageBox) Main.settings.isYes = GUILayout.Toggle(Main.settings.isYes, "未找到选择的促织自动选择“再试一次”");
            GUILayout.EndHorizontal();
            if (GUILayout.Button("获取促织列表"))
            {
                if (DateFile.instance.mianActorId <= 0)
                {
                    messageLabel = "获取失败：未进入存档";
                }
                else
                {
                    messageLabel = "获取成功";
                    Main.GetQuQuData();
                }
            }
            GUILayout.Label(messageLabel);

            if (Main.quQuGuiList.Count > 0)
            {
                GUILayout.Label("促织列表");
                GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
                int i = 0;
                foreach (KeyValuePair<int, string> ququ in Main.quQuGuiList)
                {
                    if (i % 29 == 0 && i != 0 && i != quQuGuiList.Count)
                    {
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                    }
                    if (i == 0) GUILayout.BeginVertical("Box", new GUILayoutOption[0]);

                    Main.SetGUIToToggle(ququ.Key, ququ.Value, ref Main.settings.ququList);

                    i++;
                    if (i == quQuGuiList.Count) GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 保存mod的设置
        /// </summary>
        /// <param name="modEntry">umm</param>
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Main.settings.Save(modEntry);
        }

        /// <summary>
        /// 设置GUI
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <param name="field"></param>
        static void SetGUIToToggle(int index, string name, ref List<int> field)
        {
            bool status = GUILayout.Toggle(field.Contains(index), name);
            if (GUI.changed)
            {
                if (status)
                {
                    if (!field.Contains(index)) field.Add(index);
                }
                else
                {
                    if (field.Contains(index)) field.Remove(index);
                }
            }
        }

        /// <summary>
        /// 获取所有的蛐蛐数据
        /// </summary>
        static void GetQuQuData()
        {
            foreach (KeyValuePair<int, Dictionary<int, string>> quQu in DateFile.instance.cricketDate)
            {
                if (!Main.quQuGuiList.ContainsKey(quQu.Key))
                {
                    Main.quQuGuiList.Add(quQu.Key, DateFile.instance.SetColoer(int.Parse(DateFile.instance.cricketDate[quQu.Key][1]) + 20001, quQu.Value[0]));
                }
            }
        }
    }

    /// <summary>
    /// 根据点击对话框id给是否再抓蛐蛐判断赋值
    /// </summary>
    [HarmonyPatch(typeof(OnClick), "Index")]
    public class LKX_QuQuRematch_For_OnClick_Index_Id
    {
        static void Prefix()
        {
            if (Main.enabled && OnClick.instance.ID == 8099)
            {
                LKX_QuQuRematch_GetQuquWindow.stopQuQuWindow = true;
            }
        }
    }

    /// <summary>
    /// 促织探测器助手工作
    /// </summary>
    public class LKX_QuQuRematch_GetQuquWindow
    {
        //三个开关控制流程
        public static bool active = true;
        public static bool stopQuQuWindow = false;
        public static bool closeQuQuWindow = false;

        /// <summary>
        /// 蛐蛐窗口遍历蛐蛐看看id是否存在于选择列表，并弹出对话框。
        /// </summary>
        [HarmonyPatch(typeof(GetQuquWindow), "SetGetQuquWindow")]
        public class LKX_QuQuRematch_For_GetQuquWindow_SetGetQuquWindow
        {
            static void Postfix()
            {
                if (!Main.enabled) return;

                active = true;
                int i = 0;
                foreach (KeyValuePair<int, int[]> item in GetQuquWindow.instance.cricketDate)
                {
                    if (Main.settings.ququList.Contains(item.Value[1])) i++;
                }
                
                if (i == 0)
                {
                    if (Main.settings.skipMessageBox && Main.settings.isYes)
                    {
                        stopQuQuWindow = true;
                    }
                    else
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(8099, "没有所选蛐蛐", "没有找到所选促织，是否在试一次（不消耗次数）？", false, true);
                    }
                }
                else
                {
                    if (!Main.settings.skipMessageBox)
                    {
                        YesOrNoWindow.instance.SetYesOrNoWindow(-1, "找到所选促织", "已经找到你选择的促织就在里面其中一只，能否抓到看你的运气了。", false, false);
                    }
                }
            }
        }

        /// <summary>
        /// 如果选择yes直接结束捕捉窗口节约时间。
        /// </summary>
        [HarmonyPatch(typeof(GetQuquWindow), "LateUpdate")]
        public class LKX_QuQuRematch_For_GetQuquWindow_LateUpdate
        {
            static void Prefix(ref bool ___getQuquEnd, ref bool ___startFirstTime, ref bool ___startGetQuqu, ref int ___startTime, ref int ___getQuquTime)
            {
                if (!Main.enabled) return;

                if (stopQuQuWindow)
                {
                    stopQuQuWindow = false;
                    closeQuQuWindow = true;
                    ___getQuquEnd = true;
                    ___startFirstTime = false;
                    ___startGetQuqu = false;
                    ___startTime = 0;
                    ___getQuquTime = 0;
                }
            }
        }

        /// <summary>
        /// 判断窗口关闭后蛐蛐是否再次捕捉
        /// </summary>
        [HarmonyPatch(typeof(GetQuquWindow), "CloseGetQuquWindowDone")]
        public class LKX_QuQuRematch_For_GetQuquWindow_CloseGetQuquWindowDone
        {
            /// <summary>
            /// 如果选择yes跳过CloseGetQuquWindowDone的执行
            /// </summary>
            /// <returns>true是执行原方法，false是跳过执行原方法</returns>
            static bool Prefix()
            {
                if (!Main.enabled) return true;

                if (closeQuQuWindow)
                {
                    active = false;
                    closeQuQuWindow = false;
                    //奇怪的bug暂时这样解决。
                    GetQuquWindow ququwin = GetQuquWindow.instance;
                    ququwin.getQuquWindow.SetActive(false);
                    ququwin.endGetQuquImage[0].gameObject.SetActive(false);

                    YesOrNoWindow.instance.ShwoWindowMask(ququwin.getQuquWindow.transform, false, 0.75f, 0.2f, false);

                    int ququNum = int.Parse(DateFile.instance.baseStoryDate[10006][302]);
                    DateFile.instance.SetEvent(new int[] { 0, -1, ququNum }, true, true);
                }

                return active;
            }
            /*
            /// <summary>
            /// 如果选择yes，再次触发抓蛐蛐事件来弹出抓蛐蛐窗口。
            /// </summary>
            static void Postfix()
            {
                if (active)
                {
                    //奇怪的bug
                    active = false;
                    int ququNum = int.Parse(DateFile.instance.baseStoryDate[10006][302]);
                    DateFile.instance.SetEvent(new int[] { 0, -1, ququNum }, true, true);
                }
            }
            */
        }
    }
}
