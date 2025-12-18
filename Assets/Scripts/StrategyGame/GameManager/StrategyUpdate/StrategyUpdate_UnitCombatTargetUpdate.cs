using static StrategyManagerModule.StrategyUpdate.StrategyUpdate_UnitCombatTargetUpdate;

namespace StrategyManagerModule
{
	public partial class StrategyUpdate
	{
		public class StrategyUpdate_UnitCombatTargetUpdate : StrategyUpdateSubClass<CombatTarget>
		{
			public StrategyUpdate_UnitCombatTargetUpdate(StrategyUpdate updater) : base(updater)
			{
			}
			protected override void Dispose()
			{
			}
			protected override void Start()
			{
				StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeValue, true);
			}
			private void OnChangeValue(IStrategyElement element, bool added)
			{
				if (element == null || element is not UnitObject unitObject) return;

				if (added)
				{
					this.Add(new CombatTarget(unitObject, this));
				}
				else
				{
					int findIndex = this.FindIndex(i=>i.unitObject == unitObject);
					if (findIndex < 0) return;
					this.RemoveAt(findIndex);
				}
			}
			public class CombatTarget : UpdateLogic
			{
				public readonly UnitObject unitObject;
				public readonly ICombatHandler combatController;
				public CombatTarget(UnitObject unitObject, StrategyUpdate_UnitCombatTargetUpdate thisSubClass) : base(thisSubClass)
				{
					this.unitObject = unitObject;
					combatController = unitObject;
				}
				protected override void OnDispose()
				{
				}
				protected override void OnUpdate(in float deltaTime)
				{
					if (unitObject == null) return;

					if (combatController.HasKeepAttackTarget()) return;

					combatController.ChangeCombatTarget(combatController.SearchingNewTarget(out var newTarget)
						? newTarget : null);
				}
			}
		}
	} 
}