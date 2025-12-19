using static StrategyGamePlayData;

public partial interface ISectorController
{
	SectorObject This { get; }
	ISectorController Controller { get; }
	void OnChangeSupport_Defensive(float changeLevel);
	void OnChangeSupport_Facilities(float changeLevel);
	void OnChangeSupport_Offensive(float changeLevel);
	void OnChangeSupport_Supply(float changeLevel);
	bool OnConfirmButton_SpawnTroops(in SpawnTroopsInfo spawnTroopsInfo);
	void OnControlButton_ConstructFacilities();
	void OnControlButton_DeployUniqueUnit();
	void OnControlButton_SpawnOperation();
	void OnControlButton_UseFacilitiesSkill();
	void OnFacilitiesConstruct_Finish(int slotIndex, FacilityKey facilityKey);
	void OnFacilitiesConstruct_Start(int slotIndex, FacilityKey facilityKey);
    void OnShowUI_SelectUI();
    void OnShowUI_DetailUI();
    void OnHideUI_SelectUI();

}
