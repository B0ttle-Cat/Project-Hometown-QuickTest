using static StrategyGamePlayData;

public interface ISectorController
{
	SectorObject This { get; }
	void On_SpawnTroops(in SpawnTroopsInfo spawnTroopsInfo);
	public readonly struct SpawnTroopsInfo
	{
		public readonly int factionID;
		public readonly (UnitKey key, int count)[] organizations;
		public SpawnTroopsInfo(int factionID, params (UnitKey, int)[] organizations)
		{
			this.factionID = factionID;
			this.organizations = organizations;
		}
	}
	void OnChangeSupport_Defensive(float changeLevel);
	void OnChangeSupport_Facilities(float changeLevel);
	void OnChangeSupport_Offensive(float changeLevel);
	void OnChangeSupport_Supply(float changeLevel);
	void OnControlButton_ConstructFacilities();
	void OnControlButton_DeployUniqueUnit();
	void OnControlButton_SpawnOperation();
	void OnControlButton_UseFacilitiesSkill();
	void OnFacilitiesConstruct_Finish(int slotIndex, string facilitiesKey);
	void OnFacilitiesConstruct_Start(int slotIndex, string facilitiesKey);
    void OnShowUI_SelectUI();
    void OnShowUI_DetailUI();
    void OnHideUI_SelectUI();

}
