# Phase 3 — Generator Metadata & Presets

## Goal
Turn each generator into a discoverable tool module with clear metadata and reusable presets.

## Scope
- Add generator category, description, and identifier.
- Add preset support for repeated configurations.
- Make the sidebar easier to scan.

## Changes
### BaseScriptGenerator
- Add metadata fields such as `Id`, `Category`, and `Description`.
- Add preset save/load methods.
- Add optional icon or color tags for generator types.

### GenerateScripts EditorWindow
- Show categories in the sidebar.
- Add a search field for generator filtering.
- Show description text for the selected generator.
- Allow selecting presets from a dropdown.

## Acceptance Criteria
- Generators are grouped by category.
- User can search and find generators quickly.
- Presets can be reused across sessions.
- The selected generator explains what it does.

## Risks
- Preset storage format needs to stay simple.
- Too much metadata could clutter the UI if not arranged carefully.
