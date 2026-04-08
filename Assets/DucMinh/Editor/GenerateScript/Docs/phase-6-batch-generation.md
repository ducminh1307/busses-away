# Phase 6 — Batch Generation & Feature Scaffolding

## Goal
Generate a full feature structure instead of one script at a time.

## Scope
- Support multi-step generation plans.
- Generate several files from one action.
- Create feature folders and boilerplate automatically.

## Changes
### BaseScriptGenerator
- Add plan-based generation support.
- Allow one generator to create multiple files and folders.
- Support post-generation hooks for formatting or asset refresh.

### GenerateScripts EditorWindow
- Add a batch mode checkbox or tab.
- Show all files that will be generated in the current batch.
- Add a summary of the feature scaffold before execution.

## Acceptance Criteria
- One generator can create a feature folder with multiple related files.
- User can review the full batch before execution.
- Result summary clearly lists all created items.

## Risks
- Batch output may require stronger preview and validation.
- Large batches may need better progress feedback.
