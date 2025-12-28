namespace StrategyManagerModule
{
    public class GameUIInfoUpdateWithUI_ElementInfoUpdate : GameUIInfoUpdateWithUI<GameUIInfoUpdateWithUI_ElementInfoUpdate.ElementInfoUpdate>
	{
		private int FactionCount;
		private int OperationCount;
		private int UnitCount;
        public GameUIInfoUpdateWithUI_ElementInfoUpdate(StrategyUpdate updater) : base(updater)
        {
        }


        protected override void Dispose()
		{
			UnitCount = OperationCount = FactionCount = 0;
			StrategyManager.Collector.RemoveChangeListener<Faction>(OnChangeValue);
			StrategyManager.Collector.RemoveChangeListener<OperationObject>(OnChangeValue);
			StrategyManager.Collector.RemoveChangeListener<UnitObject>(OnChangeValue);
		}

        protected override void Start()
        {
			UnitCount = OperationCount = FactionCount = 0;
			StrategyManager.Collector.AddChangeListener<Faction>(OnChangeValue);
			StrategyManager.Collector.AddChangeListener<OperationObject>(OnChangeValue);
			StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeValue);
		}

		private void OnChangeValue(Faction faction, bool added)
		{
			if (added)
			{
				this.Insert(0 + FactionCount, new FactionInfoUpdate(faction, this));
				FactionCount++;
			}
			else
			{
				int findIndex = FindIndex(i=>i.Target == faction.ThisElement);
				if (findIndex >= 0)
				{
					this.RemoveAt(findIndex);
					FactionCount--;
				}
			}
		}
		private void OnChangeValue(OperationObject operation, bool added)
		{
			if (added)
			{
				this.Insert(FactionCount + OperationCount, new OperationInfoUpdate(operation, this));
				OperationCount++;
			}
			else
			{
				int findIndex = FindIndex(i=>i.Target == operation.ThisElement);
				if (findIndex >= 0)
				{
					this.RemoveAt(findIndex);
					OperationCount--;
				}
			}
		}
		private void OnChangeValue(UnitObject unit, bool added)
		{
			if (added)
			{
				this.Insert(FactionCount + OperationCount + UnitCount, new UnitInfoUpdate(unit, this));
				UnitCount++;
			}
			else
			{
				int findIndex = FindIndex(i=>i.Target == unit.ThisElement);
				if (findIndex >= 0)
				{
					this.RemoveAt(findIndex);
					UnitCount--;
				}
			}
		}

        public abstract class ElementInfoUpdate : UpdateLogic<IStrategyElement>
        {
            protected ElementInfoUpdate(IStrategyElement target, GameUIInfoUpdateWithUI<ElementInfoUpdate> thisSubClass) : base(target, thisSubClass){}
        }
        public class FactionInfoUpdate : ElementInfoUpdate
		{
			public FactionInfoUpdate(Faction faction, GameUIInfoUpdateWithUI_ElementInfoUpdate thisSubClass) : base(faction, thisSubClass)
			{
			}

			protected override void OnDispose()
			{
			}

			protected override void OnInfpUpdate(in float deltaTime)
			{
				if (Target is not Faction faction) return;

				faction.OnChangeElementSetEvent();
			}
		}
		public class OperationInfoUpdate : ElementInfoUpdate
		{
			public OperationInfoUpdate(OperationObject operation, GameUIInfoUpdateWithUI_ElementInfoUpdate thisSubClass) : base(operation, thisSubClass)
			{
			}

			protected override void OnDispose()
			{
			}

			protected override void OnInfpUpdate(in float deltaTime)
			{
				if (Target is not OperationObject operation) return;

			}
		}
		public class UnitInfoUpdate : ElementInfoUpdate
		{
			public UnitInfoUpdate(UnitObject unit, GameUIInfoUpdateWithUI_ElementInfoUpdate thisSubClass) : base(unit, thisSubClass)
			{
			}

			protected override void OnDispose()
			{
			}

			protected override void OnInfpUpdate(in float deltaTime)
			{
				if (Target is not UnitObject unit) return;

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
    }

}
