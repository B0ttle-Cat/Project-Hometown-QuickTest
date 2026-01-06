using System;

namespace StrategyManagerModule
{
	public class UnitSelectCollector : SelectCollector<UnitObject>
	{

		protected override void Deinit()
		{
		}
		protected override void Init()
		{
		}
		protected override void OnSelected(UnitObject selectItem) => ThisSelecter.CallUnitSelected(selectItem);
		protected override void OnDeselected(UnitObject selectItem) => ThisSelecter.CallUnitDeselected(selectItem);
		protected override void OnPointing(UnitObject selectable) => ThisSelecter.CallUnitPointing(selectable);

	}
	public partial class StrategySelecter
	{
		private UnitSelectCollector selectUnit;
		public UnitSelectCollector SelectUnit
		{
			get
			{
				if (selectUnit == null)
				{
					selectUnit = gameObject.GetComponent<UnitSelectCollector>();
				}
				return selectUnit;
			}
		}
		public event Action<UnitObject,bool> OnUnitSelectChange;
		public event Action<UnitObject> OnUnitPointing;

		internal void CallUnitSelected(UnitObject sectorObject)
		{
			OnUnitSelectChange?.Invoke(sectorObject, true);
		}
		internal void CallUnitDeselected(UnitObject sectorObject)
		{
			OnUnitSelectChange?.Invoke(sectorObject, false);
		}
		internal void CallUnitPointing(UnitObject sectorObject)
		{
			OnUnitPointing?.Invoke(sectorObject);
		}
	}
}