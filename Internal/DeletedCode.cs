namespace ModSettingManagerForDolocTown.Internal
{
    internal class DeletedCode
    {
        #region 废弃函数InjectRebindActionByOriginal

        //[Obsolete("该方法代价过大，已废弃", true)]
        //private static void InjectRebindActionByOriginal(RebindActionSettingItem item, UserSettingType settingId)
        //{
        //    #region 注册RebindAction

        //    var newId = settingId.ToString();

        //    var modInfo = new RebindActionInfo(
        //        id: newId,
        //        action_path: new InputActionBinds("NormalInput", "Debug", ""), // Debug动作口
        //        action_linked_paths: Array.Empty<InputActionBinds>(),

        //        allow_empty: true,
        //        labels: new[] { item.Section }, // 用于避免冲突域的
        //        lock_mian_in_keyborad_mouse_mode: false,
        //        lock_mian_in_gamepad_mode: false
        //    );
        //    // 3. 注入到表
        //    DolocConfig.Tables.TbRebindActionInfos.DataMap[newId] = modInfo;
        //    DolocConfig.Tables.TbRebindActionInfos.DataList.Add(modInfo);
        //    //✅ 必须调用 resolve
        //    modInfo.Resolve(new Dictionary<string, object>
        //    {
        //        ["Settings.TbRebindActionInfos"] = DolocConfig.Tables.TbRebindActionInfos
        //    });
        //    //var settingId = (UserSettingType)506; // 你的自定义枚举 ID
        //    var displayName = item.Key;
        //    const string groupId = "controls";

        //    var cmp = new RebindActionSettingComponent(newId);
        //    var proto = new SettingProto(settingId, groupId, displayName, cmp);

        //    DolocConfig.Tables.TbUserSettings.DataMap[settingId] = proto;
        //    DolocConfig.Tables.TbUserSettings.DataList.Add(proto);

        //    //✅ 调用组件自身的 Resolve
        //    var tableMap = new Dictionary<string, object>
        //    {
        //        ["Settings.TbRebindActionInfos"] = DolocConfig.Tables.TbRebindActionInfos,
        //        // 其他字段如果需要也可以加
        //    };
        //    cmp.Resolve(tableMap); // ✅ ✅ ✅ 关键调用！

        //    #endregion


        //    // 打印
        //    foreach (var info in DolocConfig.Tables.TbRebindActionInfos.DataList)
        //    {
        //        Debug.Log(
        //            $"[RebindActionInfo] ID: {info.Id}, Path: {info.ActionPath?.Path}, Labels: {string.Join(",", info.Labels)}");
        //    }

        //    // 初始化值：ConfigEntry 中保存的是 KeyboardShortcut
        //    //DolocAPI.userSettings.SetValue(settingId, "null");

        //    // 监听游戏内修改按键（UI -> Config）
        //    //DolocAPI.RegisterMsgListener(settingId, (s, args) =>
        //    //{
        //    //    if (args is GameEventArgs<object> ev && ev.value is KeyboardShortcut shortcut)
        //    //    {
        //    //        item.Value = shortcut;
        //    //    }
        //    //});
        //}

        #endregion

        #region 废弃RebindActionSettingItem

        //-----------------原生按键RebindAction--------------------
        //[Obsolete("该方法代价过大，已废弃", true)]
        //internal class RebindActionSettingItem : SettingItem<KeyboardShortcut>
        //{
        //    public RebindActionSettingItem(ConfigEntry<KeyboardShortcut> entry) : base(entry,
        //        SettingItemType.RebindAction)
        //    {
        //    }
        //}

        //[Obsolete("该方法代价过大，已废弃", true)]
        //public static void AddRebindAction(ConfigEntry<KeyboardShortcut> entry)
        //{
        //    if (entry == null) throw new ArgumentNullException(nameof(entry));
        //    SettingData.AddItem(new RebindActionSettingItem(entry));
        //}


        #endregion

    }
}
