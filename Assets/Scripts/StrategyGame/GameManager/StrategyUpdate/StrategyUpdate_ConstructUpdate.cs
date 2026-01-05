using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_ConstructUpdate;

namespace StrategyManagerModule
{
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
				var list = StrategyManager.Collector.GetList<SectorObject>();
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var cb = list[i];
					this.Add(new ConstructUpdate(this, cb));
				}
			}
			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = this[i];
					if (item == null) continue;

					item.Update(in deltaTime);
				}
			}

			public class ConstructUpdate : UpdateLogic
			{
				public SectorObject sector;

				public ConstructUpdate(StrategyUpdateSubClass<ConstructUpdate> thisSubClass, SectorObject sector) : base(thisSubClass)
				{
					this.sector = sector;
				}

				protected override void OnDispose()
				{
					sector = null;
				}

				protected override void OnUpdate(in float deltaTime)
				{
				//	if (iamge == null || !iamge.isActiveAndEnabled) return;
				//	if ((data ??= iamge.Facilities) == null) return;
				//
				//	ref readonly var _data = ref data.ReadonlyData();
				//	int length = _data.slotData.Length;
				//
				//	Queue<(int,string)> finishList = new Queue<(int,string)>();
				//
				//	for (int i = 0 ; i < length ; i++)
				//	{
				//		ref var slot = ref _data.slotData[i];
				//		ref var constructing = ref slot.constructing;
				//		int slotIndex  = i;
				//		if (slotIndex < 0) continue;
				//		string facilitiesKey = constructing.facilitiesKey;
				//		if (string.IsNullOrWhiteSpace(facilitiesKey)) continue;
				//		float constructTime = constructing.constructTime;
				//		float duration = constructing.duration;
				//
				//		var currFacilitiesKey = _data.slotData[slotIndex].facilitiesKey;
				//		if (facilitiesKey.Equals(currFacilitiesKey)) continue;
				//
				//		duration -= deltaTime;
				//
				//		// 시설 건설 완료
				//		if (duration <= 0f)
				//		{
				//			finishList.Enqueue((slotIndex, facilitiesKey));
				//		}
				//	}
				//	data.InitData(_data, ignoreChangeEvent: true);
				//	if (finishList.Count > 0)
				//	{
				//		while (finishList.Count > 0)
				//		{
				//			var item = finishList.Dequeue();
				//			iamge.Controller.OnFacilitiesConstruct_Finish(item.Item1, item.Item2);
				//		}
				//	}
				}
			}
		}
	}


}