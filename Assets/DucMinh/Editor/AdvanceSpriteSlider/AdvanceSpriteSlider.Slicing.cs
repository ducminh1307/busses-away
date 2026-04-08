using UnityEditor;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void GenerateSlices()
        {
            Undo.RecordObject(this, "Generate Slices");
            ResetSliceState();

            if (_texture == null) return;
            EnsureTextureReadable();

            switch (_sliceMode)
            {
                case SliceMode.Auto:
                    GenerateAutoSlicesFromTexture();
                    break;
                case SliceMode.GridByCellSize:
                    GenerateGridSlicesByCellSize();
                    break;
                case SliceMode.GridByCellCount:
                    GenerateGridSlicesByCellCount();
                    break;
            }

            EnsureSliceBordersCapacity();
            RefreshSlicingUI();
        }

        private void ClearSlices()
        {
            Undo.RecordObject(this, "Clear Slices");
            ResetSliceState();
            RefreshSlicingUI();
        }
    }
}
