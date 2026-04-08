# Phase 1 — Validation & Preview

## Goal
Make the current generator safe to use by checking inputs before execution and showing what will be created.

## Scope
- Add generator validation.
- Add output preview in the editor window.
- Show errors and warnings inline.
- Prevent generate when validation fails.

## Changes
### BaseScriptGenerator
- Add `Validate(out string errorMessage)` or a richer validation result type.
- Add `BuildPreview()` to describe files/folders that will be created.
- Add helper methods for reporting warnings and errors.

### GenerateScripts EditorWindow
- Add a validation panel above the Generate button.
- Render preview items in a scrollable list.
- Disable or block Generate when validation fails.
- Show selected generator status in the header.

## Acceptance Criteria
- User can see validation errors before generating.
- User can preview file/folder output before generating.
- Generate is blocked if required fields are missing.
- No file is written when validation fails.

## Risks
- Some existing generators may not provide enough metadata at first.
- Preview formatting may need iteration once real generators are added.
