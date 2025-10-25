
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
			StrategyManager.Collector.OnAddChangeListListener<UnitObject>(OnChangeList);
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
			StrategyManager.Collector.OnRemoveChangeListListener<UnitObject>(OnChangeList);
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

			private readonly StatsType[] ControlBaseBuffType = new StatsType[]
			{

			};


			public UnitSpawner(StrategyUpdateSubClass<UnitSpawner> thisSubClass, UnitObject unit) : base(thisSubClass)
			{
				this.unit = unit;
			}

			protected override void OnDispose()
			{
				unit = null;
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (unit == null || !unit.isActiveAndEnabled) return;

				//string cbBugffKey = $"ControlBaseUnitBuff_{captureTag.ConnectControlBaseName}";
				//if(!TempData.TryGetValue(cbBugffKey, out List<StatsValue> cbBuffList))
				//{
				//	cbBuffList = GetControlBaseBuffState(captureTag.ConnectControlBase);
				//}
			}

			private List<StatsValue> GetControlBaseBuffState(ControlBase cb)
			{
				if (cb == null) return new List<StatsValue>();

				StatsList MainStatsList = cb.MainStatsList;
				StatsGroup FacilitiesBuffGroup = cb.FacilitiesBuffGroup;
				StatsGroup supportBuffGroup = cb.SupportBuffGroup;

				var mainList = MainStatsList.GetValueList(ControlBaseBuffType);
				var facilitiesList = FacilitiesBuffGroup.GetValueList(ControlBaseBuffType);
				var supportList = supportBuffGroup.GetValueList(ControlBaseBuffType);

				int length = ControlBaseBuffType.Length;
				var totalList = new List<StatsValue>(length);
				for (int i = 0 ; i < length ; i++)
				{
					totalList.Add(mainList[i] + facilitiesList[i] + supportList[i]);
				}

				return totalList;
			}
		}
	}
}

