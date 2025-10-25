using System.Collections.Generic;
public partial class StrategyUpdate
{
    public class StrategyUpdate_EndedResourcesSupply : StrategyUpdateSubClass<StrategyUpdate_EndedResourcesSupply.EndedResourcesSupply>
	{
        public StrategyUpdate_EndedResourcesSupply(StrategyUpdate updater) : base(updater)
        {
        }

        protected override void Start()
        {
			updateList = new List<EndedResourcesSupply>();
			var list = StrategyManager.Collector.ControlBaseList;
			int length = list.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var cb = list[i];
				if (cb == null) continue;
				updateList.Add(new EndedResourcesSupply(this, cb));
			}
		}

        protected override void Update(in float deltaTime)
        {
			int length = updateList.Count;
			for(int i = 0 ;	i < length ;i++)
			{
				var update = updateList[i];
				if(update == null) continue;
				update.Update(in deltaTime);
			}

		}

        public class EndedResourcesSupply : UpdateLogic
        {
			private ControlBase cb;
			public EndedResourcesSupply(StrategyUpdateSubClass<EndedResourcesSupply> thisSubClass, ControlBase cb) :base(thisSubClass)
			{
				this.cb = cb;
			}

			protected override void OnDispose()
            {
            }

			protected override void OnUpdate(in float deltaTime)
            {
				if (cb == null || !cb.isActiveAndEnabled) return;

				string key = $"{cb.ControlBaseName}_{UpdateLogicSort.거점_자원갱신이벤트}";
				if (!TempData.TryGetValue<bool>(key, out var isUpdate) || !isUpdate) return;

				cb.Stats.Invoke();
			}
        }
    }
}

