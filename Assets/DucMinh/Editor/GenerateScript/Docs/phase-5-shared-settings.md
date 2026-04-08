# Phase 5 — Shared Settings & Team Conventions

## Goal
Standardize output across the team with shared naming, paths, and namespace rules.

## Scope
- Add global tool settings.
- Define root namespace and folder conventions.
- Support team-wide reusable configuration.

## Changes
### BaseScriptGenerator
- Read from a shared settings object.
- Use shared path and namespace rules in helpers.
- Support feature-specific overrides when needed.

### GenerateScripts EditorWindow
- Add a settings section for global options.
- Allow editing default namespace, root folder, and naming rules.
- Save settings for reuse in later sessions.

## Acceptance Criteria
- Output folders and namespaces follow a shared convention.
- Settings are visible and editable from the tool.
- Generators can still override special cases when necessary.

## Risks
- Shared rules must not be too rigid for edge cases.
- Settings UI may need separation between global and generator-specific values.
