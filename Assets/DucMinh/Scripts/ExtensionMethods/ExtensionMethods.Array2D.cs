namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this T[,] array)
        {
            GetSize(array, out var width, out var height);
            return !(array != null && width > 0 && height > 0);
        }
        public static void GetSize<T>(this T[,] array, out int width, out int height)
        {
            if (array == null)
                width = height = 0;
            else
            {
                width = array.GetLength(1);
                height = array.GetLength(0);
            }
        }

        public static void GetCounts<T>(this T[,] array, out int rowCount, out int columnCount)
        {
            if (array == null)
                rowCount = columnCount = 0;
            else
            {
                rowCount = array.GetLength(0);
                columnCount = array.GetLength(1);
            }
        }

        public static bool IsSizeEqual<T>(this T[,] array, int width, int height)
        {
            if (array == null)
                return false;
            
            return array.GetLength(1) == width && array.GetLength(0) == height;
        }

        public static void SetValues<T>(this T[,] array, T value)
        {
            GetCounts(array, out int rowCount, out int columnCount);
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < columnCount; col++)
                {
                    array[row, col] = value;
                }
            }
        }
    }
}