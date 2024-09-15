using UnityEditor;
using UnityEngine;

namespace Davin.GUIs
{
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(BoundsOffset))]
	public class BoundsOffsetDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label = EditorGUI.BeginProperty(position, label, property);
			property.intValue = (int)(BoundsOffset)EditorGUI.EnumFlagsField(position, label, (BoundsOffset)property.intValue);
			EditorGUI.EndProperty();
		}
	}
#endif
}