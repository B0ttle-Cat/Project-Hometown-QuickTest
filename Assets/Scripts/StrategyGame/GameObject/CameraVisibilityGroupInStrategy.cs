using UnityEngine;

public class CameraVisibilityGroupInStrategy : CameraVisibilityGroup, IStrategyStartGame
{
	protected Component visibleTarget;
	protected bool isPause = false;

	protected override void RefreshRenderers()
	{
		renderers.AddRange(GetComponentsInChildren<MeshRenderer>(true));
	}

	protected override void RefreshCamera()
	{
		targetCamera = StrategyManager.MainCamera;
	}

    protected override void VisibilityUpdate()
    {
		if (isPause) return;
        base.VisibilityUpdate();
    }

    void IStrategyStartGame.OnStartGame()
    {
		isPause = false;
		OnRefreshRenderers();
		OnRefreshCamera();
		VisibilityUpdate();
	}

    void IStrategyStartGame.OnStopGame()
    {
		isPause = true;
	}

	protected override Component GetVisibleTarget()
	{
		if(visibleTarget == null)
		{ 
			var element = GetComponentInParent<IStrategyElement>();
			if(element is Component component)
			{
				visibleTarget = component;
			}
		}
		return visibleTarget;
	}

}
