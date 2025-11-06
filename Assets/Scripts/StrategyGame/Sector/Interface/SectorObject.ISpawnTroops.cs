public interface ISpawnTroops
{
	bool IsSpawnTroops { get; }
	void OnSpawnTroops();
	void OnSpawnUniqueUnit();
}
public partial class SectorObject : ISpawnTroops
{
	public bool IsSpawnTroops
	{
		get
		{
			if (CaptureFaction == null) return false;
			return true;
		}
	}

	void ISpawnTroops.OnSpawnTroops()
	{
	}

	void ISpawnTroops.OnSpawnUniqueUnit()
	{
	}
}
