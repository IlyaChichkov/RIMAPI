# :material-rocket-launch: Release Flow

This document outlines the standard procedure for publishing a new release of RIMAPI. By following these steps, we ensure that version numbers, documentation, and compiled binaries stay perfectly in sync.

!!! info "Prerequisites"
    Before starting a release, ensure that:
    
    * All new features and bug fixes have been tested locally in RimWorld.
    * The Bruno API collection runs successfully against the new build.
    * Documentation for any new endpoints has been added to the respective `_api_macroses/controllers/` YAML files.

---

## 1. Update the Changelog
Before bumping the version, finalize the `CHANGELOG.md`. 

1. Review the `## [Unreleased]` section.
2. Change the `[Unreleased]` header to the new version number and today's date (e.g., `## v1.8.3 - 2026-03-28`).
3. Ensure all significant changes (Features, Fixes, Breaking Changes) are accurately categorized.

## 2. Run the Version Bump Script
RimAPI uses a Python script to enforce a "Single Source of Truth" for versioning. You do not need to manually edit XML or C# files.

From the root directory of the repository, run the bump script with your new version number:

```bash
python bump_version.py 1.8.3
```

What this script does automatically:

- Updates <modVersion> in About/About.xml
- Updates <version> in About/Manifest.xml
- Updates <Version> in Source/RIMAPI/RimApi.csproj
- Updates the tool version in .config/dotnet-tools.json
- Updates the documentation metadata in docs/_api_macroses/controllers/_meta.yml
- Stages all modified files in Git (git add -u)

## 3. Commit and Tag

Once the script finishes and the files are staged, commit the release and create a Git tag. We follow Conventional Commits for our release history.

```bash

# Commit the version bump
git commit -m "Release v1.8.3"

# Create an annotated tag
git tag -a v1.8.3 -m "Release v1.8.3"

# Push the commit and the tag to GitHub
git push origin main
git push origin v1.8.3
```
