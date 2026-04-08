using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DucMinh
{
    [CustomPropertyDrawer(typeof(GameObjectReferenceData))]
    public class GameObjectReferenceDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Lấy các property
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty gameObjectProperty = property.FindPropertyRelative("gameObject");

            // Tính toán layout
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float fieldWidth = position.width / 2f;
            Rect labelRect = new Rect(position.x, position.y, fieldWidth, lineHeight);
            Rect nameRect = new Rect(position.x, position.y + lineHeight + 2, fieldWidth, lineHeight);
            Rect gameObjectRect = new Rect(position.x + fieldWidth, position.y + lineHeight + 2, fieldWidth, lineHeight);

            EditorGUI.LabelField(labelRect, label);

            if (EditorGUI.DropdownButton(nameRect, new GUIContent(nameProperty.stringValue), FocusType.Keyboard))
            {
                PopupWindow.Show(nameRect, new SearchPopupContent(nameProperty, nameRect.width));
            }

            EditorGUI.PropertyField(gameObjectRect, gameObjectProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        private class SearchPopupContent : PopupWindowContent
        {
            private string searchFilter = "";
            private Vector2 scrollPosition;
            private SerializedProperty nameProperty;
            private float width;

            public SearchPopupContent(SerializedProperty nameProp, float popupWidth)
            {
                nameProperty = nameProp;
                width = popupWidth;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(width, 300);
            }

            public override void OnGUI(Rect rect)
            {
                EditorGUI.BeginChangeCheck();
                searchFilter = EditorGUILayout.TextField(searchFilter);
                if (EditorGUI.EndChangeCheck())
                {
                    
                }

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                var filteredOptions = GameObjectReferenceData.NameObjectReferences
                    .Where(opt => string.IsNullOrEmpty(searchFilter) || opt.ToLower().Contains(searchFilter.ToLower()))
                    .OrderBy(n => n).ToList();

                foreach (string option in filteredOptions)
                {
                    if (GUILayout.Button(option))
                    {
                        nameProperty.stringValue = option;
                        nameProperty.serializedObject.ApplyModifiedProperties();
                        editorWindow.Close();
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}