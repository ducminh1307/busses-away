using System.Collections.Generic;
using DucMinh.UIToolkit;
using UnityEngine.UIElements;

namespace DucMinh.GenerateScript
{
    public class TestScriptGenerator : BaseScriptGenerator
    {
        // ─── Metadata ────────────────────────────────────────────────────────────

        public override string Name        => "Test Script Generator";
        public override string Id          => "test_script_generator";
        public override string Category    => "Demo";
        public override string Description => "Generates a simple test MonoBehaviour script. Used for demonstrating the tool.";

        // ─── State ───────────────────────────────────────────────────────────────

        private string _scriptName   = "MyTestScript";
        private string _outputFolder = "Assets/Scripts";

        // ─── GUI ─────────────────────────────────────────────────────────────────

        public override void SetupGUI(VisualElement container)
        {
            container.CreateTextField("Script Name", _scriptName, val => _scriptName = val)
                .SetMarginBottom(10);
            container.CreateTextField("Output Folder", _outputFolder, val => _outputFolder = val)
                .SetMarginBottom(10);
        }

        // ─── Phase 1: Validation ─────────────────────────────────────────────────

        public override ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(_scriptName))
                result.AddError("Script Name cannot be empty.");
            else if (_scriptName.Contains(" "))
                result.AddWarning("Script Name contains spaces — they will be removed.");

            if (string.IsNullOrWhiteSpace(_outputFolder))
                result.AddError("Output Folder cannot be empty.");

            return result;
        }

        // ─── Phase 1: Preview ────────────────────────────────────────────────────

        public override List<PreviewItem> BuildPreview()
        {
            var cleanName = _scriptName.Replace(" ", "");
            return new List<PreviewItem>
            {
                PreviewItem.Folder(_outputFolder),
                PreviewItem.File($"{_outputFolder}/{cleanName}.cs"),
            };
        }

        // ─── Phase 2: Generate ───────────────────────────────────────────────────

        public override GenerationResult Generate(OverwriteMode overwriteMode = OverwriteMode.Skip)
        {
            var result = new GenerationResult();
            var cleanName = _scriptName.Replace(" ", "");
            var ns = Settings.DefaultNamespace;

            var content = $@"using UnityEngine;

namespace {ns}
{{
    public class {cleanName} : MonoBehaviour
    {{
        private void Awake()
        {{
        }}

        private void Start()
        {{
        }}

        private void Update()
        {{
        }}
    }}
}}
";

            CreateFolder(_outputFolder);
            CreateFile($"{_outputFolder}/{cleanName}.cs", content, overwriteMode, result);
            RefreshAssetDatabase();

            return result;
        }

        // ─── Phase 3: Preset ─────────────────────────────────────────────────────

        protected override Dictionary<string, string> CollectPresetData() =>
            new Dictionary<string, string>
            {
                { "scriptName",   _scriptName },
                { "outputFolder", _outputFolder }
            };

        protected override void ApplyPresetData(Dictionary<string, string> data)
        {
            if (data.TryGetValue("scriptName",   out var n)) _scriptName   = n;
            if (data.TryGetValue("outputFolder", out var f)) _outputFolder = f;
        }

        // ─── Phase 6: Plan ───────────────────────────────────────────────────────

        public override GenerationPlan BuildPlan()
        {
            var cleanName = _scriptName.Replace(" ", "");
            var ns = Settings.DefaultNamespace;
            var plan = new GenerationPlan();

            plan.AddFolder(_outputFolder);
            plan.AddFile($"{_outputFolder}/{cleanName}.cs", $@"using UnityEngine;

namespace {ns}
{{
    public class {cleanName} : MonoBehaviour {{ }}
}}
");
            plan.AddPostHook(RefreshAssetDatabase);
            return plan;
        }
    }
}