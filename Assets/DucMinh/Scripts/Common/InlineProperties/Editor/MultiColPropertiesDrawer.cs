#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DucMinh
{
    public abstract class MultiColPropertyDrawerBase : PropertyDrawer
    {
        private readonly int _columns;

        private const float ColSpacing = 4f;
        private const float VerticalPad = 2f;

        protected MultiColPropertyDrawerBase(int columns)
        {
            _columns = Mathf.Max(1, columns);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float mini = EditorGUIUtility.singleLineHeight * 0.8f;
            float field = EditorGUIUtility.singleLineHeight;
            return mini + field + VerticalPad * 3f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.IndentedRect(position);

            // Chừa phần prefix cho label tổng
            // var contentRect = EditorGUI.PrefixLabel(position, label);
            var contentRect = position;

            var children = GetChildren(property);
            if (children.Count == 0)
            {
                EditorGUI.HelpBox(contentRect, "No serialized fields", MessageType.None);
                return;
            }

            var drawProps = children.Take(_columns).ToList();

            var (labels, tooltips) = GetInlineLabels(property, drawProps);

            float miniH = EditorGUIUtility.singleLineHeight * 0.8f;
            float fieldH = EditorGUIUtility.singleLineHeight;

            float totalSpacing = ColSpacing * (drawProps.Count - 1);
            float colWidth = (contentRect.width - totalSpacing) / drawProps.Count;

            var miniRow = new Rect(contentRect.x, contentRect.y + VerticalPad, colWidth, miniH);
            var fieldRow = new Rect(contentRect.x, miniRow.yMax + VerticalPad, colWidth, fieldH);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            for (int i = 0; i < drawProps.Count; i++)
            {
                var r = miniRow;
                r.x += i * (colWidth + ColSpacing);
                var lc = new GUIContent(labels[i], tooltips[i]);
                EditorGUI.LabelField(r, lc, EditorStyles.miniLabel);
            }

            for (int i = 0; i < drawProps.Count; i++)
            {
                var r = fieldRow;
                r.x += i * (colWidth + ColSpacing);
                EditorGUI.PropertyField(r, drawProps[i], GUIContent.none, includeChildren: false);
            }

            EditorGUI.indentLevel = oldIndent;
        }

        private List<SerializedProperty> GetChildren(SerializedProperty property)
        {
            var list = new List<SerializedProperty>();

            Type fieldType = GetFieldTypeOfProperty();
            string[] order = null;

            if (fieldType != null)
            {
                var orderAttr = fieldType.GetCustomAttributes(typeof(InlineOrderAttribute), true)
                                         .FirstOrDefault() as InlineOrderAttribute;
                order = orderAttr?.FieldNames;
            }

            var iterator = property.Copy();
            var end = iterator.GetEndProperty();

            if (!iterator.NextVisible(true))
                return list;

            int depth = iterator.depth;
            var allChildren = new List<SerializedProperty>();

            do
            {
                if (SerializedProperty.EqualContents(iterator, end)) break;
                if (iterator.depth == depth && iterator.propertyPath.StartsWith(property.propertyPath))
                {
                    if (iterator.name != "m_Script")
                        allChildren.Add(iterator.Copy());
                }
            }
            while (iterator.NextVisible(false));

            if (order != null && order.Length > 0)
            {
                var dict = allChildren.ToDictionary(p => p.name, p => p);
                foreach (var name in order)
                {
                    if (dict.TryGetValue(name, out var sp))
                    {
                        list.Add(sp);
                        dict.Remove(name);
                    }
                }
                foreach (var sp in allChildren)
                    if (!list.Contains(sp)) list.Add(sp);
            }
            else list = allChildren;

            return list;
        }

        private (string[] labels, string[] tooltips) GetInlineLabels(SerializedProperty property, List<SerializedProperty> drawProps)
        {
            Type fieldType = GetFieldTypeOfProperty();
            string[] lbls = null;

            if (fieldType != null)
            {
                var lblAttr = fieldType.GetCustomAttributes(typeof(InlineLabelsAttribute), true)
                                       .FirstOrDefault() as InlineLabelsAttribute;
                lbls = lblAttr?.Labels;
            }

            var labels = new string[drawProps.Count];
            var tips = new string[drawProps.Count];

            for (int i = 0; i < drawProps.Count; i++)
            {
                var sp = drawProps[i];
                string fallback = ObjectNames.NicifyVariableName(sp.name);
                labels[i] = (lbls != null && i < lbls.Length && !string.IsNullOrEmpty(lbls[i])) ? lbls[i] : fallback;
                tips[i] = sp.tooltip;
            }

            return (labels, tips);
        }

        private Type GetFieldTypeOfProperty()
        {
            return fieldInfo != null ? fieldInfo.FieldType : null;
        }
    }

    [CustomPropertyDrawer(typeof(TwoProperties),  true)]
    public class TwoPropertiesDrawer  : MultiColPropertyDrawerBase { public TwoPropertiesDrawer()  : base(2) { } }

    [CustomPropertyDrawer(typeof(ThreeProperties), true)]
    public class ThreePropertiesDrawer : MultiColPropertyDrawerBase { public ThreePropertiesDrawer() : base(3) { } }

    [CustomPropertyDrawer(typeof(FourProperties),  true)]
    public class FourPropertiesDrawer  : MultiColPropertyDrawerBase { public FourPropertiesDrawer()  : base(4) { } }
}
#endif
