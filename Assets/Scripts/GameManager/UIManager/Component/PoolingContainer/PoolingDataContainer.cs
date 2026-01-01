using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
    public class PoolingDataContainer : MonoBehaviour
	{
		[SerializeField, ReadOnly, Required]
		protected PanelGroupComponent targetComponent; 
		protected List<ITargetToPanelAPI> dataList = new List<ITargetToPanelAPI>();

		protected virtual void OnValidate()
		{
			targetComponent = GetComponent<PanelGroupComponent>();
		}

		protected virtual void Awake()
		{
			targetComponent = GetComponent<PanelGroupComponent>();
		}

		public virtual void InitData(IEnumerable<ITargetToPanelAPI> elements)
		{
			dataList.Clear();
			AddData(elements);
		}
		public virtual void ClearData() { dataList.Clear(); }
		public virtual void AddData(ITargetToPanelAPI item)
		{
			dataList.Add(item);
			targetComponent.AddItem(item);
		}
		public virtual void RemoveData(ITargetToPanelAPI item)
		{
			dataList.Remove(item);
			targetComponent.RemoveItem(item);
		}
		public virtual void AddData(IEnumerable<ITargetToPanelAPI> elements)
		{
			foreach (var item in elements)
			{
				dataList.Add(item);
				targetComponent.AddItem(item);
			}
		}
		public virtual void RemoveData(IEnumerable<ITargetToPanelAPI> elements)
		{
			foreach (var item in elements)
			{
				dataList.Remove(item);
				targetComponent.RemoveItem(item);
			}
		}
	}
}