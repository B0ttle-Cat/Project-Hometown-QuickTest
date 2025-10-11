using System;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

using static UnityEngine.UI.ContentSizeFitter;

public class RectUIBuilder : IDisposable
{
	private string objectName;
	private RectTransform parent;
	private Action<GameObject, UILayoutBuilder> contentBuilder;

	private GameObject newObj;
	private RectTransform newUI;
	private bool isHideEqualClear;
	public RectTransform RectUI => newUI;

	public RectUIBuilder(string objectName, RectTransform parent, Action<GameObject, UILayoutBuilder> contentBuilder)
	{
		if (parent == null || contentBuilder == null) return;
		this.objectName = objectName;
		this.parent = parent;
		this.contentBuilder = contentBuilder;
		isHideEqualClear = false;
	}

	public RectUIBuilder Option_HideEqualClear(bool isTrue)
	{
		isHideEqualClear = isTrue;
		return this;
	}

	private void Build()
	{
		if (parent == null || contentBuilder == null) return;
		if (newObj != null)
		{
			ClearBuild();
		}
		newObj = new GameObject(objectName);
		newObj.transform.parent = parent;
		newUI = newObj.AddComponent<RectTransform>();
		contentBuilder(newObj, new UILayoutBuilder(newUI));
	}
	public void ClearBuild()
	{
		if (newObj == null)
		{
			GameObject.Destroy(newObj);
		}
		newObj = null;
		newUI = null;
	}
	public void OnShow()
	{
		if (newObj != null)
		{
			newObj.SetActive(true);
		}
		else
		{
			Build();
		}
	}
	public void OnHide()
	{
		if (newObj != null)
		{
			if (isHideEqualClear) ClearBuild();
			else newObj.SetActive(false); ;
		}
	}
	public void Dispose()
	{
		ClearBuild();
		objectName = null;
		parent = null;
		contentBuilder = null;
	}


	public class UILayoutBuilder
	{
		protected RectTransform RootUI;
		protected RectTransform HereUI;
		public UILayoutBuilder(RectTransform root)
		{
			RootUI = root;
			HereUI = root;
		}

		public UILayoutBuilder CurrentRect(Action<RectTransform> thisRect)
		{
			thisRect?.Invoke(HereUI);
			return this;
		}
		public UILayoutBuilder Child(string rectName)
		{
			Transform parent = HereUI.transform.parent;
			HereUI = NewRectTransform(rectName, parent);
			return this;
		}
		private RectTransform NewRectTransform(string rectName, Transform parent)
		{
			Transform findName = HereUI.Find(rectName);

			var newChild = findName == null ? new GameObject(rectName) : findName.gameObject;
			newChild.transform.parent = parent;
			var newRect = newChild.AddComponent<RectTransform>();
			newRect.localPosition = Vector3.zero;
			newRect.localRotation = Quaternion.identity;
			newRect.anchorMin = Vector3.zero;
			newRect.anchorMax = Vector3.one;
			newRect.pivot = Vector3.one * 0.5f;
			newRect.anchoredPosition3D = Vector3.zero;
			newRect.sizeDelta = Vector3.zero;
			return newRect;
		}
		public UILayoutBuilder Parent()
		{
			if (HereUI == RootUI) return this;
			HereUI = HereUI.parent.GetComponent<RectTransform>();
			return this;
		}
		public UILayoutBuilder Root()
		{
			HereUI = RootUI;
			return this;
		}

		public UILayoutBuilder HorizontalLayout(Action<UILayoutBuilder> builder)
		{
			return HorizontalLayout((b, _) => builder?.Invoke(b));
		}
		public UILayoutBuilder VerticalLayout(Action<UILayoutBuilder> builder)
		{
			return VerticalLayout((b, _) => builder?.Invoke(b));
		}
		public UILayoutBuilder HorizontalLayout(Action<UILayoutBuilder, HorizontalLayoutGroup> builder)
		{
			var newLayout = HereUI.AddComponent<HorizontalLayoutGroup>();
			newLayout.childControlWidth = true;
			newLayout.childControlHeight = true;
			newLayout.childScaleWidth = true;
			newLayout.childScaleHeight = true;
			newLayout.childForceExpandWidth = true;
			newLayout.childForceExpandHeight = true;
			builder?.Invoke(new UILayoutBuilder(HereUI), newLayout);
			return this;
		}
		public UILayoutBuilder VerticalLayout(Action<UILayoutBuilder, VerticalLayoutGroup> builder)
		{
			var newLayout = HereUI.AddComponent<VerticalLayoutGroup>();
			newLayout.childControlWidth = true;
			newLayout.childControlHeight = true;
			newLayout.childScaleWidth = true;
			newLayout.childScaleHeight = true;
			newLayout.childForceExpandWidth = true;
			newLayout.childForceExpandHeight = true;
			builder?.Invoke(new UILayoutBuilder(HereUI), newLayout);
			return this;
		}
		public UILayoutBuilder Scroll_HorizontalLayout(Action<ScrollUILayoutBuilder> builder)
		{
			return Scroll_HorizontalLayout((b, _) => builder?.Invoke(b));
		}
		public UILayoutBuilder Scroll_VerticalLayout(Action<ScrollUILayoutBuilder> builder)
		{
			return Scroll_VerticalLayout((b, _) => builder?.Invoke(b));
		}
		public UILayoutBuilder Scroll_HorizontalLayout(Action<ScrollUILayoutBuilder, ScrollRect> builder)
		{
			ScrollRect scrollView = HereUI.AddComponent<ScrollRect>();
			scrollView.horizontal = true;
			scrollView.vertical = false;

			var viewport = NewRectTransform("Viewport", HereUI);
			viewport.AddComponent<RectMask2D>();

			var content = NewRectTransform("Content", viewport);
			var contentLayoutGroup = content.AddComponent<HorizontalLayoutGroup>();
			contentLayoutGroup.childControlWidth = true;
			contentLayoutGroup.childControlHeight = true;
			contentLayoutGroup.childScaleWidth = true;
			contentLayoutGroup.childScaleHeight = true;
			contentLayoutGroup.childForceExpandWidth = true;
			contentLayoutGroup.childForceExpandHeight = true;
            ContentSizeFitter contentSizeFitter =  ContentSizeFitter(content, true, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize);

			// Scrollbar
			Scrollbar scrollbar = NewScrollbar(HereUI, true);

			scrollView.viewport = viewport;
			scrollView.content = content;
			scrollView.horizontalScrollbar = scrollbar;
			scrollView.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollView.horizontalScrollbarSpacing = 0;

			builder?.Invoke(new ScrollUILayoutBuilder(HereUI, scrollView, content, true), scrollView);
			return this;
		}
		public UILayoutBuilder Scroll_VerticalLayout(Action<ScrollUILayoutBuilder, ScrollRect> builder)
		{
			ScrollRect scrollView = HereUI.AddComponent<ScrollRect>();
			scrollView.horizontal = false;
			scrollView.vertical = true;

			var viewport = NewRectTransform("Viewport", HereUI);
			viewport.AddComponent<RectMask2D>();

			var content = NewRectTransform("Content", viewport);
			var contentLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
			contentLayoutGroup.childControlWidth = true;
			contentLayoutGroup.childControlHeight = true;
			contentLayoutGroup.childScaleWidth = true;
			contentLayoutGroup.childScaleHeight = true;
			contentLayoutGroup.childForceExpandWidth = true;
			contentLayoutGroup.childForceExpandHeight = true;
            ContentSizeFitter contentSizeFitter =  ContentSizeFitter(content, false, UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize);

			// Scrollbar
			Scrollbar scrollbar = NewScrollbar(HereUI, false);

			scrollView.viewport = viewport;
			scrollView.content = content;
			scrollView.verticalScrollbar = scrollbar;
			scrollView.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollView.verticalScrollbarSpacing = 0;

			builder?.Invoke(new ScrollUILayoutBuilder(HereUI, scrollView, content, false), scrollView);
			return this;
		}
		private Scrollbar NewScrollbar(RectTransform parent, bool isHorizontal)
		{
			Scrollbar scrollbar = null;

			var scrollbarRect  = NewRectTransform("Scrollbar", parent);
			var slidingArea = NewRectTransform("Sliding Area", scrollbarRect);
			var handle = NewRectTransform("Handle", slidingArea);

			Image scrollbar_background = scrollbarRect.AddComponent<Image>();
			Image handleImage = handle.AddComponent<Image>();

			scrollbar.handleRect = handle;
			scrollbar.targetGraphic = handleImage;

			scrollbar = scrollbarRect.AddComponent<Scrollbar>();
			scrollbar.transition = Selectable.Transition.ColorTint;
			scrollbar.navigation = new Navigation() { mode = Navigation.Mode.None };
			scrollbar.SetDirection(Scrollbar.Direction.LeftToRight, false);
			scrollbar.value = 1f;

			scrollbar_background.color = new Color(190, 190, 190);
			handleImage.color = Color.white;

			if (isHorizontal) RectUISize(scrollbarRect, Diraction.Bottom, 0, 20f);
			else RectUISize(scrollbarRect, Diraction.Right, 0, 20f);


			return scrollbar;
		}

		public UILayoutBuilder ContentSizeFitter(bool isHorizontal, FitMode fitMode = FitMode.PreferredSize)
		{
			ContentSizeFitter(HereUI, isHorizontal, fitMode);
			return this;
		}
		private ContentSizeFitter ContentSizeFitter(RectTransform parent, bool isHorizontal, FitMode fitMode = FitMode.PreferredSize)
		{
			if (!parent.TryGetComponent<ContentSizeFitter>(out var size))
			{
				size = parent.AddComponent<ContentSizeFitter>();
			}
			size.horizontalFit = isHorizontal ? fitMode : UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
			size.verticalFit = isHorizontal ? UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained : fitMode;
			return size;
		}
		public UILayoutBuilder AspectRatioFitter(AspectRatioFitter.AspectMode aspectMode, float aspectRatio = 1f)
		{
			AspectRatioFitter(aspectMode, aspectRatio);
			return this;
		}

		public AspectRatioFitter AspectRatioFitter(RectTransform parent, AspectRatioFitter.AspectMode aspectMode, float aspectRatio = 1f)
		{
			if (!parent.TryGetComponent<AspectRatioFitter>(out var ratio))
			{
				ratio = parent.AddComponent<AspectRatioFitter>();
			}
			ratio.aspectMode = aspectMode;
			ratio.aspectRatio = aspectRatio;
			return ratio;
		}



		public UILayoutBuilder LayoutElement(
			float minWidth = -1, float minHeight = -1, float preferredHeight = -1, float preferredWidth = -1, float flexibleHeight = -1, float flexibleWidth = -1,
			bool ignoreLayout = false, int layoutPriority = 1)
		{
			var layoutElement = HereUI.GetComponent<LayoutElement>();
			if (layoutElement == null) HereUI.AddComponent<LayoutElement>();
			layoutElement.ignoreLayout = ignoreLayout;

			layoutElement.minWidth = minWidth;
			layoutElement.minHeight = minHeight;
			layoutElement.preferredWidth = preferredWidth;
			layoutElement.preferredHeight = preferredHeight;
			layoutElement.flexibleWidth = flexibleWidth;
			layoutElement.flexibleHeight = flexibleHeight;

			layoutElement.layoutPriority = layoutPriority;


			return this;
		}
		public UILayoutBuilder LayoutElementWidth(
			float minWidth = -1, float preferredWidth = -1, float flexibleWidth = -1,
			bool ignoreLayout = false, int layoutPriority = 1)
		{
			return LayoutElement(minWidth: minWidth, preferredWidth: preferredWidth, flexibleWidth: flexibleWidth);
		}
		public UILayoutBuilder LayoutElementHeight(
			float minHeight = -1, float preferredHeight = -1, float flexibleHeight = -1,
			bool ignoreLayout = false, int layoutPriority = 1)
		{
			return LayoutElement(minHeight: minHeight, preferredHeight: preferredHeight, flexibleHeight: flexibleHeight);
		}
		public enum Diraction
		{
			Top, Bottom, Left, Right
		}
		public UILayoutBuilder RectUISize(Diraction diraction, float offset, float length)
		{
			RectUISize(HereUI, diraction, offset, length);
			return this;
		}
		public UILayoutBuilder RectUISize(float top, float bottom, float left, float right)
		{
			RectUISize(HereUI, top, bottom, left, right);
			return this;
		}
		public UILayoutBuilder RectUISize(RectTransform rectTransform, Diraction diraction, float offset, float length)
		{
			rectTransform.anchorMin = diraction switch
			{
				Diraction.Top => new(0, 1),
				Diraction.Bottom => new(0, 0),
				Diraction.Left => new(0, 0),
				Diraction.Right => new(1, 0),
				_ => new(0, 0),
			};
			rectTransform.anchorMax = diraction switch
			{
				Diraction.Top => new(1, 1),
				Diraction.Bottom => new(1, 0f),
				Diraction.Left => new(0, 1),
				Diraction.Right => new(1, 1),
				_ => new(1, 1),
			};
			rectTransform.pivot = diraction switch
			{
				Diraction.Top => new(0.5f, 1f),
				Diraction.Bottom => new(0.5f, 0f),
				Diraction.Left => new(0, 0.5f),
				Diraction.Right => new(1, 0.5f),
				_ => new(0.5f, 0.5f),
			};
			rectTransform.anchoredPosition = diraction switch
			{
				Diraction.Top => new(0, -offset),
				Diraction.Bottom => new(0, offset),
				Diraction.Left => new(offset, 0),
				Diraction.Right => new(-offset, 0),
				_ => new(0, 0),
			};
			rectTransform.sizeDelta = diraction switch
			{
				Diraction.Top => new(0, length),
				Diraction.Bottom => new(0, length),
				Diraction.Left => new(length, 0),
				Diraction.Right => new(length, 0),
				_ => new Vector2(offset, offset) * -2,
			};

			return this;
		}
		public void RectUISize(RectTransform rectTransform, float top, float bottom, float left, float right)
		{
			rectTransform.anchorMin = new(0, 0);
			rectTransform.anchorMax = new(1, 1);
			rectTransform.pivot = new(0.5f, 0.5f);
			rectTransform.anchoredPosition = new Vector2(left - right, bottom - top) * 0.5f;
			rectTransform.sizeDelta = -new Vector2(left + right, top + bottom);
		}
		public UILayoutBuilder Component<T>() where T : Component => Component<T>(null);
		public UILayoutBuilder Component<T>(Action<T> component) where T : Component
		{
			if (!HereUI.TryGetComponent<T>(out T t))
			{
				t = HereUI.AddComponent<T>();
			}
			component?.Invoke(t);
			return this;
		}
	}
	public class ScrollUILayoutBuilder : UILayoutBuilder
	{
		private ScrollRect scrollView;
		private RectTransform content;

		private readonly bool isHorizontal;
		public UILayoutBuilder ContentLayout { get; private set; }
		public ScrollUILayoutBuilder(RectTransform root,
			ScrollRect scrollView,
			RectTransform content,
			bool isHorizontal) : base(root)
		{
			this.scrollView = scrollView;
			this.content = content;
			this.isHorizontal = isHorizontal;

			ContentLayout = new UILayoutBuilder(content);
		}
	}
}
