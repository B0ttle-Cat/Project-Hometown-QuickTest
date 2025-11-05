public interface ISectorController
{
	public SectorObject This { get; }
	public void OnChangeSupport_Defensive(float changeLevel);
	public void OnChangeSupport_Facilities(float changeLevel);
	public void OnChangeSupport_Offensive(float changeLevel);
	public void OnChangeSupport_Supply(float changeLevel);
	public void OnControlButton_ConstructFacilities();
	public void OnControlButton_DeployCombatants();
	public void OnControlButton_MoveTroops();
	public void OnControlButton_UseFacilitiesSkill();
	public void OnFacilitiesConstruct_Finish(int slotIndex, string facilitiesKey);
	public void OnFacilitiesConstruct_Start(int slotIndex, string facilitiesKey);
	public void OnShowUI_Detail();
}
public partial class SectorObject : ISectorController
{
	public SectorObject This => this;
	public ISectorController Controller => this;

	void ISectorController.OnChangeSupport_Defensive(float changeLevel)
	{
	}
	void ISectorController.OnChangeSupport_Facilities(float changeLevel)
	{
	}
	void ISectorController.OnChangeSupport_Offensive(float changeLevel)
	{
	}
	void ISectorController.OnChangeSupport_Supply(float changeLevel)
	{
	}
	void ISectorController.OnControlButton_ConstructFacilities()
	{
	}
	void ISectorController.OnControlButton_DeployCombatants()
	{
	}
	void ISectorController.OnControlButton_MoveTroops()
	{
	}
	void ISectorController.OnControlButton_UseFacilitiesSkill()
	{
	}
	void ISectorController.OnFacilitiesConstruct_Finish(int slotIndex, string facilitiesKey)
	{
		var data = FacilitiesData;
		var slot = data.slotData[slotIndex];
		var constructing = slot.constructing;
		constructing.Clear();

		slot.constructing = constructing;
		data.slotData[slotIndex] = slot;

		Facilities.SetData(data);
	}
	void ISectorController.OnFacilitiesConstruct_Start(int slotIndex, string facilitiesKey)
	{
		var data = FacilitiesData;
		var slot = data.slotData[slotIndex];
		var constructing = slot.constructing;

		constructing.facilitiesKey = facilitiesKey;
		constructing.constructTime = 10; // facilitiesKey 를 통해 올바른 값을 가져온다.
		constructing.duration = constructing.constructTime;

		slot.constructing = constructing;
		data.slotData[slotIndex] = slot;

		Facilities.SetData(data);
	}
	void ISectorController.OnShowUI_Detail()
	{
		var gamePlayData = StrategyManager.GamePlayTempData;
		StrategyManager.GameUI.DetailsPanelUI.OpenUI();
		StrategyManager.GameUI.DetailsPanelUI.OnShowSectorDetail(This);
	}
}