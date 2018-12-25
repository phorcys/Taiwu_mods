using System.Reflection;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace TaiwuHentai
{

    public class Settings : UnityModManager.ModSettings
    {
        public bool displayGlamour = false;
        public bool unrestrainedPartner = false;
        public bool unrestrainedSpouse = false;
        public bool unrestrainedSpouseFactions = false;
        public uint SpouseAge = 14;
        public int GetLoveHaveLover = 2;
        public int GetLoveHaveSpouse = 2;
        public bool MoreLottery = false;
        public bool PeerTeaching = false;
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

            GUILayout.BeginHorizontal("Box");
            settings.displayGlamour = GUILayout.Toggle(settings.displayGlamour, "是否显示 人物18岁时的魅力值立绘与特性 ");
            GUILayout.Label("说明： 选中以显示儿童与婴儿的魅力值与成长之后的立绘，注意此功能处于试用阶段，建议于回合结束前关闭此功能，以防发生意外。");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.unrestrainedPartner = GUILayout.Toggle(settings.unrestrainedPartner, "太污是否能邀请两情相悦的npc为同道");
            GUILayout.Label("说明： 选中以让太污可邀请两情相悦的npc为同道。");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.unrestrainedSpouse = GUILayout.Toggle(settings.unrestrainedSpouse, "太污是否能多夫多妻");
            GUILayout.Label("说明： 选中以解除太污的一夫一妻限制。");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.unrestrainedSpouseFactions = GUILayout.Toggle(settings.unrestrainedSpouseFactions, "是否解除门派婚姻限制");
            GUILayout.Label("说明： 选中以解除门派的求婚限制，即璇女少林的npc也可求婚（他接不接受就不一定了）。");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.PeerTeaching = GUILayout.Toggle(settings.PeerTeaching, "修改可请教同伴天赋70以上的技艺");
            GUILayout.Label("说明： 选中修改使太吾可以可请教同伴天赋70以上的技艺。");
            GUILayout.EndHorizontal();
            /*
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("提前求爱与求婚的年龄，大于切不等于该年龄的npc能求婚当然太污自己也要大于这个年龄");
            var maxBackupsToKeep = GUILayout.TextField(settings.SpouseAge.ToString(), 3);
            if (GUI.changed && !uint.TryParse(maxBackupsToKeep, out settings.SpouseAge))
            {
                settings.SpouseAge = 14;
            }
            GUILayout.EndHorizontal();*/
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("有情人的npc，新欢概率", new GUILayoutOption[0]);
            Main.settings.GetLoveHaveLover = GUILayout.SelectionGrid(Main.settings.GetLoveHaveLover, new string[]
            {
                "0倍",
                "0.5倍",
                "1倍",
                "1.5倍",
                "2倍"
            }, 5, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("已婚的npc，新欢概率", new GUILayoutOption[0]);
            Main.settings.GetLoveHaveSpouse = GUILayout.SelectionGrid(Main.settings.GetLoveHaveSpouse, new string[]
            {
                "0倍",
                "0.5倍",
                "1倍",
                "1.5倍",
                "2倍"
            }, 5, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("Box");
            settings.MoreLottery = GUILayout.Toggle(settings.MoreLottery, "是否强化无量金刚宗的贡品搜集功能 ");
            GUILayout.Label("说明： 选中以强化无量金刚宗的贡品搜集，使之能搜集生产用的引子。开关此功能需要重启游戏");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    [HarmonyPatch(typeof(DateFile), "GetActorDate")]
    public static class DateFile_GetActorDate_Patch
    {
        private static void Postfix(DateFile __instance, ref string __result, int id, int index, bool addValue = true)
        {

            if (!Main.enabled || !Main.settings.displayGlamour)
            {
                return;
            }
            if (index == 15)
            {
                string text = (!__instance.presetActorDate.ContainsKey(id) || !__instance.presetActorDate[id].ContainsKey(index)) ? "0" : __instance.presetActorDate[id][index];
                if (__instance.actorsDate.ContainsKey(id))
                {
                    if (__instance.actorsDate[id].ContainsKey(index))
                    {
                        text = __instance.actorsDate[id][index];
                    }
                    else
                    {
                        int key = (!__instance.actorsDate[id].ContainsKey(997)) ? id : int.Parse(__instance.actorsDate[id][997]);
                        if (!__instance.presetActorDate.ContainsKey(key) || !__instance.presetActorDate[key].ContainsKey(index))
                        {
                            __result = "0";
                        }
                        text = __instance.presetActorDate[key][index];
                    }
                }

                int num = 0;
                if (addValue)
                {
                    Type type = __instance.GetType();
                    object[] temp = new object[] { id, index };
                    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                    var m = type.GetMethod("ActorAddValue", flags);
                    num = (int)m.Invoke(__instance, temp);
                    if (int.Parse(__instance.GetActorDate(id, 8, false)) == 1 && int.Parse(__instance.GetActorDate(id, 305, false)) == 0)
                    {
                        __result = (int.Parse((num == 0) ? text : (int.Parse(text) + num).ToString()) * 50 / 100).ToString();
                    }
                    else
                    {

                        string text2 = (num == 0) ? text : (int.Parse(text) + num).ToString();

                        __result = text2;
                    }

                }
            }
        }




    }
    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        // Token: 0x06000008 RID: 8 RVA: 0x00002168 File Offset: 0x00000368
        private static void Postfix(WindowManage __instance, ref GameObject tips, ref Text ___itemMoneyText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
        {
            if (!Main.enabled)
            {
                return;
            }

            if ((tips != null))
            {
                Transform oldMainHoder = WindowManage.instance.informationWindow.transform.Find("AgeMianface");
                if (oldMainHoder)
                {
                    UnityEngine.Object.Destroy(oldMainHoder.gameObject);
                }

                string[] array = tips.name.Split(new char[]
                {
                    ','
                });
                Main.Logger.Log(array[0]);
                if (array[0] == "HelpIcon" && Main.settings.displayGlamour)
                {
                    int key = ActorMenu.instance.acotrId;
                    bool flageIsShow = false;
                    //Main.Logger.Log(key.ToString());
                    if (DateFile.instance.actorsDate.ContainsKey(key))
                    {
                        flageIsShow = DateFile.instance.actorsDate[key].ContainsKey(997);
                        Main.Logger.Log(flageIsShow.ToString());
                    }
                    if (flageIsShow)
                    {


                        Text text = ___informationMassage;
                        Text text2 = text;
                        text2.text += "\n";

                        //start


                        String charmText = ((int.Parse(DateFile.instance.GetActorDate(key, 11, false)) >= 0) ? ((int.Parse(DateFile.instance.GetActorDate(key, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(key, 14, false)) - 1].Split(new char[]
                {
                        '|'
                })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
                {
                        '|'
                })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
                {
                        '|'
                })[0]);
                        text2.text += "18岁时:\n魅力：" + charmText + "\n";


                        List<int> featureIDs = DateFile.instance.GetActorFeature(key);
                        for (int i = 0; i < featureIDs.Count; i++)
                        {

                            text2.text += DateFile.instance.actorFeaturesDate[featureIDs[i]][0] + " ";
                            if ((i + 1) % 3 == 0)
                            {
                                text2.text += "\n";
                            }


                        }
                        text2.text += "\n\n\n\n\n\n";
                        string[] arrayA = DateFile.instance.GetActorDate(key, 995, false).Split(new char[]
                    {
                        '|'
                    });
                        string[] array2 = DateFile.instance.GetActorDate(key, 996, false).Split(new char[]
                        {
                        '|'
                        });
                        int[] array3 = new int[arrayA.Length];
                        int[] array4 = new int[array2.Length];
                        for (int i = 0; i < arrayA.Length; i++)
                        {
                            array3[i] = int.Parse(arrayA[i]);
                        }
                        for (int j = 0; j < array2.Length; j++)
                        {
                            array4[j] = int.Parse(array2[j]);
                        }
                        int num = int.Parse(DateFile.instance.GetActorDate(key, 305, false));
                        int clotheIndex = (num <= 0) ? -1 : int.Parse(DateFile.instance.GetItemDate(num, 15, true));






                        //end

                        if (array3.Length != 1)
                        {
                            ActorFace mainHoder = UnityEngine.Object.Instantiate(ActorMenu.instance.mianActorFace, new Vector3(100f, 30f, 1), Quaternion.identity);
                            mainHoder.transform.SetParent(WindowManage.instance.informationWindow.GetComponent<RectTransform>(), false);
                            mainHoder.name = "AgeMianface";
                            float scX = mainHoder.transform.parent.localScale.x;
                            float scY = mainHoder.transform.parent.localScale.y;
                            scX = 1 / scX;
                            scY = 1 / scY;

                            mainHoder.transform.localScale = new Vector3(scX * 0.3f, scY * 0.45f, 1);

                            editFac(ref mainHoder, key, 18, int.Parse(DateFile.instance.GetActorDate(key, 14, false)), int.Parse(DateFile.instance.GetActorDate(key, 17, false)), array3, array4, clotheIndex);
                        }

                    }
                }

            }

        }
        public static bool editFac(ref ActorFace actorFace, int actorId, int age, int gender, int actorGenderChange, int[] faceDate, int[] faceColor, int clotheIndex)
        {
            int num = (actorGenderChange == 0) ? (gender - 1) : ((gender != 1) ? 0 : 1);
            int key2 = Mathf.Min(faceDate[0], int.Parse(GetSprites.instance.actorFaceDate[num][99]) - 1);
            bool flag = actorId == -1 || int.Parse(DateFile.instance.GetActorDate(actorId, 26, false)) == 0;
            Dictionary<int, Dictionary<int, List<Sprite[]>>> dictionary2 = (!actorFace.smallSize) ? GetSprites.instance.actorFace : GetSprites.instance.actorFaceSmall;
            actorFace.ageImage.gameObject.SetActive(flag);
            actorFace.body.gameObject.SetActive(true);
            actorFace.nose.gameObject.SetActive(flag);
            actorFace.faceOther.gameObject.SetActive(flag);
            actorFace.eye.gameObject.SetActive(flag);
            actorFace.eyePupil.gameObject.SetActive(flag);
            actorFace.eyebrows.gameObject.SetActive(flag);
            actorFace.mouth.gameObject.SetActive(flag);
            actorFace.beard.gameObject.SetActive(gender == 1 && num == 0 && age >= 20);
            actorFace.hair1.gameObject.SetActive(flag || (!flag && faceDate[7] == 15));
            actorFace.hair2.gameObject.SetActive(flag || (!flag && faceDate[7] == 15));
            actorFace.hairOther.gameObject.SetActive(flag || (!flag && faceDate[7] == 15));
            actorFace.clothes.gameObject.SetActive(true);
            actorFace.clothesColor.gameObject.SetActive(true);
            actorFace.ageImage.sprite = dictionary2[num][key2][0][0];
            actorFace.body.sprite = dictionary2[num][key2][1][(!flag) ? 1 : 0];
            actorFace.nose.sprite = dictionary2[num][key2][2][faceDate[1]];
            actorFace.faceOther.sprite = dictionary2[num][key2][3][faceDate[2]];
            actorFace.eye.sprite = dictionary2[num][key2][4][faceDate[3]];
            actorFace.eyePupil.sprite = dictionary2[num][key2][5][faceDate[3]];
            actorFace.eyebrows.sprite = dictionary2[num][key2][6][faceDate[4]];
            actorFace.mouth.sprite = dictionary2[num][key2][7][faceDate[5]];
            if (actorFace.beard.gameObject.activeSelf)
            {
                actorFace.beard.sprite = dictionary2[0][key2][8][faceDate[6]];
            }
            actorFace.hair1.sprite = dictionary2[num][key2][9][faceDate[7]];
            actorFace.hair2.sprite = dictionary2[num][key2][12][faceDate[7]];
            actorFace.hairOther.sprite = dictionary2[num][key2][13][faceDate[7]];
            if (clotheIndex != -1)
            {
                actorFace.clothes.sprite = dictionary2[num][key2][10][clotheIndex];
                actorFace.clothesColor.sprite = dictionary2[num][key2][11][clotheIndex];
            }
            else
            {
                actorFace.clothes.sprite = dictionary2[num][key2][10][0];
                actorFace.clothesColor.sprite = dictionary2[num][key2][11][0];
            }
            actorFace.body.color = ((!flag) ? new Color(1f, 1f, 1f, 1f) : DateFile.instance.faceColor[0][faceColor[0]]);
            actorFace.nose.color = DateFile.instance.faceColor[0][faceColor[0]];
            actorFace.eyebrows.color = DateFile.instance.faceColor[1][faceColor[1]];
            actorFace.eyePupil.color = DateFile.instance.faceColor[2][faceColor[2]];
            actorFace.mouth.color = DateFile.instance.faceColor[3][faceColor[3]];
            if (actorFace.beard.gameObject.activeSelf)
            {
                actorFace.beard.color = DateFile.instance.faceColor[4][faceColor[4]];
            }
            actorFace.hair1.color = DateFile.instance.faceColor[6][faceColor[6]];
            actorFace.hair2.color = DateFile.instance.faceColor[6][faceColor[6]];
            actorFace.faceOther.color = DateFile.instance.faceColor[5][faceColor[5]];
            actorFace.clothesColor.color = DateFile.instance.faceColor[7][faceColor[7]];
            actorFace.hairOther.color = DateFile.instance.faceColor[7][faceColor[7]];
            return actorFace;
        }

    }
    /**
    [HarmonyPatch(typeof(ActorMenu), "SetActorAttr")]
    public static class ActorMenu_SetActorAttr_Patch
    {

        private static void Postfix(ActorMenu __instance, int key, ref ActorFace ___mianActorFace)
        {
            if (!Main.enabled || !Main.settings.displayGlamour)
            {
                return;
            }

            if (__instance.actorMenuIndex == 1)
            {
                Transform Holder = __instance.mianActorFace.gameObject.transform;
                int child = Holder.childCount;
                for (int i = 0; i < child; i++)
                {
                    Holder.GetChild(i).gameObject.AddComponent<PointerEnter>();
                }
                __instance.charmText.text = ((int.Parse(DateFile.instance.GetActorDate(key, 11, false)) >= 0) ? ((int.Parse(DateFile.instance.GetActorDate(key, 305, false)) != 0) ? DateFile.instance.massageDate[25][int.Parse(DateFile.instance.GetActorDate(key, 14, false)) - 1].Split(new char[]
            {
                        '|'
            })[Mathf.Clamp(int.Parse(DateFile.instance.GetActorDate(key, 15, true)) / 100, 0, 9)] : DateFile.instance.massageDate[25][5].Split(new char[]
            {
                        '|'
            })[1]) : DateFile.instance.massageDate[25][5].Split(new char[]
            {
                        '|'
            })[0]);

                //start
                string[] array = DateFile.instance.GetActorDate(key, 995, false).Split(new char[]
            {
                        '|'
            });
                string[] array2 = DateFile.instance.GetActorDate(key, 996, false).Split(new char[]
                {
                        '|'
                });
                int[] array3 = new int[array.Length];
                int[] array4 = new int[array2.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array3[i] = int.Parse(array[i]);
                }
                for (int j = 0; j < array2.Length; j++)
                {
                    array4[j] = int.Parse(array2[j]);
                }
                int num = int.Parse(DateFile.instance.GetActorDate(key, 305, false));
                int clotheIndex = (num <= 0) ? -1 : int.Parse(DateFile.instance.GetItemDate(num, 15, true));



                int age = int.Parse(DateFile.instance.GetActorDate(key, 11, false));
                bool flageIsShow = !DateFile.instance.actorsDate[key].ContainsKey(997);
                if (!Main.settings.displayGlamour || !flageIsShow)
                {
                    ___mianActorFace.SetActorFace(key);

                }
                else
                {

                    //editFac(ref ___mianActorFace, key, 18, int.Parse(DateFile.instance.GetActorDate(key, 14, false)), int.Parse(DateFile.instance.GetActorDate(key, 17, false)), array3, array4, clotheIndex);

                }

                //TweenSettingsExtensions.SetUpdate<Tweener>(TweenSettingsExtensions.SetDelay<Tweener>(ShortcutExtensions46.DOColor(___mianActorFace.transform.GetChild(0).GetComponent<Image>(), new Color(0.1f, 0.1f, 0.1f), 0.1f), 0.1f), true);

            }
        }




    }**/
    [HarmonyPatch(typeof(MassageWindow), "SetMassageWindow")]
    public static class MassageWindow_SetMassageWindow_Patch
    {
        public static bool Prefix(ref MassageWindow __instance, int[] baseEventDate, int chooseId, ref Dictionary<int, int> ___massageGetGongFas)
        {
            if (chooseId == 901000006 && Main.enabled && Main.settings.PeerTeaching)
            {
                for (int i = 0; i < __instance.chooseHolder.childCount; i++)
                {
                    UnityEngine.Object.Destroy(__instance.chooseHolder.GetChild(i).gameObject);
                }
                __instance.massageActors.Clear();
                __instance.mianEventDate = (int[])baseEventDate.Clone();
                __instance.massageActors.Add(__instance.mianEventDate[1]);
                int num = __instance.mianEventDate[2];
                int num2 = int.Parse(DateFile.instance.eventDate[num][2]);
                //太污的id
                int num3 = DateFile.instance.MianActorID();
                //目标角色的id

                int num4 = (num2 != 0) ? ((num2 != -1) ? num2 : num3) : __instance.mianEventDate[1];
                int num5 = int.Parse(DateFile.instance.GetActorDate(num4, 19, false));
                List<string> list2 = new List<string>();
                List<string> list3 = new List<string>();
                list2.Clear();
                list3.Clear();
                int gangValueId2 = DateFile.instance.GetGangValueId(num5, int.Parse(DateFile.instance.GetActorDate(num4, 20, false)));
                if (num == 9334 && gangValueId2 == 1)
                {

                    //int gangValueId = DateFile.instance.GetGangValueId(num5, int.Parse(DateFile.instance.GetActorDate(num4, 20, false)));
                    for (int i = 0; i < 16; i++)
                    {

                        if (int.Parse(DateFile.instance.GetActorDate(num4, 501 + i, true)) > 69)
                        {


                            if (i < 6)
                            {
                                list2.Add((931900001 + i).ToString());
                            }
                            if (i == 6 || i == 7)
                            {
                                list2.Add((932200001 + (i - 6)).ToString());
                            }
                            if (i == 8 || i == 9)
                            {
                                list2.Add((932900001 + (i - 8)).ToString());
                            }
                            if (i == 10 || i == 11)
                            {
                                list2.Add((932200003 + (i - 10)).ToString());
                            }
                            if (i > 11)
                            {
                                list2.Add((932300001 + (i - 12)).ToString());
                            }
                        }


                    }
                    // Main.Logger.Log(DateFile.instance.GetActorDate(num4,));

                    list2.Add("900700001");
                    AddToList(ref __instance, ref ___massageGetGongFas, num, num3, num4, list2, list3);
                    return false;
                }


            }

            if (Main.enabled && chooseId == 901200001)
            {
                if (!Main.enabled || !Main.settings.MoreLottery)
                {
                    return true;
                }


                if (DateFile.instance.eventDate[931100001][8].Equals("TIME&10|END&90101&8|AGV&-200"))
                {

                    Main.Logger.Log("开始搜集贡品注入");
                    DateFile.instance.eventDate[931100001][8] = "TIME&10|AGV&-200";
                    DateFile.instance.eventDate[931100001][7] = "971200100";
                    Dictionary<int, string> temp = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取金银" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&8" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp2 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取奇铁" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&21" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp3 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取良木" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&22" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp4 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取美玉" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&23" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp5 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取锦缎" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&24" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp6 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取灵药" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&25" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp7 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取诡物" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&26" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    Dictionary<int, string> temp8 = new Dictionary<int, string> { { 1, "" }, { 2, "-1" }, { 3, "收取珍馔" }, { 4, "1" }, { 5, "" }, { 6, "" }, { 7, "9354" }, { 8, "END&90101&27" }, { 9, "0" }, { 10, "" }, { 11, "0" } };
                    // Dictionary<int, string> temp9 = new Dictionary<int, string> { { 0, "贡品选择" }, { 1, "0" }, { 2, "0" }, { 3, "我这便让手下去通知信众，那么……太吾想要什么贡品呢?" }, { 4, "1" }, { 5, "971200101|971200102|971200103|971200104|971200105|971200106|971200107|971200108" }, { 9, "0" }, { 11, "0" } };
                    Dictionary<int, string> temp9 = new Dictionary<int, string>();
                    foreach (var item in DateFile.instance.eventDate[9351])
                    {
                        temp9.Add(item.Key, item.Value);
                    }
                    temp9[3] = "我这便让手下去通知信众，那么……太吾想要什么贡品呢?";
                    temp9[5] = "971200101|971200102|971200103|971200104|971200105|971200106|971200107|971200108";
                    DateFile.instance.eventDate.Add(971200100, temp9);
                    DateFile.instance.eventDate.Add(971200101, temp);
                    DateFile.instance.eventDate.Add(971200102, temp2);
                    DateFile.instance.eventDate.Add(971200103, temp3);
                    DateFile.instance.eventDate.Add(971200104, temp4);
                    DateFile.instance.eventDate.Add(971200105, temp5);
                    DateFile.instance.eventDate.Add(971200106, temp6);
                    DateFile.instance.eventDate.Add(971200107, temp7);
                    DateFile.instance.eventDate.Add(971200108, temp8);

                    Main.Logger.Log("完成搜集贡品注入");

                }


            }
            return true;
        }

        public static void Postfix(ref MassageWindow __instance, int chooseId, ref Dictionary<int, int> ___massageGetGongFas)
        {
            if (!Main.enabled)
            {
                return;
            }
            int num = __instance.mianEventDate[2];

            int num2 = int.Parse(DateFile.instance.eventDate[num][2]);

            //太污的id
            int num3 = DateFile.instance.MianActorID();
            //目标角色的id

            int num4 = (num2 != 0) ? ((num2 != -1) ? num2 : num3) : __instance.mianEventDate[1];

            //目标角色身分组

            int num5 = int.Parse(DateFile.instance.GetActorDate(num4, 19, false));

            //是否是太污村的人
            bool flag2 = num5 == 16;
            List<string> list2 = new List<string>();
            List<string> list3 = new List<string>();
            list2.Clear();
            list3.Clear();
            if (Main.enabled && chooseId > 0)
            {
                int num10 = int.Parse(DateFile.instance.eventDate[chooseId][7]);
                if ((num10 + 9008) == 2)
                {




                    //目标年龄
                    int num14 = int.Parse(DateFile.instance.GetActorDate(num4, 11, false));
                    //目标身份等级
                    int num17 = Mathf.Abs(int.Parse(DateFile.instance.GetActorDate(num4, 20, false)));
                    //太污年龄
                    int num18 = int.Parse(DateFile.instance.GetActorDate(num3, 11, false));
                    list2.Clear();
                    bool flage_TONGDAO = (flag2 || DateFile.instance.GetActorSocial(num3, 302, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 303, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 308, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 309, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 310, false, false).Contains(num4));
                    bool should_TONGDAO = (flag2 || DateFile.instance.GetActorSocial(num3, 302, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 303, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 308, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 306, false, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 310, false, false).Contains(num4));

                    if (!flage_TONGDAO && should_TONGDAO && Main.settings.unrestrainedPartner)
                    {

                        //倾诉爱意即可邀为同道
                        list2.Add("900300001");
                    }
                    //判断是否足够出现亲密选项
                    bool flage_QINGMI = DateFile.instance.GetLifeDate(num3, 601, 0) != DateFile.instance.GetLifeDate(num4, 601, 0) && DateFile.instance.GetLifeDate(num3, 602, 0) != DateFile.instance.GetLifeDate(num4, 602, 0);

                    if (flage_QINGMI)
                    {
                        bool flage_QINGSUAIYI = !DateFile.instance.GetActorSocial(num3, 304, false, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 306, false, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 308, false, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 309, false, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 311, false, false).Contains(num4);

                        if (!(num18 > 14 && num14 > 14 && flage_QINGSUAIYI))
                        {
                            if (num18 > Main.settings.SpouseAge && num14 > Main.settings.SpouseAge && flage_QINGSUAIYI)
                            {
                                list2.Add("900600003");
                            }
                        }
                        bool flage_GONGJIELIANLI = (DateFile.instance.GetActorSocial(num3, 306, false, false).Contains(num4) && DateFile.instance.GetActorSocial(num3, 309, false, false).Count <= 0 && DateFile.instance.GetActorSocial(num4, 309, false, false).Count <= 0 && int.Parse(DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num5, num17)][803]) != 0 && int.Parse(DateFile.instance.GetActorDate(num3, 2, false)) == 0 && int.Parse(DateFile.instance.GetActorDate(num4, 2, false)) == 0);
                        bool should_GONGJIELIANLI = DateFile.instance.GetActorSocial(num3, 306, false, false).Contains(num4) && int.Parse(DateFile.instance.GetActorDate(num3, 2, false)) == 0 && int.Parse(DateFile.instance.GetActorDate(num4, 2, false)) == 0 && DateFile.instance.GetActorSocial(num4, 309, false, false).Count <= 0;
                        bool flage_MENPAIJIANCHA = int.Parse(DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num5, num17)][803]) != 0;
                        bool flage_DUOFUDUOQI = DateFile.instance.GetActorSocial(num3, 309, false, false).Count <= 0;

                        if (!(num18 > 14 && num14 > 14 && flage_GONGJIELIANLI))
                        {
                            bool flagemarriageA = true;
                            bool flagemarriageB = true;
                            if (!Main.settings.unrestrainedSpouseFactions)
                            {
                                flagemarriageA = flage_MENPAIJIANCHA;
                            }
                            if (!Main.settings.unrestrainedSpouse)
                            {
                                flagemarriageB = flage_DUOFUDUOQI;
                            }
                            bool flagemarriage = flagemarriageA && flagemarriageB;
                            if (num18 > Main.settings.SpouseAge && num14 > Main.settings.SpouseAge && should_GONGJIELIANLI)
                            {
                                list2.Add("900600004");
                            }

                        }
                    }
                }

                if ((num10 + 9008) == 4 && Main.settings.PeerTeaching)
                {
                    int gangValueId2 = DateFile.instance.GetGangValueId(num5, int.Parse(DateFile.instance.GetActorDate(num4, 20, false)));
                    if (gangValueId2 == 1)
                    {

                        list2.Add("901000006");

                    }

                }
                if (list2.Count > 0)
                {
                    AddToList(ref __instance, ref ___massageGetGongFas, num, num3, num4, list2, list3);
                }


            }


        }

        private static bool AddToList(ref MassageWindow __instance, ref Dictionary<int, int> ___massageGetGongFas, int num, int num3, int num4, List<string> list2, List<string> list3)
        {
            
            int num24 = 0;
            for (int num25 = 0; num25 < list2.Count; num25++)
            {
                bool flag6 = list3.Contains(list2[num25]);
                int num26 = int.Parse(list2[num25]);
                if (!__instance.removeChooseIds.Contains(num26))
                {
                    if (___massageGetGongFas.Count > 0)
                    {
                        bool flag7 = false;
                        string[] array3 = DateFile.instance.eventDate[num26][6].Split(new char[]
                        {
                    '&'
                        });
                        if (array3[0] == "GTYP")
                        {
                            int num27 = int.Parse(array3[1]);
                            List<int> list7 = new List<int>(___massageGetGongFas.Keys);
                            for (int num28 = 0; num28 < list7.Count; num28++)
                            {
                                int key = list7[num28];
                                if (int.Parse(DateFile.instance.gongFaDate[key][1]) == num27)
                                {
                                    flag7 = true;
                                    break;
                                }
                            }
                            if (!flag7)
                            {
                                goto IL_1B43;
                            }
                        }
                    }
                    int num29 = int.Parse(DateFile.instance.eventDate[num26][2]);
                    int num30 = num29;
                    if (num29 == -1)
                    {
                        num30 = num3;
                    }
                    else if (num29 != -99 && num29 < 0)
                    {
                        num30 = __instance.mianEventDate[Mathf.Abs(num29)];
                    }
                    int num31 = -1;
                    GameObject gameObject = Object.Instantiate<GameObject>((num30 == -99) ? __instance.massageChoose2 : __instance.massageChoose1, Vector3.zero, Quaternion.identity);
                    gameObject.transform.SetParent(__instance.chooseHolder, false);
                    gameObject.name = "Choose," + num26;
                    if (num30 != -99)
                    {
                        gameObject.transform.Find("NameText").GetComponent<Text>().text = DateFile.instance.GetActorName(num30, false, false) + WindowManage.instance.Mut();
                        gameObject.transform.Find("FaceHolder").Find("FaceMask").Find("MianActorFace").GetComponent<ActorFace>().SetActorFace(num30, false);
                        GameObject gameObject2 = gameObject.transform.Find("IconHolder").Find("NeedIcon").gameObject;
                        string a = DateFile.instance.eventDate[num26][6].Replace("|GN&0", "").Replace("|GN&1", "").Replace("|GN&2", "").Replace("|GN&3", "").Replace("|GN&4", "").Replace("GN&0|", "").Replace("GN&1|", "").Replace("GN&2|", "").Replace("GN&3|", "").Replace("GN&4|", "").Replace("GN&0", "").Replace("GN&1", "").Replace("GN&2", "").Replace("GN&3", "").Replace("GN&4", "");
                        gameObject2.SetActive(a != "" && a != "0");
                        gameObject2.name = "NeedIcon," + num26;
                        a = DateFile.instance.eventDate[num26][6];
                        if (a != "" && a != "0")
                        {
                            string[] array4 = DateFile.instance.eventDate[num26][6].Split(new char[]
                            {
                        '|'
                            });
                            for (int num32 = 0; num32 < array4.Length; num32++)
                            {
                                string[] array5 = array4[num32].Split(new char[]
                                {
                            '#'
                                });
                                for (int num33 = 0; num33 < array5.Length; num33++)
                                {
                                    string[] array6 = array5[num33].Split(new char[]
                                    {
                                '&'
                                    });
                                    string text = array6[0];
                                    if (text != null)
                                    {
                                        if (text == "GN")
                                        {
                                            num31 = int.Parse(array6[1]);
                                            gameObject.transform.Find("IconHolder").Find("LikeIcon,774").gameObject.SetActive(DateFile.instance.GetActorGoodness(num3) == num31);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    bool flag8 = false;
                    string[] array7 = DateFile.instance.eventDate[num26][8].Split(new char[]
                    {
                '|'
                    });
                    for (int num34 = 0; num34 < array7.Length; num34++)
                    {
                        if (array7[num34] == "RM")
                        {
                            flag8 = true;
                            break;
                        }
                    }
                    if (flag6)
                    {

                        gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(10001, string.Format("({0}).{1}", __instance.massageKeyCodeName[num24], ChangeText(ref __instance,num, DateFile.instance.eventDate[num26][3], true)), false);
                        gameObject.GetComponent<Button>().interactable = false;
                    }
                    else if (GetEventIF(ref __instance, num30, num4, num26))
                    {
                       
                        string text2 = ChangeText(ref __instance, num, DateFile.instance.eventDate[num26][3], true);
                        if (num31 >= 0)
                        {
                            text2 = string.Format("{2}{0}{3}{1}", new object[]
                            {
                        DateFile.instance.massageDate[9][0].Split(new char[]
                        {
                            '|'
                        })[num31],
                        text2,
                        DateFile.instance.massageDate[10][0].Split(new char[]
                        {
                            '|'
                        })[0],
                        DateFile.instance.massageDate[10][0].Split(new char[]
                        {
                            '|'
                        })[1]
                            });
                        }
                        gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer((!flag8) ? 20003 : 20005, string.Format("({0}).{1}", __instance.massageKeyCodeName[num24], text2), false);
                        gameObject.GetComponent<Button>().interactable = true;

                    }
                    else
                    {

                        gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(10001, string.Format("({0}).{1}", __instance.massageKeyCodeName[num24], ChangeText(ref __instance, num, DateFile.instance.eventDate[num26][3], true)), false);
                        gameObject.GetComponent<Button>().interactable = false;
                    }
                    num24++;
                }
                IL_1B43:;
            }
            return true;
        }

        private static bool GetEventIF(ref MassageWindow __instance, int actorId, int eventActor, int eventId)
        {
            bool result;
            if (__instance.testAllChoose)
            {
                result = true;
            }
            else
            {
                bool flag = true;
                string a = DateFile.instance.eventDate[eventId][6];
                if (a != "" && a != "0")
                {
                    flag = true;
                    string[] array = DateFile.instance.eventDate[eventId][6].Split(new char[]
                    {
                    '|'
                    });
                    bool[] array2 = new bool[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array3 = array[i].Split(new char[]
                        {
                        '#'
                        });
                        bool[] array4 = new bool[array3.Length];
                        for (int j = 0; j < array3.Length; j++)
                        {
                            array4[j] = true;
                            string[] array5 = array3[j].Split(new char[]
                            {
                            '&'
                            });
                            string text = array5[0];
                            switch (text)
                            {
                                case "ATTR":
                                    {
                                        int index = int.Parse(array5[1]);
                                        int num2 = int.Parse(array5[2]);
                                        int num3 = int.Parse(array5[3]);
                                        int num4 = int.Parse(DateFile.instance.GetActorDate(actorId, index, true));
                                        if (num4 < num2 || num4 > num3)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "ATTB":
                                    {
                                        int index2 = int.Parse(array5[1]);
                                        int num5 = int.Parse(array5[2]);
                                        if (int.Parse(DateFile.instance.GetActorDate(actorId, index2, true)) != num5)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "ATTMAX":
                                    {
                                        int index3 = int.Parse(array5[1]);
                                        int num6 = int.Parse(array5[2]);
                                        if (int.Parse(DateFile.instance.GetActorDate(actorId, index3, true)) < num6)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "ATTMIN":
                                    {
                                        int index4 = int.Parse(array5[1]);
                                        int num7 = int.Parse(array5[2]);
                                        if (int.Parse(DateFile.instance.GetActorDate(actorId, index4, true)) > num7)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "AGV":
                                    {
                                        int id = int.Parse(DateFile.instance.GetActorDate(__instance.mianEventDate[1], 19, false));
                                        if (DateFile.instance.GetBasePartValue(int.Parse(DateFile.instance.GetGangDate(id, 11)), int.Parse(DateFile.instance.GetGangDate(id, 3))) < int.Parse(array5[1]))
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "GF":
                                    {
                                        int num8 = int.Parse(array5[1]);
                                        int num9 = (array5.Length <= 2) ? -99 : int.Parse(array5[2]);
                                        int num10 = (array5.Length <= 3) ? -99 : int.Parse(array5[3]);
                                        if (num9 != -99 && DateFile.instance.actorGongFas[actorId].ContainsKey(num8) && DateFile.instance.GetGongFaLevel(actorId, num8, 0) < num9)
                                        {
                                            array4[j] = false;
                                        }
                                        if (num10 != -99 && DateFile.instance.actorGongFas[actorId].ContainsKey(num8) && DateFile.instance.GetGongFaFLevel(actorId, num8, false) < num10)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "FGF":
                                    {
                                        int typ = int.Parse(array5[1]);
                                        int num11 = int.Parse(array5[2]);
                                        if (DateFile.instance.GetFamilyGongFaLevel(actorId, typ, false) < num11)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "SKILL":
                                    {
                                        int skillId = int.Parse(array5[1]);
                                        int num12 = (array5.Length <= 2) ? -99 : int.Parse(array5[2]);
                                        int num13 = (array5.Length <= 3) ? -99 : int.Parse(array5[3]);
                                        if (num12 != -99 && DateFile.instance.GetSkillLevel(skillId) < num12)
                                        {
                                            array4[j] = false;
                                        }
                                        if (num13 != -99 && DateFile.instance.GetSkillFLevel(skillId) < num13)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "FSKILL":
                                    {
                                        int typ2 = int.Parse(array5[1]);
                                        int num14 = int.Parse(array5[2]);
                                        if (DateFile.instance.GetFamilySkillLevel(typ2, false) < num14)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "ITEM":
                                    {
                                        int num15 = int.Parse(array5[1]);
                                        int num16 = int.Parse(array5[2]);
                                        array4[j] = false;
                                        List<int> list = new List<int>(DateFile.instance.actorItemsDate[actorId].Keys);
                                        for (int k = 0; k < list.Count; k++)
                                        {
                                            int num17 = list[k];
                                            int num18 = int.Parse(DateFile.instance.GetItemDate(num17, 999, true));
                                            if (num18 == num15 && DateFile.instance.GetItemNumber(actorId, num17) >= num16)
                                            {
                                                array4[j] = true;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                case "FA":
                                    {
                                        int num19 = int.Parse(array5[1]);
                                        int num20 = DateFile.instance.GetActorFavor(false, DateFile.instance.MianActorID(), __instance.mianEventDate[1], true, false);
                                        if (num20 < num19)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "TIME":
                                    {
                                        int num21 = int.Parse(array5[1]);
                                        int dayTime = DateFile.instance.dayTime;
                                        if (dayTime < num21)
                                        {
                                            array4[j] = false;
                                        }
                                        break;
                                    }
                                case "MFS":
                                    if (DateFile.instance.GetFamily(true, true).Count >= DateFile.instance.GetMaxFamilySize())
                                    {
                                        array4[j] = false;
                                    }
                                    break;
                                case "BHS":
                                    if (DateFile.instance.GetActorSocial(actorId, 308, false, false).Count >= 9)
                                    {
                                        array4[j] = false;
                                    }
                                    break;
                                case "NOF":
                                    if (DateFile.instance.GetFamily(true, true).Contains(eventActor))
                                    {
                                        array4[j] = false;
                                    }
                                    break;
                                case "SHOPV":
                                    if (DateFile.instance.storyShopLevel[int.Parse(array5[1])] >= 5000 * DateFile.instance.GetMaxWorldValue() / 1000)
                                    {
                                        array4[j] = false;
                                    }
                                    break;
                            }
                        }
                        array2[i] = false;
                        for (int l = 0; l < array4.Length; l++)
                        {
                            if (array4[l])
                            {
                                array2[i] = true;
                                break;
                            }
                        }
                    }
                    for (int m = 0; m < array2.Length; m++)
                    {
                        if (!array2[m])
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                result = flag;
            }
            return result;
        }

        private static string ChangeText(ref MassageWindow __instance, int eventId, string massageText, bool noChange = false)
        {
            int num = DateFile.instance.MianActorID();
            int num2 = int.Parse(DateFile.instance.GetActorDate(num, 11, false));
            int num3 = Mathf.Max(int.Parse(DateFile.instance.GetActorDate(num, 14, false)) - 1, 0);
            int key = int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][401]);
            int key2 = int.Parse(DateFile.instance.allWorldDate[DateFile.instance.mianWorldId][402]);
            string text = massageText.Replace("GANG", DateFile.instance.GetGangDate(int.Parse(DateFile.instance.GetActorDate((__instance.mianEventDate[1] <= 0) ? num : __instance.mianEventDate[1], 19, false)), 0)).Replace("MN", DateFile.instance.GetActorName(0, false, false)).Replace("BN", DateFile.instance.GetActorName(__instance.mianEventDate[1], false, false)).Replace("GN_R", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[(num2 >= 30) ? ((num2 >= 40) ? 3 : 2) : 1].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_0", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[0].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_1", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[1].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_2", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[2].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_3", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[3].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_4", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[4].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_5", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[5].Split(new char[]
            {
        '&'
            })[num3]).Replace("GN_6", DateFile.instance.massageDate[5][1].Split(new char[]
            {
        '|'
            })[6].Split(new char[]
            {
        '&'
            })[num3]).Replace("MCITY", DateFile.instance.placeWorldDate[key][0]).Replace("MGNG", DateFile.instance.placeWorldDate[key2][0]);
            string[] array = DateFile.instance.eventDate[eventId][4].Split(new char[]
            {
        '|'
            });
            for (int i = 0; i < array.Length; i++)
            {
                int num4 = int.Parse(array[i]);
                if (num4 != 0)
                {
                    int num5 = __instance.mianEventDate[i + 3];
                    switch (num4)
                    {
                        case 1:
                            text = text.Replace(string.Format("D{0}", i), DateFile.instance.GetActorName(num5, false, false));
                            break;
                        case 2:
                            {
                                string str = DateFile.instance.GetItemDate(num5, 0, false).Replace(DateFile.instance.massageDate[11][4].Split(new char[]
                                {
                    '|'
                                })[0], "").Replace(DateFile.instance.massageDate[11][4].Split(new char[]
                                {
                    '|'
                                })[1], "");
                                text = text.Replace(string.Format("D{0}", i), DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num5, 8, true)), DateFile.instance.massageDate[10][0].Split(new char[]
                                {
                    '|'
                                })[0] + str + DateFile.instance.massageDate[10][0].Split(new char[]
                                {
                    '|'
                                })[1], noChange));
                                break;
                            }
                        case 3:
                            text = text.Replace(string.Format("D{0}", i), DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.gongFaDate[num5][2]), DateFile.instance.massageDate[11][4].Split(new char[]
                            {
                    '|'
                            })[0] + DateFile.instance.gongFaDate[num5][0] + DateFile.instance.massageDate[11][4].Split(new char[]
                            {
                    '|'
                            })[1], noChange));
                            text = text.Replace(string.Format("GFM{0}", i), DateFile.instance.gongFaDate[num5][99].Replace("“", "‘").Replace("”", "’").Replace("。", "……"));
                            break;
                        case 4:
                            text = text.Replace(string.Format("D{0}", i), DateFile.instance.SetColoer(20008, DateFile.instance.resourceDate[num5][0], noChange));
                            break;
                        case 5:
                            {
                                string str2 = DateFile.instance.GetItemDate(num5, 0, false).Replace(DateFile.instance.massageDate[11][4].Split(new char[]
                                {
                    '|'
                                })[0], "").Replace(DateFile.instance.massageDate[11][4].Split(new char[]
                                {
                    '|'
                                })[1], "");
                                text = text.Replace(string.Format("D{0}", i), DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.GetItemDate(num5, 8, true)), DateFile.instance.massageDate[10][0].Split(new char[]
                                {
                    '|'
                                })[0] + str2 + DateFile.instance.massageDate[10][0].Split(new char[]
                                {
                    '|'
                                })[1], noChange));
                                break;
                            }
                        case 6:
                            text = text.Replace(string.Format("D{0}", i), DateFile.instance.massageDate[10][0].Split(new char[]
                            {
                    '|'
                            })[0] + DateFile.instance.baseSkillDate[num5][0] + DateFile.instance.massageDate[10][0].Split(new char[]
                            {
                    '|'
                            })[1]);
                            break;
                        case 7:
                            text = text.Replace(string.Format("D{0}", i), num5.ToString());
                            break;
                        case 8:
                            text = text.Replace(string.Format("D{0}", i), DateFile.instance.SetColoer(20001 + int.Parse(DateFile.instance.skillDate[num5][2]), DateFile.instance.massageDate[11][4].Split(new char[]
                            {
                    '|'
                            })[0] + DateFile.instance.skillDate[num5][0] + DateFile.instance.massageDate[11][4].Split(new char[]
                            {
                    '|'
                            })[1], noChange));
                            text = text.Replace(string.Format("SFM{0}", i), DateFile.instance.skillDate[num5][99].Replace("“", "‘").Replace("”", "’").Replace("。", "……"));
                            break;
                        case 9:
                            {
                                int num6 = __instance.mianEventDate[4];
                                text = text.Replace(string.Format("D{0}", i), DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[0] + DateFile.instance.massageDate[25][2].Split(new char[]
                                {
                    '|'
                                })[num6] + DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[1]);
                                break;
                            }
                        case 10:
                            {
                                int num7 = DateFile.instance.GetActorFavor(false, num, __instance.mianEventDate[1], false, false);
                                text = text.Replace(string.Format("D{0}", i), (num7 == -1) ? (DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[0] + DateFile.instance.SetColoer(20002, DateFile.instance.massageDate[303][2], noChange) + DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[1]) : (DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[0] + ActorMenu.instance.Color5(num7, true, -1) + DateFile.instance.massageDate[11][3].Split(new char[]
                                {
                    '|'
                                })[1]));
                                break;
                            }
                    }
                }
            }
            return text;
        }

    }

    [HarmonyPatch(typeof(PeopleLifeAI), "AIGetLove")]
    public static class PeopleLifeAI_AIGetLove_Patch
    {
        private static void Postfix(PeopleLifeAI __instance, int actorId, int loverId, ref int __result)
        {
            if (!Main.enabled || __result <= 0)
            {
                return;
            }
            if (DateFile.instance.HaveLifeDate(loverId, 309) || DateFile.instance.HaveLifeDate(actorId, 309))
            {
                __result = __result * (int)(Main.settings.GetLoveHaveSpouse * 0.5f);
            }

            else if (DateFile.instance.HaveLifeDate(loverId, 306) || DateFile.instance.HaveLifeDate(actorId, 306))
            {
                __result = __result * (int)(Main.settings.GetLoveHaveLover * 0.5f);
            }
        }
    }

    [HarmonyPatch(typeof(MassageWindow), "EndEvent9010_1")]
    public static class MassageWindow_EndEvent9010_1_Patch
    {

        private static bool Prefix(ref MassageWindow __instance)
        {
            if (!Main.enabled)
            {
                return true;
            }


            int actorId = DateFile.instance.MianActorID();
            int num = __instance.eventValue[1];
            int level = UnityEngine.Random.Range(0, 64);
            String strlevel = Convert.ToString(level, 2).PadLeft(6, '0');
            int z = 0;
            for (int w = 0; w < strlevel.Length; w++)
            {
                if (strlevel[w] != '0')
                {
                    z++;
                }
            }
            bool flage = true;
            switch (num)
            {
                //铁
                case 21:
                    int tempInt21 = UnityEngine.Random.Range(0, 2);
                    int itemId21 = 3100;
                    itemId21 = itemId21 + (tempInt21 * 7) + 1 + z;
                    itemId21 = UnityEngine.Mathf.Clamp(itemId21, 3101, 3114);
                    num = DateFile.instance.MakeNewItem(itemId21, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //木
                case 22:
                    int tempInt22 = UnityEngine.Random.Range(0, 2);
                    int itemId22 = 3000;
                    itemId22 = itemId22 + (tempInt22 * 7) + 1 + z;
                    itemId22 = UnityEngine.Mathf.Clamp(itemId22, 3001, 3014);
                    num = DateFile.instance.MakeNewItem(itemId22, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //玉
                case 23:
                    int tempInt23 = UnityEngine.Random.Range(0, 2);
                    int itemId23 = 3200;
                    itemId23 = itemId23 + (tempInt23 * 7) + 1 + z;
                    itemId23 = UnityEngine.Mathf.Clamp(itemId23, 3201, 3214);
                    num = DateFile.instance.MakeNewItem(itemId23, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //布
                case 24:
                    int tempInt24 = UnityEngine.Random.Range(0, 2);
                    int itemId24 = 3300;
                    itemId24 = itemId24 + (tempInt24 * 7) + 1 + z;
                    itemId24 = UnityEngine.Mathf.Clamp(itemId24, 3301, 3314);
                    num = DateFile.instance.MakeNewItem(itemId24, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //药
                case 25:
                    int tempInt25 = UnityEngine.Random.Range(0, 24);
                    int itemId25 = 4000;
                    z = z / 2;
                    if (z > 3)
                    {
                        z = 3;
                    }
                    itemId25 = itemId25 + (tempInt25 * 4) + 1 + z;
                    itemId25 = UnityEngine.Mathf.Clamp(itemId25, 4001, 4096);
                    num = DateFile.instance.MakeNewItem(itemId25, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //毒
                case 26:
                    int tempInt26 = UnityEngine.Random.Range(0, 6);
                    int itemId26 = 4200;
                    itemId26 = itemId26 + (tempInt26 * 7) + 1 + z;
                    itemId26 = UnityEngine.Mathf.Clamp(itemId26, 4201, 4242);
                    num = DateFile.instance.MakeNewItem(itemId26, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
                //食
                case 27:
                    int tempInt27 = UnityEngine.Random.Range(0, 3);
                    int itemId27 = 3400;
                    if (z > 3)
                    {
                        z = 3;
                    }
                    itemId27 = itemId27 + (tempInt27 * 7) + 1 + z;
                    itemId27 = UnityEngine.Mathf.Clamp(itemId27, 3401, 3428);
                    num = DateFile.instance.MakeNewItem(itemId27, 0, 0, 50, 20);
                    DateFile.instance.GetItem(DateFile.instance.MianActorID(), num, 1, false, 0, 0);
                    flage = false;
                    break;
            }
            return flage;
        }
    }
}









