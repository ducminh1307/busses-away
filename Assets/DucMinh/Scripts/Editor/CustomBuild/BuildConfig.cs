using System;
using UnityEngine;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Build configuration class (regular class, not ScriptableObject)
    /// </summary>
    [Serializable]
    public class BuildConfig
    {
        public enum BuildType
        {
            Debug,
            Release
        }

        [Header("Build Settings")]
        public BuildType buildType = BuildType.Debug;
        public string baseFileName = "DoubleFarm";
        public string version = "1.0.0";
        public int buildNumber = 1;

        [Header("Firebase Settings")]
        public bool uploadToFirebase = false;
        public string firebaseAppId = "";
        public string testerGroups = "qa-team";
        [TextArea(3, 10)]
        public string releaseNoteContent = "Bug fixes and improvements";

        [Header("Notification Settings")]
        public bool enableNotifications = false;
        public NotificationType notificationType = NotificationType.Discord;

        [Header("Discord Configuration")]
        public string discordWebhookUrl = "";

        [Header("Telegram Configuration")]
        public string telegramBotToken = "";
        public string telegramChatId = "";

        /// <summary>
        /// Auto-generated config name based on game name and build type
        /// </summary>
        public string ConfigName => $"{baseFileName}_{buildType}";

        /// <summary>
        /// Notification type enum
        /// </summary>
        public enum NotificationType
        {
            Discord,
            Telegram
        }

        /// <summary>
        /// Creates a deep copy of this BuildConfig
        /// </summary>
        public BuildConfig Clone()
        {
            return new BuildConfig
            {
                buildType = this.buildType,
                baseFileName = this.baseFileName,
                version = this.version,
                buildNumber = this.buildNumber,
                uploadToFirebase = this.uploadToFirebase,
                firebaseAppId = this.firebaseAppId,
                testerGroups = this.testerGroups,
                releaseNoteContent = this.releaseNoteContent,
                enableNotifications = this.enableNotifications,
                notificationType = this.notificationType,
                discordWebhookUrl = this.discordWebhookUrl,
                telegramBotToken = this.telegramBotToken,
                telegramChatId = this.telegramChatId
            };
        }
    }
}
