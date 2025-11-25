using System;

using Sirenix.OdinInspector;

using UnityEngine;
public enum FactionRelationType
{
	[LabelText("중립")]
	Neutral = 0,
	[LabelText("우호")]
	Friendly = 1,
	[LabelText("적대")]
	Hostile = 2,
}
public class StrategyFactionRelation : MonoBehaviour
{
	[Serializable]
	public record FactionRelation
	{
		[SerializeField,ReadOnly]
		private int A_FactionID;

		[SerializeField,ReadOnly]
		private int B_FactionID;

		[SerializeField]
		private FactionRelationType AB_Relation;
		public FactionRelation(StrategyElementCollector collector, in StrategyStartSetterData.FactionRelation data)
		{
			var A_Faction = collector.FindFaction(data.factionA);
			A_FactionID = A_Faction.FactionID;

			var B_Faction = collector.FindFaction(data.factionB);
			B_FactionID = B_Faction.FactionID;

			AB_Relation = (FactionRelationType)data.relationType;
		}

		public bool FindRelationType(int A, int B, out FactionRelationType result)
		{
			if(A_FactionID == A && B_FactionID == B)
			{
				result = AB_Relation;
				return true;
			}
			else if (A_FactionID == B && B_FactionID == A)
			{
				result = AB_Relation;
				return true;
			}
            else
            {
				result = FactionRelationType.Neutral;
				return false;
            }
        }
	}
	private FactionRelation[] factionRelations;

	internal void Init(StrategyElementCollector collector, StrategyStartSetterData.FactionRelation[] factionRelationDatas)
	{
		int length = factionRelationDatas == null ? 0 : factionRelationDatas.Length;
		factionRelations = new FactionRelation[length];

		for (int i = 0 ; i < length ; i++)
		{
			ref var data = ref  factionRelationDatas[i];
			factionRelations[i] = new FactionRelation(collector, in data);
		}
	}
	public FactionRelationType GetRelationType(Faction factionA, Faction factionB)
	{
		return GetRelationType(factionA == null ? -1 : factionA.FactionID, factionB == null ? -1 : factionB.FactionID);
	}
	public FactionRelationType GetRelationType(int factionA, int factionB)
	{
		if(factionA == factionB) return FactionRelationType.Friendly;
		if (factionA == -1 || factionB == -1) return FactionRelationType.Neutral;


		int length = factionRelations == null ? 0 : factionRelations.Length;
        for (int i = 0 ; i < length ; i++)
        {
			var item = factionRelations[i];
			if(item == null) continue;

			if(item.FindRelationType(factionA, factionB, out var result))
			{
				return result;
			}
		}
        return FactionRelationType.Neutral;
	}
}
