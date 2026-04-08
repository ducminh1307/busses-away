# Phase 2 — Overwrite Policy & Result Reporting

## Goal
Make file generation predictable and transparent when files already exist.

## Scope
- Add overwrite policy.
- Add execution result reporting.
- Track created, skipped, and overwritten files.

## Changes
### BaseScriptGenerator
- Extend file helpers to support overwrite modes.
- Return a structured result from file creation.
- Report success, skipped files, and conflicts.

### GenerateScripts EditorWindow
- Add overwrite mode selector.
- Show a result summary after generation.
- Add buttons for opening generated folders or files.
- Display conflict resolution choices if needed.

## Acceptance Criteria
- Existing files are not silently overwritten.
- User can choose skip, overwrite, or ask mode.
- Result summary shows what happened per file.
- Failed writes are visible in the UI.

## Risks
- Different generators may need custom conflict behavior.
- UI may become crowded if the result panel is too verbose.
