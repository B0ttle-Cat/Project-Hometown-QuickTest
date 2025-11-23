using System.Collections.Generic;

using static StrategyUpdate.StrategyUpdate_StartFactionResourcesSupply;
public partial class StrategyUpdate
{
	public class StrategyUpdate_StartFactionResourcesSupply : StrategyUpdateSubClass<ResourcesSupply>
	{
		public StrategyUpdate_StartFactionResourcesSupply(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
			updateList = new List<ResourcesSupply>();
			var list = StrategyManager.Collector.FactionList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var faction = list[i];
				if (faction == null) continue;
				updateList.Add(new ResourcesSupply(this, faction));
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

