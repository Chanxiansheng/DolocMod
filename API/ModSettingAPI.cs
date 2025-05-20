using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace ModSettingManagerForDolocTown.API
{
    public static class ModSettingAPI
    {
        public static void AddToggle(ConfigEntry<bool> entry)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddToggle(entry);
        }

        public static void AddSlider(ConfigEntry<int> entry, AcceptableValueRange<int> range, int scale = 1)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddSlider(entry, range, scale);
        }

        public static void AddDropdown(ConfigEntry<string> entry, AcceptableValueList<string> list,
            bool useL10N = false)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddDropdown(entry, list, useL10N);
        }

        public static void AddKeyBindDropdown(ConfigEntry<KeyboardShortcut> entry, List<KeyCode> keys,
            bool useL10N = false)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddKeyBindDropdown(entry, keys, useL10N);
        }
    }
}
