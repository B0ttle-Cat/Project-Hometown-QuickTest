using System.Collections.Generic;

using static StrategyGamePlayData.SectorData;
using static StrategyUpdate.StrategyUpdate_ConstructUpdate;
public partial class StrategyUpdate
{
	public class StrategyUpdate_ConstructUpdate : StrategyUpdateSubClass<ConstructUpdate>
	{
		public StrategyUpdate_ConstructUpdate(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Dispose()
		{
		}

		protected override void Start()
		{
			updateList = new List<ConstructUpdate>();
			var list = StrategyManager.Collector.SectorList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var cb = list[i];
				updateList.Add(new ConstructUpdate(this, cb));
			}
		}
		protected override void Update(in float deltaTime)
		{
			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = updateList[i];
				if (item == null) continue;

				item.Update(in deltaTime);
			}
		}

		public class ConstructUpdate : UpdateLogic
		{
			public SectorObject sector;
			public Facilities data;

			public ConstructUpdate(StrategyUpdateSubClass<ConstructUpdate> thisSubClass, SectorObject sector) : base(thisSubClass)
			{
				this.sector = sector;
				this.data = sector.Facilities;
			}

			protected override void OnDispose()
			{
				sector = null;
				data = null;
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (sector == null || !sector.isActiveAndEnabled) return;
				if ((data ??= sector.Facilities) == null) return;

				ref readonly var _data = ref data.ReadonlyData();
				int length = _data.slotData.Length;

				Queue<(int,string)> finishList = new Queue<(int,string)>();

				for (int i = 0 ; i < length ; i++)
				{
					ref var slot = ref _data.slotData[i];
					ref var constructing = ref slot.constructing;
					int slotIndex  = i;
					if (slotIndex < 0) continue;
					string facilitiesKey = constructing.facilitiesKey;
					if (string.IsNullOrWhiteSpace(facilitiesKey)) continue;
					float constructTime = constructing.constructTime;
					float duration = constructing.duration;

					var currFacilitiesKey = _data.slotData[slotIndex].facilitiesKey;
					if (facilitiesKey.Equals(currFacilitiesKey)) continue;

					duration -= deltaTime;

					// 시설 건설 완료
					if (duration <= 0f)
					{
						finishList.Enqueue((slotIndex, facilitiesKey));
					}
				}
				data.SetData(_data, ignoreChangeEvent: true);
				if (finishList.Count > 0)
				{
					while (finishList.Count > 0)
					{
						var item = finishList.Dequeue();
						sector.Controller.OnFacilitiesConstruct_Finish(item.Item1, item.Item2);
					}
				}
			}
		}
	}
}

