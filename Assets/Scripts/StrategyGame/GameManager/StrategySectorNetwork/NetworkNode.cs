using Unity.Collections;

using UnityEngine;

public class NetworkNode : NetworkItem
{
	[SerializeField, ReadOnly]
	private string nodeName;
    [SerializeField, ReadOnly]
    private NetworkLine[] linkLines;
    public Transform SectorTr { get; private set; }
	public string NodeName { get => nodeName; private set => nodeName = value; }
    public NetworkLine[] LinkLines { get => linkLines; private set => linkLines = value; }

    public override void Setup(object nodeData)
	{
		if (nodeData is not SectorObject sector || sector == null) return;
		NodeName = sector.SectorName;
		var sectorTr = sector.transform;

		transform.position = sectorTr.position;
	}
	public void SetLink(NetworkLine[] linkLines)
	{
		LinkLines = linkLines;
	}
}
