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
		[HorizontalGroup, HideLabel,InlineButton("CopyKey","Copy")]
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
#if UNITY_EDITOR

#endif
	public void Reset()
	{
		list = new List<Tag>();
	}
	public void OnValidate()
	{
		RePairing();
	}
#if UNITY_EDITOR
	public void RePairing()
	{
		if (list == null || list.Count == 0) return;

		Transform root = transform;

		int length = list.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var tag = list[i];
			var target = tag.Target;

			string key = tag.key;
			if (!string.IsNullOrWhiteSpace(key))
			{
				tag.key = key.Trim();
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

	public IKeyPairChain FindPairChain(string key, out GameObject find)
	{
		find = FindPair(key);
		return this;
	}
}
public interface IKeyPairChain
{
	public KeyPairTarget This { get; }
	public IKeyPairChain FindPairChain(string key, out GameObject find);
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
	public IKeyPairChain FindPairChainAndCopy(string key, Transform parent, out GameObject find)
	{
		find = null;
		FindPairChain(key, out var obj);
		if(obj == null)
		{
			return this;
		}

		find = GameObject.Instantiate(obj, parent);
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
	public bool TryFindPair(string key, out GameObject find)
	{
		FindPairChain(key, out find);
		return find != null;
	}
	public bool TryFindPair<T>(string key, out T find) where T : Component
	{
		FindPairChain<T>(key, out find);
		return find != null;
	}
	public bool TryFindPairAndCopy(string key, Transform parent, out GameObject find)
	{
		FindPairChainAndCopy(key, parent, out find);
		return find != null;
	}
	public bool TryFindPairAndCopy<T>(string key, Transform parent, out T find) where T : Component
	{
		FindPairChainAndCopy<T>(key, parent, out find);
		return find != null;
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

