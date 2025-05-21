using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mod.Utilities
{
    // ModLogger is a simple logger class that prefixes log messages with a specified prefix.
    public class ModLogger
    {
        public string Prefix { get; set; }
        public bool IsDebug { get; set; }
        public ModLogger(string prefix = "[Mod]", bool isDebug = true)
        {
            Prefix = prefix;
            IsDebug = isDebug;

        }
        public void Log(string msg, Action<string> debugFn = null)
        {
            if (!IsDebug) return;
            string fullMsg = Prefix + msg;
            (debugFn ?? Debug.Log)(fullMsg);
        }
    }

    // ConfigEntryGroup is a utility class that allows you to group multiple ConfigEntry objects together. Suppurt spnashot and restore.
    public interface IConfigEntryWrapper
    {
        object GetValue();
        void SetValue(object value);
    }

    public class ConfigEntryWrapper<T> : IConfigEntryWrapper
    {
        private readonly ConfigEntry<T> entry;

        public ConfigEntryWrapper(ConfigEntry<T> configEntry)
        {
            entry = configEntry;
        }

        public object GetValue() => entry.Value;

        public void SetValue(object value)
        {
            if (value is T tVal)
                entry.Value = tVal;
        }
    }

    public class ConfigEntryGroup
    {
        private readonly List<IConfigEntryWrapper> entries = new List<IConfigEntryWrapper>();
        private readonly List<object> snapshot = new List<object>();

        public ConfigEntryGroup(params object[] configEntries)
        {
            foreach (var entry in configEntries)
            {
                var type = entry.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
                {
                    var valueType = type.GetGenericArguments()[0];
                    var wrapperType = typeof(ConfigEntryWrapper<>).MakeGenericType(valueType);
                    var wrapper = (IConfigEntryWrapper)Activator.CreateInstance(wrapperType, entry);
                    entries.Add(wrapper);
                }
                else
                {
                    throw new ArgumentException($"Unsupported entry type: {type.FullName}");
                }
            }

            snapshot.Capacity = entries.Count;
        }

        public void TakeSnapshot()
        {
            snapshot.Clear();
            foreach (var entry in entries)
                snapshot.Add(entry.GetValue());
        }

        public void RestoreSnapshot()
        {
            for (int i = 0; i < entries.Count; i++)
                entries[i].SetValue(snapshot[i]);
        }

        public void SetAll(object value)
        {
            foreach (var entry in entries)
                entry.SetValue(value);
        }
    }

}

