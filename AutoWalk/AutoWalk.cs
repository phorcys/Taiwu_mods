using Harmony12;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using System.Text.RegularExpressions;

namespace AutoWalk
{
    public static class Main
    {
		public static List<int> Place = new List<int>();
        public static List<int> PlaceRc = new List<int>();
        public class Settings : UnityModManager.ModSettings
        {
            public string Time = "1";
            public string Enemy = "5";
            public string Anyuan = "100";
            public bool Mengluu = true;

            public override void Save(UnityModManager.ModEntry modEntry)
            {
                UnityModManager.ModSettings.Save<Main.Settings>(this, modEntry);
            }
        }
        public static Settings settings;
        public static UnityModManager.ModEntry.ModLogger Logger;
        // Transfer a variable with data about the mod.
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            // modEntry.Info - Contains all fields from the 'Info.json' file.
            // modEntry.Path - The path to the mod folder e.g. '\Steam\steamapps\common\YourGame\Mods\TestMod\'.
            // modEntry.Active - Active or inactive.
            // modEntry.Logger - Writes logs to the 'UnityModManager.log' file.
            // modEntry.OnToggle - The presence of this function will let the mod manager know that the mod can be safely disabled during the game.
            // modEntry.OnGUI - Drawing mod options.
            // modEntry.OnSaveGUI - Called while saving.
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Logger = modEntry.Logger;
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(OnSaveGUI);
            return true;
        }
        public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value /* active or inactive */)
        {
            settings.Mengluu = value;
            return true;
        }
        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginHorizontal("Box", new GUILayoutOption[0]);
            GUILayout.BeginVertical("Box",new GUILayoutOption[0]);    
            GUILayout.Label("自动寻路，尽量躲开敌人，不碰暗渊，当然自己作死没办法。想停下来的话，按一下WASD随便一个就好", new GUILayoutOption[0]);
            settings.Time = GUILayout.TextField(settings.Time, 5, new GUILayoutOption[0]);
            settings.Time = Regex.Replace(settings.Time, "[^0-9]", "");
            GUILayout.Label("时间权重，建议填1", new GUILayoutOption[0]);
            settings.Enemy = GUILayout.TextField(settings.Enemy, 5, new GUILayoutOption[0]);
            settings.Enemy = Regex.Replace(settings.Enemy, "[^0-9]", "");
            GUILayout.Label("敌人权重，建议填5", new GUILayoutOption[0]);
            settings.Anyuan = GUILayout.TextField(settings.Anyuan, 5, new GUILayoutOption[0]);
            settings.Anyuan = Regex.Replace(settings.Anyuan, "[^0-9]", "");
            GUILayout.Label("暗渊权重，建议填100", new GUILayoutOption[0]);
            GUILayout.Label("权重越高，走路时越避开", new GUILayoutOption[0]);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

    }

    public class Walk
    {

        private int Typ(int Size,int Father,int Now)
        {
			if ((Father - Now) == Size){                                  
				return 1;
			}
			if ((Father - Now) == -Size)
			{
				return 3;
			}
			if ((Father - Now) == 1)
			{
				return 2;
			}
			if ((Father - Now) == -1)
			{
				return 4;
			}
			return -1;
		}
        private void WalkWay(int typ)
        {
            BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Instance;
            MethodInfo method = typeof(WorldMapSystem).GetMethod("GetMoveKey", flag);
            object[] pra = new object[] { typ };
            method.Invoke(WorldMapSystem.instance, pra);
            //UnityModManager.Logger.Log("开始走路");
        }
        public void WalkStar()
        {
            if (Main.Place.Count > 0)
            {
                if(!(YesOrNoWindow.instance.MaskShow() || 
                    WorldMapSystem.instance.partWorldMapWindow.activeInHierarchy ||
                    SystemSetting.instance.SystemSettingWindow.activeInHierarchy ||
                    StorySystem.instance.storySystem.activeInHierarchy ||
                    DateFile.instance.battleStart ||
                    UIDate.instance.trunChangeImage[0].gameObject.activeSelf ||
                    HomeSystem.instance.homeSystem.activeInHierarchy))
                {
                    if (!DateFile.instance.playerMoveing)
                    {
                        WalkWay(Main.Place[0]);
                        if (DateFile.instance.dayTime >= DateFile.instance.GetMapMoveNeed(DateFile.instance.mianPartId, Main.PlaceRc[0]))
                        {
                            Main.Place.RemoveAt(0);
                            Main.PlaceRc.RemoveAt(0);
                            //UnityModManager.Logger.Log(Main.Place.Count.ToString());
                        }

                    }
                }
                
            }
            
            
        }
		private int[] Neibor(int PlaceId,int Size)
		{
			int[] Nei = new int[4];
			Nei[0] = PlaceId - 1;
			Nei[1] = PlaceId + 1;
			Nei[2] = PlaceId - Size;
			Nei[3] = PlaceId + Size;
			if (PlaceId % Size == 0)
			{
				Nei[0] = -1;
			}
			if (PlaceId % Size == Size - 1)
			{
				Nei[1] = -1;
			}
			if (PlaceId < Size)
			{
				Nei[2] = -1;
			} 
			if (PlaceId >= Size * ( Size - 1 ))
			{
				Nei[3] = -1;
			}
			return Nei;
		}
		private int Distance(int ChooseId,int PlaceId,int Size)
		{
			return Math.Abs((ChooseId / Size - PlaceId / Size)) + Math.Abs((ChooseId % Size - PlaceId % Size));
		}
		public void WalkList(int PartId, int ChooesPlaceId,int PlaceId,int Size)
		{
            //UnityModManager.Logger.Log("Start Search");
            int[] Father = new int[Size * Size];
			List<int> Open = new List<int>();
            List<int> Close = new List<int>();
            int[] Weight = new int[Size * Size];
            int[] WeightRec = new int[Size * Size];
            int[] EditorWeight = new int[3] { int.Parse(Main.settings.Time), int.Parse(Main.settings.Enemy), int.Parse(Main.settings.Anyuan) };
            //UnityModManager.Logger.Log("1");
            for (int i = 0; i < Size * Size; i++)
            {
                WeightRec[i] = Math.Abs(DateFile.instance.GetMapMoveNeed(PartId, i)) * EditorWeight[0] + Math.Abs(DateFile.instance.HaveActor(PartId, i, false, false, true, false).Count) * EditorWeight[1] + Math.Abs((((DateFile.instance.GetNewMapDate(PartId, i, 999) == "20002") ? EditorWeight[2] : 0)));
            }
            UnityModManager.Logger.Log(EditorWeight[0].ToString() + EditorWeight[1].ToString() + EditorWeight[2].ToString());
            //UnityModManager.Logger.Log("2");
            Close.Remove(ChooesPlaceId);
            Open.Add(PlaceId);
            //UnityModManager.Logger.Log("3");
            while (Open.Count != 0)
            {
                int Now = Open[0];
                //UnityModManager.Logger.Log("41");
                Open.RemoveAt(0);
                //UnityModManager.Logger.Log("42");
                Close.Add(Now);
                //UnityModManager.Logger.Log("43");
                if (Now == ChooesPlaceId) { break; }
                int[] Nei = Neibor(Now, Size);
                //UnityModManager.Logger.Log("4");
                foreach (int i in Nei)
                {
                    if (i == -1) { continue; }
                    if (Close.Contains(i)) { continue; }
                    if (!Open.Contains(i))
                    {
                        //UnityModManager.Logger.Log("51");
                        Open.Add(i);
                        //UnityModManager.Logger.Log("52");
                        Father[i] = Now;
                        //UnityModManager.Logger.Log("53");
                        Weight[i] = Weight[Now] + WeightRec[i];
                        //UnityModManager.Logger.Log("5");
                    }
                    else
                    {
                        //UnityModManager.Logger.Log("61");
                        if (Weight[i]>(Weight[Now]+WeightRec[i]))
                        {
                            Father[i] = Now;
                            Weight[i] = Weight[Now] + WeightRec[i];
                        }
                        //UnityModManager.Logger.Log("6");
                    }
                }
                    Open.Sort((left, right) =>
                    {
                        if ((Distance(ChooesPlaceId, left, Size)+Weight[left]) > (Distance(ChooesPlaceId, right, Size) + Weight[right])) { return 1; }
                        else if ((Distance(ChooesPlaceId, left, Size) + Weight[left]) == (Distance(ChooesPlaceId, right, Size) + Weight[right])) { return 0; }
                        else { return -1; }
                    });
            }
			int P = ChooesPlaceId;
			while (P != PlaceId)
			{
				Main.Place.Insert(0, Typ(Size,Father[P],P));
                Main.PlaceRc.Insert(0, P);
				P = Father[P];
				//UnityModManager.Logger.Log(Weight[P].ToString());
			}
        
		}

    }


    [HarmonyPatch(typeof(WorldMapSystem), "ShowChoosePlaceMenu", new Type[] { typeof(int), typeof(int), typeof(int), typeof(Transform) })]
    static class ShowChoosePlaceMenu_Pached
    {
        public static void Prefix(int worldId, int partId, int placeId, Transform placeImage, ref bool __state)
        {
            if (Main.settings.Mengluu)
            {
                int chooseWorldId = WorldMapSystem.instance.chooseWorldId;
                int choosePartId = WorldMapSystem.instance.choosePartId;
                int choosePlaceId = WorldMapSystem.instance.choosePlaceId;
                Transform choosePlace = WorldMapSystem.instance.choosePlace;
                List<int> playerNeighbor = WorldMapSystem.instance.playerNeighbor;
                //UnityModManager.Logger.Log("Pre Complete");
                if ((worldId == chooseWorldId) && (partId == choosePartId) && (placeId == choosePlaceId) && (placeImage == choosePlace) && (worldId == DateFile.instance.mianWorldId) && (partId == DateFile.instance.mianPartId) && (placeId != DateFile.instance.mianPlaceId) && !playerNeighbor.Contains(placeId))
                {
                    __state = true;
                }
                else
                {
                    __state = false;
                }
            }
            
        }
        public static void Postfix(bool __state)
        {
            if (Main.settings.Mengluu)
            {
                //UnityModManager.Logger.Log("Post Start");
                int MainPlaceId = DateFile.instance.mianPlaceId;
                int choosePartId = WorldMapSystem.instance.choosePartId;
                int choosePlaceId = WorldMapSystem.instance.choosePlaceId;
                if (__state)
                {

                    int Size = int.Parse(DateFile.instance.partWorldMapDate[choosePartId][98]);
                    Main.Place.Clear();
                    Walk WalkWay = new Walk();
                    WalkWay.WalkList(choosePartId, choosePlaceId, MainPlaceId, Size);
                    //UnityModManager.Logger.Log("Walk");
                }
            }
        }

    }
    [HarmonyPatch(typeof(WorldMapSystem), "Update")]
    static class Update_Pached
    {
        public static void Prefix(WorldMapSystem __instance)
        {
            if (Main.settings.Mengluu)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W))
                {
                    Main.Place.Clear();
                }
            }

        }
        public static void Postfix(WorldMapSystem __instance)
        {
            if (Main.settings.Mengluu)
            {
                Walk Walk = new Walk();
                Walk.WalkStar();
            }
             
        }
    }
}
