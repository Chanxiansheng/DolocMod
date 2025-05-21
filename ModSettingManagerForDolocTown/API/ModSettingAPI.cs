using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace ModSettingManagerForDolocTown.API
{
    public static class ModSettingAPI
    {
        public static ModSettingBuilder Create()
        {
            return new ModSettingBuilder();
        }
    }
    public class ModSettingBuilder
    {
        public ModSettingBuilder AddToggle(ConfigEntry<bool> entry)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddToggle(entry);
            return this;
        }

        public ModSettingBuilder AddSlider(ConfigEntry<int> entry, AcceptableValueRange<int> range, int scale = 1)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddSlider(entry, range, scale);
            return this;
        }

        public ModSettingBuilder AddDropdown(ConfigEntry<string> entry, AcceptableValueList<string> list,
            bool useL10N = false)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddDropdown(entry, list, useL10N);
            return this;
        }

        public ModSettingBuilder AddKeyBindDropdown(ConfigEntry<KeyboardShortcut> entry, List<KeyCode> keys,
            bool useL10N = false)
        {
            Internal.ModSettingPatcherDolocTown.Manager.AddKeyBindDropdown(entry, keys, useL10N);
            return this;
        }
    }

}
