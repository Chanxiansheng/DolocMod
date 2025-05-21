using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace ModSettingManagerForDolocTown.Internal
{
    /// <summary>
    /// 数据管理类
    /// </summary> 
    public class ModSettingItemManager
    {
        // 单例
        //public static ModSettingItemManager Instance { get; } = new ModSettingItemManager();
        private static ModSettingItemManager _instance;

        public ModSettingItemManager()
        {
            if (_instance != null)
            {
                // 如果已经创建过，直接复用已有实例
                Debug.Log("[ModSettingItemManager] 全局实例已经存在");
                return;
            }

            // 初始化逻辑
            _instance = this;

            // 可选初始化代码...
            Debug.Log("[ModSettingItemManager] 创建全局实例");
        }

        //双层数据结构 按 Section 分组（以 object 存储支持泛型）
        internal Dictionary<string, List<ISettingItem>> SectionDictionary { get; } = new Dictionary<string, List<ISettingItem>>();
        private void AddItem(ISettingItem item)
        {
            var sectionName = item.Section;

            if (!SectionDictionary.ContainsKey(sectionName))
            {
                SectionDictionary.Add(sectionName, new List<ISettingItem>());
            }
            SectionDictionary[sectionName].Add(item);
        }

        // 暴露的接口
        public void AddToggle(ConfigEntry<bool> entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            AddItem(new ToggleSettingItem(entry));
        }
        public void AddSlider(
            ConfigEntry<int> entry,
            AcceptableValueRange<int> validRange,
            int scale = 1)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (validRange == null) throw new ArgumentNullException(nameof(validRange));
            AddItem(new SliderSettingItem(entry, validRange, scale));
        }
        public void AddDropdown(
            ConfigEntry<string> entry,
            AcceptableValueList<string> acceptableList,
            bool useL10N = false)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (acceptableList == null) throw new ArgumentNullException(nameof(acceptableList));
            AddItem(new DropdownSettingItem(entry, acceptableList, useL10N));
        }
        public void AddKeyBindDropdown(
            ConfigEntry<KeyboardShortcut> entry,
            List<KeyCode> list,
            bool useL10N = false)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (list == null || list.Count == 0) throw new ArgumentException("按键列表不能为空", nameof(list));

            AddItem(new KeyBindDropdownSettingItem(entry, list, useL10N));
        }


        // 数据类

        public enum SettingItemType
        {
            Toggle,
            Slider,
            Dropdown,
            //RebindAction,
            KeyBindDropdown
        }

        internal interface ISettingItem
        {
            string Section { get; }
            string Key { get; }
            string Description { get; }

            SettingItemType ItemType { get; }

            // 新增：值变化事件（CM 端变更时触发）
            //event Action<object> OnValueChanged;
        }

        public abstract class SettingItem<T> : ISettingItem
        {
            public SettingItemType ItemType { get; }

            private readonly ConfigEntry<T> _entry;
            public string Section => _entry.Definition.Section;
            public string Key => _entry.Definition.Key;

            public string Description => _entry.Description?.Description
                                         ?? _entry.Definition?.Key
                                         ?? Key;

            public virtual T Value
            {
                get => _entry.Value;
                set => _entry.Value = value;
            }

            //public event Action<object> OnValueChanged;

            protected SettingItem(ConfigEntry<T> entry, SettingItemType type)
            {
                _entry = entry;
                ItemType = type;

                // 代理 CM 的事件
                //_entry.SettingChanged += (_, __) =>
                //{
                //    // 通知外部最新值
                //    OnValueChanged?.Invoke(_entry.Value);
                //};
            }
        }


        // ----------------开关Toggle--------------------
        public class ToggleSettingItem : SettingItem<bool>
        {
            public ToggleSettingItem(ConfigEntry<bool> entry) : base(entry, SettingItemType.Toggle)
            {
            }
        }

        //-----------------滑块Slider-------------------------
        public class SliderSettingItem : SettingItem<int>
        {
            private AcceptableValueRange<int> ValidRange { get; }
            public int MinValue => Mathf.CeilToInt(ValidRange.MinValue / (float)Scale);
            public int MaxValue => Mathf.FloorToInt(ValidRange.MaxValue / (float)Scale);
            public int Scale { get; }

            /// <summary>
            /// 这里的 Value 表示「UI 滑块上的值」，已经被缩放过。
            /// base.Value 始终是「ConfigEntry 里的真实值」。
            /// </summary>
            public override int Value
            {
                // UI 上看到的 = 真实值 * 缩放
                get => Mathf.RoundToInt(base.Value / (float)Scale);

                set
                {
                    var real = value * Scale;
                    base.Value = (int)ValidRange.Clamp(real);
                }
            }

            public SliderSettingItem(
                ConfigEntry<int> entry,
                AcceptableValueRange<int> validRange,
                int scale = 1
            ) : base(entry, SettingItemType.Slider)
            {
                if (scale <= 0)
                    throw new ArgumentException("scale 必须大于 0", nameof(scale));

                ValidRange = validRange;
                Scale = scale;

                base.Value = (int)validRange.Clamp(base.Value); // 钳制范围
            }

        }


        //-----------------下拉框Dropdown-------------------------
        public class DropdownSettingItem : SettingItem<string>
        {
            public List<string> Options { get; } // 值
            public List<string> Labels { get; } // UI 标签
            public bool UseL10N { get; } // 是否翻译

            public DropdownSettingItem(
                ConfigEntry<string> entry,
                AcceptableValueList<string> acceptableList,
                bool useL10N = false
            ) : base(entry, SettingItemType.Dropdown)
            {

                if (!acceptableList.AcceptableValues.Contains(base.Value))
                    base.Value = acceptableList.AcceptableValues[0];

                var opts = acceptableList.AcceptableValues.ToList();

                if (opts.Count == 0) throw new ArgumentException("acceptableList 必须包含至少一个元素", nameof(acceptableList));

                Options = opts;
                Labels = opts;
                UseL10N = useL10N;
            }

        }

        //----------------按键-下拉框映射 KeyBindDropdown-----------

        public class KeyBindDropdownSettingItem : SettingItem<KeyboardShortcut>
        {
            public List<string> Options { get; } // 值
            public List<string> Labels { get; } // UI 标签
            public bool UseL10N { get; } // 是否翻译

            private readonly Dictionary<string, KeyCode> _strToKey;

            private readonly Dictionary<KeyCode, string> _keyToStr;

            public string StringValue
            {
                get => _keyToStr.TryGetValue(Value.MainKey, out var str) ? str : "None";
                set => Value = _strToKey.TryGetValue(value, out var key) ? new KeyboardShortcut(key) : KeyboardShortcut.Empty;
            }

            public KeyBindDropdownSettingItem(
                ConfigEntry<KeyboardShortcut> entry,
                List<KeyCode> list,
                bool useL10N = false
            ) : base(entry, SettingItemType.KeyBindDropdown)
            {
                if (list == null || list.Count == 0)
                    throw new ArgumentException("KeyCode 列表不能为空", nameof(list));

                var currentKey = entry.Value.MainKey; // ✅ 提取当前值
                // 如果当前绑定的键不在提供的选项中，加入列表
                if (!list.Contains(currentKey))
                {
                    list.Insert(0, currentKey); //插到最前
                }

                // 构造映射表
                _strToKey = list.ToDictionary(k => k.ToString(), k => k);
                _keyToStr = list.ToDictionary(k => k, k => k.ToString());

                Options = _keyToStr.Values.ToList();
                Labels = new List<string>(Options);
                UseL10N = useL10N;
            }
        }

    }
}

