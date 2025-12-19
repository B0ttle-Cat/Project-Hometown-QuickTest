namespace StrategyManagerModule
{
    public partial class StrategyUpdate
	{
        public class StrategyUpdate_ElementDestroyer : StrategyUpdateSubClass<StrategyUpdate_ElementDestroyer.ElementDestroyer>
		{
			BaseList<IStrategyElementDestroyer> destroyers;
			public StrategyUpdate_ElementDestroyer(StrategyUpdate updater) : base(updater)
            {
            }

            protected override void Dispose()
            {
				StrategyManager.Collector.RemoveChangeListener<IStrategyElementDestroyer>(OnChangeValue);
				destroyers = null;
			}

            protected override void Start()
            {
				StrategyManager.Collector.AddChangeListener<IStrategyElementDestroyer>(OnChangeValue, true);
            }

            private void OnChangeValue(IStrategyElementDestroyer destroyer, bool added)
            {
				if (destroyer == null) return;

				if(added)
				{
					Add(new ElementDestroyer(destroyer, this));
				}
            }

            public class ElementDestroyer : UpdateLogic
            {
				public IStrategyElementDestroyer destroyer;
				public ElementDestroyer(IStrategyElementDestroyer destroyer, StrategyUpdate_ElementDestroyer thisSubClass) : base(thisSubClass)
                {
					this.destroyer = destroyer;
				}

                protected override void OnDispose()
				{
					if(destroyer != null)
					{
						destroyer = null;
					}
				}

                protected override void OnUpdate(in float deltaTime)
                {
					if(destroyer == null) return;
					destroyer.OnDestroy();
					destroyer = null;
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
					item.Dispose();
				}
				this.Clear();
				(destroyers ??= StrategyManager.Collector.GetList<IStrategyElementDestroyer>())?.Clear();
			}
		}
	}
}
