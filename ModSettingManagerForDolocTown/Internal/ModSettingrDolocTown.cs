﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using DolocTown;
using DolocTown.Config;
using DolocTown.Config.Settings;
using HarmonyLib;
using UnityEngine;


namespace ModSettingManagerForDolocTown.Internal

{
    [BepInPlugin(G, N, V)]
    public class ModSettingPatcherDolocTown : BaseUnityPlugin
    {
        private const string G = "CHun.Plugin.ModSettingManager_Doloctown";
        private const string N = "ModSettingManager_Doloctown";
        private const string V = "1.0";

        public static ModSettingItemManager Manager { get; } = new ModSettingItemManager();

        private readonly Harmony _mainHarmony = new Harmony("main");

        public void Awake()
        {
            //Debug.Log("类型名: " + typeof(ModSettingItemManager).AssemblyQualifiedName);
            //var test = Config.Bind<bool>("test", "test", true, new ConfigDescription("", null));
            //SettingManager.AddToggle(test);
            //settingManager = new ModSettingItemManager();

            MethodInfo targetMethod = typeof(UserSettings).GetMethod("OnEverythingLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var postfix = typeof(ModSettingPatcherDolocTown).GetMethod("UserSettings_OnEverythingLoaded_Postfix", BindingFlags.Static | BindingFlags.Public);

            _mainHarmony.Patch(targetMethod, postfix: new HarmonyMethod(postfix));

            //Harmony.CreateAndPatchAll(typeof(ModSettingPatcherDolocTown));
        }

        // 主注入入口 UserSettings => OnEverythingLoaded
        private static bool _hasPostfix;

        [HarmonyPostfix, HarmonyPatch(typeof(UserSettings), "OnEverythingLoaded")]
        public static void UserSettings_OnEverythingLoaded_Postfix()
        {
            if (_hasPostfix) return;
            _hasPostfix = true;

            if (!ModSettingUtils.DolocReflectionVerify.Verify())
            {
                Debug.LogError("[ModSettingInjector] Doloc核心结构绑定失败，ModSettingBinder 无法继续！");
                return;
            }

            Debug.Log("[ModSettingInjector] 开始注入模组设置项...");
            ModSettingUtils.InjectIntoUI();
        }

        public void Start()
        {
            //StartCoroutine(DeferredPatch());
        }
        private System.Collections.IEnumerator DeferredPatch()
        {
            // 稍微等一会，确保 InputSystem 初始化完毕
            yield return new WaitForSecondsRealtime(.5f); 
            var targetMethod = typeof(DolocAPI).GetMethod(
                "SaveUserSettings",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(UserSettings) },
                null
            );
            var prefix = typeof(ModSettingPatcherDolocTown).GetMethod(
                "DolocAPI_SaveUserSettings_Prefix",
                BindingFlags.Static | BindingFlags.Public
            );
            _mainHarmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));
            Debug.Log("[ModSettingInjector] 延迟 patch 成功！");
        }

        //保存UserSetting时，筛选数据并分离(弃用）
        [HarmonyPrefix, HarmonyPatch(typeof(DolocAPI), "SaveUserSettings", new[] { typeof(UserSettings) })]
        public static bool DolocAPI_SaveUserSettings_Prefix(UserSettings settings)
        {
            Debug.Log("[ModSettingInjector] 检测到 UserSetting 保存行为");

            var currentData = Traverse.Create(settings).Field<Dictionary<UserSettingType, object>>("currentData").Value;

            if (currentData == null)
            {
                Debug.LogWarning("[ModSettingInjector] currentData 为 null，跳过过滤");
                return true;
            }

            var keysToRemove = new List<UserSettingType>();

            foreach (var key in currentData.Keys)
            {
                if (ModSettingUtils.SettingIdGenerator.IsModSettingId(key))
                {
                    keysToRemove.Add(key);
                    Debug.Log($"[ModSettingInjector] 移除自定义配置项：{key}（ID={(int)key}）");
                }
            }

            foreach (var key in keysToRemove)
                currentData.Remove(key);

            return true;
        }


        private void OnApplicationQuit()
        {
            try
            {
                var settings = DolocAPI.userSettings;
                if (settings == null)
                {
                    Debug.LogWarning("[ModSettingInjector] DolocAPI.userSettings 为 null，json清理失败");
                    return;
                }

                var data = Traverse.Create(settings)
                    .Field<Dictionary<UserSettingType, object>>("currentData")
                    .Value;

                var keysToRemove = data.Keys
                    .Where(ModSettingUtils.SettingIdGenerator.IsModSettingId)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    data.Remove(key);
                    Debug.Log($"[ModSettingInjector] 清除退出前的配置项：{key}");
                }

                if (!DolocAPI.SaveUserSettings(settings))
                {
                    Debug.LogWarning("[ModSettingInjector] 退出时保存 调用DolocAPI.SaveUserSettings 失败");
                }
                else
                {
                    Debug.Log("[ModSettingInjector] 已清理 UserSettings.json");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ModSettingInjector] 退出清理过程中异常: " + ex);
            }
        }

        private static class ModSettingUtils
        {
            #region Inject工具类

            private const string GroupId = "MOD_SettingBinder_Group";
            private const string GroupTitle = "MOD 设置";
            private static bool _hasInjected;

            private static void RegisterProto(SettingProto proto, UserSettingType settingId)
            {
                var tbl = DolocConfig.Tables.TbUserSettings;
                tbl.DataList.Add(proto);
                tbl.DataMap[settingId] = proto;
            }

            private static void InjectToggle(ModSettingItemManager.ToggleSettingItem item, UserSettingType settingId)
            {
                // 构造组件
                var toggle = new ToggleSettingComponent(item.Value, false);
                var proto = new SettingProto(settingId, GroupId, item.Key, toggle);

                RegisterProto(proto, settingId);

                // 初始化值和回调
                DolocAPI.userSettings.SetValue(settingId, item.Value);

                // UI更新CM-回调函数
                DolocAPI.RegisterMsgListener(settingId, (s, args) =>
                {
                    if (args is GameEventArgs<object> ev && ev.value is bool b)
                        item.Value = b;
                });

                // CM更新UI-回调函数(未生效)
                //item.OnValueChanged += newVal => { DolocAPI.userSettings.SetValue(settingId, (bool)newVal); };
            }

            private static void InjectSlider(ModSettingItemManager.SliderSettingItem item, UserSettingType settingId)
            {
                // 构造滑块：UI 用 MinValue/MaxValue，Value 就是已适配的 UI 值
                var slider = new SliderSettingComponent(
                    default_value: item.Value,
                    min_value: item.MinValue,
                    max_value: item.MaxValue,
                    scale: item.Scale
                );
                var proto = new SettingProto(settingId, GroupId, item.Key, slider);

                RegisterProto(proto, settingId);

                DolocAPI.userSettings.SetValue(settingId, item.Value);

                // UI更新CM-回调函数
                DolocAPI.RegisterMsgListener(settingId, (s, args) =>
                {
                    Debug.Log("[ModSettingInjector] 触发回调");
                    if (args is GameEventArgs<object> ev)
                    {
                        // raw 是 UI 值，Round 后正好触发 item.Value 的 setter 做映射/Clamp
                        //var raw = (int)ev.value;
                        if (ev.value is float rawF)
                        {
                            item.Value = Mathf.RoundToInt(rawF);
                            //Debug.Log("float");
                        }
                        else if (ev.value is int rawI)
                        {
                            item.Value = rawI;
                            //Debug.Log("int");
                        }

                        Debug.Log("[ModSettingInjector] 更改值：" + item.Value);
                    }
                });

                //// CM更新UI-回调函数(未生效)
                //item.OnValueChanged += newVal =>
                //{


                //};
            }

            private static void InjectDropdown(ModSettingItemManager.DropdownSettingItem item, UserSettingType settingId)
            {
                // 组件只接受 string 列表
                var dropdown = new OptionSettingComponent(
                    default_value: item.Value,
                    option_values: item.Options,
                    use_l10n_key: item.UseL10N,
                    option_lables: item.Labels
                );
                var proto = new SettingProto(settingId, GroupId, item.Key, dropdown);


                RegisterProto(proto, settingId);

                DolocAPI.userSettings.SetValue(settingId, item.Value);
                DolocAPI.RegisterMsgListener(settingId, (s, args) =>
                {
                    if (args is GameEventArgs<object> ev && ev.value is string sel
                                                         && item.Options.Contains(sel))
                    {
                        item.Value = sel;
                    }
                });
            }

            private static void InjectKeyBindDropdown(
                ModSettingItemManager.KeyBindDropdownSettingItem item,
                UserSettingType settingId)
            {
                // 构造 OptionSettingComponent（注意：值为 string，UI 下拉使用字符串）
                var dropdown = new OptionSettingComponent(
                    default_value: item.StringValue,
                    option_values: item.Options,
                    use_l10n_key: item.UseL10N,
                    option_lables: item.Labels
                );
                // 创建 SettingProto，并注册
                var proto = new SettingProto(settingId, GroupId, item.Key, dropdown);
                RegisterProto(proto, settingId);
                // 设置默认值（注意：值为 string，转为 KeyCode 的字符串）
                DolocAPI.userSettings.SetValue(settingId, item.StringValue);
                // 注册监听器：当用户通过 UI 修改下拉框时，更新真实的 ConfigEntry<KeyboardShortcut>
                DolocAPI.RegisterMsgListener(settingId, (s, args) =>
                {
                    if (args is GameEventArgs<object> ev && ev.value is string sel
                                                         && item.Options.Contains(sel))
                    {
                        item.StringValue = sel; // 自动转换为 KeyCode + 更新 Value
                    }
                });
            }

            private static void InjectSectionHeader(string sectionTitle, UserSettingType settingId)
            {
                // 小标题组件
                var sectionHeader = new EmptySettingComponent();
                var titleText = $"{sectionTitle}";
                var proto = new SettingProto(settingId, GroupId, titleText, sectionHeader);
                RegisterProto(proto, settingId);
            }


            internal static class SettingIdGenerator
            {
                private static int _startId; // 起始自定义 ID（避免冲突）
                private static int _currentId;
                private static HashSet<int> _allocatedIds; // id使用记录表

                private static UserSettings userSettings;
                private static Dictionary<UserSettingType, object> data;

                public static void Initialize(int startId = 1000)
                {
                    _startId = startId;
                    _currentId = startId;
                    _allocatedIds = new HashSet<int> (); // 清空已分配ID集合

                    // 反射拿到字典
                    userSettings = DolocAPI.userSettings;
                    if (userSettings == null)
                    {
                        Debug.LogError("[ModSettingInjector] DolocAPI.userSettings 为 null，无法分配 ID");
                    }

                    data = Traverse.Create(userSettings)
                        .Field<Dictionary<UserSettingType, object>>("currentData")
                        .Value;
                    if (data == null)
                    {
                        Debug.LogError("[ModSettingInjector] currentData 为 null，无法进行 ID 去重");
                    }
                }

                public static int NextId()
                {
                    // 跳过所有已存在的 ID 和不合法ID
                    while (Enum.IsDefined(typeof(UserSettingType), _currentId)
                           ||
                        data.ContainsKey((UserSettingType)_currentId))
                    {
                        Debug.LogWarning($"[ModSettingInjector] 跳过已存在ID和不合法ID: {_currentId}");
                        _currentId++;
                    }

                    // 动态扩容
                    while (_currentId >= CurrentEventArrayLength)
                    {
                        Debug.LogWarning($"[ModSettingInjector] 目前事件列表长度{CurrentEventArrayLength}，目前ID{_currentId}，执行动态扩容");
                        // 持续扩容，直到满足需要
                        int newLength = Math.Max(CurrentEventArrayLength + 32, _currentId + 1);
                        if (!ExpandEventsArray(newLength))
                        {
                            Debug.LogError("[ModSettingInjector] 扩容事件表失败！");
                        }
                    }
                    int result = _currentId++;
                    _allocatedIds.Add(result);   // 显式记录
                    return result;
                }
                /// <summary>
                /// 判断指定 ID 是否属于 mod 的生成范围
                /// </summary>
                public static bool IsModSettingId(UserSettingType id)
                {
                    return _allocatedIds.Contains((int)id);
                }

                private static int CurrentEventArrayLength => TryGetEventsArray()?.Length ?? 0;
                private static Array TryGetEventsArray()
                {
                    try
                    {
                        var messageSystemObj = Traverse.Create(typeof(DolocAPI))
                            .Field("messageSystem")
                            .GetValue();

                        if (messageSystemObj == null)
                        {
                            Debug.LogError("[ModSettingInjector] DolocAPI.messageSystem 为 null！");
                            return null;
                        }

                        var currentArray = Traverse.Create(messageSystemObj)
                            .Field("_events")
                            .GetValue() as Array;

                        if (currentArray == null)
                        {
                            Debug.LogError("[ModSettingInjector] _events 字段为 null 或类型错误！");
                        }

                        return currentArray;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ModSettingInjector] 获取 _events 时发生异常：{ex}");
                        return null;
                    }
                }

                /// <summary>
                /// 使用 Harmony Traverse 扩容 DolocAPI.messageSystem._events 数组
                /// </summary>
                private static bool ExpandEventsArray(int requiredLength)
                {
                    try
                    {
                        // Traverse 到 DolocAPI.messageSystem
                        var messageSystemObj = Traverse.Create(typeof(DolocAPI))
                            .Field("messageSystem")
                            .GetValue();

                        if (messageSystemObj == null)
                        {
                            Debug.LogError("[ModSettingInjector] DolocAPI.messageSystem 为 null！");
                            return false;
                        }

                        // Traverse 到 _events 字段
                        var traverse = Traverse.Create(messageSystemObj);
                        var currentArray = traverse.Field("_events").GetValue() as Array;

                        if (currentArray == null)
                        {
                            Debug.LogError("[ModSettingInjector] _events 字段为 null 或类型错误！");
                            return false;
                        }

                        var oldLen = currentArray.Length;
                        if (oldLen >= requiredLength)
                        {
                            Debug.Log("[ModSettingInjector] 当前事件表长度已满足，无需扩容。");
                            return true;
                        }

                        var elementType = currentArray.GetType().GetElementType();
                        if (elementType != null)
                        {
                            var newArray = Array.CreateInstance(elementType, requiredLength);
                            Array.Copy(currentArray, newArray, oldLen);

                            for (var i = oldLen; i < requiredLength; i++)
                            {
                                newArray.SetValue(Activator.CreateInstance(elementType), i);
                            }

                            traverse.Field("_events").SetValue(newArray);
                        }

                        Debug.Log($"[ModSettingInjector] 成功扩容事件表至 {requiredLength}。");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ModSettingInjector] 扩容事件表时异常：{ex}");
                        return false;
                    }
                }

            }

            public static void InjectIntoUI()
            {
                if (_hasInjected) return;
                _hasInjected = true;

                // 1. 初始化ID生成器
                SettingIdGenerator.Initialize(666);


                Debug.Log("[ModSettingBinder] 正在注入MOD设置...");

                // 2. 添加标签页
                var groupInfo = new UserSettingGroupInfo(
                    GroupId,
                    GroupTitle,
                    "mod::config::" + GroupId);

                DolocConfig.Tables.TbUserSettingGroupInfos.DataList.Add(groupInfo);
                DolocConfig.Tables.TbUserSettingGroupInfos.DataMap[GroupId] = groupInfo;

                // Debug口
                foreach (var kv in Manager.SectionDictionary)
                {
                    Debug.Log($"[ModSettingInjector] Section: {kv.Key}, Count: {kv.Value.Count}");
                }
                // 4. 遍历各Section
                foreach (var kv in Manager.SectionDictionary)
                {
                    var sectionName = kv.Key;
                    var itemsList = kv.Value;

                    // 4.1. 在 UI 里先插入一个小标题：Section
                    InjectSectionHeader(sectionName, (UserSettingType)SettingIdGenerator.NextId());

                    foreach (var item in itemsList)
                    {
                        try
                        {
                            // 4.2. 先拿到一个唯一 ID
                            var rawId = SettingIdGenerator.NextId();
                            var settingId = (UserSettingType)rawId;

                            // 4.3. 调用分发函数
                            switch (item.ItemType)
                            {
                                case ModSettingItemManager.SettingItemType.Toggle:
                                    InjectToggle((ModSettingItemManager.ToggleSettingItem)item, settingId);
                                    break;
                                case ModSettingItemManager.SettingItemType.Slider:
                                    InjectSlider((ModSettingItemManager.SliderSettingItem)item, settingId);
                                    break;
                                case ModSettingItemManager.SettingItemType.Dropdown:
                                    InjectDropdown((ModSettingItemManager.DropdownSettingItem)item, settingId);
                                    break;
                                case ModSettingItemManager.SettingItemType.KeyBindDropdown:
                                    InjectKeyBindDropdown((ModSettingItemManager.KeyBindDropdownSettingItem)item, settingId);
                                    break;
                                default:
                                    throw new NotSupportedException($"Unknown type {item.ItemType}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ModSettingInjector] 注入 {item.Key} 类型 {item.ItemType} 失败：{ex}");
                        }
                    }
                }

                // 刷新缓存
                DolocAPI.userSettings.UpdateCachedData();
                Debug.Log("[ModSettingInjector] 注入完成");
            }

            #endregion

            #region Inject校验类

            public static class DolocReflectionVerify
            {
                private static MessageSystem<UserSettingType> UserSettingMessageSystem { get; set; }
                private static FieldInfo EventsField { get; set; }

                private static Array EventsArray { get; set; }
                private static object Tables { get; set; }
                private static object TbUserSettings { get; set; }

                public static bool Verify()
                {
                    try
                    {
                        Debug.Log("[ModSettingInjector] DolocReflectionVerify 正在校验反射类型...");

                        // 获取 DolocAPI.messageSystem
                        var msField =
                            typeof(DolocAPI).GetField("messageSystem", BindingFlags.NonPublic | BindingFlags.Static);
                        if (msField == null)
                        {
                            Debug.LogError("[ModSettingInjector] 获取 DolocAPI.messageSystem 字段失败！");
                            return false;
                        }

                        UserSettingMessageSystem = msField.GetValue(null) as MessageSystem<UserSettingType>;
                        if (UserSettingMessageSystem == null)
                        {
                            Debug.LogError("[ModSettingInjector] DolocAPI.messageSystem 为 null 或类型不匹配！");
                            return false;
                        }

                        // 获取 _events 字段
                        EventsField = UserSettingMessageSystem.GetType()
                            .GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (EventsField == null)
                        {
                            Debug.LogError("[ModSettingInjector] 无法获取 _events 字段！");
                            return false;
                        }

                        EventsArray = EventsField.GetValue(UserSettingMessageSystem) as Array;
                        if (EventsArray == null)
                        {
                            Debug.LogError("[ModSettingInjector] _events 字段为空或类型不匹配！");
                            return false;
                        }

                        Debug.Log("[ModSettingInjector] DolocReflectionBridge 初始化成功！");

                        // 获取 DolocConfig.Tables
                        var tablesField =
                            typeof(DolocConfig).GetProperty("Tables", BindingFlags.Public | BindingFlags.Static);
                        if (tablesField == null)
                        {
                            Debug.LogError("[ModSettingInjector] DolocConfig.Tables 属性未找到！");
                            return false;
                        }

                        Tables = tablesField.GetValue(null);

                        // 获取 TbUserSettings
                        var userSettingsProp = Tables.GetType().GetProperty("TbUserSettings");
                        if (userSettingsProp == null)
                        {
                            Debug.LogError("[ModSettingInjector] TbUserSettings 属性未找到！");
                            return false;
                        }

                        TbUserSettings = userSettingsProp.GetValue(Tables);

                        // 获取 TbUserSettingGroupInfos
                        var groupInfosProp = Tables.GetType().GetProperty("TbUserSettingGroupInfos");
                        if (groupInfosProp == null)
                        {
                            Debug.LogError("[ModSettingInjector] TbUserSettingGroupInfos 属性未找到！");
                            return false;
                        }


                        var rebindProp = Tables.GetType().GetProperty("TbRebindActionInfos");
                        if (rebindProp == null)
                        {
                            Debug.LogError("[ModSettingInjector] 未能找到 TbRebindActionInfos");
                            return false;
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("[ModSettingInjector] DolocReflectionBridge 初始化异常：" + ex);
                        return false;
                    }
                }
            }
        }

        #endregion
    }
}