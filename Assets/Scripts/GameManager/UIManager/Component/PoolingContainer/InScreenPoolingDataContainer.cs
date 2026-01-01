using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	public class InScreenPoolingDataContainer : PoolingDataContainer
	{
		[SerializeField, Required]
		private RectTransform poolItemRect;
		[SerializeField]
		private Camera screenCamera;

		private Vector2 boundaryRatio;
		private HashSet<ITargetToPanelAPI> isInScreen;

		public override void InitData(IEnumerable<ITargetToPanelAPI> elements)
		{
			screenCamera = screenCamera.IsNotNullRef() ? screenCamera : Camera.main;
			var poolItemSize = poolItemRect.rect.size;
			boundaryRatio.x = Screen.width <= 0 ? 1 : poolItemSize.x / Screen.width;
			boundaryRatio.y = Screen.height <= 0 ? 1 : poolItemSize.y / Screen.height;

			isInScreen ??= new HashSet<ITargetToPanelAPI>();
			isInScreen.Clear();

			dataList.Clear();
			AddData(elements);
		}
		public override void ClearData() {
			dataList.Clear();

			isInScreen?.Clear();
			isInScreen = null;
		}
		public override void AddData(ITargetToPanelAPI item)
		{
			dataList.Add(item);
			UpdateState(item);
		}
		public override void RemoveData(ITargetToPanelAPI item)
		{
			dataList.Remove(item);
			if (isInScreen.Remove(item))
				RemoveItem(item);
		}
		public override void AddData(IEnumerable<ITargetToPanelAPI> elements)
		{
			foreach (var item in elements)
			{
				dataList.Add(item);
				UpdateState(item);
			}
		}
		public override void RemoveData(IEnumerable<ITargetToPanelAPI> elements)
		{
			foreach (var item in elements)
			{
				dataList.Remove(item);
				if (isInScreen.Remove(item))
					RemoveItem(item);
			}
		}

		private void LateUpdate()
		{
			int length = dataList == null ? 0 : dataList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				ITargetToPanelAPI target = dataList[i];
				UpdateState(target);
			}
		}

		private void UpdateState(ITargetToPanelAPI item)
		{
			if (item.IsNullRef()) return;
			if (item is ITargetToLabelAPI labelAPI)
			{
				Vector3 worldPosition = labelAPI.LabelWorldPosition();
				Vector3 viewportPoint = screenCamera.WorldToViewportPoint(worldPosition);

				bool isInSCreen = viewportPoint.x >= boundaryRatio.x && viewportPoint.x <= 1-boundaryRatio.x
						&& viewportPoint.y >= boundaryRatio.y && viewportPoint.y <= 1-boundaryRatio.y;

				if (isInSCreen)
				{
					if(isInScreen.Add(item))
					{
						AddItem(item);
					}
					return;
				}
			}

			if (isInScreen.Remove(item))
			{
				RemoveItem(item);
			}
		}
	}
}

