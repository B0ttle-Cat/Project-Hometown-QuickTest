using System;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyDetailsPanelUI : MonoBehaviour, IGamePanelUI, IStartGame
{
	[SerializeField]
	private  TabPanelUI tabPanelUI;
	private ITabControl tabControl;

	[SerializeField]
	private RectTransform contentRoot;
	[SerializeField, ReadOnly]
	private RectTransform currrentContent;
    public RectTransform CurrrentContent { get => currrentContent; private set => currrentContent = value; }
	public void Reset()
	{
		Init();
	}
	public void Awake()
    {
		Init();
	}
	private void Init()
	{
		fieldInfoDetailsPanelUI = null;
		sectorDetailsPanelUI = null;
	}
    private void OnDestroy()
    {
		fieldInfoDetailsPanelUI?.Dispose();
		sectorDetailsPanelUI?.Dispose();

		fieldInfoDetailsPanelUI = null;
		sectorDetailsPanelUI = null;
	}

    public void OpenUI()
	{
		tabControl = tabPanelUI.GetTabControl();
		currrentContent = null;
		gameObject.SetActive(true);
	}
	public void CloseUI()
	{
		gameObject.SetActive(false);

		if(currrentContent != null)
		{
			fieldInfoDetailsPanelUI?.OnHideFieldInfoDetails();
			sectorDetailsPanelUI?.OnHideSectorDetail();
		}
		currrentContent = null;
	}

	void IStartGame.OnStartGame()
	{
		(this as IGamePanelUI).CloseUI();
	}

	void IStartGame.OnStopGame()
	{
		(this as IGamePanelUI).CloseUI();
	}

	public abstract class DetailsContentPanel : IDisposable
	{
		private bool isShow;
		private bool isDispose;
		protected StrategyDetailsPanelUI ThisPanel;
		protected RectTransform ThisContent;
		protected IKeyPairChain PairChain;
		protected DetailsContentPanel(StrategyDetailsPanelUI thisPanel, RectTransform contentPrefab)
		{
			ThisPanel = thisPanel;
			ThisContent = GameObject.Instantiate(contentPrefab, thisPanel.contentRoot);
			PairChain = ThisContent.gameObject.GetPairChain();
			isShow = false;
			isDispose = false;
			ThisContent.gameObject.SetActive(false);
		}

		public virtual void Show()
		{
			if (isShow) return;
			isShow = true;

			if (ThisPanel.CurrrentContent != null)
			{
				ThisPanel.CurrrentContent.gameObject.SetActive(false);
			}
			ThisPanel.CurrrentContent = ThisContent;
			ThisContent.gameObject.SetActive(true);
			OnShow();
		}
		public virtual void Hide()
		{
			if (!isShow) return;
			isShow = false;

			OnHide();
			ThisContent.gameObject.SetActive(false);
			ThisPanel.CurrrentContent = null;
		}
		public virtual void Dispose()
		{
			if (isDispose) return;
			isDispose = true;

			Hide();
			OnDispose();
			ThisPanel = null;
			if (ThisContent != null)
			{
				GameObject.Destroy(ThisContent.gameObject);
				ThisContent = null;
			}
			PairChain = null;
		}
		protected abstract void OnShow();
		protected abstract void OnHide();
		protected abstract void OnDispose();
	}
}