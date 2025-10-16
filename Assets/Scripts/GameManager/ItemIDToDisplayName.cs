using System;

using Sirenix.OdinInspector;

using UnityEngine;

[CreateAssetMenu(fileName = "ItemIDToDisplayName", menuName = "Scriptable Objects/ItemIDToDisplayName")]
public class ItemIDToDisplayName : ScriptableObject
{
	public ItemIDToDisplayName parent;
	public ItemIDToDisplayName[] chains;

	[Serializable]	
	private struct Pair_ID_Name
	{
		[HorizontalGroup, HideLabel, SuffixLabel("ItemID", overlay: true)]
		public string id;
		[HorizontalGroup, HideLabel, SuffixLabel("DisplayName", overlay: true)]
		public string name;
	}
	[SerializeField]
	[ListDrawerSettings(ShowPaging = false, ShowFoldout = false)]
	private Pair_ID_Name[] pair_ID_Names;

	public string DisplayName(string itemID)
	{
		if (string.IsNullOrWhiteSpace(itemID)) return itemID;
		return _GetDisplayName(in itemID);
	}
	private string _GetDisplayName(in string itemID)
	{
		int length = pair_ID_Names == null ? 0 : pair_ID_Names.Length;
		for (int i = 0 ; i < length ; i++)
		{
			if (pair_ID_Names[i].id.Equals(itemID))
			{
				return pair_ID_Names[i].name;
			}
		}
		if (parent == null) return itemID;
		return parent._GetDisplayName(in itemID);
	}


	public static ItemIDToDisplayName Load(Language.Type type, string name)
	{
		return Resources.Load<ItemIDToDisplayName>($"{nameof(ItemIDToDisplayName)}/{type.ToString()}/{name}");
	}
}
