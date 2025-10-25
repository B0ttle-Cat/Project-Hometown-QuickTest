using System.Collections.Generic;

using static StrategyGamePlayData.ControlBaseData;
using static StrategyUpdate.StrategyUpdate_ConstructUpdate;
public partial class StrategyUpdate
{
	public class StrategyUpdate_ConstructUpdate : StrategyUpdateSubClass<ConstructUpdate>
	{
        public StrategyUpdate_ConstructUpdate(StrategyUpdate updater) : base(updater)
        {
        }

        protected override void Start()
		{
			updateList = new List<ConstructUpdate>();
			var list = StrategyManager.Collector.ControlBaseList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var cb = list[i];
				updateList.Add(new ConstructUpdate(this,cb));
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
			public ControlBase cb;
			public Facilities data;

			public ConstructUpdate(StrategyUpdateSubClass<ConstructUpdate> thisSubClass, ControlBase cb):base(thisSubClass)
			{
				this.cb = cb;
				this.data = cb.Facilities;
			}

			protected override void OnDispose()
			{
				cb = null;
				data = null;
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (cb == null || !cb.isActiveAndEnabled) return;
				if((data ??= cb.Facilities) == null) return;

				var _data = data.GetData();
				int slotLength = _data.slotData.Length;
				for (int i = 0 ; i < slotLength ; i++)
				{
					var slot = _data.slotData[i];
					var constructing = slot.constructing;
					var oldFacilitiesKey = slot.facilitiesKey;
					var nextFacilitiesKey = constructing.facilitiesKey;

					if (string.IsNullOrWhiteSpace(nextFacilitiesKey)
						|| nextFacilitiesKey.Equals(oldFacilitiesKey))
					{
						continue;
					}

					float timeRemaining = constructing.timeRemaining;
					timeRemaining -= deltaTime;
					if(timeRemaining <= 0)
					{
						// 시설 공사 완료
						cb.OnFinishFacilitiesConstruct(i, constructing.facilitiesKey);
					}

					slot.constructing = constructing;
					_data.slotData[i] = slot;
				}

				data.SetData(_data);
			}
		}
	}
}

