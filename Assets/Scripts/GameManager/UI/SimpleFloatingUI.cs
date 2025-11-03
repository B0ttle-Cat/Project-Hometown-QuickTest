using System;

using UnityEngine;

public interface SimpleFloatingUI : IDisposable
{
	public SimpleFloatingUI ThisFloating { get; }
	public GameObject gameObject { get;}
	public Positioning ThisPsitioning { get; set; }
	public interface Positioning : IDisposable
	{
		public SimpleFloatingUI ThisUI { get; set; }
		public RectTransform TargetRect { get; set; }
		public void Init(SimpleFloatingUI thisUI, RectTransform targetRect)
		{
			ThisUI = thisUI;
			TargetRect = targetRect;
			OnInit();
		}
		public void OnInit() { }
		public void UpdatePosition();
        void IDisposable.Dispose()
        {
			OnDispose();
			ThisUI = null;
			TargetRect = null;
		}
		public void OnDispose() { }
	}
	public void NewPsitioning<T>(T t, RectTransform rectTransform = null) where T : Positioning
	{
		t.Init(this, (rectTransform == null ? gameObject.GetComponent<RectTransform>() : rectTransform));
		ThisPsitioning = t;
	}
	public void NewPsitioning<T>(RectTransform rectTransform = null) where T : Positioning, new()
	{
		Positioning newPsitioning = new T();
		newPsitioning.Init(this, (rectTransform == null ? gameObject.GetComponent<RectTransform>() : rectTransform));

		ThisPsitioning = newPsitioning;
	}
	public void UpdatePosition()
	{
		ThisPsitioning?.UpdatePosition();
	}

    void IDisposable.Dispose()
    {
         if(ThisPsitioning != null)
			ThisPsitioning.Dispose();
		ThisPsitioning = null;
	}
}
