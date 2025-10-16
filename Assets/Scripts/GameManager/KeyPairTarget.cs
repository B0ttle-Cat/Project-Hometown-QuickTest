using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

public class KeyPairTarget : MonoBehaviour
{
	[Serializable]
	public struct Tag
	{
#if UNITY_EDITOR
		[LabelWidth(50), DisplayAsString, EnableGUI, InlineButton("Refresh")]
		public string path;
		private void Refresh()
		{
			if (target == null)
			{
				path = "";
				return;
			}
			ChangePath(target.transform);
		}
		private void ChangePath(Transform tr)
		{
			List<string> paths = new List<string>();
			paths.Add(tr.name);
			ParentUp();

			path = string.Join("/", paths);

			void ParentUp()
			{
				tr = tr.parent;
				if (tr == null)
				{
					paths.Clear();
					return;
				}
				else if (tr.TryGetComponent<KeyPairTarget>(out _))
				{
					return;
				}
				else
				{
					paths.Insert(0, tr.name);
					ParentUp();
				}
			}
		}
#endif
		[HorizontalGroup, LabelWidth(50)]
		public string key;
		private GameObject target;

		[HorizontalGroup(width: 0.5f), ShowInInspector, HideLabel, InlineButton("Clear")]
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
#if UNITY_EDITOR
			ChangePath(target.transform);
#endif
		}

		public void Clear()
		{
			path = "";
			key = "";
			target = null;
		}
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
			if (target != null && !target.transform.IsChildOf(root))
			{
				target = null;
			}

			string path = tag.path;
			if (!string.IsNullOrWhiteSpace(path))
			{
				var find = root.Find(path);
				tag.Target = find == null ? null : find.gameObject;
			}
			else
			{
				tag.Target = target;
			}

			string key = tag.key;
			if (!string.IsNullOrWhiteSpace(key))
			{
				tag.key = key.Trim();
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
}

public static class KeyPairTargetEx
{
	public static GameObject FindPair(this GameObject gameObject, string key)
	{
		if (gameObject == null) return null;
		KeyPairTarget keyPairTarget = gameObject.GetComponentInParent<KeyPairTarget>();
		if (keyPairTarget == null) return null;

		return keyPairTarget.FindPair(key);
	}
	
	public static bool TryFindPair(this GameObject gameObject, string key, out GameObject find)
	{
		return (find = FindPair(gameObject, key)) != null;
	}

	public static T FindPair<T>(this GameObject gameObject, string key) where T : Component
	{
		if(TryFindPair(gameObject, key, out GameObject find))
		{
			if(find.TryGetComponent<T>(out T component))
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
}

