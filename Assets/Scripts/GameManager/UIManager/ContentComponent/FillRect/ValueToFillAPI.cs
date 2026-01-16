using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
	public class ValueToFillAPI : MonoBehaviour
	{
		private enum FillMathord
		{
			None,
			Image,
			FillGroup,
			FillItem,
		}

		[SerializeField, ReadOnly]
		private FillMathord fillMathord;

		private IFillGroup fillGroup;
		private IFillItem fillItem;
		private Image image;
		public void Awake()
		{
			if (TryGetComponent<IFillGroup>(out var _fillGroup))
			{
				fillGroup = _fillGroup;
				fillMathord = FillMathord.FillGroup;
			}
			else if (TryGetComponent<IFillItem>(out var _fillItem))
			{
				fillItem = _fillItem;
				fillMathord = FillMathord.FillItem;
			}
			else if (TryGetComponent<Image>(out var _image))
			{
				image = _image;
				fillMathord = FillMathord.Image;
			}
			else
			{
				fillMathord = FillMathord.None;
			}
		}
		public void SetValue(float value, int index = 0)
		{
			SetValue(value, new Vector2(0f, 1f), index);
		}
		public void SetValue(float value, Vector2 minMax, int index = 0)
		{
			switch (fillMathord)
			{
				case FillMathord.Image:
				if (image.IsNotNullRef())
				{
					float min = Mathf.Min(minMax.x,minMax.y);
					float max = Mathf.Max(minMax.x,minMax.y);

					float fillAmount = (value - min) / (max - min);
					if (!float.IsNormal(fillAmount)) fillAmount = 0;
					image.fillAmount = fillAmount;
				}
				break;
				case FillMathord.FillGroup:
				if (fillGroup.IsNotNullRef())
				{
					fillGroup.MinMax = minMax;
					fillGroup[index] = value;
				}
				break;
				case FillMathord.FillItem:
				if (fillItem.IsNotNullRef())
				{
					fillItem.MinMax = minMax;
					fillItem.Value = new Vector2(0f, 1f) * value;
				}
				break;
			}
		}
		public void SetValue(int value, Vector2Int minMax, int index = 0)
		{
			SetValue(value, new Vector2(minMax.x, minMax.y), index);
		}
	}
}
