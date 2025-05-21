using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using DolocTown;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine.InputSystem;
using Mod.Utilities;
//using ModSettingManagerForDolocTown.API;
using UI_Doloctown;

namespace EasyFishing_DolocTown
{

    [BepInPlugin(G, N, V)]
    public class EasyFishingDolocTown : BaseUnityPlugin
    {
        private const string G = "CHun.Plugin.EasyFishing_Doloctown";
        private const string N = "EasyFishing_Doloctown";
        private const string V = "1.0";

        private static readonly ModLogger MyLogger = new ModLogger("[EasyFishing] ", isDebug: false);

        //-----------------------
        private static ConfigEntry<bool> _toggleLockCastTimer;
        private static ConfigEntry<bool> _toggleFastHook;
        private static ConfigEntry<bool> _toggleAutoHook;
        private static ConfigEntry<bool> _toggleSkipBattleGame;
        private static ConfigEntry<int> _intFairSpeedTime;
        private static ConfigEntry<KeyboardShortcut> _keyAutoFish;
        private const string SectionName = "EasyFishing";

        private void SetupConfig()
        {
            _toggleLockCastTimer = Config.Bind(SectionName, "抛竿完美", false, new ConfigDescription("锁定抛竿距离为完美（最远）", null, new ConfigurationManagerAttributes { Order = 0 }));
            _toggleFastHook = Config.Bind(SectionName, "快速上钩", false, new ConfigDescription("减少等待时机，快速上钩", null, new ConfigurationManagerAttributes { Order = -1 }));
            _toggleAutoHook = Config.Bind(SectionName, "自动上钩", false, new ConfigDescription("自动触发上钩，无需用户点击", null, new ConfigurationManagerAttributes { Order = -2 }));
            _toggleSkipBattleGame = Config.Bind(SectionName, "跳过小游戏", false, new ConfigDescription("跳过Battle小游戏", null, new ConfigurationManagerAttributes { Order = -3 }));

            var avr = new AcceptableValueRange<int>(0, 30);
            _intFairSpeedTime = Config.Bind(SectionName, "钓鱼额外耗时(分钟)", 0, new ConfigDescription("每次钓鱼成功会跳过若干分钟 - 如果你只是希望减少后期钓鱼的机械重复操作，可以设置这个选项以平衡影响", avr, new ConfigurationManagerAttributes { Order = -4 }));

            _keyAutoFish = Config.Bind(SectionName, "自动钓鱼(按键)", new KeyboardShortcut(KeyCode.None), new ConfigDescription("自动挂机钓鱼(须手持鱼竿，启用后会周期性甩杆，同时其他设置会强制生效)-请自行配置快捷键，配置快捷键按下触发，任意按键按下结束", null, new ConfigurationManagerAttributes { Order = -5 }));


            // 注册到游戏UI
            Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
            if (apiType != null && apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static) != null)
            {
                Debug.LogWarning("[EasyFishing] 找到 ModSettingAPI！");

                dynamic api = apiType.GetMethod("Create").Invoke(null, null);
                api
                    .AddToggle(_toggleLockCastTimer)
                    .AddToggle(_toggleFastHook)
                    .AddToggle(_toggleAutoHook)
                    .AddToggle(_toggleSkipBattleGame)
                    .AddSlider(
                        _intFairSpeedTime,
                        avr,
                        scale: 5
                    )
                    .AddKeyBindDropdown(
                        _keyAutoFish,
                        new List<KeyCode>
                        {
                            KeyCode.None,
                            KeyCode.F1,
                            KeyCode.F2,
                            KeyCode.F3,
                            KeyCode.F4,
                            KeyCode.F5,
                            KeyCode.F6,
                            KeyCode.F7,
                            KeyCode.F8,
                            KeyCode.F9,
                            KeyCode.F10,
                            KeyCode.F11,
                            KeyCode.F12,
                        });
            }
            else
            {
                Debug.LogWarning("[EasyFishing] 未找到 ModSettingAPI，设置注入将被跳过！");
            }

        }


        public static Harmony MainHarmony;
        public static EasyFishingDolocTown Instance;

        public void Awake()
        {
            try
            {
                this.SetupConfig();
                Instance = this;
                MainHarmony = new Harmony("main");
                MainHarmony.PatchAll(typeof(EasyFishingDolocTown));
                MyLogger.Log("Harmony Patch Success!");

            }
            catch (Exception ex)
            {
                MyLogger.Log("Harmony Patch Fail: " + ex, Debug.LogError);
            }
        }


        //----钓鱼周期Ready-cast-Wait-Battle-Pull
        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingReady), "OnEnter")]
        public static bool AgentStateFishingReady_OnEnter_Prefix()
        {
            MyLogger.Log("钓鱼-Ready");
            return true;
        }

        // 功能一：抛竿距离为完美，并锁定
        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingReady), "OnPlay")]
        public static bool AgentStateFishingReady_OnPlay_Prefix(AgentStateFishingReady __instance)
        {
            if (!_toggleLockCastTimer.Value)
            {
                return true;
            }
            try
            {
                // 通过反射获取 _castTimer
                var castTimerField = AccessTools.Field(__instance.GetType(), "_castTimer");
                var castTimer = castTimerField.GetValue(__instance);

                // 获取 _interval 字段
                var intervalField = AccessTools.Field(castTimer.GetType(), "_interval");
                var interval = (float)intervalField.GetValue(castTimer);

                // 设置 _value 为 _interval
                var valueField = AccessTools.Field(castTimer.GetType(), "_value");
                valueField.SetValue(castTimer, interval);

                // 确保 _isIncrease 为 true，防止递减
                var isIncreaseField = AccessTools.Field(castTimer.GetType(), "_isIncrease");
                isIncreaseField.SetValue(castTimer, true);

            }
            catch (Exception ex)
            {
                MyLogger.Log("锁定抛竿距离计时器失败" + ex, Debug.LogWarning);
            }
            return true;
        }


        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingCast), "OnEnter")]
        public static bool AgentStateFishingCast_OnEnter_Prefix()
        {
            MyLogger.Log("钓鱼-Cast");
            return true;
        }

        private static bool _isAgentStateFishingWait;      // Wait状态标志
        // 功能二：快速上钩
        [HarmonyPostfix, HarmonyPatch(typeof(AgentStateFishingWait), "OnEnter")]
        public static void AgentStateFishingWait_OnEnter_Postfix(AgentStateFishingWait __instance)
        {
            MyLogger.Log("钓鱼-Wait");
            _isAgentStateFishingWait = true;

            if (!_toggleFastHook.Value)
            {
                return;
            }
            try
            {
                var t = __instance.GetType();
                // 让钩概率为1，保证RollFish被立即触发
                AccessTools.Field(t, "_hookProbability").SetValue(__instance, 1f);
                //AccessTools.Field(t, "_fishOnHookDuration").SetValue(__instance, 1f);
                // 让_tuCounter的计时器立即到点
                var tuCounter = AccessTools.Field(t, "_tuCounter").GetValue(__instance);

                tuCounter.GetType().GetMethod("SetInterval")?.Invoke(tuCounter, new object[] { 0.1f });
            }
            catch (Exception ex)
            {
                MyLogger.Log("快速上钩失败" + ex, Debug.LogWarning);
            }
        }
        [HarmonyPostfix, HarmonyPatch(typeof(AgentStateFishingWait), "OnExit")]
        public static void AgentStateFishingWait_OnExit_Postfix()
        {
            _isAgentStateFishingWait = false;
        }

        // 功能三：自动触发上钩
        [HarmonyPrefix, HarmonyPatch(typeof(DolocUserInput), "NormalFishing", MethodType.Getter)]
        public static bool BodyController_get_NormalFishing_Prefix(ref bool __result)
        {
            //MyLogger.Log($"触发-isAgentStateFishingWait{isAgentStateFishingWait}");
            if (_isAgentStateFishingWait && _toggleAutoHook.Value)
            {
                __result = true;
                return false; // 阻止原方法执行
            }
            return true;
        }

        // 功能四：跳过Battle节拍小游戏
        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingBattle), "OnEnter")]
        public static bool AgentStateFishingBattle_OnEnter_Prefix()
        {
            MyLogger.Log("钓鱼-Battle");
            //DolocAPI.gameManager.gameInitConfig.skipFishingGame = true;
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingBattle), "NextState")]
        public static bool AgentStateFishingBattle_NextState_Prefix(AgentStateFishingBattle __instance)
        {
            if (!_toggleSkipBattleGame.Value)
            {
                return true;
            }
            try
            {
                var gameHandle = Traverse.Create(__instance).Field("gameHandle").GetValue();
                var fieldInfo = gameHandle.GetType().GetField("currentGameStatus", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var gameStatusType = fieldInfo.FieldType;
                    var successValue = Enum.Parse(gameStatusType, "Success");
                    fieldInfo.SetValue(gameHandle, successValue);
                }
            }
            catch (Exception e)
            {
                MyLogger.Log("跳过Battle节拍小游戏失败" + e, Debug.LogWarning);
            }

            return true;
        }

        // 功能五：钓鱼平衡-每次钓鱼成功跳过若干分钟
        [HarmonyPrefix, HarmonyPatch(typeof(AgentStateFishingPull), "OnEnter")]
        public static bool AgentStateFishingPull_OnEnter_Prefix()
        {
            MyLogger.Log("钓鱼-Pull");
            return true;
        }
        [HarmonyPostfix, HarmonyPatch(typeof(AgentStateFishingPull), "ReceiveItem")]
        public static void AgentStateFishingPull_ReceiveItem_Postfix(AgentStateFishingPull __instance)
        {

            if (_intFairSpeedTime.Value > 0)
            {
                int passTime = _intFairSpeedTime.Value;
                var method = AccessTools.Method(typeof(DolocAPI), "Command_PassMinutes", new Type[] { typeof(int) });
                method.Invoke(null, new object[] { passTime });
                MyLogger.Log($"跳过了:{passTime}分钟");
            }

            var fishName = Traverse.Create(__instance)
                .Field("body")
                .Property("FishingCache")
                .Property("FishItem")
                .Property("name")
                .GetValue<string>();

            MyLogger.Log($"钓上来的鱼/物品: {fishName}");
        }


        // 功能六：自动钓鱼-----------------------------
        public static AutoFishController AutoFishControllerInstance { get; private set; }

        public void Start()
        {
            // 初始化 AutoFishController
            if (!AutoFishController.Instance)
            {
                var controllerObject = new GameObject("AutoFishController");

                AutoFishControllerInstance = controllerObject.AddComponent<AutoFishController>();
            }
        }
        public class AutoFishController : MonoBehaviour
        {
            private static AutoFishController _instance;
            public static AutoFishController Instance => _instance;

            public bool isInAutoFishing = false;

            public void Awake()
            {
                if (_instance && _instance != this)
                {
                    Destroy(gameObject); // 防止多实例
                    return;
                }
                _instance = this;
                DontDestroyOnLoad(gameObject); // 跨场景存在
            }
            public void Start()
            {
                MyLogger.Log("AutoFishController Start");
            }

            public void Update()
            {
                if (isInAutoFishing == true)
                {
                    if (Keyboard.current.anyKey.wasPressedThisFrame)
                    {
                        MyLogger.Log("任意按键被按下", Debug.LogError);
                        CancelAutoFish();
                    }
                }
                else
                {
                    if (EasyFishingDolocTown._keyAutoFish != null &&
                        EasyFishingDolocTown._keyAutoFish.Value.MainKey != KeyCode.None &&
                        EasyFishingDolocTown._keyAutoFish.Value.IsDown())
                    {
                        StartAutoFish();
                    }
                }
            }

            private Item _selectedItem ;
            private BodyController _body;
            private AgentStateManager _stateManager;
            private bool CheckLegal()
            {
                try
                {
                    _selectedItem = DolocAPI.SelectedItem;
                    _body = DolocAPI.agent;
                    _stateManager = _body.StateManager;

                    if (!_body)
                    {
                        MyLogger.Log("DolocAPI.agent为空", Debug.LogError);
                        return false;
                    }
                    if (_body.IsCurrentStateSupportUseItem == false && _stateManager.CheckState<AgentStateIdle>())
                    {
                        MyLogger.Log("当前状态不正确", Debug.LogError);
                        return false;
                    }
                    if (DolocUtils.IsInteractWithUI())
                    {
                        MyLogger.Log("当前有UI交互", Debug.LogError);
                        return false;
                    }
                    if (_selectedItem == null || !(_selectedItem is ItemFishingRod))
                    {
                        MyLogger.Log("未持有鱼竿", Debug.LogError);
                        return false;
                    }
                }
                catch (Exception ex) {
                    MyLogger.Log("未知错误" + ex, Debug.LogError);
                    return false;
                }
                return true;
            }
            public void UseFishRoll()
            {
                if (CheckLegal())
                {
                    _selectedItem.UseAsTool();
                    MyLogger.Log("触发使用鱼竿");
                }
                else
                {
                    CancelAutoFish();
                }
            }
            private readonly float _firstDelay = 0.2f;
            private readonly float _repeatDelay = 4f;
            private DynamicMessageBox _messageBox;
            private ConfigEntryGroup _settingGroup;

            public void StartAutoFish()
            {
                MyLogger.Log("开始自动钓鱼");
                Instance?.InvokeRepeating(nameof(UseFishRoll), _firstDelay, _repeatDelay);
                isInAutoFishing = true;
                _messageBox = new GameObject("DynamicMessageBox").AddComponent<DynamicMessageBox>();
                _messageBox.Show("自动钓鱼中...任意按键取消", Color.white);

                // 保存当前设置快照并设置为true
                _settingGroup = new ConfigEntryGroup(_toggleAutoHook, _toggleFastHook, _toggleLockCastTimer, _toggleSkipBattleGame);
                _settingGroup.TakeSnapshot();
                _settingGroup.SetAll(true);
            }
            public void CancelAutoFish()
            {
                MyLogger.Log("取消自动钓鱼");
                Instance?.CancelInvoke(nameof(UseFishRoll));
                isInAutoFishing = false;
                _messageBox.Hide();
                if (_messageBox)
                {
                    Destroy(_messageBox.gameObject);
                    _messageBox = null;
                }

                // 恢复设置快照
                _settingGroup.RestoreSnapshot();
            }
        }
    }
}
