using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh.UIToolkit;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            var leftPanel = root.Create<VisualElement>("LeftPanel")
                .SetWidth(320)
                .SetPadding(10);
            leftPanel.style.borderRightWidth = 1;
            leftPanel.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f);

            _settingsContainer = leftPanel.CreateScrollView();

            var rightPanel = root.Create<VisualElement>("RightPanel").SetFlex(1);
            rightPanel.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

            var toolbar = rightPanel.CreateToolbar();
            toolbar.CreateToolbarButton("Center View", () =>
            {
                _zoom = 1f;
                var layout = _previewContainer.layout;
                _scrollPos = new Vector2(layout.x, layout.y);
                _previewContainer.MarkDirtyRepaint();
            });
            toolbar.CreateToolbarButton("Zoom In", () => { _zoom *= 1.25f; _previewContainer.MarkDirtyRepaint(); });
            toolbar.CreateToolbarButton("Zoom Out", () => { _zoom *= 0.8f; _previewContainer.MarkDirtyRepaint(); });
            toolbar.CreateToolbarButton("Save Preset", SavePreset);
            toolbar.CreateToolbarButton("Load Preset", LoadPreset);

            _previewContainer = new IMGUIContainer(OnPreviewGUI) { style = { flexGrow = 1, overflow = Overflow.Hidden } };
            _previewContainer.RegisterCallback<WheelEvent>(OnScrollWheel);
            rightPanel.Add(_previewContainer);

            BuildSettingsUI();
        }

        private void ScheduleBuildSettingsUI()
        {
            if (_isRebuildScheduled) return;
            _isRebuildScheduled = true;
            rootVisualElement.schedule.Execute(() =>
            {
                _isRebuildScheduled = false;
                BuildSettingsUI();
            });
        }

        private void BuildSettingsUI()
        {
            if (_settingsContainer == null) return;
            _settingsContainer.Clear();

            _settingsContainer.CreateLabel("Advanced Sprite Slicer").SetFontSize(16).style.unityFontStyleAndWeight = FontStyle.Bold;
            _settingsContainer.CreateDivider();

            _settingsContainer.CreateObjectField("Texture", typeof(Texture2D), false, _texture, obj =>
            {
                RecordUndo("Change Texture");
                _texture = obj as Texture2D;
                LoadExistingSlices();
                ScheduleBuildSettingsUI();
                _previewContainer.MarkDirtyRepaint();
            });

            _settingsContainer.CreateEnumField("Interaction Mode", _interactionMode, val =>
            {
                RecordUndo("Change Interaction Mode");
                _interactionMode = (InteractionMode)val;
                if (_selectedSliceIndex >= 0)
                {
                    if (_interactionMode == InteractionMode.Edit)
                    {
                        _selectedSliceIndices.Clear();
                        _selectedSliceIndices.Add(_selectedSliceIndex);
                    }
                    else if (!_selectedSliceIndices.Contains(_selectedSliceIndex))
                    {
                        _selectedSliceIndices.Add(_selectedSliceIndex);
                    }
                }
                ScheduleBuildSettingsUI();
                _previewContainer.MarkDirtyRepaint();
            });

            if (_texture == null)
            {
                _settingsContainer.CreateHelpBox("Please select a Texture2D to begin.", HelpBoxMessageType.Warning);
                return;
            }

            _settingsContainer.CreateDivider();
            _settingsContainer.CreateLabel("Slicing Setup").style.unityFontStyleAndWeight = FontStyle.Bold;

            _settingsContainer.CreateEnumField("Slicing Mode", _sliceMode, val =>
            {
                RecordUndo("Change Slicing Mode");
                _sliceMode = (SliceMode)val;
                ScheduleBuildSettingsUI();
                _previewContainer.MarkDirtyRepaint();
            });

            if (_sliceMode == SliceMode.GridByCellSize)
            {
                _settingsContainer.CreateVector2IntField("Cell Size", _cellSize, v =>
                {
                    RecordUndo("Change Cell Size");
                    _cellSize = v;
                    _previewContainer.MarkDirtyRepaint();
                });
                _settingsContainer.CreateVector2IntField("Padding", _padding, v =>
                {
                    RecordUndo("Change Padding");
                    _padding = v;
                    _previewContainer.MarkDirtyRepaint();
                });
                _settingsContainer.CreateVector2IntField("Offset", _offset, v =>
                {
                    RecordUndo("Change Offset");
                    _offset = v;
                    _previewContainer.MarkDirtyRepaint();
                });
            }
            else if (_sliceMode == SliceMode.GridByCellCount)
            {
                _settingsContainer.CreateVector2IntField("Cell Count", _cellCount, v =>
                {
                    RecordUndo("Change Cell Count");
                    _cellCount = v;
                    _previewContainer.MarkDirtyRepaint();
                });
            }

            var btnRow1 = _settingsContainer.CreateRow();
            btnRow1.style.marginTop = 10;
            btnRow1.CreateButton("Slice", GenerateSlices).SetFlex(1).style.backgroundColor = new Color(0.2f, 0.5f, 0.2f);
            btnRow1.CreateButton("Clear", ClearSlices).SetFlex(1);

            _settingsContainer.CreateDivider();
            _settingsContainer.CreateLabel("Export Settings").style.unityFontStyleAndWeight = FontStyle.Bold;

            var saveFolderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_saveFolder);
            _settingsContainer.CreateObjectField("Save Folder", typeof(DefaultAsset), false, saveFolderObj, obj =>
            {
                RecordUndo("Change Save Folder");
                if (TrySetSaveFolder(obj))
                {
                    ScheduleBuildSettingsUI();
                }
            });

            _settingsContainer.CreateDivider();
            _settingsContainer.CreateLabel("Sprite Settings").style.unityFontStyleAndWeight = FontStyle.Bold;

            var namingRow = _settingsContainer.CreateRow();
            namingRow.CreateTextField("Naming Prefix", _namingPrefix, v =>
            {
                RecordUndo("Change Naming Prefix");
                _namingPrefix = v;
            }).SetFlex(1);
            namingRow.CreateButton("Rename", () =>
            {
                RecordUndo("Rename Slice Names");
                RenameSlicesByPrefix();
                ScheduleBuildSettingsUI();
                _previewContainer.MarkDirtyRepaint();
            });

            _settingsContainer.CreateEnumField("Pivot", _pivot, val =>
            {
                RecordUndo("Change Pivot");
                _pivot = (SpriteAlignment)val;
                if (_pivot == SpriteAlignment.Custom) ScheduleBuildSettingsUI();
            });

            if (_pivot == SpriteAlignment.Custom)
            {
                _settingsContainer.CreateVector2Field("Custom Pivot", _customPivot, v =>
                {
                    RecordUndo("Change Custom Pivot");
                    _customPivot = v;
                });
            }

            var btnRow2 = _settingsContainer.CreateRow();
            btnRow2.style.marginTop = 10;
            btnRow2.CreateButton("Apply Slices", ApplyToTexture).SetFlex(1).style.backgroundColor = new Color(0.1f, 0.4f, 0.6f);
            btnRow2.CreateButton("Revert", LoadExistingSlices).SetFlex(1);

            if (_interactionMode == InteractionMode.Select)
            {
                _settingsContainer.CreateDivider();
                _settingsContainer.CreateLabel($"Selected Count: {_selectedSliceIndices.Count}").style.unityFontStyleAndWeight = FontStyle.Bold;
            }
            else if (_selectedSliceIndex >= 0 && _selectedSliceIndex < _slices.Count)
            {
                _settingsContainer.CreateDivider();
                _settingsContainer.CreateLabel($"Selected Slice: {_selectedSliceIndex}").style.unityFontStyleAndWeight = FontStyle.Bold;

                var currentName = _sliceNames.Count > _selectedSliceIndex ? _sliceNames[_selectedSliceIndex] : $"{_namingPrefix}{_selectedSliceIndex}";
                _settingsContainer.CreateTextField("Name", currentName, v =>
                {
                    RecordUndo("Change Slice Name");
                    EnsureSliceNamesCapacity();
                    _sliceNames[_selectedSliceIndex] = v;
                });

                var rectToolRow = _settingsContainer.CreateRow();
                rectToolRow.CreateButton("Copy Rect", () =>
                {
                    var selected = _slices[_selectedSliceIndex];
                    _rectClipboard = new RectInt(
                        Mathf.RoundToInt(selected.x),
                        Mathf.RoundToInt(selected.y),
                        Mathf.RoundToInt(selected.width),
                        Mathf.RoundToInt(selected.height));
                }).SetFlex(1);
                rectToolRow.CreateButton("Paste Rect", () =>
                {
                    if (_rectClipboard == null) return;
                    RecordUndo("Paste Slice Rect");
                    var rect = _rectClipboard.Value;
                    _slices[_selectedSliceIndex] = new Rect(rect.x, rect.y, rect.width, rect.height);
                    _previewContainer.MarkDirtyRepaint();
                }).SetFlex(1);

                var selectedRect = _slices[_selectedSliceIndex];
                var rectInt = new RectInt(
                    Mathf.RoundToInt(selectedRect.x),
                    Mathf.RoundToInt(selectedRect.y),
                    Mathf.RoundToInt(selectedRect.width),
                    Mathf.RoundToInt(selectedRect.height));
                _settingsContainer.CreateRectIntField("Rect", rectInt, v =>
                {
                    RecordUndo("Change Slice Rect");
                    _slices[_selectedSliceIndex] = new Rect(v.x, v.y, v.width, v.height);
                    _previewContainer.MarkDirtyRepaint();
                });

                var borderToolRow = _settingsContainer.CreateRow();
                borderToolRow.CreateButton("Copy Border", () =>
                {
                    _borderClipboard = GetSliceBorder(_selectedSliceIndex);
                }).SetFlex(1);
                borderToolRow.CreateButton("Paste Border", () =>
                {
                    if (_borderClipboard == null) return;
                    RecordUndo("Paste Slice Border");
                    SetSliceBorder(_selectedSliceIndex, _borderClipboard.Value);
                    _previewContainer.MarkDirtyRepaint();
                    ScheduleBuildSettingsUI();
                }).SetFlex(1);

                var selectedBorder = GetSliceBorder(_selectedSliceIndex);
                var borderInt = new RectInt(
                    Mathf.RoundToInt(selectedBorder.x),
                    Mathf.RoundToInt(selectedBorder.y),
                    Mathf.RoundToInt(selectedBorder.z),
                    Mathf.RoundToInt(selectedBorder.w));
                _settingsContainer.CreateRectIntField("Border (L,B,R,T)", borderInt, v =>
                {
                    RecordUndo("Change Slice Border");
                    SetSliceBorder(_selectedSliceIndex, new Vector4(v.x, v.y, v.width, v.height));
                    _previewContainer.MarkDirtyRepaint();
                });
            }
        }

        private void EnsureSliceNamesCapacity()
        {
            while (_sliceNames.Count < _slices.Count)
            {
                _sliceNames.Add($"{_namingPrefix}{_sliceNames.Count}");
            }
            while (_sliceNames.Count > _slices.Count)
            {
                _sliceNames.RemoveAt(_sliceNames.Count - 1);
            }

            EnsureSliceBordersCapacity();
        }
    }
}
