# Phase 4 — Auto-Discovery & Registration

## Goal
Remove manual registration so new generators appear automatically.

## Scope
- Discover all generator classes at startup.
- Sort generators by category and name.
- Cache the discovered list for the session.

## Changes
### BaseScriptGenerator
- No major UI changes required.
- Ensure every generator exposes metadata required by discovery.

### GenerateScripts EditorWindow
- Replace hard-coded generator list with reflection-based discovery.
- Handle abstract classes and invalid constructors safely.
- Add a refresh button for rescanning modules.

## Acceptance Criteria
- Adding a new generator class does not require editing the main window.
- Abstract or broken classes do not crash the tool.
- User can refresh the module list manually.

## Risks
- Reflection may need filtering for editor-only assemblies.
- Discovery order may change if sorting rules are not defined.
