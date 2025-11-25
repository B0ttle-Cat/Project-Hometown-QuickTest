using System.Collections.Generic;

using UnityEngine;

using static StrategyUpdate.StrategyUpdate_EndedSectorResourcesSupply;
public partial class StrategyUpdate
{
	public class StrategyUpdate_EndedSectorResourcesSupply : StrategyUpdateSubClass<ResourcesSupply>
	{
		public StrategyUpdate_EndedSectorResourcesSupply(StrategyUpdate updater) : base(updater)
		{
		}
		protected override void Dispose()
		{
		}
		protected override void Start()
		{
			updateList = new List<ResourcesSupply>();
			var list = StrategyManager.Collector.SectorList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var sector = list[i];
				if (sector == null) continue;
				updateList.Add(new ResourcesSupply(this, sector));
			}
		}

		protected override void Update(in float deltaTime)
		{
			int length = updateList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var update = updateList[i];
				if (update == null) continue;
				update.Update(in deltaTime);
			}
		}

		public class ResourcesSupply : UpdateLogic
		{
			private SectorObject sector;
			public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, SectorObject sector) : base(thisSubClass)
			{
				this.sector = sector;
			}

			protected override void OnDispose()
			{
			}

			protected override void OnUpdate(in float deltaTime)
			{
				if (sector == null || !sector.isActiveAndEnabled) return;

				string key = $"{sector.SectorName}_ResourcesSupply";
				if (!TempData.GetTrigger(key)) return;
				if (!TempData.TryGetValue<TempSupplyValue>(SectorTempSupplyValueKey(sector), out var tempValue)) return;
			
				if (tempValue.electricIsUpdate)
				{
					sector.SetElectric(Mathf.Clamp(tempValue.electric, 0, tempValue.electricMax));
				}
				if (tempValue.materialIsUpdate)
				{
					sector.SetElectric(Mathf.Clamp(tempValue.material, 0, tempValue.materialMax));
				}
				if (tempValue.manpowerIsUpdate)
				{
					sector.SetElectric(Mathf.Clamp(tempValue.manpower, 0, tempValue.manpowerMax));
				}
				if (tempValue.electricIsUpdate || tempValue.materialIsUpdate || tempValue.manpowerIsUpdate)
				{
					sector.Stats.Invoke();
				}
			}
		}
	}
}

