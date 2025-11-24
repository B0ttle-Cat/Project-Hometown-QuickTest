using System.Collections.Generic;
using System.Linq;

using static StrategyGamePlayData;

public static class FactionUtility
{
	public static List<UnitKey> GetAvailableUnitKeyList(this Faction faction)
	{
		List<UnitKey> result = new List<UnitKey>();
		if (faction == null) return result;

		result.AddRange(faction.AvailableUnitKeyList);

		var factionSectorList = StrategyManager.Collector.FindElementList<SectorObject>(FindSector);
		bool FindSector(SectorObject sector)
		{
			if (sector == null) return false;
			if (sector.CaptureFaction == null) return false;
			return sector.CaptureFaction == faction;
		}
		var facilitiesList = factionSectorList.SelectMany(s => s.FacilitiesData.slotData).Select(s=>s.facilitiesKey).ToHashSet();
		if (facilitiesList.Count > 0)
		{
			// 시설물 중에서 사용 가능한 유닛 종류를 늘려주는 시설이 있는지 검색해야 함.
		}


		return result;
	}

}
