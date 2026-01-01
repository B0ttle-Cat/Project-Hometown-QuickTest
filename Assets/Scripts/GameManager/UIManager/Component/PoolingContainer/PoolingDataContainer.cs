using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
    public class PoolingDataContainer : MonoBehaviour
	{
		[SerializeField, ReadOnly, Required]
		protected PanelGroupComponent targetComponent; 
		protected List<IObjectForPanel> dataList = new List<IObjectForPanel>();

		protected virtual void OnValidate()
		{
			targetComponent = GetComponent<CardGroupPanelComponent>();
		}

		protected virtual void Awake()
		{
			targetComponent = GetComponent<CardGroupPanelComponent>();
		}

		public virtual void InitData(IEnumerable<IObjectForPanel> elements)
		{
			dataList.Clear();
			AddData(elements);
		}
		public virtual void ClearData() { dataList.Clear(); }
		public virtual void AddData(IObjectForPanel item)
		{
			dataList.Add(item);
			targetComponent.AddItem(item);
		}
		public virtual void RemoveData(IObjectForPanel item)
		{
			dataList.Remove(item);
			targetComponent.RemoveItem(item);
		}
		public virtual void AddData(IEnumerable<IObjectForPanel> elements)
		{
			foreach (var item in elements)
			{
				dataList.Add(item);
				targetComponent.AddItem(item);
			}
		}
		public virtual void RemoveData(IEnumerable<IObjectForPanel> elements)
		{
			foreach (var item in elements)
			{
				dataList.Remove(item);
				targetComponent.RemoveItem(item);
			}
		}
	}
}