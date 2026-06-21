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

## 2. Format and Validate API Documentation
Before finalizing the release, ensure all YAML documentation is properly formatted, HTTP methods are synced, and no endpoints are missing or undocumented. Run the following documentation scripts from the root directory:

```bash
echo "🧹 Formatting YAML files..."
python scripts/docs/mkdocs_format_yaml.py

echo "🔧 Auto-fixing HTTP methods..."
python scripts/docs/mkdocs_fix_yaml_methods.py

echo "🔍 Running final validation check..."
python scripts/docs/mkdocs_check_api.py
```

## 3. Run the Version Bump Script
RimAPI uses a Python script to enforce a "Single Source of Truth" for versioning. You do not need to manually edit XML or C# files.

From the root directory of the repository, run the bump script with your new version number:

```bash
python scripts/bump_version.py 1.8.3
```

What this script does automatically:

- Updates <modVersion> in About/About.xml
- Updates <version> in About/Manifest.xml
- Updates <Version> in Source/RIMAPI/RimApi.csproj
- Updates the tool version in .config/dotnet-tools.json
- Updates the documentation metadata in docs/_api_macroses/controllers/_meta.yml
- Stages all modified files in Git (git add -u)

## 4. Build the Mod (Multi-Version Support)

!!! warning "Important: Build for 1.5 and 1.6"

    RIMAPI currently supports both RimWorld 1.5 and 1.6. You MUST compile the binaries for both engine versions before packaging the final release folder or uploading to Steam Workshop.

Ensure your build pipeline or IDE targets both versions so that the respective 1.5/Assemblies and 1.6/Assemblies folders are correctly populated.

## 5. Commit and Tag

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

## 6. Sync develop after the squash merge

After the PR is squash-merged on GitHub, master contains a single new commit that represents the entire release. Because a squash merge creates no git link between master and develop, the branches are graph-diverged. Run the following to re-link them:

```bash
# Make sure you have the latest master locally
git fetch origin

# Switch to develop
git checkout develop

# Merge the squash commit back into develop.
# --no-ff forces a merge commit even if a fast-forward is possible,
# which keeps develop's own commit line visible in the graph.
git merge origin/master --no-ff -m "chore: sync develop with squash merge vX.Y.Z"

# Push develop
git push origin develop
```

The resulting graph will look like:

```
*   chore: sync develop with squash merge vX.Y.Z  ← develop HEAD
|\
| * Release vX.Y.Z (#N)                            ← master (tagged)
* | Release vX.Y.Z                                 ← develop's last commit
* | ...                                            ← all develop commits preserved
|/
* (previous release)
```

This keeps master's line clean (only tagged squash commits) while preserving the full develop commit history as a separate, visible branch.
