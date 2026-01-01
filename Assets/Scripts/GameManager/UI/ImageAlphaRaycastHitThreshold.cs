using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageAlphaRaycastHitThreshold : MonoBehaviour
{
	[InfoBox(@"레이캐스트 이벤트가 이미지에 ""Hit""로 간주되기 위해 픽셀이 가져야 하는 최소 알파 값을 지정합니다.

알파 값이 이 값보다 작으면 레이캐스트 이벤트가 이미지를 통과합니다.
이 값이 1이면 완전히 불투명한 픽셀에서만 레이캐스트 이벤트가 이미지에 등록됩니다.
테스트에 사용되는 알파 값은 이미지 스프라이트에서만 가져오며, 이미지 그래픽 색상의 알파 값은 무시됩니다.

기본값은 0입니다. 이미지 사각형 내의 모든 레이캐스트 이벤트는 ""Hit""로 간주됩니다.
0보다 큰 값을 사용하려면 이미지에서 사용하는 스프라이트의 픽셀을 읽을 수 있어야 합니다.
이를 위해서는 스프라이트의 고급 텍스처 가져오기 설정에서 읽기/쓰기를 활성화하고 스프라이트의 아틀라스 생성을 비활성화하면 됩니다.")]

	[Range(0f,1f)]
	[InfoBox("스프라이트의 'Read/Write' 옵션이 비활성화되어 있어 Alpha Hit Test가 작동하지 않습니다. 텍스처 설정에서 Read/Write Enabled를 체크하십시오.", InfoMessageType.Error, "IsReadWriteDisabled")]
	public float alphaRaycastHitThreshold;
#if UNITY_EDITOR
	public void OnValidate()
	{
		SetThreshold();
	}
	private bool IsReadWriteDisabled()
	{
		if (TryGetComponent<Image>(out var image))
		{
			if (image.sprite.IsNullRef()) return true;
			return !image.sprite.texture.isReadable;
		}
		return false;
	}
#endif
	public void Awake()
	{
		SetThreshold();
	}
	private void SetThreshold()
	{
		if (TryGetComponent<Image>(out var image))
		{
			image.alphaHitTestMinimumThreshold = alphaRaycastHitThreshold;
		}
#if !UNITY_EDITOR
		Destroy(this);
#endif
	}
}
