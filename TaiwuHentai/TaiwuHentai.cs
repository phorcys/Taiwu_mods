using System.Reflection;
using Harmony12;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using GameData;

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
        public int FriendNumber = 1;
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

            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("提前求爱与求婚的年龄，大于切不等于该年龄的npc能求婚当然太污自己也要大于这个年龄,对婴儿不起作用");
            var maxBackupsToKeep = GUILayout.TextField(settings.SpouseAge.ToString(), 3);
            if (GUI.changed && !uint.TryParse(maxBackupsToKeep, out settings.SpouseAge))
            {
                settings.SpouseAge = 14;
            }
            GUILayout.EndHorizontal();
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
            GUILayout.BeginHorizontal("Box");
            GUILayout.Label("同道数量", new GUILayoutOption[0]);
            Main.settings.FriendNumber = GUILayout.SelectionGrid(Main.settings.FriendNumber, new string[]
            {
                "1倍",
                "2倍",
                "3倍",
                "4倍",
                "5倍"
            }, 5, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }
    [HarmonyPatch(typeof(DateFile), "GetMaxFamilySize")]
    public static class DateFile_GetMaxFamilySize_Patch
    {
        private static void Postfix(DateFile __instance, ref int __result)
        {

            if (!Main.enabled )
            {
                return;
            }
            if(__result > 0)
            __result = __result * (Main.settings.FriendNumber+1);
        }




    }
    [HarmonyPatch(typeof(DateFile), "GetActorDate")]
    public static class DateFile_GetActorDate_Patch
    {

        private static void Postfix(DateFile __instance, ref string __result, int actorId, int key, bool applyBonus = true)
        {

            if (!Main.enabled || !Main.settings.displayGlamour)
            {
                return;
            }
            int id;
            if (key == 15)
            {
                string text = (!__instance.presetActorDate.ContainsKey(actorId) || !__instance.presetActorDate[actorId].ContainsKey(key)) ? "0" : __instance.presetActorDate[actorId][key];
                string charProperty = Characters.GetCharProperty(actorId, key);
                bool flag2 = charProperty == null;
                if (Characters.HasChar(actorId))
                {
                    //    if (__instance.actorsDate[actorId].ContainsKey(key))
                    //    {
                    //        text = __instance.[actorId][key];
                    //    }
                    //    else
                    //    {
                    //        int pId = (!__instance.actorsDate[actorId].ContainsKey(997)) ? actorId : int.Parse(__instance.actorsDate[actorId][997]);
                    //     
                    //        text = __instance.presetActorDate[pId][key];
                    //    }

                    id = actorId;
                }
                else
                {
                    string charProperty2 = Characters.GetCharProperty(actorId, 997);
                   
                    id = ((charProperty2 != null) ? int.Parse(charProperty2) : actorId);
                }
                if (!__instance.presetActorDate.ContainsKey(id) || !__instance.presetActorDate[id].ContainsKey(key))
                            {
                                __result = "0";
                            }
                    int num = 0;

                int num2 = Mathf.Clamp(int.Parse((num != 0) ? (int.Parse(charProperty) + num).ToString() : charProperty), 0, 900);

                if (applyBonus)
                {
                    bool flag9 = int.Parse(__instance.GetActorDate(actorId, 8, false)) == 1 && int.Parse(__instance.GetActorDate(actorId, 305, false)) == 0;
                    if (flag9)
                    {
                        __result = (num2 * 50 / 100).ToString();
                    }


                }
                __result = num2.ToString();
            }
        }




    }

    [HarmonyPatch(typeof(WindowManage), "WindowSwitch")]
    public static class WindowManage_WindowSwitch_Patch
    {
        static public List<int> GetActorFeature(int key)
        {
            List<int> result;



            List<int> list = new List<int>();
            string[] array = DateFile.instance.GetActorDate(key, 101, false).Split(new char[]
            {
                '|'
            });


            int j = 0;
            while (j < array.Length)
            {
                int num4 = int.Parse(array[j]);
                if (num4 != 0)
                {
                    list.Add(num4);
                }

                j++;


            }

            result = list;

            return result;
        }
        // Token: 0x06000008 RID: 8 RVA: 0x00002168 File Offset: 0x00000368
        private static void Postfix(WindowManage __instance, ref GameObject tips, ref Text ___itemMoneyText, ref Text ___informationMassage, ref Text ___informationName, ref bool ___anTips)
        {
            if (!Main.enabled)
            {
                return;
            }

            if ((tips != null))
            {
                Transform oldMainHoder = WindowManage.instance.informationMassage.GetComponent<RectTransform>().Find("AgeMianface");
                if (oldMainHoder)
                {
                    UnityEngine.Object.Destroy(oldMainHoder.gameObject);
                }

                string[] array = tips.name.Split(new char[]
                {
                    ','
                });
                //Main.Logger.Log(array[0]);
                if (array[0] == "HelpIcon" && Main.settings.displayGlamour)
                {
                    int key = ActorMenu.instance.actorId;
                    bool flageIsShow = true;
                    //Main.Logger.Log(key.ToString());

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


                        List<int> featureIDs = GetActorFeature(key);
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

                            //RectTransform component = __instance.informationWindow.GetComponent<RectTransform>();
                            //ActorFace mainHoder = UnityEngine.Object.Instantiate(ActorMenu.instance.mianActorFace, new Vector3(100f, 30f, 1), Quaternion.identity);
                            ActorFace mainHoder = Object.Instantiate<ActorFace>(ActorMenu.instance.mianActorFace, new Vector3(110f, -160f, 1f), Quaternion.identity);
                            // mainHoder.transform.SetParent(__instance.informationWindow.GetComponent<RectTransform>(), false);
                            mainHoder.name = "AgeMianface";

                            editFac(ref mainHoder, key, 18, int.Parse(DateFile.instance.GetActorDate(key, 14, false)), int.Parse(DateFile.instance.GetActorDate(key, 17, false)), array3, array4, clotheIndex);


                            Main.Logger.Log((mainHoder.GetComponent<RectTransform>() == null).ToString());

                            //mainHoder.transform.localScale = new Vector3(0.73f, 0.73f, 1f);


                            //mainHoder.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
                            mainHoder.transform.SetParent(WindowManage.instance.informationMassage.GetComponent<RectTransform>(), false);
                            //mainHoder.transform.localScale = new Vector3(scX * 0.3f, scY * 0.45f, 1);

                            float scX = mainHoder.transform.parent.localScale.x;
                            float scY = mainHoder.transform.parent.localScale.y;
                            scX = 1 / scX;
                            scY = 1 / scY;
                            mainHoder.transform.localScale = new Vector3(scX * 0.68f, scY * 0.7f, 1f);
                            //mainHoder.transform.localScale = new Vector3(0.73f, 0.73f, 1f);
                            //Main.Logger.Log(mainHoder.transform.parent.name);
                            //mainHoder.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 500f);
                            //mainHoder.GetComponent<Graphic>().CrossFadeAlpha(1f, 0.2f, true);
                            // Main.Logger.Log(mainHoder.body.name);
                        }

                    }
                }

            }

        }
        public static bool editFac(ref ActorFace actorFace, int actorId, int age, int gender, int actorGenderChange, int[] faceDate, int[] faceColor, int clotheIndex)
        { bool flag = faceDate.Length == 1;
            actorFace.ageImage.gameObject.SetActive(false);
            actorFace.nose.gameObject.SetActive(false);
            actorFace.faceOther.gameObject.SetActive(false);
            actorFace.eye.gameObject.SetActive(false);
            actorFace.eyePupil.gameObject.SetActive(false);
            actorFace.eyebrows.gameObject.SetActive(false);
            actorFace.mouth.gameObject.SetActive(false);
            actorFace.beard.gameObject.SetActive(false);
            actorFace.hair1.gameObject.SetActive(false);
            actorFace.hair2.gameObject.SetActive(false);
            actorFace.hairOther.gameObject.SetActive(false);
            actorFace.clothes.gameObject.SetActive(false);
            actorFace.clothesColor.gameObject.SetActive(false);
            actorFace.body.gameObject.SetActive(true);
            bool flag5 = true;
            if (flag5)
            {
                actorFace.body.sprite = Resources.Load<Sprite>("Graphics/ActorFaceSmall/NPCFace/NPCFace_Dead");
            }
            else
            {
                actorFace.body.sprite = Resources.Load<Sprite>("Graphics/ActorFace/NPCFace/NPCFace_Dead");
            }
            actorFace.body.color = new Color(1f, 1f, 1f, 1f);
            if (flag)
            {
                actorFace.ageImage.gameObject.SetActive(false);
                actorFace.nose.gameObject.SetActive(false);
                actorFace.faceOther.gameObject.SetActive(false);
                actorFace.eye.gameObject.SetActive(false);
                actorFace.eyePupil.gameObject.SetActive(false);
                actorFace.eyebrows.gameObject.SetActive(false);
                actorFace.mouth.gameObject.SetActive(false);
                actorFace.beard.gameObject.SetActive(false);
                actorFace.hair1.gameObject.SetActive(false);
                actorFace.hair2.gameObject.SetActive(false);
                actorFace.hairOther.gameObject.SetActive(false);
                actorFace.clothes.gameObject.SetActive(false);
                actorFace.clothesColor.gameObject.SetActive(false);
                actorFace.body.gameObject.SetActive(true);
                bool flag2 = actorFace.smallSize;
                if (flag2)
                {
                    actorFace.body.sprite = Resources.Load<Sprite>("Graphics/ActorFaceSmall/NPCFace/NPCFace_" + faceDate[0].ToString());
                }
                else
                {
                    actorFace.body.sprite = Resources.Load<Sprite>("Graphics/ActorFace/NPCFace/NPCFace_" + faceDate[0].ToString());
                }
                actorFace.body.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                bool flag3 = (true || actorId == -1 || int.Parse(DateFile.instance.GetActorDate(actorId, 26, false)) == 0);
                Main.Logger.Log(flag3.ToString());
                actorFace.body.color = new Color(1f, 1f, 1f, 1f);
                int num = (actorGenderChange != 0) ? ((gender == 1) ? 1 : 0) : (gender - 1);
                int num2 = Mathf.Min(faceDate[0], int.Parse(GetSprites.instance.actorFaceDate[num][98]) - 1);
                int num3 = Mathf.Min(faceDate[0], int.Parse(GetSprites.instance.actorFaceDate[num][99]) - 1);
                actorFace.ageImage.gameObject.SetActive(flag3);
                actorFace.body.gameObject.SetActive(true);
                actorFace.nose.gameObject.SetActive(flag3);
                actorFace.faceOther.gameObject.SetActive(flag3);
                actorFace.eye.gameObject.SetActive(flag3);
                actorFace.eyePupil.gameObject.SetActive(flag3);
                actorFace.eyebrows.gameObject.SetActive(flag3);
                actorFace.mouth.gameObject.SetActive(flag3);
                actorFace.beard.gameObject.SetActive(gender == 1 && num == 0 && age >= 20);
                actorFace.hair1.gameObject.SetActive(flag3 || (!flag3 && faceDate[7] == 15));
                actorFace.hair2.gameObject.SetActive(flag3 || (!flag3 && faceDate[7] == 15));
                actorFace.hairOther.gameObject.SetActive(flag3 || (!flag3 && faceDate[7] == 15));
                actorFace.clothes.gameObject.SetActive(true);
                actorFace.clothesColor.gameObject.SetActive(true);
                string text2 = actorFace.smallSize ? "actorFaceSmall" : "actorFace";
                DynamicSetSprite instance2 = SingletonObject.getInstance<DynamicSetSprite>();
                Image image2 = actorFace.ageImage;
                string groupName2 = text2;
                int[] array2 = new int[4];
                array2[0] = num;
                array2[1] = num3;
                instance2.SetImageSprite(image2, groupName2, array2);
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.body, text2, new int[]
                        {
                        num,
                        num3,
                        1,
                        flag3 ? 0 : 1
                        });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.nose, text2, new int[]
                {
                        num,
                        num3,
                        2,
                        faceDate[1]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.faceOther, text2, new int[]
                {
                        num,
                        num3,
                        3,
                        faceDate[2]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.eye, text2, new int[]
                {
                        num,
                        num3,
                        4,
                        faceDate[3]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.eyePupil, text2, new int[]
                {
                        num,
                        num3,
                        5,
                        faceDate[3]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.eyebrows, text2, new int[]
                {
                        num,
                        num3,
                        6,
                        faceDate[4]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.mouth, text2, new int[]
                {
                        num,
                        num3,
                        7,
                        faceDate[5]
                });
                bool activeSelf = actorFace.beard.gameObject.activeSelf;
                if (activeSelf)
                {
                    SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.beard, text2, new int[]
                    {
                            0,
                            num3,
                            8,
                            faceDate[6]
                    });
                }
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.hair1, text2, new int[]
                {
                        num,
                        num3,
                        9,
                        faceDate[7]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.hair2, text2, new int[]
                {
                        num,
                        num3,
                        12,
                        faceDate[7]
                });
                SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.hairOther, text2, new int[]
                {
                        num,
                        num3,
                        13,
                        faceDate[7]
                });
                bool flag10 = clotheIndex != -1;
                if (flag10)
                {
                    SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.clothes, text2, new int[]
                    {
                            num,
                            num3,
                            10,
                            clotheIndex
                    });
                    SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(actorFace.clothesColor, text2, new int[]
                    {
                            num,
                            num3,
                            11,
                            clotheIndex
                    });
                }
                else
                {
                    DynamicSetSprite instance3 = SingletonObject.getInstance<DynamicSetSprite>();
                    Image image3 = actorFace.clothes;
                    string groupName3 = text2;
                    int[] array3 = new int[4];
                    array3[0] = num;
                    array3[1] = num3;
                    array3[2] = 10;
                    instance3.SetImageSprite(image3, groupName3, array3);
                    DynamicSetSprite instance4 = SingletonObject.getInstance<DynamicSetSprite>();
                    Image image4 = actorFace.clothesColor;
                    string groupName4 = text2;
                    int[] array4 = new int[4];
                    array4[0] = num;
                    array4[1] = num3;
                    array4[2] = 11;
                    instance4.SetImageSprite(image4, groupName4, array4);
                }
                actorFace.body.color = (flag3 ? DateFile.instance.faceColor[0][faceColor[0]] : new Color(1f, 1f, 1f, 1f));
                actorFace.nose.color = DateFile.instance.faceColor[0][faceColor[0]];
                actorFace.eyebrows.color = DateFile.instance.faceColor[1][faceColor[1]];
                actorFace.eyePupil.color = DateFile.instance.faceColor[2][faceColor[2]];
                actorFace.mouth.color = DateFile.instance.faceColor[3][faceColor[3]];
                bool activeSelf3 = actorFace.beard.gameObject.activeSelf;
                if (activeSelf3)
                {
                    actorFace.beard.color = DateFile.instance.faceColor[4][faceColor[4]];
                }
                actorFace.hair1.color = DateFile.instance.faceColor[6][faceColor[6]];
                actorFace.hair2.color = DateFile.instance.faceColor[6][faceColor[6]];

                actorFace.faceOther.color = DateFile.instance.faceColor[5][faceColor[5]];
                actorFace.clothesColor.color = DateFile.instance.faceColor[7][faceColor[7]];
                actorFace.hairOther.color = DateFile.instance.faceColor[7][faceColor[7]];
            }



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
    [HarmonyPatch(typeof(ui_MessageWindow), "SetMassageWindow")]
    public static class ui_MessageWindow_SetMassageWindow_Patch
    {
        private static void SetMassageItem(ref ui_MessageWindow _instance, int eventId)
        {
            List<int> list = new List<int>();
            string[] array = DateFile.instance.eventDate[eventId][4].Split(new char[]
            {
        '|'
            });
            for (int i = 0; i < array.Length; i++)
            {
                int num = int.Parse(array[i]);
                bool flag = num != 0;
                if (flag)
                {
                    int item = MessageEventManager.Instance.MainEventData[i + 3];
                    bool flag2 = num == 2;
                    if (flag2)
                    {
                        list.Add(item);
                        break;
                    }
                }
            }
            for (int j = 0; j < _instance.eventItemHolder.childCount; j++)
            {
                Object.Destroy(_instance.eventItemHolder.GetChild(j).gameObject);
            }
            bool flag3 = list.Count > 0;
            if (flag3)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    int num2 = list[k];
                    bool flag4 = num2 > 0;
                    if (flag4)
                    {
                        GameObject gameObject = Object.Instantiate<GameObject>(_instance.eventItemIcon, Vector3.zero, Quaternion.identity);
                        gameObject.transform.SetParent(_instance.eventItemHolder, false);
                        gameObject.name = "EventItem," + num2;
                        Image component = gameObject.transform.Find("ItemBack").GetComponent<Image>();
                        SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(component, "itemBackSprites", new int[]
                        {
                    int.Parse(DateFile.instance.GetItemDate(num2, 4, true))
                        });
                        component.color = DateFile.instance.LevelColor(int.Parse(DateFile.instance.GetItemDate(num2, 8, true)));
                        GameObject gameObject2 = gameObject.transform.Find("ItemIcon").gameObject;
                        SingletonObject.getInstance<DynamicSetSprite>().SetImageSprite(gameObject2.GetComponent<Image>(), "itemSprites", new int[]
                        {
                    int.Parse(DateFile.instance.GetItemDate(num2, 98, true))
                        });
                        gameObject2.name = "ItemIcon," + num2;
                        bool flag5 = int.Parse(DateFile.instance.GetItemDate(num2, 6, true)) > 0;
                        if (flag5)
                        {
                            gameObject.transform.Find("ItemNumberText").GetComponent<Text>().text = "×" + DateFile.instance.GetItemNumber(DateFile.instance.MianActorID(), num2);
                        }
                        else
                        {
                            int num3 = int.Parse(DateFile.instance.GetItemDate(num2, 901, true));
                            int num4 = int.Parse(DateFile.instance.GetItemDate(num2, 902, true));
                            gameObject.transform.Find("ItemNumberText").GetComponent<Text>().text = string.Format("{0}{1}</color>/{2}", DateFile.instance.Color3(num3, num4), num3, num4);
                        }
                    }
                }
            }
        }
        public static bool Prefix(ref ui_MessageWindow __instance, int[] baseEventDate, int chooseId)
        {
            if (chooseId == 901000006 && Main.enabled && Main.settings.PeerTeaching)
            {
                
                for (int i = 0; i < __instance.chooseHolder.childCount; i++)
                {
                    UnityEngine.Object.Destroy(__instance.chooseHolder.GetChild(i).gameObject);
                }
                MessageEventManager.Instance.massageActors.Clear();
                MessageEventManager.Instance.MainEventData = (int[])baseEventDate.Clone();
                MessageEventManager.Instance.massageActors.Add(MessageEventManager.Instance.MainEventData[1]);
                int num = MessageEventManager.Instance.MainEventData[2];
                //__instance.massageActors.Clear();
                //__instance.mianEventDate = (int[])baseEventDate.Clone();
                //__instance.massageActors.Add(__instance.mianEventDate[1]);
                //int num = __instance.mianEventDate[2];
                int num2 = int.Parse(DateFile.instance.eventDate[num][2]);
                //太污的id
                int num3 = DateFile.instance.MianActorID();
                //目标角色的id
               
                int num4 = (num2 != 0) ? ((num2 != -1) ? num2 : num3) : MessageEventManager.Instance.MainEventData[1];
                int num5 = int.Parse(DateFile.instance.GetActorDate(num4, 19, false));
                SetMassageItem(ref __instance, num);
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
                    AddToList(ref __instance, num, num3, num4, list2, list3);
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

        public static void Postfix(ref ui_MessageWindow __instance, int chooseId)
        {
            if (!Main.enabled)
            {
                return;
            }
            int num = MessageEventManager.Instance.MainEventData[2];
            int num2 = int.Parse(DateFile.instance.eventDate[num][2]);

            //太污的id
            int num3 = DateFile.instance.MianActorID();
            //目标角色的id

            int num4 = (num2 == 0) ? MessageEventManager.Instance.MainEventData[1] : ((num2 == -1) ? num3 : num2);

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
                    bool flage_TONGDAO = (flag2 || DateFile.instance.GetActorSocial(num3, 302, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 303, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 308, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 309, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 310, false).Contains(num4));
                    bool should_TONGDAO = (flag2 || DateFile.instance.GetActorSocial(num3, 302, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 303, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 308, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 306, false).Contains(num4) || DateFile.instance.GetActorSocial(num3, 310, false).Contains(num4));

                    if (!flage_TONGDAO && should_TONGDAO && Main.settings.unrestrainedPartner)
                    {

                        //倾诉爱意即可邀为同道
                        list2.Add("900300001");
                    }
                    //判断是否足够出现亲密选项
                    bool flage_QINGMI = DateFile.instance.GetLifeDate(num3, 601, 0) != DateFile.instance.GetLifeDate(num4, 601, 0) && DateFile.instance.GetLifeDate(num3, 602, 0) != DateFile.instance.GetLifeDate(num4, 602, 0);

                    if (flage_QINGMI)
                    {
                        bool flage_QINGSUAIYI = !DateFile.instance.GetActorSocial(num3, 304, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 306, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 308, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 309, false).Contains(num4) && !DateFile.instance.GetActorSocial(num3, 311, false).Contains(num4);

                        if (!(num18 > 14 && num14 > 14 && flage_QINGSUAIYI))
                        {
                            if (num18 > Main.settings.SpouseAge && num14 > Main.settings.SpouseAge && flage_QINGSUAIYI)
                            {
                                list2.Add("900600003");
                            }
                        }
                        bool flage_GONGJIELIANLI = (DateFile.instance.GetActorSocial(num3, 306, false).Contains(num4) && DateFile.instance.GetActorSocial(num3, 309, false).Count <= 0 && DateFile.instance.GetActorSocial(num4, 309, false).Count <= 0 && int.Parse(DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num5, num17)][803]) != 0 && int.Parse(DateFile.instance.GetActorDate(num3, 2, false)) == 0 && int.Parse(DateFile.instance.GetActorDate(num4, 2, false)) == 0);
                        bool should_GONGJIELIANLI = DateFile.instance.GetActorSocial(num3, 306, false).Contains(num4) && int.Parse(DateFile.instance.GetActorDate(num3, 2, false)) == 0 && int.Parse(DateFile.instance.GetActorDate(num4, 2, false)) == 0 && DateFile.instance.GetActorSocial(num4, 309, false).Count <= 0;
                        bool flage_MENPAIJIANCHA = int.Parse(DateFile.instance.presetGangGroupDateValue[DateFile.instance.GetGangValueId(num5, num17)][803]) != 0;
                        bool flage_DUOFUDUOQI = DateFile.instance.GetActorSocial(num3, 309, false).Count <= 0;

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
                    AddToList(ref __instance, num, num3, num4, list2, list3);
                }


            }


        }
        public static class Reflection
        {
            static MethodInfo getEventIF = typeof(MessageEventManager).GetMethod("GetEventIF", BindingFlags.NonPublic | BindingFlags.Instance);
            public static bool GetEventIF(int actorId, int eventActor, int eventId)
            {
               return (bool)getEventIF.Invoke(MessageEventManager.Instance, new object[] { actorId, eventActor, eventId });
            }
            static MethodInfo changeText = typeof(ui_MessageWindow).GetMethod("ChangeText", BindingFlags.NonPublic | BindingFlags.Instance);
            public static string ChangeText(int eventId, string massageText, bool noChange = false)
            {
                return (string)changeText.Invoke(ui_MessageWindow.Instance, new object[] { eventId, massageText, noChange });
            }
        }
            private static bool AddToList(ref ui_MessageWindow __instance, int num, int num3, int num4, List<string> list3, List<string> list4)
        {

            int num39 = 0;
            //__instance.chooseCount = list2.Count;
            for (int num40 = 0; num40 < list3.Count; num40++)
            {
                int num41 = int.Parse(list3[num40]);
                bool flag84 = __instance.removeChooseIds.Contains(num41);
                if (flag84)
                {
                    //__instance.chooseCount--;
                }
                else
                {
                    bool flag85 = MessageEventManager.Instance.massageGetGongFas.Count > 0;
                    if (flag85)
                    {
                        bool flag86 = false;
                        string[] array3 = DateFile.instance.eventDate[num41][6].Split(new char[]
                        {
                        '&'
                        });
                        bool flag87 = array3[0] == "GTYP";
                        if (flag87)
                        {
                            int num42 = int.Parse(array3[1]);
                            List<int> list11 = new List<int>(MessageEventManager.Instance.massageGetGongFas.Keys);
                            for (int num43 = 0; num43 < list11.Count; num43++)
                            {
                                int key = list11[num43];
                                bool flag88 = int.Parse(DateFile.instance.gongFaDate[key][1]) == num42;
                                if (flag88)
                                {
                                    flag86 = true;
                                    break;
                                }
                            }
                            bool flag89 = !flag86;
                            if (flag89)
                            {
                                //this.chooseCount--;
                                goto IL_2310;
                            }
                        }
                    }
                    int num44 = int.Parse(DateFile.instance.eventDate[num41][2]);
                    int num45 = num44;
                    bool flag90 = num44 == -1;
                    if (flag90)
                    {
                        num45 = num3;
                    }
                    else
                    {
                        bool flag91 = num44 != -99 && num44 < 0;
                        if (flag91)
                        {
                            num45 = MessageEventManager.Instance.MainEventData[Mathf.Abs(num44)];
                        }
                    }
                    int num46 = -1;
                    GameObject gameObject = Object.Instantiate<GameObject>((num45 != -99) ? __instance.massageChoose1 : __instance.massageChoose2, Vector3.zero, Quaternion.identity);
                    gameObject.transform.SetParent(__instance.chooseHolder, false);
                    gameObject.name = "Choose," + num41;
                    bool flag92 = num45 != -99;
                    if (flag92)
                    {
                        gameObject.transform.Find("NameText").GetComponent<Text>().text = DateFile.instance.GetActorName(num45, false, false) + WindowManage.instance.Mut();
                        gameObject.transform.Find("FaceHolder").Find("FaceMask").Find("MianActorFace").GetComponent<ActorFace>().SetActorFace(num45, false);
                        GameObject gameObject2 = gameObject.transform.Find("IconHolder").Find("NeedIcon").gameObject;
                        string a = DateFile.instance.eventDate[num41][6].Replace("|GN&0", "").Replace("|GN&1", "").Replace("|GN&2", "").Replace("|GN&3", "").Replace("|GN&4", "").Replace("GN&0|", "").Replace("GN&1|", "").Replace("GN&2|", "").Replace("GN&3|", "").Replace("GN&4|", "").Replace("GN&0", "").Replace("GN&1", "").Replace("GN&2", "").Replace("GN&3", "").Replace("GN&4", "");
                        gameObject2.SetActive(a != "" && a != "0");
                        gameObject2.name = "NeedIcon," + num41;
                        a = DateFile.instance.eventDate[num41][6];
                        bool flag93 = a != "" && a != "0";
                        if (flag93)
                        {
                            string[] array4 = DateFile.instance.eventDate[num41][6].Split(new char[]
                            {
                            '|'
                            });
                            for (int num47 = 0; num47 < array4.Length; num47++)
                            {
                                string[] array5 = array4[num47].Split(new char[]
                                {
                                '#'
                                });
                                for (int num48 = 0; num48 < array5.Length; num48++)
                                {
                                    string[] array6 = array5[num48].Split(new char[]
                                    {
                                    '&'
                                    });
                                    string a2 = array6[0];
                                    if (a2 == "GN")
                                    {
                                        num46 = int.Parse(array6[1]);
                                        gameObject.transform.Find("IconHolder").Find("LikeIcon,774").gameObject.SetActive(DateFile.instance.GetActorGoodness(num3) == num46);
                                    }
                                }
                            }
                        }
                    }
                    bool flag94 = false;
                    string[] array7 = DateFile.instance.eventDate[num41][8].Split(new char[]
                    {
                    '|'
                    });
                    for (int num49 = 0; num49 < array7.Length; num49++)
                    {
                        bool flag95 = array7[num49] == "RM";
                        if (flag95)
                        {
                            flag94 = true;
                            break;
                        }
                    }
                    string text = Reflection.ChangeText(num, DateFile.instance.eventDate[num41][3], true);
                    bool flag96 = num46 >= 0;
                    if (flag96)
                    {
                        text = string.Format("{2}{0}{3}{1}", new object[]
                        {
                        DateFile.instance.massageDate[9][0].Split(new char[]
                        {
                            '|'
                        })[num46],
                        text,
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
                    bool flag97 = list4.Contains(list3[num40]);
                    if (flag97)
                    {
                        gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(10001, string.Format("({0}).{1}", MessageEventManager.Instance.massageKeyCodeName[num39], text), false);
                        gameObject.GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        bool eventIF = MessageEventManager.Instance.GetEventIF(num45, num4, num41);
                        if (eventIF)
                        {
                            bool activeInHierarchy = __instance.inputTextField.gameObject.activeInHierarchy;
                            if (activeInHierarchy)
                            {
                                gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(10001, string.Format("({0}).{1}", MessageEventManager.Instance.massageKeyCodeName[num39], text), false);
                                gameObject.GetComponent<Button>().interactable = false;
                            }
                            else
                            {
                                gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(flag94 ? 20005 : 20003, string.Format("({0}).{1}", MessageEventManager.Instance.massageKeyCodeName[num39], text), false);
                                gameObject.GetComponent<Button>().interactable = true;
                            }
                        }
                        else
                        {
                            gameObject.transform.Find("MassageChooseText").GetComponent<Text>().text = DateFile.instance.SetColoer(10001, string.Format("({0}).{1}", MessageEventManager.Instance.massageKeyCodeName[num39], text), false);
                            gameObject.GetComponent<Button>().interactable = false;
                        }
                    }
                    num39++;
                }
                IL_2310:;
            }
            return true;
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

    [HarmonyPatch(typeof(MessageEventManager), "EndEvent9010_1")]
    public static class MassageWindow_EndEvent9010_1_Patch
    {

        private static bool Prefix(ref MessageEventManager __instance)
        {
            if (!Main.enabled)
            {
                return true;
            }


            int actorId = DateFile.instance.MianActorID();
            int num = __instance.EventValue[1];
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









