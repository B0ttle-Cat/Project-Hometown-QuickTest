public partial class SectorObject : ISectorController
{
	public SectorObject This => this;
	public ISectorController Controller => this;

    public void On_SpawnTroops(in ISectorController.SpawnTroopsInfo spawnTroopsInfo)
    {
		StrategyElementUtility.Instantiate(in spawnTroopsInfo);
	}

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
    void ISectorController.OnControlButton_DeployUniqueUnit()
    {
    }

    void ISectorController.OnControlButton_SpawnTroops()
    {
		StrategyManager.GameUI.ControlPanelUI.OpenUI();
		var selecter = StrategyManager.GameUI.ControlPanelUI.ShowSpawnTroops();
		selecter.AddTarget(this);
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

    void ISectorController.OnHideUI_SelectUI()
    {
		StrategyManager.GameUI.ControlPanelUI.HideSectorSelectPanel();
	}

    void ISectorController.OnShowUI_DetailUI()
	{
		var gamePlayData = StrategyManager.GamePlayTempData;
		StrategyManager.GameUI.DetailsPanelUI.OpenUI();
		StrategyManager.GameUI.DetailsPanelUI.OnShowSectorDetail(This);
	}

    void ISectorController.OnShowUI_SelectUI()
    {
		StrategyManager.GameUI.ControlPanelUI.OpenUI();
		var selecter = StrategyManager.GameUI.ControlPanelUI.ShowSectorSelectPanel();
		selecter.AddTarget(this);
	}
}