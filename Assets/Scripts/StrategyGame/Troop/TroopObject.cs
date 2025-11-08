using UnityEngine;

public class TroopObject : MonoBehaviour, IStrategyElement
{
	bool IStrategyElement.IsInCollector { get; set; }

	void IStrategyElement.InStrategyCollector()
	{
	}
	void IStrategyElement.OutStrategyCollector()
	{
	}
	void IStartGame.OnStartGame()
	{
	}
	void IStartGame.OnStopGame()
	{
	}
}
