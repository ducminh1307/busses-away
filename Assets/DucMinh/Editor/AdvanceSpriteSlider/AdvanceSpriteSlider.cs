using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.AdvanceSpriteSlider
{
    [System.Serializable]
    public class SlicerPreset
    {
        public Vector2Int cellSize;
        public Vector2Int padding;
        public Vector2Int offset;
        public Vector2Int cellCount;
        public int sliceMode;
    }

    public partial class AdvanceSpriteSlider : EditorWindow
    {
        private const string SaveFolderPrefsKey = "DucMinh.AdvanceSpriteSlider.SaveFolder";

        [SerializeField] private Texture2D _texture;
        [SerializeField] private List<Rect> _slices = new List<Rect>();
        [SerializeField] private List<string> _sliceNames = new List<string>();
        [SerializeField] private List<Vector4> _sliceBorders = new List<Vector4>();
        [SerializeField] private int _selectedSliceIndex = -1;
        [SerializeField] private List<int> _selectedSliceIndices = new List<int>();
        [SerializeField] private string _namingPrefix = "Sprite_";

        private enum SliceMode { Manual, Auto, GridByCellSize, GridByCellCount }
        [SerializeField] private SliceMode _sliceMode = SliceMode.Manual;
        private enum InteractionMode { Select, Edit }
        [SerializeField] private InteractionMode _interactionMode = InteractionMode.Edit;

        [SerializeField] private Vector2Int _cellSize = new Vector2Int(64, 64);
        [SerializeField] private Vector2Int _padding = Vector2Int.zero;
        [SerializeField] private Vector2Int _offset = Vector2Int.zero;
        [SerializeField] private Vector2Int _cellCount = new Vector2Int(4, 4);
        [SerializeField] private SpriteAlignment _pivot = SpriteAlignment.Center;
        [SerializeField] private Vector2 _customPivot = new Vector2(0.5f, 0.5f);
        [SerializeField] private Vector4 _border = Vector4.zero;
        [SerializeField] private string _saveFolder = "Assets";

        private ScrollView _settingsContainer;
        private IMGUIContainer _previewContainer;

        private Vector2 _scrollPos;
        private float _zoom = 1f;

        private enum ResizeHandle
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
            BottomLeft,
            TopLeft,
            TopRight,
            BottomRight
        }

        private bool _isDragging;
        private bool _isResizing;
        private bool _isSelectingArea;
        private bool _selectionAdditive;
        private bool _selectionMoved;
        private int _selectionClickIndex = -1;
        private ResizeHandle _resizeHandle = ResizeHandle.None;
        private Vector2 _dragStartPos;
        private Rect _dragStartRect;
        private Vector2 _selectionStartPos;
        private Rect _selectionScreenRect;
        private int _interactionUndoGroup = -1;
        private bool _interactionUndoActive;
        private const float HANDLE_SIZE = 8f;

        private bool _isRebuildScheduled;
        private Texture2D _checkerTex;

        private readonly List<Rect> _clipboardSlices = new List<Rect>();
        private readonly List<string> _clipboardNames = new List<string>();
        private readonly List<Vector4> _clipboardBorders = new List<Vector4>();

        private RectInt? _rectClipboard;
        private Vector4? _borderClipboard;

        [MenuItem("DucMinh/Advance Sprite Slider")]
        public static void ShowExample()
        {
            var wnd = GetWindow<AdvanceSpriteSlider>();
            wnd.titleContent = new GUIContent("Advanced Sprite Slicer");
            wnd.minSize = new Vector2(800, 500);
        }

        private void OnDestroy()
        {
            if (_checkerTex != null)
            {
                DestroyImmediate(_checkerTex);
            }
        }

        private void OnEnable()
        {
            LoadSaveFolderPreference();
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            EndInteractionUndo();
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            ScheduleBuildSettingsUI();
            if (_previewContainer != null) _previewContainer.MarkDirtyRepaint();
        }

        private void RecordUndo(string actionName)
        {
            Undo.RecordObject(this, actionName);
            EditorUtility.SetDirty(this);
        }

        private void BeginInteractionUndo(string actionName)
        {
            if (_interactionUndoActive) return;
            Undo.IncrementCurrentGroup();
            _interactionUndoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(actionName);
            Undo.RegisterCompleteObjectUndo(this, actionName);
            _interactionUndoActive = true;
        }

        private void EndInteractionUndo()
        {
            if (!_interactionUndoActive) return;
            EditorUtility.SetDirty(this);
            Undo.CollapseUndoOperations(_interactionUndoGroup);
            _interactionUndoGroup = -1;
            _interactionUndoActive = false;
        }
    }
}
