using UnityEngine;

using static StrategyGamePlayData;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_EndedSectorResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_EndedSectorResourcesSupply.ResourcesSupply>
		{
			public StrategyUpdate_EndedSectorResourcesSupply(StrategyUpdate updater) : base(updater)
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
					var sector = list[i];
					if (sector == null) continue;
					this.Add(new ResourcesSupply(this, sector));
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = this.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var update = this[i];
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
						sector.StatsValue.SetStatsValue(StatsType.거점_전력_현재, Mathf.Clamp(tempValue.electric, 0, tempValue.electricMax));
					}
					if (tempValue.materialIsUpdate)
					{
						sector.StatsValue.SetStatsValue(StatsType.거점_전력_현재, Mathf.Clamp(tempValue.material, 0, tempValue.materialMax));
					}
					if (tempValue.manpowerIsUpdate)
					{
						sector.StatsValue.SetStatsValue(StatsType.거점_전력_현재, Mathf.Clamp(tempValue.manpower, 0, tempValue.manpowerMax));
					}
				}
			}
		}
	}
}
