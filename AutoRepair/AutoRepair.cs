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

namespace AutoRepair
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool open = false;

        public bool familiy = false;
        public bool weapon = false;
        public bool hat = false;
        public bool armor = false;
        public bool shouse = false;
        public bool pearl = false;

        public bool bymoney = false;
        public int number = 0;

        public string[] paymentText = new string[] { "木材", "金石", "织物", "银两" };//1,2,3,5
        public int payment = 0;
        public string[] cardText = new string[] { "琥珀会员", "赤血会员" };
        public int card = 0;

        public bool yes = false;
        public bool no = false;
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

        public static bool ExistMianActor()
        {
            return DateFile.instance != null && GameData.Characters.HasChar(DateFile.instance.MianActorID());
        }

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
            if(!ExistMianActor())
                GUILayout.Label("存档未载入!", new GUILayoutOption[0]);
            else
            {
                settings.open = GUILayout.Toggle(settings.open, "开启自动修理业务", new GUILayoutOption[0]);
                if (settings.open)
                {
                    GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                    GUILayout.Label("选择修理部位", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                    GUILayout.BeginHorizontal();
                    settings.weapon = GUILayout.Toggle(settings.weapon, "武器", new GUILayoutOption[0]);
                    settings.hat = GUILayout.Toggle(settings.hat, "头盔", new GUILayoutOption[0]);
                    settings.armor = GUILayout.Toggle(settings.armor, "护甲", new GUILayoutOption[0]);
                    settings.shouse = GUILayout.Toggle(settings.shouse, "鞋子", new GUILayoutOption[0]);
                    settings.pearl = GUILayout.Toggle(settings.pearl, "其他", new GUILayoutOption[0]);
                    GUILayout.EndHorizontal();
                    settings.familiy = GUILayout.Toggle(settings.familiy, "也修理队友的装备", new GUILayoutOption[0]);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                    GUILayout.Label("请选择会员卡种类：", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                    settings.card = GUILayout.Toolbar(settings.card, settings.cardText, new GUILayoutOption[] { GUILayout.Width(400f) });
                    GUILayout.EndVertical();
                    if (settings.card == 0)
                    {
                        if (!DateFile.instance.HaveLifeDate(10001, 79))
                        {
                            //想不到吧！我把会员卡偷偷塞入了初代太吾的菊花里！
                            //（茄茄跟我说不会坏档来着）
                            if (GUILayout.Button("点击办理会员卡", new GUILayoutOption[] { GUILayout.Width(180f) }))
                            {
                                if (!DateFile.instance.actorLife.ContainsKey(10001))
                                {
                                    DateFile.instance.actorLife.Add(10001, new Dictionary<int, List<int>> { { 79, new List<int> { 0, 0, 0, 0 } } });
                                }
                                else
                                    DateFile.instance.actorLife[10001].Add(79, new List<int> { 0, 0, 0, 0 });
                            }
                        }
                        else

                        {

                            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                            GUILayout.Label("您的账户余额如下：", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("木材:{0}", DateFile.instance.actorLife[10001][79][0]), new GUILayoutOption[] { GUILayout.Width(180f) });
                            GUILayout.Label(string.Format("金石:{0}", DateFile.instance.actorLife[10001][79][1]), new GUILayoutOption[] { GUILayout.Width(180f) });
                            GUILayout.Label(string.Format("织物:{0}", DateFile.instance.actorLife[10001][79][2]), new GUILayoutOption[] { GUILayout.Width(180f) });
                            GUILayout.Label(string.Format("银两:{0}", DateFile.instance.actorLife[10001][79][3]), new GUILayoutOption[] { GUILayout.Width(180f) });
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                            GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                            GUILayout.Space(8);
                            GUILayout.Label("请选择充值目标（银两会在相应资源不足时支付维修费用，但会消耗50%额外手续费）：", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                            settings.payment = GUILayout.Toolbar(settings.payment, settings.paymentText, new GUILayoutOption[] { GUILayout.Width(400f) });
                            GUILayout.Space(15);
                            GUILayout.Label("请输入充值数量：", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                            var deal = GUILayout.TextField(settings.number.ToString(), 6, GUILayout.Width(100));
                            if (GUI.changed)
                            {
                                if (!int.TryParse(deal, out settings.number)) { settings.number = 0; }
                            }
                            GUILayout.EndVertical();

                            GUILayout.BeginHorizontal("Box", new GUILayoutOption[] { GUILayout.Width(180f) });
                            settings.yes = GUILayout.Button("确认充值", new GUILayoutOption[] { GUILayout.Width(180f) });
                            settings.no = GUILayout.Button("注销会员", new GUILayoutOption[] { GUILayout.Width(180f) });
                            var test = "";
                            GUILayout.Label(test, new GUILayoutOption[0]);
                            GUILayout.EndHorizontal();
                            if (settings.yes)
                            {
                                if (settings.number != 0)
                                {
                                    if (Autofix.Buy(settings.payment, settings.number)) test = "交易成功(^･ᴗ･^)";
                                    else test = "充值失败！充值额度不可超过所持资源上限！";
                                    GUILayout.Label(test, new GUILayoutOption[0]);
                                    GUILayout.EndVertical();
                                }
                            }

                            if (settings.no)
                            {
                                int taiu = DateFile.instance.MianActorID();
                                UIDate.instance.ChangeResource(taiu, 1, DateFile.instance.actorLife[10001][79][0], true);
                                UIDate.instance.ChangeResource(taiu, 2, DateFile.instance.actorLife[10001][79][1], true);
                                UIDate.instance.ChangeResource(taiu, 3, DateFile.instance.actorLife[10001][79][2], true);
                                UIDate.instance.ChangeResource(taiu, 5, DateFile.instance.actorLife[10001][79][3], true);
                                DateFile.instance.actorLife[10001].Remove(79);
                            }
                        }
                    }
                    else
                    {
                        GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                        GUILayout.Space(8);
                        GUILayout.Label("好懒，不想充值……我堂堂太吾传人难道还付不起修理费吗？！修就对啦！！！", new GUIStyle { normal = { textColor = new Color(0.999999f, 0.537255f, 0.537255f) } }, new GUILayoutOption[0]);
                        settings.bymoney = GUILayout.Toggle(settings.bymoney, "以银两支付修理费用（会消耗50%额外手续费）", new GUILayoutOption[0]);
                        GUILayout.EndVertical();
                    }

                }

            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    public static class Autofix
    {
        //1,2,3,5 ----  0,1,2,3
        public static bool Buy(int index, int num)
        {
            int taiu = DateFile.instance.MianActorID();
            int type = index == 3 ? index + 2 : index + 1;
            int num0 = DateFile.instance.ActorResource(taiu)[type];
            if (num0 < num) return false;
            else
            {
                UIDate.instance.ChangeResource(taiu, type, -num, true);
                DateFile.instance.actorLife[10001][79][index] += num;
                return true;
            }
        }

        public static int Charge(int id, int maxhp)
        {
            int hp = int.Parse(DateFile.instance.GetItemDate(id, 901, true));
            if (hp >= maxhp) return 0;
            else
            {
                int basic = int.Parse(DateFile.instance.GetItemDate(id, 45, true)) * int.Parse(DateFile.instance.GetItemDate(id, 49, true)) / 375;
                int perc = (maxhp - hp) * basic * 15 / maxhp;
                if (hp != 0) return perc;
                else return perc + basic * 20;
            }
        }

        public static int Getpoint(int index)
        {
            if (DateFile.instance.HaveLifeDate(10001, 79) && index<4)
                return DateFile.instance.actorLife[10001][79][index];
            else return 0;
        }


        public static void LazyBone(List<int> people, List<int> type)
        {
            foreach (int key in people)
            {
                foreach (int part in type)
                {
                    int partid = int.Parse(DateFile.instance.GetActorDate(key, 301 + part, true));
                    if (partid > 0)
                    {
                        var maxhp = DateFile.instance.GetItemDate(partid, 902, true);
                        var flag = DateFile.instance.GetItemDate(partid, 4, true);
                        var flag2 = DateFile.instance.GetItemDate(partid, 49, true);
                        int Rtype = int.Parse(DateFile.instance.GetItemDate(partid, 506, true));//0织 1金 2木 3玉---2 1 0 1
                        if (maxhp != "0" && flag == "4" && flag2 != "0" && Rtype < 4)
                        {
                            int charge = Charge(partid, int.Parse(maxhp));
                            if (charge != 0)
                            {
                                int index = 0;
                                switch (Rtype)
                                {
                                    case 0:  index = 2; break; 
                                    case 1:  index = 1; break; 
                                    case 2:  index = 0; break; 
                                    case 3:  index = 1; break; 
                                }


                                if (Main.settings.card == 0)
                                {
                                    int num0 = Getpoint(index);
                                    if (num0 >= charge)
                                    {
                                        GameData.Items.SetItemProperty(partid, 901, maxhp);
                                        DateFile.instance.actorLife[10001][79][index] = num0 - charge;
                                        
                                    }
                                    else if (Getpoint(3) >= (charge + charge / 2))
                                    {
                                        GameData.Items.SetItemProperty(partid, 901, maxhp);
                                        DateFile.instance.actorLife[10001][79][3] = Getpoint(3) - charge - charge / 2;
                                    }

                                }
                                else
                                {
                                    int taiu = DateFile.instance.MianActorID();
                                    if (Main.settings.bymoney)
                                    { index = 3; charge = charge + charge / 2; }

                                    int type2 = index == 3 ? index + 2 : index + 1;
                                    int num0 = DateFile.instance.ActorResource(taiu)[type2];
                                    if (num0 >= charge)
                                    {
                                        GameData.Items.SetItemProperty(partid, 901, maxhp);
                                        UIDate.instance.ChangeResource(taiu, type2, -charge, false);

                                    }
                                }
                            }

                        }
                    }
                }
            }
            
        }
    }

    [HarmonyPatch(typeof(BattleEndWindow), "ShowBattleEndWindow")]
    // public static class BattleSystem_BattleEndShowOver_Patch
    public static class BattleEndWindow_ShowBattleEndWindow_Patch
    {

        private static void Postfix()
        {
            if (!Main.enabled || !Main.settings.open)
            {
                return;
            }
            if(Main.settings. card==0 && Autofix.Getpoint(0)==0 && Autofix.Getpoint(1) == 0 && Autofix.Getpoint(2) == 0 && Autofix.Getpoint(3) == 0)
            {
                return; 
            }
            List<int> part = new List<int>();
            if (Main.settings.weapon) part=new List<int> { 0, 1, 2 };
            if (Main.settings.hat) part.Add(3);
            if (Main.settings.armor) part.Add(5);
           if (Main.settings.shouse) part.Add(6);
            if (Main.settings.pearl) part.AddRange (new List<int> { 4 ,7, 8, 9 ,10});
            if (part.Count == 0) return;
            List<int> people = new List<int> { DateFile.instance.MianActorID() };
            if (Main.settings.familiy) people.AddRange(DateFile.instance.GetFamily(false, false));
           
            Autofix.LazyBone(people,part);
        }
    }
   
}

    