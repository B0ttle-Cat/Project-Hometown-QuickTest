using System.Collections.Generic;
using System.Linq;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static StrategyGamePlayData;

public class UnitPickupCardGroupPanel : CardGroupPanelComponent<UnitPickupCardItemPanel>, IShowHideAsync
{
	[SerializeField,InlineButton("ResetTextFormat")]
	private TMP_Text pickUpInfoText;
	[SerializeField, TextArea(0,5)]
	private string pickInfoTextFormat = @"<line-height=0%><align=left>배치인원<br><line-height=120%><align=right>{0}
<size=80%><line-height=100%><align=left>생산비용
<line-height=0%><align=left>인력<br><line-height=100%><align=right>{1}
<line-height=0%><align=left>재료<br><line-height=100%><align=right>{2}
<line-height=0%><align=left>전력<br><line-height=100%><align=right>{3}";

#if UNITY_EDITOR
	private void ResetTextFormat()
	{
		pickInfoTextFormat = @"<line-height=0%><align=left>배치인원<br><line-height=120%><align=right>{0}
<size=80%><line-height=100%><align=left>생산비용
<line-height=0%><align=left>인력<br><line-height=100%><align=right>{1}
<line-height=0%><align=left>재료<br><line-height=100%><align=right>{2}
<line-height=0%><align=left>전력<br><line-height=100%><align=right>{3}";
	}
#endif

	private Dictionary<UnitKey, int> playerFactionAvailableUnitKeyList;


	void IShowHide.EndedHide()
	{
		if (playerFactionAvailableUnitKeyList != null)
			playerFactionAvailableUnitKeyList.Clear();

		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{

		}
		AllHideAndClear();
	}
	void IShowHide.StartShow()
	{
		playerFactionAvailableUnitKeyList ??= new Dictionary<UnitKey, int>();
		playerFactionAvailableUnitKeyList.Clear();

		Faction playerFaction = FactionAPI.ID2Faction(StrategyManager.PlayerFactionID);
		if (playerFaction != null)
		{
			var fixedList = playerFaction.StatsData.AvailableUnitKeyList;
			int length = fixedList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				playerFactionAvailableUnitKeyList.Add(fixedList[i], 1);
			}
			playerFaction.AddChangeFacility(OnChangeValue);

			var unitProfileList = playerFactionAvailableUnitKeyList.Keys.Select(
				key => StrategyManager.Key2Unit.GetAsset(key).UnitProfileObject);
			InitObjects(unitProfileList);
		}
		AllShow();
		OnChangePickupCount();
	}
	private void OnChangeValue(IStrategyElement element, bool added)
	{
		if (element is not FacilityObject facility) return;

		List<UnitKey> unitKeyList = facility.StaticData.AvailableUnitKeyList;
		List<UnitKey> changeList = new List<UnitKey>();
		int length = unitKeyList.Count;
		for (int i = 0 ; i < length ; i++)
		{
			var uniyKey = unitKeyList[i];
			if (uniyKey == UnitKey.None) continue;

			if (added)
			{
				if (playerFactionAvailableUnitKeyList.TryGetValue(uniyKey, out var value))
				{
					playerFactionAvailableUnitKeyList[uniyKey] = value + 1;
				}
				else
				{
					playerFactionAvailableUnitKeyList.Add(uniyKey, 1);
					changeList.Add(uniyKey);
				}
			}
			else
			{
				if (playerFactionAvailableUnitKeyList.TryGetValue(uniyKey, out var value))
				{
					value--;
					if (value <= 0)
					{
						playerFactionAvailableUnitKeyList.Remove(uniyKey);
						changeList.Add(uniyKey);
					}
					else
					{
						playerFactionAvailableUnitKeyList[uniyKey] = value;
					}
				}
			}
		}

		if (changeList.Count == 0) return;

		var unitProfileList = changeList.Select(key => StrategyManager.Key2Unit.GetAsset(key).UnitProfileObject);
		foreach (var unitProfile in unitProfileList)
		{
			if (unitProfile.IsNullRef()) continue;

			if (added)
			{
				AddObject(unitProfile);
			}
			else
			{
				RemoveObject(unitProfile);
			}
		}
	}

	protected override bool SetPanelObject(UnitPickupCardItemPanel newPanel, ITargetToPanelAPI item)
	{
		if (base.SetPanelObject(newPanel, item))
		{
			newPanel.OnChangeCount += OnChangePickupCount;
			return true;
		}
		return false;
	}

	private void OnChangePickupCount()
	{
		if (pickUpInfoText.IsNotNullRef())
		{
			SpawnTroopsInfo troopsInfo = GetSpawnTroopsInfo();
			if (troopsInfo.totalCount == 0)
			{
				pickUpInfoText.text = "비어있음";
			}
			else
			{
				pickUpInfoText.text = string.Format(pickInfoTextFormat, troopsInfo.totalCount, troopsInfo.costPersonnel, troopsInfo.costMaterial, troopsInfo.costElectric);
			}
		}
	}

	internal SpawnTroopsInfo GetSpawnTroopsInfo()
	{
		int factionID = StrategyManager.PlayerFactionID;

		int length = Count;

		Dictionary<UnitKey, int> spawnTroopsInfo = new Dictionary<UnitKey, int>();

		int costPersonnel = 0;
		int costMaterial  = 0;
		int costElectric  = 0;

		for (int i = 0 ; i < length ; i++)
		{
			var item = Items[i];
			if (item.IsNullRef()) continue;
			if (item is not UnitPickupCardItemPanel pickupCard) continue;

			if (spawnTroopsInfo.TryGetValue(pickupCard.unitKey, out int value))
			{
				spawnTroopsInfo[pickupCard.unitKey] += pickupCard.pickupCount;
				costPersonnel += pickupCard.pickupCount * pickupCard.costPersonnel;
				costMaterial += pickupCard.pickupCount * pickupCard.costMaterial;
				costElectric += pickupCard.pickupCount * pickupCard.costElectric;
			}
			else
			{
				spawnTroopsInfo.Add(pickupCard.unitKey, pickupCard.pickupCount);
				costPersonnel += pickupCard.pickupCount * pickupCard.costPersonnel;
				costMaterial += pickupCard.pickupCount * pickupCard.costMaterial;
				costElectric += pickupCard.pickupCount * pickupCard.costElectric;
			}
		}
		return new SpawnTroopsInfo(factionID, spawnTroopsInfo, costPersonnel, costMaterial, costElectric);
	}

	internal void SetPickupData(in SpawnTroopsInfo spawnTroopsInfo)
	{
		var organizations = spawnTroopsInfo.organizations;
		int length = organizations.Length;

		for (int i = 0 ; i < length ; i++)
		{
			(UnitKey key, int count) = organizations[i];

			int index = Items.FindIndex(i=>i.unitKey == key);
			UnitPickupCardItemPanel item = Items[index];
			if (item.IsNullRef()) continue;
			item.SetData(count);
		}
		OnChangePickupCount();
	}
}
