using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModUtilities;
using System;
using System.Reflection;
using UnityEngine;

// using ModSettingManagerForDolocTown.API;

namespace NpcMapMarkers_Doloctown
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class NpcMapMarkers : BaseUnityPlugin
    {
        private const string ModGuid = "YourCompany.NpcMapMarkers_Doloctown";
        private const string ModName = "NpcMapMarkers_Doloctown";
        private const string ModVersion = "1.0";

        private new static readonly ModLogger Logger = new ModLogger(ModName, isDebug: true);

        //====================== 配置项字段（示例，可删改） ======================
        //private static ConfigEntry<bool> _toggleFeatureX;
        //private static ConfigEntry<int> _numericParameterY;
        //private static ConfigEntry<KeyboardShortcut> _keyActionZ;

        private const string Section = ModName;

        private void SetupConfig()
        {
            //_toggleFeatureX = Config.Bind(
            //    Section,
            //    "启用功能X",
            //    false,
            //    new ConfigDescription("启用某个核心功能", null, new ConfigurationManagerAttributes { Order = 0 })
            //);

            //var intRange = new AcceptableValueRange<int>(0, 100);
            //_numericParameterY = Config.Bind(
            //    Section,
            //    "参数Y（数值）",
            //    10,
            //    new ConfigDescription("该数值用于控制 Y 行为的强度或延迟等", intRange, new ConfigurationManagerAttributes { Order = -1 })
            //);

            //_keyActionZ = Config.Bind(
            //    Section,
            //    "触发操作Z的按键",
            //    new KeyboardShortcut(KeyCode.None),
            //    new ConfigDescription("用于触发某个特殊操作的快捷键", null, new ConfigurationManagerAttributes { Order = -2 })
            //);

            //TryRegisterWithModSettingManager();
        }

        private void TryRegisterWithModSettingManager()
        {
            Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
            if (apiType != null && apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null)
            {
                try
                {
                    Logger.Log("发现 ModSettingAPI，尝试注入设置项...");
                    dynamic api = apiType.GetMethod("Create").Invoke(null, null);

                    // 示例注册（取消注释并替换变量）
                    // api.AddToggle(_toggleFeatureX);
                    // api.AddSlider(_numericParameterY);
                    // api.AddKeyBind(_keyActionZ);
                }
                catch (Exception ex)
                {
                    Logger.Log("设置注入失败：" + ex.Message, Debug.LogError);
                }
            }
            else
            {
                Logger.Log("未找到 ModSettingAPI，跳过设置注入", Debug.LogWarning);
            }
        }

        public void Awake()
        {
            try
            {
                SetupConfig();
                Harmony.CreateAndPatchAll(typeof(NpcMapMarkers));
                Logger.Log("MOD 启动并完成 Harmony 注入！");
            }
            catch (Exception ex)
            {
                Logger.Log("初始化失败：" + ex, Debug.LogError);
            }
        }
    }
}
