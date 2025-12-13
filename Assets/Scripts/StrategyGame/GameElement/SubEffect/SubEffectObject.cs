using UnityEngine;

public class SubEffectObject : MonoBehaviour, IStrategyPoolingElement
{
    IStrategyElement IStrategyElement.ThisElement => this;
    GameObject IStrategyPoolingElement.PrefabReference { get; set; }
    int IStrategyElement.ID { get; set; }

    void IStrategyElement.InStrategyCollector()
    {
        InStrategyCollector();
	}

    void IStrategyStartGame.OnStartGame()
	{
        OnStartGame();
	}

    void IStrategyStartGame.OnStopGame()
    {
        OnStopGame();
	}

    void IStrategyElement.OutStrategyCollector()
    {
        OutStrategyCollector();
	}

    protected virtual void InStrategyCollector(){}
    protected virtual void OnStartGame(){}
    protected virtual void OnStopGame(){}
    protected virtual void OutStrategyCollector(){}
}
