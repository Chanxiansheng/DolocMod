# ModSettingManagerForDolocTown

**ModSettingManagerForDolocTown** 是一款为《多洛可小镇》（DolocTown）设计的轻量级设置注入插件，基于 BepInEx 框架开发。该插件允许 mod 开发者将自定义配置项整合进游戏原生设置面板中，提供一致的用户体验和灵活的 API 支持。

---

## ✨ 功能特性

- 🎛️ **支持设置类型**：
  - `Toggle`（开关）
  - `Slider`（滑动条）
  - `Dropdown`（下拉菜单）
  - `KeyBindDropdown`（按键绑定）
- 🧩 **基于 BepInEx.ConfigEntry<T>**：
  - 所有配置项存储在标准的 BepInEx 配置系统中
  - 与 ConfigurationManager 插件完全兼容
- 📦 **支持多个 mod 共用**：
  - 设置项按 Section 分组，不同 mod 可在同一面板下并存
- 🧠 **友好的 API 调用方式**：
  - 支持链式调用（Fluent 风格）

---

## 📦 使用方式

1. 为了避免对 `ModSettingManagerForDolocTown` 的强依赖，建议通过 **反射调用** 的方式注册设置项。这样即使未安装该管理器，Mod 仍可正常运行。
2. 当前仅支持在插件的 `Awake` 或 `Start` 生命周期内进行设置注入，**不支持运行时动态添加**配置项。

```c#
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

public class YourPlugin : BaseUnityPlugin
{
    private ConfigEntry<bool> toggleOption;
    private ConfigEntry<int> sliderOption;
    private ConfigEntry<string> dropdownOption;
    private ConfigEntry<KeyboardShortcut> keyBindOption;

    private void Start()
    {
        // 初始化配置
        toggleOption = Config.Bind("General", "EnableFeature", true, "开启某功能");
        sliderOption = Config.Bind("General", "Speed", 5, new ConfigDescription("速度", new AcceptableValueRange<int>(1, 10)));
        dropdownOption = Config.Bind("General", "Mode", "OptionA", new ConfigDescription("模式", new AcceptableValueList<string>("OptionA", "OptionB", "OptionC")));
        keyBindOption = Config.Bind("General", "Hotkey", new KeyboardShortcut(KeyCode.F1), "快捷键");

        // 反射调用 ModSettingManager API
        Type apiType = Type.GetType("ModSettingManagerForDolocTown.API.ModSettingAPI, ModSettingManagerForDolocTown");
        if (apiType != null)
        {
            MethodInfo createMethod = apiType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            if (createMethod != null)
            {
                dynamic api = createMethod.Invoke(null, null);
                api
                    .AddToggle(toggleOption)
                    .AddSlider(sliderOption, new AcceptableValueRange<int>(1, 10))
                    .AddDropdown(dropdownOption, new AcceptableValueList<string>("OptionA", "OptionB", "OptionC"))
                    .AddKeyBindDropdown(keyBindOption, new List<KeyCode> {
                        KeyCode.None, KeyCode.F1, KeyCode.F2, KeyCode.F3,
                        KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7,
                        KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12
                    });
            }
            else
            {
                Debug.LogWarning("[YourMod] ModSettingAPI.Create 方法未找到");
            }
        }
        else
        {
            Debug.LogWarning("[YourMod] 未找到 ModSettingManagerForDolocTown 插件，设置注入跳过");
        }
    }
}

```

