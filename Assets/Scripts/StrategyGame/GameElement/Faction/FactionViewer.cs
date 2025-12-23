using Sirenix.OdinInspector;

using UnityEngine;

public class FactionViewer : MonoBehaviour
{
	private bool IsValid => faction != null || faction.FactionID >= 0;
	private string Title => IsValid ? $"{faction.FactionName}({faction.FactionID:00})" : "Null";

	[InlineProperty, HideLabel, ShowIf("IsValid"), Title("Title")]
	public Faction faction;

}
