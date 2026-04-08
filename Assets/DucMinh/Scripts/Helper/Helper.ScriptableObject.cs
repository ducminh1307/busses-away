using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DucMinh
{
    public static partial class Helper
    {
        public static void CreateScriptableObjectWithNamespace(ScriptableObject instance)
        {
            string namespacePath = instance.GetType().Namespace;
            string folderPath;

            if (!string.IsNullOrEmpty(namespacePath))
            {
                folderPath = Path.Combine("Assets", namespacePath.Replace('.', '/'), "Resources");
            }
            else
            {
                folderPath = Path.Combine("Assets", "Resources");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = Path.Combine(folderPath, instance.GetType().Name + ".asset");

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
    }
}