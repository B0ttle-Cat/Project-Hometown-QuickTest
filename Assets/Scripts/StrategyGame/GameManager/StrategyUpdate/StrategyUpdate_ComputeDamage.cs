namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
        public class StrategyUpdate_ComputeDamage : StrategyUpdateSubClass<StrategyUpdate_ComputeDamage.ComputeDamage>
		{
			public StrategyUpdate_ComputeDamage(StrategyUpdate updater) : base(updater)
			{
			}

			protected override void Dispose()
			{
				StrategyManager.Collector.RemoveChangeListener<CombatUtility.DamageCommander>(OnChangeValue);
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<CombatUtility.DamageCommander>(OnChangeValue, true);
			}

			private void OnChangeValue(CombatUtility.DamageCommander commander, bool added)
			{
				if (commander == null) return;
				if (added)
				{
					UpdateList.Add(new ComputeDamage(commander, this));
				}
			}

			public class ComputeDamage : UpdateLogic
			{
				public readonly CombatUtility.DamageCommander commander;

				public ComputeDamage(CombatUtility.DamageCommander commander, StrategyUpdateSubClass<ComputeDamage> thisSubClass) : base(thisSubClass)
				{
					this.commander = commander;
				}

				protected override void OnDispose()
				{
					if (commander == null) return;
					commander.Dispose();
				}

				protected override void OnUpdate(in float deltaTime)
				{
					if (commander == null) return;
					commander.ComputeDamage();
					commander.InjectDamage();
				}
			}

			protected override void Update(in float deltaTime)
			{
				int length = UpdateList.Count;
				for (int i = 0 ; i < length ; i++)
				{
					var item = UpdateList[i];
					if (item == null) continue;
					item.Update(in deltaTime);
					item.Dispose();
				}
				UpdateList.Clear();
			}
		}
	}
}
