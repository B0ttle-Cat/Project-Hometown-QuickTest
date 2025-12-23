using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	public interface IFundUIObject
	{
		IFundUIObject ThisUIFinder { get; }
		public List<KeyPairObject> KeyPairs { get; }

		public bool IsPathMatch(string pattern, string targetKey)
		{
			if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(targetKey)) return false;
			if (!pattern.Contains("..")) return pattern.Equals(targetKey);
			if (pattern.Equals("..")) return true;
			string regexPattern = Regex.Escape(pattern).Replace(@"\.\.", ".*");
			if (!pattern.StartsWith("..")) regexPattern = "^" + regexPattern;
			if (!pattern.EndsWith("..")) regexPattern = regexPattern + "$";
			return Regex.IsMatch(targetKey, regexPattern, RegexOptions.IgnoreCase);
		}

		private GameObject FindInternal(string key, Func<GameObject, bool> condition)
		{
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return null;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (condition == null || condition.Invoke(item.Target)) return item.Target;
			}
			return null;
		}

		private T FindComponentInternal<T>(string key, Func<T, bool> condition) where T : class
		{
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return null;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (item.Target.TryGetComponent<T>(out var component))
				{
					if (condition == null || condition.Invoke(component)) return component;
				}
			}
			return null;
		}

		public bool TryFind(string key, Func<GameObject, bool> condition, out GameObject find)
		{
			find = FindInternal(key, condition);
			return find != null;
		}

		public bool TryFind<T>(out T find) where T : class => TryFind<T>("..", null, out find);
		public bool TryFind<T>(Func<T, bool> condition, out T find) where T : class => TryFind<T>("..", condition, out find);
		public bool TryFind<T>(string key, Func<T, bool> condition, out T find) where T : class
		{
			find = FindComponentInternal<T>(key, condition);
			return find != null;
		}

		public int TryFinds(string key, Func<GameObject, bool> condition, out List<GameObject> find)
		{
			find = new List<GameObject>();
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return 0;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (condition == null || condition.Invoke(item.Target)) find.Add(item.Target);
			}
			return find.Count;
		}
		public int TryFinds<T>(out List<T> find) where T : class => TryFinds<T>("..", null, out find);
		public int TryFinds<T>(Func<T, bool> condition, out List<T> find) where T : class => TryFinds<T>("..", condition, out find);
		public int TryFinds<T>(string key, Func<T, bool> condition, out List<T> find) where T : class
		{
			find = new List<T>();
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return 0;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (item.Target.TryGetComponent<T>(out var component))
				{
					if (condition == null || condition.Invoke(component)) find.Add(component);
				}
			}
			return find.Count;
		}

		public bool TryInstantiate(string key, Transform parent, Func<GameObject, bool> condition, out GameObject find)
		{
			GameObject target = FindInternal(key, condition);
			find = target != null ? InstantiateUIObject(target, parent) : null;
			if (find != null) find.SetActive(true);
			return find != null;
		}

		public bool TryInstantiate<T>(string key, Transform parent, Func<T, bool> condition, out T find) where T : class
		{
			T targetComponent = FindComponentInternal<T>(key, condition);
			find = null;
			if (targetComponent != null && targetComponent is Component comp)
			{
				GameObject instantiated = InstantiateUIObject(comp.gameObject, parent);
				instantiated.SetActive(true);
				return instantiated.TryGetComponent<T>(out find);
			}
			return false;
		}

		public GameObject InstantiateUIObject(GameObject target, Transform parent)
		{
			return GameObject.Instantiate(target, parent.IsNullRef() ? target.transform.parent : parent);
		}

		[Serializable, InlineProperty]
		public class KeyPairObject
		{
			[ToggleLeft, HideLabel, SerializeField, HorizontalGroup(width: 16), OnValueChanged("TargetObjectChange")]
			private bool customKey;
			[HideLabel, SerializeField, HorizontalGroup, EnableIf("@customKey")]
			public string Key;
			[HideLabel, HorizontalGroup, OnValueChanged("TargetObjectChange")]
			[PropertyOrder(1)]
			public GameObject Target;
#if UNITY_EDITOR
			[HorizontalGroup(width: 40), Button, PropertyOrder(0)]
			public void Copy()
			{
				GUIUtility.systemCopyBuffer = Key;
			}
			public void TargetObjectChange(object root)
			{
				if (customKey && !string.IsNullOrWhiteSpace(Key)) return;
				if (Target == null) { Key = string.Empty; return; }
				if (root is not Component rootComp) { Key = string.Empty; return; }
				Transform current = Target.transform;
				Transform rootTransform = rootComp.transform;
				if (current == rootTransform) { Key = string.Empty; return; }
				string path = current.name;
				while (current.parent != null && current.parent != rootTransform)
				{
					current = current.parent;
					path = $"{current.name}/{path}";
				}
				Key = path;
			}
			[ShowInInspector, HideLabel, HorizontalGroup(width: 16), EnableGUI]
			[PropertyOrder(100)]
			public bool testFindit { get; set; }
#endif
		}
	}
}