using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModUtilities
{
    // ModLogger is a simple logger class that prefixes log messages with a specified prefix.
    public class ModLogger
    {
        private string Prefix { get; set; }
        private bool IsDebug { get; set; }
        public ModLogger(string prefix = "[Mod]", bool isDebug = true)
        {
            Prefix = prefix;
            IsDebug = isDebug;

        }
        public void Log(string msg, Action<string> debugFn = null)
        {
            if (!IsDebug) return;
            var fullMsg = Prefix + msg;
            (debugFn ?? Debug.Log)(fullMsg);
        }
    }

    // ConfigEntryGroup is a utility class that allows you to group multiple ConfigEntry objects together. Support snapshot and restore.
    public interface IConfigEntryWrapper
    {
        object GetValue();
        void SetValue(object value);
    }

    public class ConfigEntryWrapper<T> : IConfigEntryWrapper
    {
        private readonly ConfigEntry<T> _entry;

        public ConfigEntryWrapper(ConfigEntry<T> configEntry)
        {
            _entry = configEntry;
        }

        public object GetValue() => _entry.Value;

        public void SetValue(object value)
        {
            if (value is T tVal)
                _entry.Value = tVal;
        }
    }

    public class ConfigEntryGroup
    {
        private readonly List<IConfigEntryWrapper> _entries = new List<IConfigEntryWrapper>();
        private readonly List<object> _snapshot = new List<object>();

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
                    _entries.Add(wrapper);
                }
                else
                {
                    throw new ArgumentException($"Unsupported entry type: {type.FullName}");
                }
            }

            _snapshot.Capacity = _entries.Count;
        }

        public void TakeSnapshot()
        {
            _snapshot.Clear();
            foreach (var entry in _entries)
                _snapshot.Add(entry.GetValue());
        }

        public void RestoreSnapshot()
        {
            for (int i = 0; i < _entries.Count; i++)
                _entries[i].SetValue(_snapshot[i]);
        }

        public void SetAll(object value)
        {
            foreach (var entry in _entries)
                entry.SetValue(value);
        }
    }

}

