using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

using static StrategyStartSetterData;

public class StrategySectorNetwork : MonoBehaviour, IStartGame
{
	[SerializeField, ReadOnly]
	private NetworkNode sampleNode;
	[SerializeField, ReadOnly]
	private NetworkLine sampleLine;

	[SerializeField, ReadOnly]
	private List<NetworkNode> networkNodes = new List<NetworkNode> ();
	[SerializeField, ReadOnly]
	private List<NetworkLine> networkLines = new List<NetworkLine> ();

	public async Awaitable Init(List<SectorObject> sectorList, StrategyStartSetterData.NetworkData[] networkDatas)
    {
		if(sampleNode == null || sampleLine == null)
		{
			Debug.LogError("StrategySectorNetwork: Sample Node or Line is not assigned.");
			return;
		}

		sampleNode.gameObject.SetActive(false);
		sampleLine.gameObject.SetActive(false);

		Transform parent = transform;
		NetworkNode[] newNodes = await GameObject.InstantiateAsync(sampleNode, sectorList.Count, parent);
		NetworkLine[] newlines = await GameObject.InstantiateAsync(sampleLine, networkDatas.Length, parent);

		int length = newNodes.Length;
		networkNodes = new(length);
		for (int i = 0 ; i < length ; i++)
		{
            NetworkNode node = newNodes[i];
            node.Setup(sectorList[i]);
			networkNodes.Add(node);
			node.gameObject.SetActive(true);
		}
		length = networkDatas.Length;
		networkLines = new(length);
		for (int i = 0 ; i < length ; i++)
		{
			var line = newlines[i];
			line.Setup(networkDatas[i]);
			networkLines.Add(line);
			line.gameObject.SetActive(true);
		}

		length = networkLines.Count;
        for (int i = 0 ; i < length ; i++)
        {
			var node = networkNodes[i];
			var nodeName = node.NodeName;
			List<NetworkLine> linkList = new List<NetworkLine>();
			int lineLength = networkLines.Count;
			for (int ii = 0 ; ii < length ; ii++)
            {
				NetworkLine line = networkLines[ii];
				if(line.NodeNameA == nodeName || line.NodeNameB == nodeName)
				{
					linkList.Add(line);
				}
			}			   
			node.SetLink(linkList.ToArray());
        }
    }
    void IStartGame.OnStartGame()
	{

	}
    void IStartGame.OnStopGame()
	{
		if(networkNodes != null)
		{
			int length = networkNodes.Count;
			for (int i = 0 ; i < length ; i++)
            {
				if(networkNodes[i] != null)
				{
					GameObject.Destroy(networkNodes[i].gameObject);
				}
			}
            networkNodes.Clear();
			networkNodes = null;
		}
		if (networkLines != null)
		{
			int length = networkLines.Count;
			for (int i = 0 ; i < length ; i++)
			{
				if (networkLines[i] != null)
				{
					GameObject.Destroy(networkLines[i].gameObject);
				}
			}
			networkLines.Clear();
			networkLines = null;
		}
	}


	public enum SerchMode
	{
		ForwardOnly,
		Both
	}


	private IEnumerable<NetworkLine> GetNextLine(NetworkNode current, SerchMode serchMode)
	{
		string nodeName = current.NodeName;
		NetworkLine[] list = current.LinkLines;
		return list.Where(line =>
		{
			var connectDir = line.ConnectDir;
            return connectDir switch
            {
                NetworkData.ConnectDir.Both => true,
				NetworkData.ConnectDir.AtoB =>
					serchMode == SerchMode.Both || serchMode == SerchMode.ForwardOnly && nodeName.Equals(line.NodeNameA),
                NetworkData.ConnectDir.BtoA => 
					serchMode == SerchMode.Both || serchMode == SerchMode.ForwardOnly && nodeName.Equals(line.NodeNameB),
                _ => false
            };
		} );
	}
}
