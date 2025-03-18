// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections.Generic;
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// The debug system is a global singleton that handles debug settings, actions and readouts that can be accessed via the debug menu
    /// Settings are grouped by user-defined categories that can be dynamically constructed at runtime
    /// 
    /// Supported value types are:
    /// - float
    /// - int
    /// - bool
    /// - option (enum values)
    /// - action (callback triggered by a button press)
    /// - watch (function that returns a string)
    /// 
    /// New settings are added via using type, name, category and lamba callbacks for when values are updated
    /// Settings can optionally be saved in PlayerPrefs whenever they are updated
    /// 
    /// The system is completely generic and re-useable with automatic bootstrapping by adding itself to the scene via the RuntimeInitializeOnLoadMethod attribute
    /// 
    /// </summary>
    public class DebugSystem : Singleton<DebugSystem>
    {
        private const string ASSETNAME = "DebugSystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoad()
        {
            var debugSystem = Instantiate(Resources.Load(ASSETNAME));
            DontDestroyOnLoad(debugSystem);
        }

        [Serializable]
        public abstract class DebugValueBase
        {
            [field: SerializeField] public string Category { get; protected set; }
            [field: SerializeField] public string Name { get; protected set; }

            public abstract string KeyPath { get; }
            public abstract string PrefabName { get; }

            public abstract void ReadFromStorage();
            public abstract void WriteToStorage();

            public abstract void ConnectToPrefab(GameObject prefab);
        }

        public abstract class DebugValue<T> : DebugValueBase
        {
            public T Value
            {
                get => m_value;
                set
                {
                    m_value = value;
                    OnSet?.Invoke(m_value);
                }
            }
            [field: SerializeField] public T InitialValue { get; protected set; }
            [field: SerializeField] public bool Persistent { get; protected set; } = true;
            [field: SerializeField] public UnityEvent<T> OnSet { get; protected set; } = new UnityEvent<T>();

            private T m_value;

            public override string KeyPath => $"{Category}/{Name}";
        }

        [Serializable]
        public class FloatValue : DebugValue<float>
        {
            [field: SerializeField] public float Min { get; private set; }
            [field: SerializeField] public float Max { get; private set; }

            public override string PrefabName => "DebugFloatValue";

            public FloatValue() { }

            public FloatValue(string category, string name, float min, float max, float initial, bool persistent, UnityAction<float> onSet)
            {
                Category = category;
                Name = name;
                Min = min;
                Max = max;
                InitialValue = initial;
                Persistent = persistent;
                Value = initial;
                Persistent = persistent;
                OnSet.AddListener(onSet);
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugFloatValueHandler>().ConnectedValue = this;
            }

            public override void ReadFromStorage()
            {
                if (Persistent) Value = PlayerPrefs.GetFloat(KeyPath, InitialValue);
            }

            public override void WriteToStorage()
            {
                if (Persistent) PlayerPrefs.SetFloat(KeyPath, Value);
            }
        }

        [Serializable]
        public class IntValue : DebugValue<int>
        {
            [field: SerializeField] public int Min { get; private set; }
            [field: SerializeField] public int Max { get; private set; }

            public override string PrefabName => "DebugIntValue";

            public IntValue() { }

            public IntValue(string category, string name, int min, int max, int initial, bool persistent, UnityAction<int> onSet)
            {
                Category = category;
                Name = name;
                Min = min;
                Max = max;
                InitialValue = initial;
                Value = initial;
                Persistent = persistent;
                OnSet.AddListener(onSet);
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugIntValueHandler>().ConnectedValue = this;
            }

            public override void ReadFromStorage()
            {
                if (Persistent) Value = PlayerPrefs.GetInt(KeyPath, InitialValue);
            }

            public override void WriteToStorage()
            {
                if (Persistent) PlayerPrefs.SetInt(KeyPath, Value);
            }
        }

        [Serializable]
        public class BoolValue : DebugValue<bool>
        {
            public override string PrefabName => "DebugBoolValue";

            public BoolValue() { }

            public BoolValue(string category, string name, bool initial, bool persistent, UnityAction<bool> onSet)
            {
                Category = category;
                Name = name;
                InitialValue = initial;
                Value = initial;
                OnSet.AddListener(onSet);
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugBoolValueHandler>().ConnectedValue = this;
            }

            public override void ReadFromStorage()
            {
                if (Persistent) Value = PlayerPrefs.GetInt(KeyPath, InitialValue ? 1 : 0) == 1;
            }

            public override void WriteToStorage()
            {
                if (Persistent) PlayerPrefs.SetInt(KeyPath, Value ? 1 : 0);
            }
        }

        [Serializable]
        public class OptionsValue : IntValue
        {
            [field: SerializeField] public List<string> Options { get; private set; }

            public OptionsValue() { }

            public OptionsValue(string category, string name, List<string> options, int initial, bool persistent, UnityAction<int> onSet)
            {
                Category = category;
                Name = name;
                Options = options;
                InitialValue = initial;
                Value = initial;
                OnSet.AddListener(onSet);
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugOptionValueHandler>().ConnectedValue = this;
            }
        }

        [Serializable]
        public class DebugAction : DebugValueBase
        {
            [field: SerializeField] public UnityEvent Action { get; protected set; } = new UnityEvent();

            public override string KeyPath => "";

            public override string PrefabName => "DebugAction";

            public DebugAction() { }

            public DebugAction(string category, string name, UnityAction action)
            {
                Category = category;
                Name = name;
                Action.AddListener(action);
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugActionHandler>().ConnectedValue = this;
            }

            public override void ReadFromStorage() { }
            public override void WriteToStorage() { }
        }

        [Serializable]
        public class DebugWatch : DebugValueBase
        {
            public delegate string ReadValueAction();

            public ReadValueAction ReadValue { get; protected set; }

            public override string KeyPath => "";

            public override string PrefabName => "DebugWatch";

            public DebugWatch() { }

            public DebugWatch(string category, string name, ReadValueAction readValue)
            {
                Category = category;
                Name = name;
                ReadValue = readValue;
            }

            public override void ConnectToPrefab(GameObject prefab)
            {
                prefab.GetComponent<DebugWatchHandler>().ConnectedValue = this;
            }

            public override void ReadFromStorage() { }
            public override void WriteToStorage() { }
        }

        public static readonly Type[] PropertyTypes =
        {
            typeof(FloatValue),
            typeof(IntValue),
            typeof(BoolValue),
            typeof(OptionsValue),
            typeof(DebugAction),
            typeof(DebugWatch),
        };

        [Serializable]
        public class Category
        {
            public string Name;
            public GameObject Prefab;
            [NonSerialized] public List<DebugValueBase> Properties = new();
            [NonSerialized] public GameObject Content;
        }

        [SerializeReference] public List<DebugValueBase> Properties = new();
        public List<Category> DefaultCategories = new();


        public List<Category> Categories = new();

        [SerializeField] public event Action OnNeedsRebuild;

        private Dictionary<string, DebugValueBase> m_propertyMap = new();
        private Dictionary<string, Category> m_categoryMap = new();

        protected override void InternalAwake()
        {
            base.InternalAwake();
            foreach (var category in DefaultCategories)
            {
                Categories.Add(category);
                m_categoryMap.Add(category.Name, category);
            }
        }

        private void OnDisable()
        {
            WritePropertiesToStorage();
        }

        private void OnApplicationQuit()
        {
            WritePropertiesToStorage();
        }

        private void WritePropertiesToStorage()
        {
            foreach (var property in Properties)
            {
                property.WriteToStorage();
            }
        }

        public void ResetOptions()
        {
            WritePropertiesToStorage();
            Categories.Clear();
            Properties.Clear();
            m_categoryMap.Clear();
            m_propertyMap.Clear();

            foreach (var category in DefaultCategories)
            {
                Categories.Add(category);
                m_categoryMap.Add(category.Name, category);
            }
        }

        private Category GetOrCreateCategory(string name)
        {
            if (m_categoryMap.ContainsKey(name))
            {
                return m_categoryMap[name];
            }
            else
            {
                var category = new Category { Name = name };
                Categories.Add(category);
                m_categoryMap.Add(name, category);
                return category;
            }
        }

        private T AddProperty<T>(T property) where T : DebugValueBase
        {
            var category = GetOrCreateCategory(property.Category);
            category.Properties.Add(property);
            Properties.Add(property);
            m_propertyMap.Add(property.Name, property);
            property.ReadFromStorage();
            OnNeedsRebuild?.Invoke();
            return property;
        }

        public T Get<T>(string name) where T : DebugValueBase
        {
            if (m_propertyMap.TryGetValue(name, out var value))
            {
                if (value is not FloatValue)
                {
                    Debug.LogWarning($"Property {name} is not a {typeof(T)}");
                    return null;
                }
                return value as T;
            }
            return null;
        }

        public FloatValue GetFloat(string name)
        {
            return Get<FloatValue>(name);
        }

        public IntValue GetInt(string name)
        {
            return Get<IntValue>(name);
        }

        public BoolValue GetBool(string name)
        {
            return Get<BoolValue>(name);
        }

        public OptionsValue GetOption(string name)
        {
            return Get<OptionsValue>(name);
        }

        public DebugAction GetAction(string name)
        {
            return Get<DebugAction>(name);
        }

        public FloatValue AddFloat(string category, string name, float min, float max, float initial, bool persistent, UnityAction<float> onSet)
        {
            return AddProperty(new FloatValue(category, name, min, max, initial, persistent, onSet));
        }

        public IntValue AddInt(string category, string name, int min, int max, int initial, bool persistent, UnityAction<int> onSet)
        {
            return AddProperty(new IntValue(category, name, min, max, initial, persistent, onSet));
        }

        public BoolValue AddBool(string category, string name, bool initial, bool persistent, UnityAction<bool> onSet)
        {
            return AddProperty(new BoolValue(category, name, initial, persistent, onSet));
        }

        public OptionsValue AddOptions(string category, string name, List<string> options, int initial, bool persistent, UnityAction<int> onSet)
        {
            return AddProperty(new OptionsValue(category, name, options, initial, persistent, onSet));
        }

        public DebugAction AddAction(string category, string name, UnityAction action)
        {
            return AddProperty(new DebugAction(category, name, action));
        }

        public DebugWatch AddWatch(string category, string name, DebugWatch.ReadValueAction readValue)
        {
            return AddProperty(new DebugWatch(category, name, readValue));
        }
    }
}
