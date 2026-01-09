using System;

using GameUI;

using static StrategyGamePlayData;

public class UnitPickupCardItemPanel : CardItemPanelComponent
{
	private PickupCardItemElementReferrer referrer;
	private PickupCardItemElementReferrer Referrer => referrer.IsNotNullRef() ? referrer : TryGetComponent<PickupCardItemElementReferrer>(out referrer) ? referrer : null;

	public UnitKey unitKey;
	public int pickupCount;

	public int costPersonnel;
	public int costMaterial;
	public int costElectric;

	public event Action OnChangeCount;

	protected override void OnAttachUI(ITargetToPanelAPI target)
	{
		if (target is not UnitProfileObject profile) return;
		unitKey = profile.unitKey;
		if (unitKey == UnitKey.None) return;

		referrer = Referrer;
		if (referrer.IsNullRef()) return;
		referrer.Init();

		referrer.SetTItleText(profile.displayName);
		referrer.SetTitleImage(profile.unitPortraitSprite);

		referrer.OnPlusButtonClick += Referrer_OnPlusButtonClick;
		referrer.OnMinusButtonClick += Referrer_OnMinusButtonClick;
		referrer.OnPointEnterInCard += Referrer_OnPointEnterInCard;
		referrer.OnPointExitInCard += Referrer_OnPointExitInCard;

		pickupCount = 0;
		costPersonnel = profile.stats.DeploymentCostPersonnel;
		costMaterial = profile.stats.DeploymentCostMaterial;
		costElectric = profile.stats.DeploymentCostElectric;
		referrer.SetCostText(costPersonnel, costMaterial, costElectric);
	}
	protected override void OnReleaseUI()
	{
		if (referrer.IsNotNullRef())
		{
			referrer.OnPlusButtonClick -= Referrer_OnPlusButtonClick;
			referrer.OnMinusButtonClick -= Referrer_OnMinusButtonClick;
			referrer.OnPointEnterInCard -= Referrer_OnPointEnterInCard;
			referrer.OnPointExitInCard -= Referrer_OnPointExitInCard;

			referrer.Deinit();
			referrer = null;
		}

		OnChangeCount = null;
	}
	public void SetData(int pickupCount)
	{
		this.pickupCount = pickupCount;
		referrer.SetCountText(pickupCount);
		referrer.OnShowCountRect(pickupCount > 0);
	}
	private void Referrer_OnPlusButtonClick()
	{
		if (referrer.IsNullRef()) return;

		pickupCount++;

		referrer.SetCountText(pickupCount);
		OnChangeCount?.Invoke();
	}

	private void Referrer_OnMinusButtonClick()
	{
		if (referrer.IsNullRef()) return;

		pickupCount--;
		if (pickupCount < 0)
		{
			pickupCount = 0;
			return;
		}

		referrer.SetCountText(pickupCount);
		OnChangeCount?.Invoke();
	}
	private void Referrer_OnPointEnterInCard(UnityEngine.EventSystems.BaseEventData arg0)
	{
		if (referrer.IsNullRef()) return;

		referrer.SetCountText(pickupCount);
		referrer.OnShowCountRect(true);
	}
	private void Referrer_OnPointExitInCard(UnityEngine.EventSystems.BaseEventData arg0)
	{
		if (referrer.IsNullRef()) return;

		referrer.OnShowCountRect(pickupCount > 0);
	}

	protected override void OnUpdateUI()
	{
	}
}
