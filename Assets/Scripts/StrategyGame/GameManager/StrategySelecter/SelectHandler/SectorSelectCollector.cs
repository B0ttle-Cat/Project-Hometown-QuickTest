using System;
using System.Collections.Generic;

namespace StrategyManagerModule
{
	public class SectorSelectCollector : SelectCollector<SectorObject>,  IList<SectorObject>
	{
		protected override void Init()
		{
		}
		protected override void Deinit()
		{
		}
		protected override void OnSelected(SectorObject selectItem) => ThisSelecter.CallSectorSelected(selectItem);
		protected override void OnDeselected(SectorObject selectItem) => ThisSelecter.CallSectorDeselected(selectItem);
		protected override void OnPointing(SectorObject selectable) => ThisSelecter.CallSectorPointing(selectable);
	}
	public partial class StrategySelecter
	{
		private SectorSelectCollector selectSector;
		public SectorSelectCollector SelectSector
		{
			get
			{
				if (selectSector == null)
				{
					selectSector = gameObject.GetComponent<SectorSelectCollector>();
				}
				return selectSector;
			}
		}
		public event Action<SectorObject,bool> OnSectorSelectChange;
		public event Action<SectorObject> OnSectorPointing;

		internal void CallSectorSelected(SectorObject sectorObject)
		{
			OnSectorSelectChange?.Invoke(sectorObject, true);
		}
		internal void CallSectorDeselected(SectorObject sectorObject)
		{
			OnSectorSelectChange?.Invoke(sectorObject, false);
		}
		internal void CallSectorPointing(SectorObject sectorObject)
		{
			OnSectorPointing?.Invoke(sectorObject);
		}
	}
}