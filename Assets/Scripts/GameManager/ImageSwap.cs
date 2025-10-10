using System;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageSwap : MonoBehaviour
{
	private Image image;

	[SerializeField]
	private SwapData[] swapDatas;
	[Serializable]
	private struct SwapData
	{
		public Sprite sprite;
		public Color color;
	}
	public void Swap(int index)
	{
		if (swapDatas == null || swapDatas.Length == 0) return;

		if(image == null)
			image = GetComponent<Image>();

		if (image == null) return;

		if (index < 0) index = 0;
		if (index >= swapDatas.Length) index = swapDatas.Length - 1;

		var data = swapDatas[index];
		image.sprite = data.sprite;
		image.color = data.color;
	}
}
