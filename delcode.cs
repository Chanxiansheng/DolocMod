
namespace EasyFishing_Doloctown
{
    internal class DeletedCode
    {
        // 控制台？？功能未定
        //public void openDev()
        //{
        //    var devHelper = GameObject.FindObjectOfType<DevHelper>();
        //    Debug.Log(devHelper.name);
        //    if (devHelper != null)
        //    {
        //        devHelper.gameObject.SetActive(true);
        //        devHelper.Console.gameObject.SetActive(true);
        //        devHelper.GetType()
        //            .GetField("shouldFuncListShowing", BindingFlags.NonPublic | BindingFlags.Instance)
        //            .SetValue(devHelper, true);

        //        var funcListField = typeof(DevHelper).GetField("funcList", BindingFlags.NonPublic | BindingFlags.Instance);
        //        var dict = (Dictionary<string, Action>)funcListField.GetValue(devHelper);
        //        dict["打印玩家信息"] = () =>
        //        {
        //            Debug.Log("玩家金币：");
        //        };
        //    }
        //}

        //public void GetRebindActionItem()
        //{
            //RebindActionUI component = gameObject.GetComponent<RebindActionUI>();
            //component.config = itemData;
            //component.Init(); // ❗️这行可能访问 null 内部字段
        //}

        // 有配置接口，暂时不知道怎么用
        //[HarmonyPostfix, HarmonyPatch(typeof(GameInitConfig), "RunPatchCommandScripts")]
        //public static void GameInitConfig_InitCommandScripts_Postfix(GameInitConfig __instance)
        //{
        //    //modDebug("!!!HarmonyPatch test");
        //    //modDebug(__instance);
        //    //__instance.skipFishingGame = true;
        //    //__instance.skipFishingWait = true;
        //    //__instance.forceRollFish = true;
        //}



        //public class Test:MonoBehaviour
        //{
        //    public void Start()
        //    {
        //        //从Scene下开始查找，根据GameObject的名字进行查找，允许使用"/"穿越层次查找
        //        GameObject.Find("Unity");

        //        //根据Tag查找一个GameObject
        //        GameObject.FindWithTag("Player");

        //        // 根据Tag批量查找GameObject
        //        GameObject.FindGameObjectsWithTag("Player");

        //        //查找第一个带有Camra组件的物体
        //        GameObject.FindObjectOfType<Camera>();

        //    }




        //}




        //// 功能三：自动触发上钩
        //// IL 82-99
        //[HarmonyTranspiler, HarmonyPatch(typeof(AgentStateFishingWait), "NextState")]
        //public static IEnumerable<CodeInstruction> AgentStateFishingWait_NextState_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    try
        //    {
        //        // 使用 CodeMatcher 精准定位插入点
        //        var matcher = new CodeMatcher(instructions)
        //            .MatchForward(false,
        //                new CodeMatch(OpCodes.Ldarg_0),
        //                new CodeMatch(OpCodes.Ldfld), 
        //                new CodeMatch(OpCodes.Callvirt, typeof(DolocUserInput).GetMethod("get_UserInput")),
        //                new CodeMatch(OpCodes.Callvirt, typeof(DolocUserInput).GetMethod("get_NormalUseTool")),
        //                new CodeMatch(OpCodes.Brtrue),
        //                new CodeMatch(OpCodes.Ldarg_0)
        //            );
        //        modDebug($"{matcher.Pos}");
        //        if (matcher.IsInvalid)
        //        {
        //            modDebug("IL Patch Failed! 未找到插入点", Debug.LogError);
        //            return instructions;
        //        }
        //        // 反射获取字段和方法
        //        FieldInfo toggleAutoHookField = typeof(EasyFishing_Doloctown).GetField(
        //            nameof(toggle_autoHook),
        //            BindingFlags.Public | BindingFlags.Static
        //        );
        //        MethodInfo getValueMethod = typeof(ConfigEntry<bool>).GetProperty("Value")?.GetGetMethod();

        //        if (toggleAutoHookField == null || getValueMethod == null)
        //        {
        //            modDebug("Reflection failed!", Debug.LogError);
        //            return instructions;
        //        }

        //        // 插入跳转逻辑
        //        int offset = 17; // IL_0142 的索引

        //        if (matcher.Pos + offset >= matcher.Length)
        //        {
        //            modDebug("Jump target out of range", Debug.LogError);
        //            return instructions;
        //        }
        //        Label targetLabel = new Label();
        //        matcher.Advance(offset);
        //        modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");
        //        matcher.Labels.Insert(0,targetLabel);
        //        matcher.Advance(-offset);
        //        modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");


        //        //matcher.Insert(new CodeInstruction(OpCodes.Ldsfld, toggleAutoHookField));
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");
        //        //matcher.Advance(1);
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");


        //        //matcher.Insert(new CodeInstruction(OpCodes.Call, getValueMethod));
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");
        //        //matcher.Advance(1);
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");

        //        //matcher.Insert(new CodeInstruction(OpCodes.Ldc_I4_1));
        //        //matcher.Advance(1);


        //        //matcher.Insert(new CodeInstruction(OpCodes.Brtrue, targetLabel));
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");
        //        //matcher.Advance(1);
        //        //modDebug($"{matcher.Pos}:{matcher.Opcode}-{matcher.Operand}");

        //        matcher.Insert(new CodeInstruction(OpCodes.Br, targetLabel));


        //        modDebug("IL Patch Success!");
        //        return matcher.InstructionEnumeration();
        //    }
        //    catch (Exception e)
        //    {
        //        modDebug($"IL Patch Failed! {e.Message}",Debug.LogError);
        //        return instructions;
        //    }
        //}


        // 功能六：自动钓鱼
        //private static bool enable_autofish = false;
        //public void Update()
        //{
        //    if (enable_autofish == true)
        //    {
        //        // 任意按键被按下取消自动钓鱼
        //        if (Keyboard.current.anyKey.wasPressedThisFrame)
        //        {
        //            enable_autofish = false;
        //            Instance?.CancelInvoke("UseFishRoll");
        //            modDebug("有按键被触发，自动钓鱼结束", Debug.LogError);
        //        }
        //    }
        //    else
        //    {
        //        if (key_autoFish.Value.IsDown())
        //        {
        //            Instance?.InvokeRepeating("UseFishRoll", 0.5f, 3f);

        //            //this.UseFishRoll();
        //            enable_autofish = true;
        //        }
        //    }
        //}

        //[HarmonyPostfix, HarmonyPatch(typeof(AgentStateFishingPull), "OnExit")]
        //public static void AgentStateFishingPull_OnExit_Postfix(AgentStateFishingPull __instance)
        //{
        //    //if (enable_autofish)
        //    //{
        //    //    Instance?.Invoke("UseFishRoll", 1.5f);
        //    //}
        //}


        //public void UseFishRoll()
        //{

        //    if (DolocAPI.agent == null)
        //    {
        //        modDebug("DolocAPI.agent为空，自动钓鱼失效", Debug.LogError);
        //        //enable_autofish = false;
        //        enable_autofish = false;
        //        Instance?.CancelInvoke("UseFishRoll");
        //        return;
        //    }
        //    if (DolocAPI.agent.IsCurrentStateSupportUseItem == false)
        //    {
        //        modDebug("当前状态不正确，自动钓鱼失效", Debug.LogError);
        //        //enable_autofish = false;
        //        enable_autofish = false;
        //        Instance?.CancelInvoke("UseFishRoll");
        //        return;
        //    }
        //    Item selectedItem = DolocAPI.SelectedItem;
        //    if (selectedItem == null || !(selectedItem is ItemFishingRod))
        //    {
        //        modDebug("未持有鱼竿,自动钓鱼失效", Debug.LogError);
        //        //enable_autofish = false;
        //        enable_autofish = false;
        //        Instance?.CancelInvoke("UseFishRoll");
        //        return;
        //    }
        //    selectedItem.UseAsTool();
        //    modDebug("触发使用鱼竿");
        //}


        //private struct FishingSettingsSnapshot
        //{
        //    public bool autoHook;
        //    public bool fastHook;
        //    public bool lockCastTimer;
        //    public bool skipBattleGame;

        //    public FishingSettingsSnapshot(bool auto, bool fast, bool lockCast, bool skip)
        //    {
        //        autoHook = auto;
        //        fastHook = fast;
        //        lockCastTimer = lockCast;
        //        skipBattleGame = skip;
        //    }

        //    public void Apply()
        //    {
        //        toggle_autoHook.Value = autoHook;
        //        toggle_fastHook.Value = fastHook;
        //        toggle_lockCastTimer.Value = lockCastTimer;
        //        toggle_skipBattleGame.Value = skipBattleGame;
        //    }

        //    public void AllSet(bool flag)
        //    {
        //        toggle_autoHook.Value = flag;
        //        toggle_fastHook.Value = flag;
        //        toggle_lockCastTimer.Value = flag;
        //        toggle_skipBattleGame.Value = flag;
        //    }
        //}

        //DolocAPI.ShowMessageBoxAttention("自动钓鱼中。。。",0f);
        //DolocAPI.uiSystem.messageBoxManager.messageBoxCool.Show("自动钓鱼中。。。", 10f);
        //DolocAPI.uiSystem.messageBoxManager.messageBoxCool.SetVisible(true);
        //DolocAPI.uiSystem.messageBoxManager.messageBoxNode.Show(LocSprites.UI_INFOICON_COMPLETE_24PX, "自动钓鱼中。。。");
        //DolocAPI.uiSystem.messageBoxManager.messageBoxNode.SetVisible(true);
        //DolocAPI.uiSystem.messageBoxManager.messageBoxCool.SetVisible(false);
        //DolocAPI.uiSystem.messageBoxManager.messageBoxNode.SetVisible(false);


        //static bool hasInjected = false;

        //[HarmonyPrefix, HarmonyPatch(typeof(SettingPanel), "Render")]

        //public static bool SettingPanel_Render_Postfix(SettingPanel __instance) {

        //    Debug.LogError($"SettingPanel Render Patch Success");

        //    // 避免重复注入
        //    if (hasInjected) return true;
        //    hasInjected = true;
        //    var titleMenu = __instance.titleMenu;
        //    Debug.LogError($"[MOD] TitleMenu{titleMenu.name}");

        //    if (titleMenu == null)
        //    {
        //        return true;
        //    }
        //    // 获取配置表
        //    var groupTableProp = AccessTools.Property(typeof(SettingPanel), "groupTable");
        //    var groupTable = groupTableProp.GetValue(__instance);

        //    var dataListProp = groupTable.GetType().GetProperty("DataList");
        //    var dataList = dataListProp.GetValue(groupTable) as IList<UserSettingGroupInfo>;

        //    // 创建你的设置分组
        //    var myGroup = new UserSettingGroupInfo(
        //        id: "MOD_MySettings",
        //        title: "模组扩展设置",    // 这是 UI 上会显示的文字
        //        device_paths: ""         // 暂时可为空，或模仿其他组
        //    );

        //    // 添加到列表
        //    dataList.Add(myGroup);

        //    Debug.Log("[MOD] 成功注入设置分组: " + myGroup.Title);

        //    return true;

        //// 获取 TitleMenu 里的按钮（通常是 Button 列表或 Transform 子物体）
        //Transform titleParent = titleMenu.transform.Find("Content") ?? titleMenu.transform;

        //Debug.LogError($"[MOD] TitleMenu 按钮{titleParent.name}");

        //if (titleParent.childCount == 0)
        //{
        //    return true;
        //}
        //// 克隆第一个 tab 作为模版
        //var sampleTab = titleParent.GetChild(0);
        //var modTab = GameObject.Instantiate(sampleTab, titleParent);
        //modTab.name = "MOD_Settings_Tab";
        //modTab.SetAsLastSibling(); // 放到最后

        //// 修改文本
        //var textComp = modTab.GetComponentInChildren<Text>();
        //if (textComp != null)
        //{
        //    textComp.text = "MOD 设置";
        //}

        //return true;

        //
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(SettingPanel), "InitSettingItems")]
        //public static void Debug_InitSettingItems(SettingPanel __instance)
        //{
        //    var uiItemMgr = Traverse.Create(__instance).Field<SettingUIItemManager>("uiItemMgr").Value;
        //    Debug.Log("[MOD] SettingPanel.InitSettingItems 前调试开始");
        //    if (uiItemMgr == null)
        //    {
        //        Debug.LogWarning("[MOD] ❌ 无法访问 uiItemMgr");
        //        return;
        //    }

        //    Debug.Log("[MOD] 🧩 SettingPanel.InitSettingItems 后调试开始：");
        //    foreach (var uiItem in uiItemMgr.selectableCmps)
        //    {
        //        var config = uiItem.config;
        //        string id = config?.Id.ToString() ?? "null";
        //        string title = config?.Title ?? "无标题";
        //        string group = config?.Group ?? "（无组）";
        //        string type = uiItem.GetType().Name;

        //        Debug.Log($"[MOD] ── UIItem: {type} ──");
        //        Debug.Log($"       → ID: {id}");
        //        Debug.Log($"       → Title: {title}");
        //        Debug.Log($"       → Group: {group}");

        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(TitleMenu), "Render")]
        //public static void DebugLabelsRender(string[] labels)
        //{
        //    Debug.Log("[MOD] 当前设置页签组：");
        //    foreach (var label in labels)
        //    {
        //        Debug.Log($" ├─ Label Group: {label}");
        //    }
        //}


        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(RebindActionSlot), "BindAction")]
        //public static void DebugSlotBinding(InputAction action, int bindingIndex, RebindActionUI rebindActionUI)
        //{
        //    Debug.Log($"[MOD] 🔧 绑定槽位:");
        //    Debug.Log($"  ├─ Action: {action?.name}");
        //    Debug.Log($"  ├─ Binding Index: {bindingIndex}");
        //    Debug.Log($"  ├─ Path: {action?.bindings[bindingIndex].path}");
        //    Debug.Log($"  ├─ Effective Path: {action?.bindings[bindingIndex].effectivePath}");
        //    Debug.Log($"  ├─ Label Group: {string.Join(",", rebindActionUI.rebindActionData.InputAction_Ref.Labels)}");
        //    Debug.Log($"  └─ Title: {rebindActionUI.config?.Title}");
        //}
        //var rawInt = (int)newVal;
        //// 把最新的真实值映射成 UI 值
        //var uiValue = Mathf.RoundToInt(rawInt / (float)item.Scale);

        //// 若值未变，说明是 UI 触发后自身通知自己，无需重复设置
        //if (DolocAPI.userSettings.GetOrDefault(settingId) is int existing && existing == uiValue)
        //{
        //    return;
        //}
        //// 写到游戏设置里，SettingProto 会同步到对应组件
        //DolocAPI.userSettings.SetValue(settingId, uiValue);

        //// 🔁 刷新 UI 面板的滑块控件
        //if (DolocConfig.Tables.TbUserSettings.DataMap.TryGetValue(settingId, out proto))
        //{
        //    //if (SettingPanel.Instance.GetItemCmp<SliderItemUI>(settingId, out var sliderUI))
        //    //{
        //    //    sliderUI.InitValue(
        //    //        currentValue: uiValue,
        //    //        minValue: item.MinValue,
        //    //        maxValue: item.MaxValue,
        //    //        valueScale: item.Scale
        //    //    );
        //    //}
        //    // 已知你能获取 SettingPanelUiState

        //    var go = GameObject.Find("/CanvasOverlay_Panels/Container/SettingPanel(Clone)");
        //    var settingPanel = go?.GetComponent<SettingPanel>();

        //    if (!settingPanel)
        //    {
        //        Debug.Log("找不到settingPanel");
        //        return;
        //    }

        //    var uiItemMgr = Traverse.Create(settingPanel).Field("uiItemMgr").GetValue<SettingUIItemManager>();

        //    uiItemMgr.GetItemCmp<SliderItemUI>(settingId, out var sliderUI);

        //    var sliderItem = Traverse.Create(sliderUI).Field("slider").GetValue<Slider>();

        //    Debug.Log("Slider 类型: " + sliderItem.GetType().FullName);
        //    Debug.Log("sliderItem.value:"+sliderItem.value);
        //    Debug.Log("sliderItem.maxValue:" + sliderItem.maxValue);
        //    Debug.Log("sliderItem.minValue:" + sliderItem.minValue);
        //    Debug.Log("uiValue:"+sliderItem.value);

        //    sliderItem.value = uiValue;
        //}
        ////var eventArgs = new GameEventArgs<object>(item.Value);
        ////DolocAPI.Broadcast(settingId, eventArgs, null);
        ////DolocAPI.userSettings.UpdateCachedData();
    }
}
