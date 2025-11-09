using UnityEngine;

public class CameraVisibilityGroupInStrategy : CameraVisibilityGroup, IStrategyStartGame
{
	protected bool isPause = false;

	protected override void RefreshRenderers()
	{
		renderers.Clear();
		renderers.AddRange(GetComponentsInChildren<MeshRenderer>(true));
	}

	protected override void RefreshCamera()
	{
		targetCamera = StrategyManager.MainCamera;
	}

    protected override void LateUpdate()
    {
		if (isPause) return;

        base.LateUpdate();
    }

    void IStrategyStartGame.OnStartGame()
    {
		isPause = false;
		RefreshRenderers();
		RefreshCamera();
	}

    void IStrategyStartGame.OnStopGame()
    {
		isPause = true;
	}
}
