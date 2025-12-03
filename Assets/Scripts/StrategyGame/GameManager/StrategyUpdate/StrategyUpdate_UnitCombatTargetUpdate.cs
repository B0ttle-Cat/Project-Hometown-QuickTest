using static StrategyUpdate.StrategyUpdate_UnitCombatTargetUpdate;
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
			StrategyManager.Collector.AddChangeListener<UnitObject>(OnChangeValue, ForeachAll);
			void ForeachAll(IStrategyElement element)
			{
				OnChangeValue(element, true);
			}
		}
		private void OnChangeValue(IStrategyElement element, bool added)
		{
			if (element == null || element is not UnitObject unitObject) return;

			if (added)
			{
				UpdateList.Add(new CombatTarget(unitObject, this));
			}
			else
			{
				int findIndex = UpdateList.FindIndex(i=>i.unitObject == unitObject);
				if (findIndex < 0) return;
				UpdateList.RemoveAt(findIndex);
			}
		}
		public class CombatTarget : UpdateLogic
		{
			public readonly UnitObject unitObject;
			public readonly IUnitCombatController combatController;
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

				combatController.UpdateParameters();

				if (combatController.IsKeepingTargetAllowed()) return;

				combatController.ChangeCombatTarget(combatController.SearchingNewTarget(out var newTarget)
					? newTarget : null);
			}
		}
	}
}