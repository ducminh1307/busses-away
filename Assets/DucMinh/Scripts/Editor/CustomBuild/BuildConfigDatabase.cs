using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Singleton ScriptableObject database for managing multiple build configurations
    /// </summary>
    [CreateAssetMenu(fileName = "BuildConfigDatabase", menuName = "DucMinh/Build/Build Config Database")]
    public class BuildConfigDatabase : DucMinh.SingletonScriptableObject<BuildConfigDatabase>
    {
        [SerializeField]
        private List<BuildConfig> buildConfigs = new List<BuildConfig>();

        [SerializeField]
        private int selectedConfigIndex = 0;

        /// <summary>
        /// Gets all build configurations
        /// </summary>
        public List<BuildConfig> BuildConfigs => buildConfigs;

        /// <summary>
        /// Gets/Sets the currently selected config index
        /// </summary>
        public int SelectedConfigIndex
        {
            get => selectedConfigIndex;
            set => selectedConfigIndex = Mathf.Clamp(value, 0, buildConfigs.Count - 1);
        }

        /// <summary>
        /// Gets the currently selected build config
        /// </summary>
        public BuildConfig SelectedConfig
        {
            get
            {
                if (buildConfigs.Count == 0)
                {
                    Debug.LogWarning("[BuildConfigDatabase] No build configs available. Creating default config.");
                    CreateDefaultConfig();
                }

                selectedConfigIndex = Mathf.Clamp(selectedConfigIndex, 0, buildConfigs.Count - 1);
                return buildConfigs[selectedConfigIndex];
            }
        }

        /// <summary>
        /// Gets a config by name
        /// </summary>
        public BuildConfig GetConfigByName(string configName)
        {
            return buildConfigs.FirstOrDefault(c => c.ConfigName == configName);
        }

        /// <summary>
        /// Gets all config names for dropdown
        /// </summary>
        public string[] GetConfigNames()
        {
            return buildConfigs.Select(c => c.ConfigName).ToArray();
        }

        /// <summary>
        /// Adds a new build config
        /// </summary>
        public void AddConfig(BuildConfig config)
        {
            // Create a clone to avoid reference issues
            buildConfigs.Add(config.Clone());
        }


        /// <summary>
        /// Adds a new default build config
        /// </summary>
        /// <returns>True if config was added, false if duplicate exists</returns>
        public bool AddNewConfig()
        {
            var newConfig = new BuildConfig
            {
                buildType = BuildConfig.BuildType.Debug,
                baseFileName = "DoubleFarm",
                version = "1.0.0",
                buildNumber = 1,
                uploadToFirebase = false,
                firebaseAppId = "",
                testerGroups = "qa-team",
                releaseNoteContent = "Bug fixes and improvements",
                enableNotifications = false,
                notificationType = BuildConfig.NotificationType.Discord,
                discordWebhookUrl = "",
                telegramBotToken = "",
                telegramChatId = ""
            };

            // Check for duplicate
            string configName = newConfig.ConfigName;
            if (HasConfigWithName(configName))
            {
                Debug.LogWarning($"[BuildConfigDatabase] Config with name '{configName}' already exists! Please change the base file name or build type.");
                return false;
            }

            buildConfigs.Add(newConfig);
            return true;
        }

        /// <summary>
        /// Checks if a config with the given name already exists
        /// </summary>
        public bool HasConfigWithName(string configName)
        {
            return buildConfigs.Any(c => c.ConfigName == configName);
        }

        /// <summary>
        /// Removes duplicate configs (keeps first occurrence)
        /// </summary>
        public int RemoveDuplicateConfigs()
        {
            var uniqueConfigs = new List<BuildConfig>();
            var seenNames = new HashSet<string>();
            int duplicatesRemoved = 0;

            foreach (var config in buildConfigs)
            {
                string configName = config.ConfigName;
                if (!seenNames.Contains(configName))
                {
                    seenNames.Add(configName);
                    uniqueConfigs.Add(config);
                }
                else
                {
                    duplicatesRemoved++;
                    Debug.Log($"[BuildConfigDatabase] Removed duplicate config: {configName}");
                }
            }

            buildConfigs = uniqueConfigs;

            // Adjust selected index if needed
            if (selectedConfigIndex >= buildConfigs.Count)
            {
                selectedConfigIndex = Mathf.Max(0, buildConfigs.Count - 1);
            }

            return duplicatesRemoved;
        }

        /// <summary>
        /// Removes a build config at index
        /// </summary>
        public void RemoveConfigAt(int index)
        {
            if (index >= 0 && index < buildConfigs.Count)
            {
                buildConfigs.RemoveAt(index);

                if (selectedConfigIndex >= buildConfigs.Count)
                {
                    selectedConfigIndex = Mathf.Max(0, buildConfigs.Count - 1);
                }
            }
        }

        /// <summary>
        /// Creates a default config if none exists
        /// </summary>
        private void CreateDefaultConfig()
        {
            var defaultConfig = new BuildConfig
            {
                buildType = BuildConfig.BuildType.Debug,
                baseFileName = "DoubleFarm",
                version = "1.0.0",
                buildNumber = 1
            };

            buildConfigs.Add(defaultConfig);
        }

        /// <summary>
        /// Ensures at least one config exists
        /// </summary>
        private void OnEnable()
        {
            if (buildConfigs.Count == 0)
            {
                CreateDefaultConfig();
            }
        }
    }
}
