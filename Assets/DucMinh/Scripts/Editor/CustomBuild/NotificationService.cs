using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Service for sending notifications to Discord and Telegram
    /// </summary>
    public static class NotificationService
    {
        /// <summary>
        /// Broadcasts notifications to all enabled channels
        /// </summary>
        public static void BroadcastNotifications(BuildConfig config, string buildPath, long sizeBytes)
        {
            // Format file size
            double sizeMB = sizeBytes / (1024.0 * 1024.0);
            string fileName = System.IO.Path.GetFileName(buildPath);

            // Construct notification message
            string message = $"🚀 **New Build Available!**\n\n" +
                            $"**App:** {config.baseFileName}\n" +
                            $"**Version:** {config.version} (Build {config.buildNumber})\n" +
                            $"**Type:** {config.buildType}\n" +
                            $"**Platform:** Android\n" +
                            $"**Size:** {sizeMB:F2} MB\n" +
                            $"**File:** {fileName}";

            Debug.Log("[NotificationService] Broadcasting notifications...");

            // Send based on notification type
            if (config.notificationType == BuildConfig.NotificationType.Discord)
            {
                if (!string.IsNullOrEmpty(config.discordWebhookUrl))
                {
                    SendDiscordMessage(config.discordWebhookUrl, message);
                }
                else
                {
                    Debug.LogWarning("[NotificationService] Discord webhook URL is empty!");
                }
            }
            else if (config.notificationType == BuildConfig.NotificationType.Telegram)
            {
                if (!string.IsNullOrEmpty(config.telegramBotToken) && !string.IsNullOrEmpty(config.telegramChatId))
                {
                    SendTelegramMessage(config.telegramBotToken, config.telegramChatId, message);
                }
                else
                {
                    Debug.LogWarning("[NotificationService] Telegram bot token or chat ID is empty!");
                }
            }
        }

        /// <summary>
        /// Sends a message to Discord via webhook
        /// </summary>
        private static void SendDiscordMessage(string webhookUrl, string message)
        {
            Debug.Log("[NotificationService] Sending Discord notification...");

            // Construct JSON payload
            string jsonPayload = "{\"content\":\"" + EscapeJson(message) + "\"}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

            UnityWebRequest request = new UnityWebRequest(webhookUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send request synchronously (editor context)
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[NotificationService] Discord notification sent successfully!");
            }
            else
            {
                Debug.LogError($"[NotificationService] Discord notification failed: {request.error}");
            }

            request.Dispose();
        }

        /// <summary>
        /// Sends a message to Telegram via Bot API
        /// </summary>
        private static void SendTelegramMessage(string token, string chatId, string message)
        {
            Debug.Log("[NotificationService] Sending Telegram notification...");

            // Explicitly construct the full Telegram API URL
            string telegramUrl = $"https://api.telegram.org/bot{token}/sendMessage";

            // Construct JSON payload
            string jsonPayload = "{\"chat_id\":\"" + chatId + "\",\"text\":\"" + EscapeJson(message) + "\"}";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

            UnityWebRequest request = new UnityWebRequest(telegramUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send request synchronously (editor context)
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone) { }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[NotificationService] Telegram notification sent successfully!");
            }
            else
            {
                Debug.LogError($"[NotificationService] Telegram notification failed: {request.error}");
                Debug.LogError($"[NotificationService] Response: {request.downloadHandler.text}");
            }

            request.Dispose();
        }

        /// <summary>
        /// Escapes special characters for JSON
        /// </summary>
        private static string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }
}
