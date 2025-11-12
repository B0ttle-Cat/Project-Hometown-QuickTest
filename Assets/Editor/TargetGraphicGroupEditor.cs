#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(TargetGraphicGroup))]
[CanEditMultipleObjects]
public class TargetGraphicGroupOdinEditor : OdinEditor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Graphics", EditorStyles.boldLabel);

		// Auto Collect 버튼은 항상
		if (GUILayout.Button("Auto Collect Children"))
		{
			foreach (var t in targets)
			{
				var group = t as TargetGraphicGroup;
				var method = typeof(TargetGraphicGroup).GetMethod("AutoCollectChildren",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				method?.Invoke(group, null);
			}
		}

		// 단일 선택일 때만 나머지 UI 표시
		if (targets.Length == 1)
		{
			var group = targets[0] as TargetGraphicGroup;
			var graphicsProp = serializedObject.FindProperty("graphics");

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("+"))
				graphicsProp.arraySize++;
			if (GUILayout.Button("-") && graphicsProp.arraySize > 0)
				graphicsProp.DeleteArrayElementAtIndex(graphicsProp.arraySize - 1);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			// 리스트 렌더링
			for (int i = 0 ; i < graphicsProp.arraySize ; i++)
			{
				var element = graphicsProp.GetArrayElementAtIndex(i);
				var graphicProp = element.FindPropertyRelative("graphic");
				var enableProp = element.FindPropertyRelative("enableTransition");

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(graphicProp, GUIContent.none);
				enableProp.boolValue = EditorGUILayout.ToggleLeft("Transition", enableProp.boolValue, GUILayout.Width(100));
				EditorGUILayout.EndHorizontal();
			}
		}

		serializedObject.ApplyModifiedProperties();
	}
}
#endif
