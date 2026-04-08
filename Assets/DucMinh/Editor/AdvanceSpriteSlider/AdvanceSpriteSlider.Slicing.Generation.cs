using System.Collections.Generic;
using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void GenerateGridSlicesByCellSize()
        {
            if (_cellSize.x <= 0 || _cellSize.y <= 0) return;

            var textureWidth = _texture.width;
            var textureHeight = _texture.height;
            var stepX = Mathf.Max(1, _cellSize.x + _padding.x);
            var stepY = Mathf.Max(1, _cellSize.y + _padding.y);

            for (var y = textureHeight - _offset.y - _cellSize.y; y >= 0; y -= stepY)
            {
                for (var x = _offset.x; x + _cellSize.x <= textureWidth; x += stepX)
                {
                    TryAddSlice(new Rect(x, y, _cellSize.x, _cellSize.y));
                }
            }
        }

        private void GenerateGridSlicesByCellCount()
        {
            if (_cellCount.x <= 0 || _cellCount.y <= 0) return;

            var cellWidth = (float)_texture.width / _cellCount.x;
            var cellHeight = (float)_texture.height / _cellCount.y;

            for (var row = _cellCount.y - 1; row >= 0; row--)
            {
                for (var column = 0; column < _cellCount.x; column++)
                {
                    TryAddSlice(new Rect(column * cellWidth, row * cellHeight, cellWidth, cellHeight));
                }
            }
        }

        private void TryAddSlice(Rect rect)
        {
            if (IsRectEmpty(rect)) return;

            _slices.Add(rect);
            _sliceNames.Add($"{_namingPrefix}{_sliceNames.Count}");
        }

        private void GenerateAutoSlicesFromTexture()
        {
            var width = _texture.width;
            var height = _texture.height;
            var pixels = _texture.GetPixels32();
            var visited = new bool[width * height];
            var stack = new Stack<int>(256);

            const byte alphaThreshold = 10;
            const int minRegionPixelCount = 4;

            var detectedRects = new List<Rect>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var startIndex = y * width + x;
                    if (visited[startIndex] || pixels[startIndex].a <= alphaThreshold) continue;

                    var minX = x;
                    var maxX = x;
                    var minY = y;
                    var maxY = y;
                    var pixelCount = 0;

                    visited[startIndex] = true;
                    stack.Push(startIndex);

                    while (stack.Count > 0)
                    {
                        var index = stack.Pop();
                        var currentX = index % width;
                        var currentY = index / width;

                        pixelCount++;
                        if (currentX < minX) minX = currentX;
                        if (currentX > maxX) maxX = currentX;
                        if (currentY < minY) minY = currentY;
                        if (currentY > maxY) maxY = currentY;

                        TryVisit(currentX - 1, currentY);
                        TryVisit(currentX + 1, currentY);
                        TryVisit(currentX, currentY - 1);
                        TryVisit(currentX, currentY + 1);
                    }

                    if (pixelCount < minRegionPixelCount) continue;

                    detectedRects.Add(new Rect(
                        minX,
                        minY,
                        maxX - minX + 1,
                        maxY - minY + 1));

                    void TryVisit(int nextX, int nextY)
                    {
                        if (nextX < 0 || nextX >= width || nextY < 0 || nextY >= height) return;

                        var nextIndex = nextY * width + nextX;
                        if (visited[nextIndex]) return;

                        visited[nextIndex] = true;
                        if (pixels[nextIndex].a > alphaThreshold)
                        {
                            stack.Push(nextIndex);
                        }
                    }
                }
            }

            detectedRects.Sort((left, right) =>
            {
                var yCompare = right.yMax.CompareTo(left.yMax);
                return yCompare != 0 ? yCompare : left.xMin.CompareTo(right.xMin);
            });

            _slices.AddRange(detectedRects);
            RenameSlicesByPrefix();
        }
    }
}
