public partial class StrategyUpdate
{
	public class StrategyUpdate_UnitBuff : StrategyUpdateSubClass<StrategyUpdate_UnitBuff.UnitSpawner>
	{
		public StrategyUpdate_UnitBuff(StrategyUpdate updater) : base(updater)
		{
		}

		protected override void Start()
		{
		
		}

		protected override void Update(in float deltaTime)
		{
		
		}
		protected override void Dispose()
		{
		}


        public class UnitSpawner : UpdateLogic
        {
            public UnitSpawner(StrategyUpdateSubClass<UnitSpawner> thisSubClass) : base(thisSubClass)
            {
            }

            protected override void OnDispose()
            {
                throw new System.NotImplementedException();
            }

            protected override void OnUpdate(in float deltaTime)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}

