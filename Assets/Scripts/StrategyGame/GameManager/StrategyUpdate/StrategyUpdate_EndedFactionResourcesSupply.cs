using System;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		[Obsolete("StrategyUpdate_ResourcesSupply 를 사용", true)]
		public class StrategyUpdate_EndedFactionResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_EndedFactionResourcesSupply.ResourcesSupply>
		{
			public StrategyUpdate_EndedFactionResourcesSupply(StrategyUpdate updater) : base(updater)
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
					if (faction == null || faction.IsNotAlive()) return;
				}
			}
		}
	}


}