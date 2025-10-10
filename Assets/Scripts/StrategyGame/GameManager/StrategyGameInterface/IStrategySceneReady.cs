public interface IStrategySceneReady
{
	public float GetSceneReadyTimeout()
	{
		return 1f;
	}
	public bool IsSceneReady()
	{
		return true;
	}
}
