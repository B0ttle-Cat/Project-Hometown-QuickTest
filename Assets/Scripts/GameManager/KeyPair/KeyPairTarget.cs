using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public class KeyPairTarget : MonoBehaviour, IKeyPairChain
{
	private KeyPairTarget parent = null;

	public KeyPairTarget This => this;
	public KeyPairTarget Parent
	{
		get
		{
			if (parent == null)
			{
				if (transform.parent == null) return null;
				parent = transform.parent.GetComponentInParent<KeyPairTarget>();
			}
			return parent;
		}
	}

	[Serializable]
	public struct Tag
	{
		[HorizontalGroup, HideLabel, InlineButton("CopyKey", "Copy")]
		public string key;
		[SerializeField, HideInInspector]
		private GameObject target;

		[HorizontalGroup(width: 0.5f), ShowInInspector, HideLabel]
		public GameObject Target
		{
			get => target;
			set { ChangeTarget(value); }
		}

		public void ChangeTarget(GameObject value)
		{
			if (target == value) return;
			target = value;
			if (target == null)
			{
				key = "";
				return;
			}
			if (string.IsNullOrWhiteSpace(key))
				key = target.name;
		}

#if UNITY_EDITOR
		private void CopyKey()
		{
			GUIUtility.systemCopyBuffer = key;
		}
#endif
	}

	public List<Tag> list = new List<Tag>();

	public void Reset()
	{
		list = new List<Tag>();
	}

	public void OnValidate()
	{
		RePairing();
	}

#if UNITY_EDITOR
	[Button("AllCopy")]
	private void AllCopy()
	{
		if (list == null || list.Count == 0)
			return;

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.AppendLine("KeyPair");

		foreach (var tag in list)
		{
			if (string.IsNullOrWhiteSpace(tag.key))
				continue;

			string key = tag.key.Trim();
			// 변수명은 key에서 공백, 특수문자 제거
			string varName = MakeValidVariableName(key);

			sb.AppendLine($"\t.FindPairChain(\"{key}\", out var {varName})");
		}
		sb.AppendLine("\t;");

		GUIUtility.systemCopyBuffer = sb.ToString();
		Debug.Log("복사 완료:\n" + sb);
	}

	private static string MakeValidVariableName(string key)
	{
		// 변수명으로 쓸 수 없거나 특수문자, 공백 제거
		var clean = System.Text.RegularExpressions.Regex.Replace(key, @"[^a-zA-Z0-9_]", "_");
		// 숫자로 시작하면 접두사 붙이기
		if (char.IsDigit(clean[0]))
			clean = "_" + clean;
		return clean;
	}

	public void RePairing()
	{
		if (list == null || list.Count == 0) return;

		for (int i = 0 ; i < list.Count ; i++)
		{
			var tag = list[i];
			if (!string.IsNullOrWhiteSpace(tag.key))
			{
				tag.key = tag.key.Trim();
			}
			else if (tag.Target != null)
			{
				tag.key = tag.Target.name;
			}
			list[i] = tag;
		}
	}
#endif

	public GameObject FindPair(string key)
	{
		key = key.Trim();
		int count = list.Count;
		for (int i = 0 ; i < count ; i++)
		{
			var tag = list[i];
			if (!tag.key.Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
			return tag.Target;
		}
		return null;
	}

	public GameObject FindPairAndCopy(string key, Transform parent)
	{
		GameObject gameObject = FindPair(key);
		if (gameObject == null) return null;
		gameObject = GameObject.Instantiate(gameObject, parent);
		return gameObject;
	}
}

public interface IKeyPairChain
{
	public KeyPairTarget This { get; }
	public IKeyPairChain FindPairChain(string key, out GameObject find)
	{
		find = This.FindPair(key);
		return this;
	}
	public IKeyPairChain FindPairChainAndCopy(string key, Transform parent, out GameObject find)
	{
		find = null;
		FindPairChain(key, out var obj);
		if (obj == null)
		{
			return this;
		}

		find = GameObject.Instantiate(obj, parent);
		return this;
	}
	public bool TryFindPair(string key, out GameObject find)
	{
		FindPairChain(key, out find);
		return find != null;
	}
	public bool TryFindPairAndCopy(string key, Transform parent, out GameObject find)
	{
		FindPairChainAndCopy(key, parent, out find);
		return find != null;
	}

	public IKeyPairChain FindPairChain<T>(string key, out T find) where T : Component
	{
		find = null;
		FindPairChain(key, out GameObject obj);
		if (obj == null)
		{
			return this;
		}
		if (obj.TryGetComponent<T>(out T component))
		{
			find = component;
		}
		return this;
	}
	public IKeyPairChain FindPairChainAndCopy<T>(string key, Transform parent, out T find) where T : Component
	{
		find = null;
		FindPairChain(key, out GameObject obj);
		if (obj == null)
		{
			return this;
		}
		if (obj.TryGetComponent<T>(out T _))
		{
			find = GameObject.Instantiate(obj, parent).GetComponent<T>();
		}
		return this;
	}
	public bool TryFindPair<T>(string key, out T find) where T : Component
	{
		FindPairChain<T>(key, out find);
		return find != null;
	}
	public bool TryFindPairAndCopy<T>(string key, Transform parent, out T find) where T : Component
	{
		FindPairChainAndCopy<T>(key, parent, out find);
		return find != null;
	}

	// T1, T2
	public IKeyPairChain FindPairChain<T1, T2>(string key, out T1 find1, out T2 find2)
		where T1 : Component
		where T2 : Component
	{
		find1 = null;
		find2 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			obj.TryGetComponent<T1>(out find1);
			obj.TryGetComponent<T2>(out find2);
		}
		return this;
	}

	public IKeyPairChain FindPairChainAndCopy<T1, T2>(string key, Transform parent, out T1 find1, out T2 find2)
		where T1 : Component
		where T2 : Component
	{
		find1 = null;
		find2 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			var copy = GameObject.Instantiate(obj, parent);
			copy.TryGetComponent<T1>(out find1);
			copy.TryGetComponent<T2>(out find2);
		}
		return this;
	}

	public bool TryFindPair<T1, T2>(string key, out T1 find1, out T2 find2)
		where T1 : Component
		where T2 : Component
	{
		FindPairChain<T1, T2>(key, out find1, out find2);
		return find1 != null || find2 != null;
	}

	public bool TryFindPairAndCopy<T1, T2>(string key, Transform parent, out T1 find1, out T2 find2)
		where T1 : Component
		where T2 : Component
	{
		FindPairChainAndCopy<T1, T2>(key, parent, out find1, out find2);
		return find1 != null || find2 != null;
	}

	// T1, T2, T3
	public IKeyPairChain FindPairChain<T1, T2, T3>(string key, out T1 find1, out T2 find2, out T3 find3)
		where T1 : Component
		where T2 : Component
		where T3 : Component
	{
		find1 = null;
		find2 = null;
		find3 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			obj.TryGetComponent<T1>(out find1);
			obj.TryGetComponent<T2>(out find2);
			obj.TryGetComponent<T3>(out find3);
		}
		return this;
	}

	public IKeyPairChain FindPairChainAndCopy<T1, T2, T3>(string key, Transform parent, out T1 find1, out T2 find2, out T3 find3)
		where T1 : Component
		where T2 : Component
		where T3 : Component
	{
		find1 = null;
		find2 = null;
		find3 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			var copy = GameObject.Instantiate(obj, parent);
			copy.TryGetComponent<T1>(out find1);
			copy.TryGetComponent<T2>(out find2);
			copy.TryGetComponent<T3>(out find3);
		}
		return this;
	}

	public bool TryFindPair<T1, T2, T3>(string key, out T1 find1, out T2 find2, out T3 find3)
		where T1 : Component
		where T2 : Component
		where T3 : Component
	{
		FindPairChain<T1, T2, T3>(key, out find1, out find2, out find3);
		return find1 != null || find2 != null || find3 != null;
	}

	public bool TryFindPairAndCopy<T1, T2, T3>(string key, Transform parent, out T1 find1, out T2 find2, out T3 find3)
		where T1 : Component
		where T2 : Component
		where T3 : Component
	{
		FindPairChainAndCopy<T1, T2, T3>(key, parent, out find1, out find2, out find3);
		return find1 != null || find2 != null || find3 != null;
	}

	// T1, T2, T3, T4
	public IKeyPairChain FindPairChain<T1, T2, T3, T4>(string key, out T1 find1, out T2 find2, out T3 find3, out T4 find4)
		where T1 : Component
		where T2 : Component
		where T3 : Component
		where T4 : Component
	{
		find1 = null;
		find2 = null;
		find3 = null;
		find4 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			obj.TryGetComponent<T1>(out find1);
			obj.TryGetComponent<T2>(out find2);
			obj.TryGetComponent<T3>(out find3);
			obj.TryGetComponent<T4>(out find4);
		}
		return this;
	}

	public IKeyPairChain FindPairChainAndCopy<T1, T2, T3, T4>(string key, Transform parent, out T1 find1, out T2 find2, out T3 find3, out T4 find4)
		where T1 : Component
		where T2 : Component
		where T3 : Component
		where T4 : Component
	{
		find1 = null;
		find2 = null;
		find3 = null;
		find4 = null;

		FindPairChain(key, out GameObject obj);
		if (obj != null)
		{
			var copy = GameObject.Instantiate(obj, parent);
			copy.TryGetComponent<T1>(out find1);
			copy.TryGetComponent<T2>(out find2);
			copy.TryGetComponent<T3>(out find3);
			copy.TryGetComponent<T4>(out find4);
		}
		return this;
	}

	public bool TryFindPair<T1, T2, T3, T4>(string key, out T1 find1, out T2 find2, out T3 find3, out T4 find4)
		where T1 : Component
		where T2 : Component
		where T3 : Component
		where T4 : Component
	{
		FindPairChain<T1, T2, T3, T4>(key, out find1, out find2, out find3, out find4);
		return find1 != null || find2 != null || find3 != null || find4 != null;
	}

	public bool TryFindPairAndCopy<T1, T2, T3, T4>(string key, Transform parent, out T1 find1, out T2 find2, out T3 find3, out T4 find4)
		where T1 : Component
		where T2 : Component
		where T3 : Component
		where T4 : Component
	{
		FindPairChainAndCopy<T1, T2, T3, T4>(key, parent, out find1, out find2, out find3, out find4);
		return find1 != null || find2 != null || find3 != null || find4 != null;
	}

	public IKeyPairChain FindSubPairChain(string key)
	{
		FindPairChain<KeyPairTarget>(key, out KeyPairTarget find);
		return find;
	}
	public IKeyPairChain BackParentChain(string key)
	{
		return This.Parent;
	}

}

public static class KeyPairTargetEx
{
	public static IKeyPairChain GetKeyPairChain(this GameObject gameObject)
	{
		if (gameObject == null) return null;
		KeyPairTarget keyPairTarget = gameObject.GetComponentInParent<KeyPairTarget>(true);
		return keyPairTarget;
	}
	public static bool TryGetKeyPairChain(this GameObject gameObject, out IKeyPairChain KeyPair)
	{
		return (KeyPair = gameObject.GetKeyPairChain()) != null;
	}

	public static GameObject FindPair(this GameObject gameObject, string key)
	{
		if (gameObject == null) return null;
		KeyPairTarget keyPairTarget = gameObject.GetComponentInParent<KeyPairTarget>(true);
		if (keyPairTarget == null) return null;

		return keyPairTarget.FindPair(key);
	}
	public static bool TryFindPair(this GameObject gameObject, string key, out GameObject find)
	{
		return (find = FindPair(gameObject, key)) != null;
	}

	public static T FindPair<T>(this GameObject gameObject, string key) where T : Component
	{
		if (TryFindPair(gameObject, key, out GameObject find))
		{
			if (find.TryGetComponent<T>(out T component))
			{
				return component;
			}
		}
		return null;
	}
	public static bool TryFindPair<T>(this GameObject gameObject, string key, out T find) where T : Component
	{
		return (find = FindPair<T>(gameObject, key)) != null;
	}

	public static GameObject FindPairAndCopy(this GameObject gameObject, string key, Transform parent)
	{
		if (gameObject == null) return null;
		KeyPairTarget keyPairTarget = gameObject.GetComponentInParent<KeyPairTarget>();
		if (keyPairTarget == null) return null;

		return keyPairTarget.FindPairAndCopy(key, parent);
	}
	public static bool TryFindPairAndCopy(this GameObject gameObject, string key, Transform parent, out GameObject find)
	{
		return (find = FindPairAndCopy(gameObject, key, parent)) != null;
	}
	public static T FindPairAndCopy<T>(this GameObject gameObject, string key, Transform parent) where T : Component
	{
		if (TryFindPairAndCopy(gameObject, key, parent, out GameObject find))
		{
			if (find.TryGetComponent<T>(out T component))
			{
				return component;
			}
		}
		return null;
	}
	public static bool TryFindPairAndCopy<T>(this GameObject gameObject, string key, Transform parent, out T find) where T : Component
	{
		return (find = FindPairAndCopy<T>(gameObject, key, parent)) != null;
	}
}

