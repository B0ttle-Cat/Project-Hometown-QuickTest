using UnityEngine;

public class UnitDebugRender : MonoBehaviour
{
	MeshRenderer meshRenderer;
	MaterialPropertyBlock materialPropertyBlock;

	internal void SetColor(Color color)
	{
		if (meshRenderer == null)
		{
			TryGetComponent<MeshRenderer>(out meshRenderer);
		}

		if (meshRenderer == null) return;

		materialPropertyBlock ??= new MaterialPropertyBlock();

		meshRenderer.GetPropertyBlock(materialPropertyBlock);
		materialPropertyBlock.SetColor("_BaseColor", color);
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}
    private void OnDestroy()
    {
		meshRenderer = null;
		materialPropertyBlock = null;
	}
}
