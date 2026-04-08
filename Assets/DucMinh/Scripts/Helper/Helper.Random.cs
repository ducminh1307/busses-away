using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace DucMinh
{
    public static partial class Helper
    {
        private static ListPool<object> _listPool = new();
        public static bool GetRandomBool()
        {
            return Random.value > .5f;
        }

        public static int GetRandom100()
        {
            return Random.Range(0, 100);
        }
        
        public static T GetRandomValue<T>(List<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                Log.Error("List is null");
                return default;
            }
            
            var count = list.Count;
            return list[Random.Range(0, count)];
        }
        
        public static T GetRandomValue<T>(T[] array)
        {
            if (array.IsNullOrEmpty())
            {
                Log.Error("Array is null");
                return default;
            }

            return GetRandomValue(array.ToList());
        }
        
        public static T GetRandomAndRemove<T>(ref List<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                Log.Error("Array is null");
                return default;
            }

            var value = GetRandomValue(list);
            list.Remove(value);
            return value;
        }
        
        public static List<T> GetRandomValues<T>(List<T> list, int amount)
        {
            if (list.IsNullOrEmpty())
            {
                Log.Error("Array is null");
                return null;
            }

            if (amount < 0 || amount > list.Count)
            {
                Log.Error("Amount is out of range");
                return null;
            }
            
            var temp = list.ToList();
            List<T> resultList = new();
            for (int i = 0; i < amount; i++)
            {
                var value = GetRandomAndRemove(ref temp);
                resultList.Add(value);
            }
            return resultList;
        }
        
        public static T[] GetRandomValues<T>(T[] array, int amount)
        {
            if (array.IsNullOrEmpty())
            {
                Log.Error("Array is null");
                return null;
            }

            if (amount < 0 || amount > array.Length)
            {
                Log.Error("Amount is out of range");
                return null;
            }
            
            var resultList = GetRandomValues(array.ToList(), amount);
            return resultList.ToArray();
        }

        public static T GetRandomArray2D<T>(T[,] array)
        {
            array.GetCounts(out var rowCount, out var colCount);
            return array[Random.Range(0, rowCount), Random.Range(0, colCount)];
        }

        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
        
        public static void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
            }
        }

        public static T GetRandomEnumValue<T>() where T : struct, Enum
        {
            var count = GetEnumCount<T>();
            
            var result = Random.Range(0, count);
            return (T)Enum.ToObject(typeof(T), result);
        }

        public static int GetRandomRatios(out int randomValue, params int[] ratios)
        {
            randomValue = 0;
            if (ratios.IsNullOrEmpty())
            {
                Log.Error("Ratio are null or empty");
                return 0;
            }

            var total = 0;
            foreach (var ratio in ratios)
            {
                if (ratio < 0)
                {
                    Log.Warning($"Ratio must be greater than or equal to 0: {ratio}");
                }
                else
                {
                    total += ratio;
                }
            }

            if (total == 0)
            {
                Log.Error("Total ratio must be greater than 0");
                return 0;
            }
            
            var randomValuee = Random.Range(0, total);
            var currentSum = 0;
            for (var i = 0; i < ratios.Length; i++)
            {
                currentSum += ratios[i];
                if (randomValuee < currentSum)
                {
                    randomValue = randomValuee;
                    return i;
                }
            }
            
            return ratios.Length - 1;
        }
    }
}