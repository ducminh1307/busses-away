using UnityEngine;
using UnityEngine.SceneManagement;

namespace DucMinh
{
    public static class SceneHelper
    {
            /// <summary>
        /// Tải một scene theo tên.
        /// </summary>
        /// <param name="sceneName">Tên của scene cần tải.</param>
        /// <param name="mode">Chế độ tải scene (mặc định là LoadSceneMode.Single).</param>
        public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (IsSceneValid(sceneName))
            {
                SceneManager.LoadScene(sceneName, mode);
            }
            else
            {
                Debug.LogError($"Scene '{sceneName}' không tồn tại trong Build Settings!");
            }
        }

        /// <summary>
        /// Tải một scene bất đồng bộ theo tên.
        /// </summary>
        /// <param name="sceneName">Tên của scene cần tải.</param>
        /// <param name="onComplete">Callback được gọi khi tải hoàn tất (tùy chọn).</param>
        public static void LoadSceneAsync(string sceneName, System.Action onComplete = null)
        {
            if (!IsSceneValid(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' không tồn tại trong Build Settings!");
                return;
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            if (asyncLoad != null)
            {
                asyncLoad.completed += (operation) => onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Tải lại scene hiện tại.
        /// </summary>
        public static void ReloadCurrentScene()
        {
            Log.Debug("Reloading current scene");
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadScene(currentSceneName);
        }

        /// <summary>
        /// Tải lại scene hiện tại với chế độ tải tùy chọn.
        /// </summary>
        /// <param name="mode">Chế độ tải scene (mặc định là LoadSceneMode.Single).</param>
        public static void ReloadScene(LoadSceneMode mode = LoadSceneMode.Single)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadScene(currentSceneName, mode);
        }

        /// <summary>
        /// Thoát game.
        /// </summary>
        public static void QuitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }

        /// <summary>
        /// Lấy tên của scene hiện tại.
        /// </summary>
        /// <returns>Tên của scene hiện tại.</returns>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Kiểm tra xem một scene có tồn tại trong Build Settings hay không.
        /// </summary>
        /// <param name="sceneName">Tên của scene cần kiểm tra.</param>
        /// <returns>True nếu scene tồn tại, ngược lại là False.</returns>
        private static bool IsSceneValid(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tải scene tiếp theo trong danh sách build settings.
        /// </summary>
        public static void LoadNextScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = (currentIndex + 1) % SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene(nextIndex);
        }

        /// <summary>
        /// Tải scene trước đó trong danh sách build settings.
        /// </summary>
        public static void LoadPreviousScene()
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int previousIndex = (currentIndex - 1 + SceneManager.sceneCountInBuildSettings) % SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene(previousIndex);
        }
    }
}