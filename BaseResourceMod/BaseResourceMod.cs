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
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;

namespace BaseResourceMod
{
    public class Settings : UnityModManager.ModSettings
    {
        public bool save_config = false;
        public bool load_custom_config = true;
        public bool save_sprite = false;
        public bool save_ququ = false;
        public bool save_avatar = false;
        public bool save_maptile = false;

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
        public static string backupdir = "./Backup/txt/";
        public static string backupimgdir = "./Backup/Texture/";
        public static string resdir = "./Data/";
        public static string imgresdir = "./Texture/";

        public static Dictionary<string, string> mods_res_dict = new Dictionary<string, string>();
        public static Dictionary<string, string> mods_sprite_dict = new Dictionary<string, string>();


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            
            if (!Directory.Exists(backupdir))
            {
                System.IO.Directory.CreateDirectory(backupdir);
            }

            Logger = modEntry.Logger;
            settings = Settings.Load<Settings>(modEntry);

            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            return true;
        }

        public static void registModResDir(UnityModManager.ModEntry modEntry,string respath)
        {
            mods_res_dict[modEntry.Info.DisplayName] = respath;
        }

        public static void registModSpriteDir(UnityModManager.ModEntry modEntry, string spritepath)
        {
            mods_sprite_dict[modEntry.Info.DisplayName] = spritepath;
        }

        public static void registModResourceDir(UnityModManager.ModEntry modEntry, string respath, string spritepath)
        {
            mods_res_dict[modEntry.Info.DisplayName] = respath;
            mods_sprite_dict[modEntry.Info.DisplayName] = spritepath;
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
            GUILayout.Label("基础资源框架：");
            settings.load_custom_config = GUILayout.Toggle(settings.load_custom_config, "启动时增量载入 游戏 根目录下 Data/txt 内的配置文件");
            GUILayout.Label("自定义配置文件命名方式形如 Item_date.txt.001.txt  其中数字如 001 为加载顺序，从001 开始 顺序加载，最后加载 不带数字后缀的文件");
            GUILayout.Label("更多信息参见 https://github.com/phorcys/Taiwu_mods");
            GUILayout.Label("以下为调试开发选项，正常游戏请关闭");
            settings.save_config = GUILayout.Toggle(settings.save_config, "启动时保存原始配置文件到游戏 根目录下的 Backup/txt 目录下");
            GUILayout.Label("开启后启动时，mod会保存当前版本游戏配置文件到 Backup/txt 目录下，txt为原始csv文件，json为解析后的游戏内配置数据");
            settings.save_sprite = GUILayout.Toggle(settings.save_sprite, "启动时保存基础Sprite 到游戏 根目录下的 Backup/Graphics 目录下");
            GUILayout.Label("开启后启动时，mod会保存当前版本 基础Sprite 到游戏 根目录下的 Backup/Graphics 目录下，格式为PNG图片");
            settings.save_ququ = GUILayout.Toggle(settings.save_ququ, "是否Dump 蛐蛐图像 （注意，很大，很慢）");
            settings.save_avatar = GUILayout.Toggle(settings.save_avatar, "是否Dump 人物纸娃娃图像 （注意，很大，很慢）");
            settings.save_maptile = GUILayout.Toggle(settings.save_maptile, "是否Dump 各类地形图像 （注意，很大，很慢）");
            GUILayout.EndVertical();
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static Dictionary<int, string> textcolor = new Dictionary<int, string>()
        {
            { 10000,"<color=#323232FF>"},
            { 10001,"<color=#4B4B4BFF>"},
            { 10002,"<color=#B97D4BFF>"},
            { 10003,"<color=#9B8773FF>"},
            { 10004,"<color=#AF3737FF>"},
            { 10005,"<color=#FFE100FF>"},
            { 10006,"<color=#FF44A7FF>"},
            { 20001,"<color=#E1CDAAFF>"},
            { 20002,"<color=#8E8E8EFF>"}, //九品灰
            { 20003,"<color=#FBFBFBFF>"}, //八品白
            { 20004,"<color=#6DB75FFF>"}, //七品绿
            { 20005,"<color=#8FBAE7FF>"}, //六品青
            { 20006,"<color=#63CED0FF>"}, //五品蓝
            { 20007,"<color=#AE5AC8FF>"}, //四品紫
            { 20008,"<color=#E3C66DFF>"}, //三品金
            { 20009,"<color=#F28234FF>"}, //二品橙
            { 20010,"<color=#E4504DFF>"}, //一品红
            { 20011,"<color=#EDA723FF>"},
        };

        static public Dictionary<string, string> date_instance_dict = new Dictionary<string, string>()
        {
            {
                "ability_date",
                "abilityDate"
            }, {
                "actorattr_date",
                "actorAttrDate"
            }, {
                "actorfeature_date",
                "actorFeaturesDate"
            }, {
                "actormassage_date",
                "actorMassageDate"
            }, {
                "actorname_date",
                "actorNameDate"
            }, {
                "actorsurname_date",
                "actorSurnameDate"
            }, {
                "age_date",
                "ageDate"
            }, {
                "aishoping_date",
                "aiShopingDate"
            }, {
                "allworldmap_date",
                "allWorldDate"
            }, {
                "attacktyp_date",
                "attackTypDate"
            }, {
                "basemission_date",
                "baseMissionDate"
            }, {
                "baseskill_date",
                "baseSkillDate"
            }, {
                "basetips_date",
                "baseTipsDate"
            }, {
                "battlemap_date",
                "battleMapDate"
            }, {
                "battlerated_date",
                "battleRatedDate"
            }, {
                "battlestate_date",
                "battleStateDate"
            }, {
                "battletyp_date",
                "battleTypDate"
            }, {
                "body_date",
                "bodyInjuryDate"
            }, {
                "buffattr_date",
                "buffAttrDate"
            }, {
                "changeequip_date",
                "changeEquipDate"
            }, {
                "cricketbattle_date",
                "cricketBattleDate"
            }, {
                "cricket_date",
                "cricketDate"
            }, {
                "cricketplace_date",
                "cricketPlaceDate"
            }, {
                "enemyrand_date",
                "enemyRandDate"
            }, {
                "enemyteam_date",
                "enemyTeamDate"
            }, {
                "event_date",
                "eventDate"
            }, {
                "gamelevel_date",
                "gameLevelDate"
            }, {
                "gangequip_date",
                "gangEquipDate"
            }, {
                "ganggroup_date",
                "presetGangGroupDate"
            }, {
                "ganggroupvalue_date",
                "presetGangGroupDateValue"
            }, {
                "generation_date",
                "generationDate"
            }, {
                "gongfa_date",
                "gongFaDate"
            }, {
                "gongfafpower_date",
                "gongFaFPowerDate"
            }, {
                "gongfaotherfpower_date",
                "gongFaFPowerDate"
            }, // otherFPower 合并入 gongFaFPowerDate
            {
                "goodness_date",
                "goodnessDate"
            }, {
                "homeplace_date",
                "basehomePlaceDate"
            }, {
                "homeshopevent_date",
                "homeShopEventDate"
            }, {
                "homeshopeventtyp_date",
                "homeShopEventTypDate"
            }, {
                "hometyp_date",
                "homeTypDate"
            }, {
                "identity_date",
                "identityDate"
            }, {
                "injury_date",
                "injuryDate"
            }, {
                "item_date",
                "presetitemDate"
            }, {
                "itempower_date",
                "itemPowerDate"
            }, {
                "loadingimage_date",
                "loadingImageDate"
            }, {
                "makeitem_date",
                "makeItemDate"
            }, {
                "massage_date",
                "massageDate"
            }, {
                "moodtyp_date",
                "moodTypDate"
            }, {
                "palceworld_date",
                "placeWorldDate"
            }, {
                "partworldmap_date",
                "partWorldMapDate"
            }, {
                "poison_date",
                "poisonDate"
            }, {
                "presetactor_date",
                "presetActorDate"
            }, {
                "presetgang_date",
                "presetGangDate"
            }, {
                "qivaluestate_date",
                "qiValueStateDate"
            }, {
                "readbook_date",
                "readBookDate"
            }, {
                "resource_date",
                "resourceDate"
            }, {
                "scorebooty_date",
                "scoreBootyDate"
            }, {
                "scorevalue_date",
                "scoreValueDate"
            }, {
                "skill_date",
                "skillDate"
            }, {
                "solarterms_date",
                "solarTermsDate"
            }, {
                "storybuff_date",
                "storyBuffDate"
            }, {
                "story_date",
                "baseStoryDate"
            }, {
                "storyevent_date",
                "storyEventDate"
            }, {
                "storyshop_date",
                "storyShopDate"
            }, {
                "storyterrain_date",
                "storyTerrain"
            }, {
                "studydisk_date",
                "studyDiskDate"
            }, {
                "talk_date",
                "talkDate"
            }, {
                "teaching_date",
                "teachingDate"
            }, {
                "timeworkbooty_date",
                "timeWorkBootyDate"
            }, {
                "tipsmassage_date",
                "tipsMassageDate"
            }, {
                "trunevent_date",
                "trunEventDate"
            }, {
                "villagename_date",
                "villageNameDate"
            }
        };

        public static Dictionary<string, string> sprite_instance_dict = new Dictionary<string, string>()
        {
            {"TrunEventImage", "trunEventImage"},
            {"HomeMap/ReadBookIcon", "readStateIcon"},
            {"HomeMap/StudyStateImage", "studyStateImage"},
            {"HomeMap/StudyBookPage", "bookPageIcon"},
            {"StoryMap/StoryPlaceIcon", "storyPlaceIcon"},
            {"ItemIcon/ItemIcon", "itemSprites"},
            {"ItemIcon/ItemIconBack", "itemBackSprites"},
            {"GongFaImage/GongFaIcon", "gongFaSprites"},
            {"GongFaImage/GongFaCostIcon", "gongFaCostSprites"},
            {"HomeMap/HomeMapBack", "homeMapSprites"},
            {"StoryMap/StoryTerrain", "storyMapTerrain"},
            {"StoryMap/StoryMapBase", "storyMapBase"},
            {"StoryMap/StoryMapArrow", "storyMapArrow"},
            {"NewGame/CityIcon", "mapPlaceIcon"},
            {"SkillImage/AllSkillIcon", "baseSkillIcon"},
            {"HomeMap/HomeMapTypIcon", "homeTypIcon"},
            {"Cricket/CricketPlaceImage", "cricketPlaceImage"},
            {"Cricket/CricketBox", "cricketBox"},
            {"AttIcons", "attSprites"},
            {"InjuryIcons", "injuryIcon"},
            {"FeatureStarIcon", "featureStarIcon"},
            {"BattleTypText", "battleTypSprite"},
            {"BattleEndTyp", "battleEndTypImage"},
            {"BattleTeamActionIcon", "battleTeamActionIcon"},
            {"BattleFirstText", "battleFristSprite"},
        };

        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Instance);

            if (field == null)
                throw new ArgumentException("fieldName", "No such field was found.");

            if (!typeof(T).IsAssignableFrom(field.FieldType))
                throw new InvalidOperationException("Field type and requested type are not compatible.");

            return (T)field.GetValue(obj);
        }

        public static void SetFieldValue<T>(object obj, string fieldName, T value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Instance);

            if (field == null)
                throw new ArgumentException("fieldName", "No such field was found.");

            if (!field.FieldType.IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("Field type and requested type are not compatible.");

            field.SetValue(obj, value);
        }

        static public Dictionary<int, Dictionary<int, string>>  getCSVDictRef(string cate)
        {
            if(cate == "ActorFace_Date")
            {
                return GetSprites.instance.actorFaceDate;
            }
            if(date_instance_dict.ContainsKey(cate))
            {
                return GetFieldValue<Dictionary<int, Dictionary<int, string>>>(DateFile.instance, date_instance_dict[cate]);
            }
            return null;
        }

        static public T getSpriteRef<T>(string cate)
        {
            if (sprite_instance_dict.ContainsKey(cate))
            {
                return GetFieldValue<T>(GetSprites.instance, sprite_instance_dict[cate]);
            }
            return default(T);
        }

        static public void SetSprite<T>(string cate, T value)
        {
            if (sprite_instance_dict.ContainsKey(cate))
            {
                SetFieldValue(GetSprites.instance, sprite_instance_dict[cate], value);
            }
        }
    }



}