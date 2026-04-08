using UnityEditor;
using UnityEngine;

namespace DucMinh.CustomBuild
{
    /// <summary>
    /// Custom editor for BuildConfigDatabase with inline editing and build capabilities
    /// </summary>
    [CustomEditor(typeof(BuildConfigDatabase))]
    public class BuildConfigDatabaseEditor : UnityEditor.Editor
    {
        private bool isEditable = false;
        private bool buildAsAab = false;

        public override void OnInspectorGUI()
        {
            var database = (BuildConfigDatabase)target;
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Build Config Database", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Build Configs List - Show with default Unity list controls
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Configs", EditorStyles.boldLabel);

            // Editable Toggle for the list
            isEditable = EditorGUILayout.Toggle("Editable", isEditable);

            EditorGUILayout.Space(3);

            // Disable list editing if not editable
            GUI.enabled = isEditable;

            SerializedProperty configsProperty = serializedObject.FindProperty("buildConfigs");
            EditorGUILayout.PropertyField(configsProperty, true);

            GUI.enabled = true; // Re-enable

            EditorGUILayout.Space(3);

            // Clean Duplicates Button
            if (GUILayout.Button("Clean Duplicate Configs"))
            {
                int removed = database.RemoveDuplicateConfigs();
                if (removed > 0)
                {
                    serializedObject.Update();
                    EditorUtility.DisplayDialog("Duplicates Removed",
                        $"Removed {removed} duplicate config(s).",
                        "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("No Duplicates",
                        "No duplicate configs found.",
                        "OK");
                }
            }

            EditorGUILayout.EndVertical();

            if (database.BuildConfigs.Count == 0)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(5);

            // Config Selection & Preview Section (Read-Only)
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Config Preview (Read-Only)", EditorStyles.boldLabel);

            string[] configNames = database.GetConfigNames();

            // Dropdown to select config by name
            EditorGUI.BeginChangeCheck();
            int selectedIndex = database.SelectedConfigIndex;
            selectedIndex = EditorGUILayout.Popup("Select Config", selectedIndex, configNames);
            if (EditorGUI.EndChangeCheck())
            {
                database.SelectedConfigIndex = selectedIndex;
                EditorUtility.SetDirty(database);
            }

            EditorGUILayout.EndVertical();

            var config = database.SelectedConfig;

            EditorGUILayout.Space(5);

            // All preview fields are disabled (read-only)
            GUI.enabled = false;

            // Display Config Name (Auto-generated)
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Config Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Config Name", config.ConfigName);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Build Configuration Section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            config.buildType = (BuildConfig.BuildType)EditorGUILayout.EnumPopup("Build Type", config.buildType);
            config.baseFileName = EditorGUILayout.TextField("Base File Name", config.baseFileName);
            config.version = EditorGUILayout.TextField("Version", config.version);
            config.buildNumber = EditorGUILayout.IntField("Build Number", config.buildNumber);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Firebase Settings Section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Firebase Settings", EditorStyles.boldLabel);

            config.uploadToFirebase = EditorGUILayout.Toggle("Upload to Firebase", config.uploadToFirebase);
            if (config.uploadToFirebase)
            {
                EditorGUI.indentLevel++;
                config.firebaseAppId = EditorGUILayout.TextField("Firebase App ID", config.firebaseAppId);
                config.testerGroups = EditorGUILayout.TextField("Tester Groups", config.testerGroups);
                EditorGUILayout.LabelField("Release Notes:");
                config.releaseNoteContent = EditorGUILayout.TextArea(config.releaseNoteContent, GUILayout.Height(60));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Notification Settings Section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Notification Settings", EditorStyles.boldLabel);

            config.enableNotifications = EditorGUILayout.Toggle("Enable Notifications", config.enableNotifications);

            if (config.enableNotifications)
            {
                EditorGUILayout.Space(5);

                // Notification Type Selector
                config.notificationType = (BuildConfig.NotificationType)EditorGUILayout.EnumPopup("Notification Type", config.notificationType);

                EditorGUILayout.Space(3);

                // Show settings based on selected notification type
                if (config.notificationType == BuildConfig.NotificationType.Discord)
                {
                    // Discord Section
                    EditorGUILayout.BeginVertical("helpBox");
                    EditorGUILayout.LabelField("Discord Configuration", EditorStyles.boldLabel);
                    config.discordWebhookUrl = EditorGUILayout.TextField("Webhook URL", config.discordWebhookUrl);
                    EditorGUILayout.EndVertical();
                }
                else if (config.notificationType == BuildConfig.NotificationType.Telegram)
                {
                    // Telegram Section
                    EditorGUILayout.BeginVertical("helpBox");
                    EditorGUILayout.LabelField("Telegram Configuration", EditorStyles.boldLabel);
                    config.telegramBotToken = EditorGUILayout.TextField("Bot Token", config.telegramBotToken);
                    config.telegramChatId = EditorGUILayout.TextField("Chat ID", config.telegramChatId);
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();

            GUI.enabled = true; // Re-enable for buttons

            EditorGUILayout.Space(10);

            // Build Options
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);
            buildAsAab = EditorGUILayout.Toggle("Build as AAB (App Bundle)", buildAsAab);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Build Button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Build & Upload", GUILayout.Height(40)))
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                BuildEngine.RunBuildProcess(config, buildAsAab);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            // Generate CI/CD Scripts Button
            if (GUILayout.Button("Generate CI/CD Scripts", GUILayout.Height(30)))
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                CICDGenerator.GenerateScripts();
            }

            EditorGUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
