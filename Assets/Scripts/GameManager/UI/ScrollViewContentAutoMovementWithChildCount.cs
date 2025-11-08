using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ScrollViewContentAutoMovementWithChildCount : MonoBehaviour
{
	HorizontalOrVerticalLayoutGroup horizontalVerticalLayoutGroup;

	public int minChildrenForScrollView = 5;
	public ScrollRect scrollView;

	private Transform outScrollParent;
	private int outScrollTransformOrder;

	private RectTransform thisRect;
	public bool scrollViewAutoActive = true;


	Vector2 anchorMin;
	Vector2 anchorMax;
	Vector2 anchoredPosition;
	Vector2 sizeDelta;
	Vector2 pivot;

	private void Awake()
    {


		OnTransformChildrenChanged();
	}
	public void ControlChildSize(bool isOn)
	{
		if(horizontalVerticalLayoutGroup == null)
			horizontalVerticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
		if(horizontalVerticalLayoutGroup == null)
			horizontalVerticalLayoutGroup = GetComponent<HorizontalLayoutGroup>();

		if (horizontalVerticalLayoutGroup == null) return;

		if(horizontalVerticalLayoutGroup is VerticalLayoutGroup)
		{
			horizontalVerticalLayoutGroup.childControlHeight = isOn;
		}
		else
		{
			horizontalVerticalLayoutGroup.childControlWidth = isOn;
		}
	}

	public void OnTransformChildrenChanged()
    {
		if(thisRect == null) thisRect = GetComponent<RectTransform>();


		if (transform.childCount >= minChildrenForScrollView)
		{
			InsideScroll();
		}
		else
		{
			OutsideScroll();
		}

		void InsideScroll()
		{
			if (scrollView != null && thisRect.parent != scrollView.viewport)
			{
				outScrollParent = thisRect.parent;
				outScrollTransformOrder = thisRect.GetSiblingIndex();

				anchorMin = thisRect.anchorMin;
				anchorMax = thisRect.anchorMax;
				anchoredPosition = thisRect.anchoredPosition;
				sizeDelta = thisRect.sizeDelta;
				pivot = thisRect.pivot;

				thisRect.SetParent(scrollView.viewport);
				thisRect.anchorMin = new Vector2(0, 1f);
				thisRect.anchorMax = new Vector2(1, 1f);
				thisRect.anchoredPosition = Vector2.zero;
				thisRect.sizeDelta = Vector2.zero;
				thisRect.pivot = new Vector2(0.5f, 1f);

				if (scrollViewAutoActive)
				{
					scrollView.gameObject.SetActive(true);
				}

				ControlChildSize(true);
			}
		}
		void OutsideScroll()
		{
			if (outScrollParent != null && thisRect.parent != outScrollParent)
			{
				thisRect.SetParent(outScrollParent);
				thisRect.SetSiblingIndex(outScrollTransformOrder);

				thisRect.anchorMin = anchorMin;
				thisRect.anchorMax = anchorMax;
				thisRect.anchoredPosition = anchoredPosition;
				thisRect.sizeDelta = sizeDelta;
				thisRect.pivot = pivot;

				if (scrollViewAutoActive)
				{
					scrollView.gameObject.SetActive(false);
				}

				ControlChildSize(false);
			}
		}
	}
}
