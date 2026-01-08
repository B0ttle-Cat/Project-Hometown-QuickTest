using System.Collections.Generic;
using System.Linq;

using static StrategyGamePlayData;

public readonly struct SpawnTroopsInfo
{
	public readonly int factionID;
	public readonly (UnitKey key, int count)[] organizations;

	public readonly int totalCount;
	public readonly int costPersonnel;
	public readonly int costMaterial;
	public readonly int costElectric;
	public SpawnTroopsInfo(int factionID, (UnitKey, int)[] organizations, int costPersonnel = 0, int costMaterial = 0, int costElectric = 0)
	{
		this.factionID = factionID;
		totalCount = 0;

		this.organizations = organizations;
		int length = organizations == null ? 0 :organizations.Length;
		for (int i = 0 ; i < length ; i++)
        {
			totalCount += organizations[i].Item2;
		}

		this.costPersonnel = costPersonnel;
		this.costMaterial = costMaterial;
		this.costElectric = costElectric;

    }
	public SpawnTroopsInfo(int factionID, KeyValuePair<UnitKey, int>[] keyValue, int costPersonnel = 0, int costMaterial = 0, int costElectric = 0)
	{
		this.factionID = factionID;
		totalCount = 0;

		int  length = keyValue == null ? 0 : keyValue.Length;
		this.organizations = new (UnitKey key, int count)[length];
        for (int i = 0 ; i < length ; i++)
		{
			totalCount += keyValue[i].Value;
			this.organizations[i] = (keyValue[i].Key, keyValue[i].Value);
		}

		this.costPersonnel = costPersonnel;
		this.costMaterial = costMaterial;
		this.costElectric = costElectric;
	}
	public SpawnTroopsInfo(int factionID, Dictionary<UnitKey, int> dictionary, int costPersonnel = 0, int costMaterial = 0, int costElectric = 0)
	{
		this.factionID = factionID;
		totalCount = 0;

		var keyValue =  dictionary == null ? null : dictionary.ToArray();
		int  length = keyValue == null ? 0 : keyValue.Length;
		this.organizations = new (UnitKey key, int count)[length];
		for (int i = 0 ; i < length ; i++)
		{
			totalCount += keyValue[i].Value;
			this.organizations[i] = (keyValue[i].Key, keyValue[i].Value);
		}

		this.costPersonnel = costPersonnel;
		this.costMaterial = costMaterial;
		this.costElectric = costElectric;
	}

}
