using Harmony12;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace BaseResourceMod
{
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>保存原始配置文件</summary>
        public bool save_config = false;
        /// <summary>启动时增量载入配置文件</summary>
        public bool load_custom_config = true;
        /// <summary>将加载配置文件的详细信息记录到日志</summary>
        public bool detailed_custom_config_log = false;
        /// <summary>启动时载入外部贴图</summary>
        public bool load_custom_texture = true;
        /// <summary>将加载贴图文件的详细信息记录到日志</summary>
        public bool detailed_custom_texture_log = false;
        /// <summary>是否保存基础Sprite</summary>
        public bool save_common_sprite = false;
        /// <summary>是否保存蛐蛐图像</summary>
        public bool save_ququ = false;
        /// <summary>是否保存人物纸娃娃图像</summary>
        public bool save_avatar = false;
        /// <summary>是否保存各类地形图像</summary>
        public bool save_maptile = false;
        /// <summary>是否保存战斗背景</summary>
        public bool save_battleBack = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

    }
    public static class Main
    {
        internal static bool enabled;
        internal static Settings settings;
        internal static UnityModManager.ModEntry.ModLogger Logger;
        /// <summary>数据导出路径</summary>
        public const string backupdir = "./Backup/txt/";
        /// <summary>贴图导出路径</summary>
        public const string backupimgdir = "./Backup/Texture/";
        /// <summary>自定义数据导入路径</summary>
        public const string resdir = "./Data/";
        /// <summary>自定义贴图导入路径</summary>
        public const string imgresdir = "./Texture/";

        internal static readonly Dictionary<string, string> mods_res_dict = new Dictionary<string, string>();
        internal static readonly Dictionary<string, string> mods_sprite_dict = new Dictionary<string, string>();
        internal static GetSpritesInfoAsset getSpritesInfoAsset;
        /// <summary>保存自定义的sprite路径信息</summary>
        // 使用ValueTuple避免创建过多小Object, 需要注意ValueTuple是value type
        internal static readonly Dictionary<string, ValueTuple<string, Sprite>> customSpriteInfosDic = new Dictionary<string, ValueTuple<string, Sprite>>();
        /// <summary>待导出的sprite数目</summary>
        internal static int spriteCounter;
        /// <summary>index: 0. 需要载入的txt配置文件数目；1.成功载入的txt配置文件数目</summary>
        internal static int[] csvFilesCounter = new int[2];
        /// <summary>index: 0. 需要载入的外部贴图文件数目；1.成功载入的外部贴图文件数目</summary>
        internal static int[] imageFilesCounter = new int[2];


        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (!Directory.Exists(backupdir))
            {
                Directory.CreateDirectory(backupdir);
            }

            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            // 动态加载贴图类
            DynamicSetSprite dynamicSetSprites = SingletonObject.getInstance<DynamicSetSprite>();
            // 游戏贴图信息类
            getSpritesInfoAsset = (GetSpritesInfoAsset)Traverse.Create(dynamicSetSprites).Field("gsInfoAsset").GetValue();
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());
            return true;
        }
        /// <summary>
        /// 从基础资源框架MOD中导入自定义数据
        /// </summary>
        /// <param name="modEntry"></param>
        /// <param name="respath">外部数据路径</param>
        public static void registModResDir(UnityModManager.ModEntry modEntry, string respath)
        {
            mods_res_dict[modEntry.Info.DisplayName] = respath;
        }
        /// <summary>
        /// 从基础资源框架MOD中导入自定义贴图
        /// </summary>
        /// <param name="modEntry"></param>
        /// <param name="spritepath">外部贴图路径</param>
        public static void registModSpriteDir(UnityModManager.ModEntry modEntry, string spritepath)
        {
            mods_sprite_dict[modEntry.Info.DisplayName] = spritepath;
        }
        /// <summary>
        /// 从基础资源框架MOD中导入自定义数据和贴图
        /// </summary>
        /// <param name="modEntry"></param>
        /// <param name="respath">外部数据路径</param>
        /// <param name="spritepath">外部贴图路径</param>
        public static void registModResourceDir(UnityModManager.ModEntry modEntry, string respath, string spritepath)
        {
            mods_res_dict[modEntry.Info.DisplayName] = respath;
            mods_sprite_dict[modEntry.Info.DisplayName] = spritepath;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (!value)
            {
                return false;
            }

            enabled = value;

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical("Box");
            {
                GUILayout.Label("基础资源框架：");
                GUILayout.Label($"<color=#F28234>当前的游戏根目录:{Environment.CurrentDirectory}</color>");
                // 基本设置
                GUILayout.BeginVertical("Box");
                {
                    settings.load_custom_config = GUILayout.Toggle(settings.load_custom_config, $"启动时增量载入 游戏 根目录下 <color=#8FBAE7>{resdir}</color> 的<color=#F28234>子文件夹</color>内的配置文件");
                    GUILayout.Label("<color=#F28234>注意： 只有放置在<color=#F28234>子文件夹</color>内的配置文件才会增量载入: 例如<color=#8FBAE7>{resdir}</color>/abcd/Item_Date.txt.001.txt");
                    GUILayout.Label("自定义配置文件命名方式形如 Item_date.txt.001.txt  其中数字如 001 为加载顺序，从001开始顺序加载，最后加载不带数字后缀的文件，详情见更多信息。");
                    settings.load_custom_texture = GUILayout.Toggle(settings.load_custom_texture, $"启动时载入 游戏 根目录下 <color=#8FBAE7>{imgresdir}</color> 内的贴图");
                    GUILayout.Label($"自定义贴图目录结构设置方法参见更多信息。");
                }
                GUILayout.EndVertical();
                // 更多信息
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("更多信息参见:", GUILayout.ExpandWidth(false));
                    if (GUILayout.Button("https://github.com/phorcys/Taiwu_mods/tree/master/BaseResourceMod"))
                    {
                        Application.OpenURL("https://github.com/phorcys/Taiwu_mods/tree/master/BaseResourceMod");
                    }
                }
                GUILayout.EndHorizontal();
                // 结果统计
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("载入结果统计:");
                    GUILayout.Label($" 配置文件：<color=#8FBAE7>{resdir}</color> 中共有<color=#F28234>{csvFilesCounter[0]}</color>个txt文件，实际载入<color=#F28234>{csvFilesCounter[1]}</color>个txt文件，欲知详情请勾选\"将加载配置文件的详细信息记录到日志\"并重新启动游戏");
                    GUILayout.Label($" 外部贴图：<color=#8FBAE7>{imgresdir}</color> 中共有<color=#F28234>{imageFilesCounter[0]}</color>个png文件, 实际载入<color=#F28234>{imageFilesCounter[1]}</color>个png文件，欲知详情请勾选\"将加载贴图文件的详细信息记录到日志\"并重新启动游戏");
                }
                GUILayout.EndVertical();
                // 日志
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("日志：(用于查看哪些文件载入/替换成功)");
                    if (settings.load_custom_config)
                    {
                        GUILayout.BeginVertical("Box");
                        settings.detailed_custom_config_log = GUILayout.Toggle(settings.detailed_custom_config_log, "将加载配置文件的详细信息记录到日志（开始游戏时载入速度可能会变慢）");
                        GUILayout.EndVertical();
                    }
                    if (settings.load_custom_texture)
                    {
                        GUILayout.BeginVertical("Box");
                        settings.detailed_custom_texture_log = GUILayout.Toggle(settings.detailed_custom_texture_log, "将加载贴图文件的详细信息记录到日志（开始游戏时载入速度可能会变慢）");
                        GUILayout.EndVertical();
                    }
                    if (GUILayout.Button("打开日志文件", GUILayout.ExpandWidth(false)))
                    {
                        Process.Start($"{UnityModManager.modsPath}\\UnityModManager.log");
                    }
                }
                GUILayout.EndVertical();
                // 调试选项
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("<color=#F28234>以下为调试开发增量MOD和美化MOD选项，正常游戏请不要使用</color>");
                    GUILayout.BeginVertical("Box");
                    {
                        settings.save_config = GUILayout.Toggle(settings.save_config, $"<color=#F28234>游戏启动时</color>保存原始配置文件到游戏 根目录下的 <color=#8FBAE7>{backupdir}</color> 目录下");
                        GUILayout.Label("开启后游戏启动时(<color=#8FBAE7>跟下面的保存按钮无关</color>)，mod会保存当前版本游戏配置文件到 Backup/txt 目录下，txt为原始csv文件，json为解析后的游戏内配置数据");
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.Label("导出游戏贴图(Sprites)：");
                        GUILayout.Label($"将游戏中当前版本的Sprite保存到游戏根目录下的<color=#8FBAE7>{backupimgdir}</color>目录下，格式为PNG图片。\n<color=#F28234>注意: 会删除该目录下的所有文件!</color>");
                        settings.save_common_sprite = GUILayout.Toggle(settings.save_common_sprite, $"是否保存基础Sprite");
                        settings.save_ququ = GUILayout.Toggle(settings.save_ququ, "是否保存 蛐蛐图像 （注意，很大，很慢）");
                        settings.save_avatar = GUILayout.Toggle(settings.save_avatar, "是否保存 人物纸娃娃图像 （注意，很大，很慢）");
                        settings.save_maptile = GUILayout.Toggle(settings.save_maptile, "是否保存 各类地形图像 （注意，很大，很慢）");
                        //settings.save_battleBack = GUILayout.Toggle(settings.save_battleBack, "是否保存 各类战斗背景图像 （注意，很大，很慢）");
                        StartSaveSprites();
                        GUILayout.Label($"剩余<color=#F28234>{spriteCounter}</color>个sprite等待导出");
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static void StartSaveSprites()
        {
            // 游戏载入成功后再允许导出，否则会造成游戏UI NullReferenceException
            if (!DateFile.instance.openGame)
            {
                GUILayout.Label("请先进入游戏主菜单即可保存......");
            }
            else
            {
                if (spriteCounter == 0)
                {
                    if (GUILayout.Button("点击导出贴图(Sprites)", GUILayout.Width(150f)))
                    {
                        try
                        {
                            // 删除备份目录中的文件并导出贴图
                            if (Directory.Exists(backupimgdir))
                            {
                                Directory.Delete(backupimgdir, true);
                            }
                            Directory.CreateDirectory(backupimgdir);
                            SpriteLoadHelper.GetInstance().DumpCommonSprite();
                            SpriteLoadHelper.GetInstance().DumpSpecialSprite();
                            SpriteLoadHelper.ClearInstance();
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"保存Sprite:\n{e.ToString()}");
                        }
                    }
                }
                else
                {
                    GUILayout.Label("正在导出...", GUILayout.Width(150f));
                }
            }
        }

        internal static readonly Dictionary<int, string> textcolor = new Dictionary<int, string>
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

        internal static readonly Dictionary<string, string> date_instance_dict = new Dictionary<string, string>
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
                "juniorxiangshu_data",
                "juniorXiangshuData"
            }, {
                "legendbook_data",
                "legendBookData"
            }, {
                "legendbookeffect_data",
                "legendBookEffectData"
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
                "taichidisc_data",
                "taichiDiscData"
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
            },
            {
                "fame_date",
                "actorFameDate"
            }
        };

        internal static readonly Dictionary<string, string> sprite_instance_dict = new Dictionary<string, string>
        {
            {"TrunEventImage", "trunEventImage"},
            {"HomeMap/HomeBuildingIcon", "buildingSprites"},
            {"HomeMap/ReadBookIcon", "readStateIcon"},
            {"HomeMap/StudyStateImage", "studyStateImage"},
            {"HomeMap/StudyBookPage", "bookPageIcon"},
            {"StoryMap/StoryPlaceIcon", "storyPlaceIcon"},
            {"ItemIcon/ItemIcon", "itemSprites"},
            {"ItemIcon/ItemIconBack", "itemBackSprites"},
            {"GongFaImage/GongFaIcon", "gongFaSprites"},
            {"GongFaImage/GongFaCostIcon", "gongFaCostSprites"},
            {"GongFaImage/BigGongFaImage", "bigGongFaImage"},
            {"GangPowerIcon", "gangPowerIcon"},
            {"GoodnessIcon", "goodnessIcon"},
            {"HomeMap/HomeMapSprites", "homeMapSprites"},
            {"HomeMap/HomeMapBack", "homeMapBack"},
            {"StoryMap/StoryTerrain", "storyMapTerrain"},
            {"StoryMap/StoryMapBase", "storyMapBase"},
            {"StoryMap/StoryMapArrow", "storyMapArrow"},
            {"StoryMap/StoryEvent", "eventSprites"},
            {"NewGame/CityIcon", "mapPlaceIcon"},
            {"SkillImage/AllSkillIcon", "baseSkillIcon"},
            {"SkillImage/BigSkillImage", "bigSkillImage"},
            {"SkillImage/SkillImage", "baseSkillIcon"},
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
            {"BattleUseImage", "battleUseImage"},
            {"BattlerDangerIcon", "battlerDangerIcon"},
            {"MakeResourceIcon", "makeResourceIcon"},
            {"PartIcon", "partIcon"},
            {"WorldMapPLayerIcon", "worldMapPlayerIcon"},
            {"PoisonSprites", "poisonSprites"},
            {"TimePaseSprite", "timePaseSprite"},
            {"TimeWorkImage", "timeWorkImage"},
        };

        /*internal static readonly Dictionary<string, string> sprite_loadpath_dict =new Dictionary<string, string>
        {
            {
                ""
            }
        }*/

        internal static T GetFieldValue<T>(object obj, string fieldName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Instance);

            if (field == null)
            {
                throw new ArgumentException("fieldName", "No such field was found.");
            }

            if (!typeof(T).IsAssignableFrom(field.FieldType))
            {
                throw new InvalidOperationException("Field type and requested type are not compatible.");
            }

            return (T)field.GetValue(obj);
        }

        /// <summary>
        /// 获取数据文件对应的游戏数据字典
        /// </summary>
        /// <param name="cate"></param>
        /// <returns></returns>
        internal static Dictionary<int, Dictionary<int, string>> GetCSVDictRef(string cate)
        {
            if (cate == "actorface_date")
            {
                return GetSprites.instance.actorFaceDate;
            }
            if (date_instance_dict.TryGetValue(cate, out var dictName))
            {
                return GetFieldValue<Dictionary<int, Dictionary<int, string>>>(DateFile.instance, dictName);
            }
            return null;
        }
    }
}
