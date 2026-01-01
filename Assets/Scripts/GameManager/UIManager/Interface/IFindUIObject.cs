using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	public interface IFindUIObject
	{
		/*
	public IFindUIObject ThisUIFinder => this;
	[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
	List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }
		  */

		IFindUIObject ThisUIFinder { get; }
		public List<KeyPairObject> KeyPairs { get; set; }

#if UNITY_EDITOR
		[PropertyOrder(-91), ButtonGroup]
		private void CopyKeyPairs()
		{
			CopyKeyPairObjectData copy = new CopyKeyPairObjectData()
			{
				keyPairs = KeyPairs
			};
			GUIUtility.systemCopyBuffer = JsonUtility.ToJson(copy);
		}
		[PropertyOrder(-91), ButtonGroup]
		private void PasteKeyPairs()
		{
			try
			{
				CopyKeyPairObjectData data = JsonUtility.FromJson<CopyKeyPairObjectData>(GUIUtility.systemCopyBuffer);
				KeyPairs = data.keyPairs.ToList();
			}
			catch { }
		}


		[Serializable]
		private class CopyKeyPairObjectData
		{
			public List<IFindUIObject.KeyPairObject> keyPairs;
		}

		public bool TestIsPathMatch(string pattern, string targetKey) => IsPathMatch(pattern, targetKey);
#endif


		private bool IsPathMatch(string pattern, string targetKey)
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
		public bool TryFind<T>(string key, out T find) where T : class => TryFind<T>(key, null, out find);
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
		public int TryFinds<T>(string key, out List<T> find) where T : class => TryFinds<T>(key, null, out find);
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


		#region TryFind Multi Components
		// --- TryFind 2 Components ---
		public bool TryFind<T, TT>(out T find1, out TT find2) where T : class where TT : class => TryFind<T, TT>("..", null, out find1, out find2);
		public bool TryFind<T, TT>(string key, out T find1, out TT find2) where T : class where TT : class => TryFind<T, TT>(key, null, out find1, out find2);
		public bool TryFind<T, TT>(Func<GameObject, bool> condition, out T find1, out TT find2) where T : class where TT : class => TryFind<T, TT>("..", condition, out find1, out find2);
		public bool TryFind<T, TT>(string key, Func<GameObject, bool> condition, out T find1, out TT find2) where T : class where TT : class
		{
			GameObject target = FindInternal(key, condition);
			find1 = target != null ? target.GetComponent<T>() : null;
			find2 = target != null ? target.GetComponent<TT>() : null;
			return find1 != null && find2 != null;
		}

		// --- TryFind 3 Components ---
		public bool TryFind<T, TT, TTT>(out T find1, out TT find2, out TTT find3) where T : class where TT : class where TTT : class => TryFind<T, TT, TTT>("..", null, out find1, out find2, out find3);
		public bool TryFind<T, TT, TTT>(string key, out T find1, out TT find2, out TTT find3) where T : class where TT : class where TTT : class => TryFind<T, TT, TTT>(key, null, out find1, out find2, out find3);
		public bool TryFind<T, TT, TTT>(Func<GameObject, bool> condition, out T find1, out TT find2, out TTT find3) where T : class where TT : class where TTT : class => TryFind<T, TT, TTT>("..", condition, out find1, out find2, out find3);
		public bool TryFind<T, TT, TTT>(string key, Func<GameObject, bool> condition, out T find1, out TT find2, out TTT find3) where T : class where TT : class where TTT : class
		{
			GameObject target = FindInternal(key, condition);
			find1 = target != null ? target.GetComponent<T>() : null;
			find2 = target != null ? target.GetComponent<TT>() : null;
			find3 = target != null ? target.GetComponent<TTT>() : null;
			return find1 != null && find2 != null && find3 != null;
		}

		// --- TryFind 4 Components ---
		public bool TryFind<T, TT, TTT, TTTT>(out T find1, out TT find2, out TTT find3, out TTTT find4) where T : class where TT : class where TTT : class where TTTT : class => TryFind<T, TT, TTT, TTTT>("..", null, out find1, out find2, out find3, out find4);
		public bool TryFind<T, TT, TTT, TTTT>(string key, out T find1, out TT find2, out TTT find3, out TTTT find4) where T : class where TT : class where TTT : class where TTTT : class => TryFind<T, TT, TTT, TTTT>(key, null, out find1, out find2, out find3, out find4);
		public bool TryFind<T, TT, TTT, TTTT>(Func<GameObject, bool> condition, out T find1, out TT find2, out TTT find3, out TTTT find4) where T : class where TT : class where TTT : class where TTTT : class => TryFind<T, TT, TTT, TTTT>("..", condition, out find1, out find2, out find3, out find4);
		public bool TryFind<T, TT, TTT, TTTT>(string key, Func<GameObject, bool> condition, out T find1, out TT find2, out TTT find3, out TTTT find4) where T : class where TT : class where TTT : class where TTTT : class
		{
			GameObject target = FindInternal(key, condition);
			find1 = target != null ? target.GetComponent<T>() : null;
			find2 = target != null ? target.GetComponent<TT>() : null;
			find3 = target != null ? target.GetComponent<TTT>() : null;
			find4 = target != null ? target.GetComponent<TTTT>() : null;
			return find1 != null && find2 != null && find3 != null && find4 != null;
		}
		#endregion

		#region TryFinds Multi Components
		// --- TryFinds 2 Components ---
		public int TryFinds<T, TT>(out List<T> find1, out List<TT> find2) where T : class where TT : class => TryFinds<T, TT>("..", null, out find1, out find2);
		public int TryFinds<T, TT>(string key, out List<T> find1, out List<TT> find2) where T : class where TT : class => TryFinds<T, TT>(key, null, out find1, out find2);
		public int TryFinds<T, TT>(Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2) where T : class where TT : class => TryFinds<T, TT>("..", condition, out find1, out find2);
		public int TryFinds<T, TT>(string key, Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2) where T : class where TT : class
		{
			find1 = new List<T>(); find2 = new List<TT>();
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return 0;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (condition != null && !condition.Invoke(item.Target)) continue;
				if (item.Target.TryGetComponent<T>(out var c1) && item.Target.TryGetComponent<TT>(out var c2))
				{
					find1.Add(c1); find2.Add(c2);
				}
			}
			return find1.Count;
		}

		// --- TryFinds 3 Components ---
		public int TryFinds<T, TT, TTT>(out List<T> find1, out List<TT> find2, out List<TTT> find3) where T : class where TT : class where TTT : class => TryFinds<T, TT, TTT>("..", null, out find1, out find2, out find3);
		public int TryFinds<T, TT, TTT>(string key, out List<T> find1, out List<TT> find2, out List<TTT> find3) where T : class where TT : class where TTT : class => TryFinds<T, TT, TTT>(key, null, out find1, out find2, out find3);
		public int TryFinds<T, TT, TTT>(Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2, out List<TTT> find3) where T : class where TT : class where TTT : class => TryFinds<T, TT, TTT>("..", condition, out find1, out find2, out find3);
		public int TryFinds<T, TT, TTT>(string key, Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2, out List<TTT> find3) where T : class where TT : class where TTT : class
		{
			find1 = new List<T>(); find2 = new List<TT>(); find3 = new List<TTT>();
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return 0;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (condition != null && !condition.Invoke(item.Target)) continue;
				if (item.Target.TryGetComponent<T>(out var c1) && item.Target.TryGetComponent<TT>(out var c2) && item.Target.TryGetComponent<TTT>(out var c3))
				{
					find1.Add(c1); find2.Add(c2); find3.Add(c3);
				}
			}
			return find1.Count;
		}

		// --- TryFinds 4 Components ---
		public int TryFinds<T, TT, TTT, TTTT>(out List<T> find1, out List<TT> find2, out List<TTT> find3, out List<TTTT> find4) where T : class where TT : class where TTT : class where TTTT : class => TryFinds<T, TT, TTT, TTTT>("..", null, out find1, out find2, out find3, out find4);
		public int TryFinds<T, TT, TTT, TTTT>(string key, out List<T> find1, out List<TT> find2, out List<TTT> find3, out List<TTTT> find4) where T : class where TT : class where TTT : class where TTTT : class => TryFinds<T, TT, TTT, TTTT>(key, null, out find1, out find2, out find3, out find4);
		public int TryFinds<T, TT, TTT, TTTT>(Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2, out List<TTT> find3, out List<TTTT> find4) where T : class where TT : class where TTT : class where TTTT : class => TryFinds<T, TT, TTT, TTTT>("..", condition, out find1, out find2, out find3, out find4);
		public int TryFinds<T, TT, TTT, TTTT>(string key, Func<GameObject, bool> condition, out List<T> find1, out List<TT> find2, out List<TTT> find3, out List<TTTT> find4) where T : class where TT : class where TTT : class where TTTT : class
		{
			find1 = new List<T>(); find2 = new List<TT>(); find3 = new List<TTT>(); find4 = new List<TTTT>();
			if (string.IsNullOrWhiteSpace(key) || KeyPairs == null) return 0;
			int count = KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = KeyPairs[i];
				if (item == null || item.Target.IsNullRef() || !IsPathMatch(key, item.Key)) continue;
				if (condition != null && !condition.Invoke(item.Target)) continue;
				if (item.Target.TryGetComponent<T>(out var c1) && item.Target.TryGetComponent<TT>(out var c2) && item.Target.TryGetComponent<TTT>(out var c3) && item.Target.TryGetComponent<TTTT>(out var c4))
				{
					find1.Add(c1); find2.Add(c2); find3.Add(c3); find4.Add(c4);
				}
			}
			return find1.Count;
		}
		#endregion
	}
}