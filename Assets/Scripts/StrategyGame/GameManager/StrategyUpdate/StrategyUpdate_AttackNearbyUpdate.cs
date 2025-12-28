namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
        public class StrategyUpdate_AttackNearbyUpdate : StrategyUpdateSubClass<StrategyUpdate_AttackNearbyUpdate.NearbyUpdate>
		{
			public StrategyUpdate_AttackNearbyUpdate(StrategyUpdate updater) : base(updater)
			{
			}

			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<UnitObject>(OnChangeElement);
			}

			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeElement);
			}

			private void OnChangeElement(UnitObject element, bool added)
			{
				if (element.IsNullRef()) return;
				if (added)
				{
					this.Add(new NearbyUpdate(element, this));
				}
                else
                {
					int findIndex = this.FindIndex(f=>f.unitObject == element);
					this.RemoveAt(findIndex);
				}
            }

			public class NearbyUpdate : UpdateLogic
			{
				public readonly UnitObject unitObject;
				private readonly INearbySearcherAPI AttackStartAPI;
				private readonly INearbySearcherAPI AttackLimitAPI;

				public NearbyUpdate(UnitObject unitObject, StrategyUpdate_AttackNearbyUpdate thisSubClass) : base(thisSubClass)
				{
					this.unitObject = unitObject;
					AttackStartAPI = unitObject.AttackStartSearcherAPI;
					AttackLimitAPI = unitObject.AttackLimitSearcherAPI;
				}

				protected override void OnDispose()
				{
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if(unitObject.IsNullRef()) return;
					if (AttackStartAPI.IsNullRef()) return;
					if (AttackLimitAPI.IsNullRef()) return;

					if(unitObject.HasOperation)
					{
						AttackLimitAPI.OnNearbySearching(unitObject.Operation.ActionSearcherAPI);
					}
					else 
					{
						AttackLimitAPI.OnNearbySearching(unitObject.Faction.DetectedList.NearbyList);
					}


					if(AttackLimitAPI.HasNearbySomthing())
					{
						AttackStartAPI.OnNearbySearching(AttackLimitAPI);
					}
					else
					{
						AttackStartAPI.ClearSearching();
					}
				}
			}
		}
	}
}