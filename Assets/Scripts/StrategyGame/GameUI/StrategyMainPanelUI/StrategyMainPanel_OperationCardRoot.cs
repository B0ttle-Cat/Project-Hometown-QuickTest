using GameUI;

using StrategyManagerModule;

using UnityEngine;
using UnityEngine.UI;

public class StrategyMainPanel_OperationCardRoot : PanelItemComponent, IShowHideAsync
{
	[SerializeField]
	private Button onShowCardGroupButton;
	[SerializeField]
	private Button onShowPickupButton;
	[SerializeField]
	private Button onInstanceOperationButton;
	[Space]
	[SerializeField]
	private OperationCardGroupPanel CardPanel;
	[SerializeField]
	private UnitPickupCardGroupPanel PickupPanel;

	[Space]
	[SerializeField]
	private GameObject showInstanceOperationGuide;
	[SerializeField]
	private bool awaitInstanceOperation;

	void IShowHide.EndedHide()
	{
		onShowCardGroupButton.onClick.RemoveAllListeners();
		onShowPickupButton.onClick.RemoveAllListeners();
		onInstanceOperationButton.onClick.RemoveAllListeners();

		SowCardGroupView(false);
		ShowPickupView(false);
		ShowInstanceGuideView(false);

		awaitInstanceOperation = false;
	}
	void IShowHide.StartShow()
	{
		onShowCardGroupButton.onClick.RemoveAllListeners();
		onShowPickupButton.onClick.RemoveAllListeners();
		onInstanceOperationButton.onClick.RemoveAllListeners();
		onShowCardGroupButton.onClick.AddListener(OnShowCardGroupButton);
		onShowPickupButton.onClick.AddListener(OnShowPickupButton);
		onInstanceOperationButton.onClick.AddListener(OnInstanceOperation);
		OnShowCardGroupButton();

		awaitInstanceOperation = false;
	}

	public void OnShowCardGroupButton()
	{
		SowCardGroupView(true);
		ShowPickupView(false);
		ShowInstanceGuideView(false);
	}
	public void OnShowPickupButton()
	{
		SowCardGroupView(false);
		ShowPickupView(true);
		ShowInstanceGuideView(false);
	}
	public async void OnInstanceOperation()
	{
		if (awaitInstanceOperation) return;
		if (PickupPanel.IsNullRef()) return;
		SpawnTroopsInfo spawnTroopsInfo = PickupPanel.GetSpawnTroopsInfo();
		if (spawnTroopsInfo.totalCount == 0) return;

		bool isCancel = true;
		SowCardGroupView(false);
		ShowPickupView(false);
		ShowInstanceGuideView(true);

		awaitInstanceOperation = true;
		{
			using var pointingAtSector =  new ProcessOverrider_PointingAtSector(OnProcessOverrider_PointingAtSector);
			using var onPressedEscapeKey = new ProcessOverrider_OnPressedEscapeKey(OnProcessOverrider_OnPressedEscapeKey);

			while (awaitInstanceOperation)
			{
				await Awaitable.NextFrameAsync();
			}
		}
		awaitInstanceOperation = false;

		if (isCancel)
		{
			OnShowPickupButton();
			PickupPanel.SetPickupData(in spawnTroopsInfo);
		}
		else
		{
			OnShowCardGroupButton();
		}

		void OnProcessOverrider_PointingAtSector(SectorObject pointing)
		{
			if (!awaitInstanceOperation) return;
			awaitInstanceOperation = false;
			StrategyElementFactory.Instantiate(pointing, in spawnTroopsInfo);
			isCancel = false;
		}
		void OnProcessOverrider_OnPressedEscapeKey()
		{
			if (!awaitInstanceOperation) return;
			awaitInstanceOperation = false;
			isCancel = true;
		}
	}


	private void SowCardGroupView(bool show)
	{
		if (CardPanel.IsNullRef()) return;
		if (CardPanel is IShowHide showHide)
		{
			if (show) showHide.OnShow();
			else showHide.OnHide();
		}
		else
		{
			CardPanel.gameObject.SetActive(show);
		}
	}
	private void ShowPickupView(bool show)
	{
		if (PickupPanel.IsNullRef()) return;
		if (PickupPanel is IShowHide showHide)
		{
			if (show) showHide.OnShow();
			else showHide.OnHide();
		}
		else
		{
			PickupPanel.gameObject.SetActive(show);
		}
	}
	private void ShowInstanceGuideView(bool show)
	{
		if (showInstanceOperationGuide.IsNullRef()) return;

		showInstanceOperationGuide.SetActive(show);
	}
}
