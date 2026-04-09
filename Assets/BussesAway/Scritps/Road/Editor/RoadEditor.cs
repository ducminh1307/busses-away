#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BussesAway
{
    /// <summary>
    /// Custom Inspector cho Road component.
    /// Cung cấp các nút:
    ///   • Auto-Detect (Road này)    – phát hiện neighbor của 1 tile đang chọn
    ///   • Auto-Link ALL Roads        – phát hiện neighbor cho TOÀN BỘ Road trong scene
    ///   • Clear Connections          – xoá connections của Road đang chọn
    /// </summary>
    [CustomEditor(typeof(Road))]
    public class RoadEditor : Editor
    {
        // ── Foldout state ──────────────────────────────────────────────────────
        private bool _showDefaultInspector = true;

        public override void OnInspectorGUI()
        {
            var road = (Road)target;

            // ── Nút Auto-Detect (1 road) ───────────────────────────────────────
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button("⚡  Auto-Detect (Road này)", GUILayout.Height(30)))
                {
                    Undo.RecordObject(road, "Auto-Detect Road Connections");
                    road.AutoDetectConnections();
                    EditorUtility.SetDirty(road);
                }
                GUI.backgroundColor = Color.white;
            }

            // ── Nút Auto-Link ALL ─────────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
                if (GUILayout.Button("🔗  Auto-Link ALL Roads in Scene", GUILayout.Height(30)))
                {
                    AutoLinkAllRoads();
                }
                GUI.backgroundColor = Color.white;
            }

            // ── Nút Clear ─────────────────────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("✕  Clear Connections (Road này)", GUILayout.Height(22)))
                {
                    Undo.RecordObject(road, "Clear Road Connections");
                    road.AutoDetectConnections(); // gọi với connections clear bên trong
                    // Thực ra chỉ cần clear – dùng SerializedProperty
                    serializedObject.Update();
                    serializedObject.FindProperty("connections").ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(6);

            // ── Summary ───────────────────────────────────────────────────────
            var conns = road.Connections;
            var style = new GUIStyle(EditorStyles.helpBox) { richText = true };
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>Connections ({conns.Count}/4)</b>");
            if (conns.Count == 0)
            {
                sb.Append("  <i>Chưa có connection. Bấm Auto-Detect để tự động tìm.</i>");
            }
            else
            {
                foreach (var c in conns)
                {
                    var neighborName = c.neighbor != null ? c.neighbor.name : "<color=red>NULL</color>";
                    sb.AppendLine($"  <b>{c.direction}</b> → {neighborName}");
                }
            }
            EditorGUILayout.LabelField(sb.ToString(), style);

            EditorGUILayout.Space(4);

            // ── Default Inspector (có thể ẩn/hiện) ───────────────────────────
            _showDefaultInspector = EditorGUILayout.Foldout(_showDefaultInspector, "Inspector chi tiết", true);
            if (_showDefaultInspector)
            {
                DrawDefaultInspector();
            }
        }

        // ── Static helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Tìm tất cả Road trong scene (kể cả inactive), gọi AutoDetectConnections()
        /// cho từng cái, hỗ trợ Undo toàn bộ.
        /// </summary>
        [MenuItem("BussesAway/Road/Auto-Link ALL Roads in Scene")]
        public static void AutoLinkAllRoads()
        {
            var allRoads = FindObjectsByType<Road>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (allRoads.Length == 0)
            {
                EditorUtility.DisplayDialog("Auto-Link Roads", "Không tìm thấy Road nào trong scene.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Auto-Link ALL Roads");
            var group = Undo.GetCurrentGroup();

            var linked = 0;
            foreach (var road in allRoads)
            {
                Undo.RecordObject(road, "Auto-Link Road");
                road.AutoDetectConnections();
                EditorUtility.SetDirty(road);
                linked++;
            }

            Undo.CollapseUndoOperations(group);

            Debug.Log($"[RoadEditor] Auto-Link hoàn tất: {linked} Roads đã được cập nhật connections.");
            EditorUtility.DisplayDialog(
                "Auto-Link Roads",
                $"Đã tự động link {linked} Roads!\n\nKiểm tra Gizmos trong Scene View để xác nhận.",
                "OK");
        }
    }
}
#endif
