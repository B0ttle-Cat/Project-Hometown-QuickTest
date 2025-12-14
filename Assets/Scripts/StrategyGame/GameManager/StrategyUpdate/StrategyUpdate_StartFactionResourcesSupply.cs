using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_StartFactionResourcesSupply;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_StartFactionResourcesSupply : StrategyUpdateSubClass<ResourcesSupply>
		{
			public StrategyUpdate_StartFactionResourcesSupply(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
			}
			protected override void Start()
			{
				var list = StrategyManager.Collector.GetList<Faction>();
				int length = list.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var faction = list[i];
					if (faction == null) continue;
					this.Add(new ResourcesSupply(this, faction));
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
				private Faction faction;
				public ResourcesSupply(StrategyUpdateSubClass<ResourcesSupply> thisSubClass, Faction faction) : base(thisSubClass)
				{
					this.faction = faction;
				}

				protected override void OnDispose()
				{
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (faction.IsNotAlive()) return;

					TempData.SetTrigger(FactionIsAliveKey(faction), UpdateLogicSort.End);
					TempData.SetValue(FactionTempSupplyValueKey(faction), new TempSupplyValue(faction), UpdateLogicSort.세력_자원갱신종료);
				}
			}
		}
	}


}