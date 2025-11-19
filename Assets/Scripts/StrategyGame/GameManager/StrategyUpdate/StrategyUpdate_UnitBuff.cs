
using System.Collections;
using System.Collections.Generic;

using static StrategyGamePlayData;

public partial class StrategyUpdate
{
	public class StrategyUpdate_UnitBuff : StrategyUpdateSubClass<StrategyUpdate_UnitBuff.UnitSpawner>
	{
		public StrategyUpdate_UnitBuff(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
			updateList = new List<UnitSpawner>();
			var unitList = StrategyManager.Collector.UnitList;
			int length = unitList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var unit = unitList[i];
				if (unit == null) continue;
				updateList.Add(new UnitSpawner(this, unit));
			}
			StrategyManager.Collector.AddChangeListListener<UnitObject>(OnChangeList);
		}

		protected override void Update(in float deltaTime)
		{
			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var update = updateList[i];
				if (update == null) continue;
				update.Update(deltaTime);
			}
		}
		protected override void Dispose()
		{
			StrategyManager.Collector.RemoveChangeListListener<UnitObject>(OnChangeList);
		}
		private void OnChangeList(IList changeList)
		{
			if (changeList is not List<UnitObject> untList) return;

			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = updateList[i];
				if (item == null) continue;
				item.Dispose();
			}

			updateList.Clear();
			for (int i = 0 ; i < untList.Count ; i++)
			{
				var unit = untList[i];
				if (unit == null) continue;
				updateList.Add(new UnitSpawner(this, unit));
			}
		}

		public class UnitSpawner : UpdateLogic
		{
			private UnitObject unit;
			private UnitObjectTrigger unitTrigger;

			private readonly StatsType[] SectorBuffType = new StatsType[]
			{
				 StatsType.유닛_최대내구도,
				 StatsType.유닛_회복력,
			};


			public UnitSpawner(StrategyUpdateSubClass<UnitSpawner> thisSubClass, UnitObject unit) : base(thisSubClass)
			{
				this.unit = unit;
				unitTrigger = unit.GetComponentInChildren<UnitObjectTrigger>();
			}

			protected override void OnDispose()
			{
				unit = null;
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (unit == null || !unit.isActiveAndEnabled) return;

				StatsList tempStatsList = new StatsList();

				tempStatsList.SumStats(unit.MainStatsList.GetValueList());
				tempStatsList.SumStats(unit.SkillBuffGroup.GetValueList());
				tempStatsList.SumStats(GetSectorBuffState());

				string unitKey = $"Unit{unit.UnitID}_Stats";
				TempData.SetValue(unitKey, tempStatsList.GetValueList(true), UpdateLogicSort.End);
				tempStatsList.Dispose();
			}

			private SectorObject GetEnterSector()
			{
				string connectSectorName = unit.SectorData.VisiteSectorName;
				if (!StrategyManager.Collector.TryFindSector(connectSectorName, out var sector)) return null;
				if (sector.CaptureData.captureFactionID != unit.FactionID) return null;
				return sector;
			}

			private List<StatsValue> GetSectorBuffState()
			{
				return new List<StatsValue>();
				//var sector = GetEnterSector();
				//if (sector == null) return new List<StatsValue>();
				//
				//var enterSector = GetEnterSector();
				//
				//string cbBugffKey = $"SectorUnitBuff_{enterSector.SectorName}";
				//if (TempData.TryGetValue(cbBugffKey, out List<StatsValue> cbBuffList))
				//{
				//	return cbBuffList;
				//}
				//
				//StatsList CurrStatsList = sector.CurrStatsList;
				//StatsGroup FacilitiesBuffGroup = sector.TryGetStatsList_Facilities;
				//StatsGroup supportBuffGroup = sector.SupportBuffGroup;
				//
				//var mainList = CurrStatsList.GetValueList(SectorBuffType);
				//var facilitiesList = FacilitiesBuffGroup.GetValueList(SectorBuffType);
				//var supportList = supportBuffGroup.GetValueList(SectorBuffType);
				//
				//int length = SectorBuffType.Length;
				//cbBuffList = new List<StatsValue>(length);
				//for (int i = 0 ; i < length ; i++)
				//{
				//	cbBuffList.Add(mainList[i] + facilitiesList[i] + supportList[i]);
				//}
				//
				//return cbBuffList;
			}
		}
	}
}

