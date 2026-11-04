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
2. Ensure all significant changes (Features, Fixes, Breaking Changes) are accurately categorized.
3. Keep the `[Unreleased]` header in place: the version bump script creates the release section. Add today's date to the generated version header after running it (e.g., `## v1.12.0 - 2026-11-04`).

## 2. Format and Validate API Documentation
Before finalizing the release, ensure all YAML documentation is properly formatted, HTTP methods are synced, and no endpoints are missing or undocumented.

!!! warning "Preserve routes that share a path"
    The current YAML format contains duplicate route keys for GET and POST on `/api/v1/map/zone/growing`. The formatter can discard one entry and the method fixer can change the retained entry's method. Run both modifying scripts in a disposable copy first and inspect their diff. Preserve both route definitions in the release source; do not apply lossy changes. Run the final validator against the actual release source as well.

Run the following documentation scripts from the root of that copy:

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
- Creates a version heading beneath `[Unreleased]` in CHANGELOG.md
- Stages all modified tracked files in Git (git add -u); inspect the staged diff before committing

## 4. Build the Mod (Multi-Version Support)

!!! warning "Important: Build for 1.5, 1.6 and 1.7"

    RIMAPI currently supports RimWorld 1.5, 1.6 and 1.7. You MUST compile the binaries for all three engine versions before packaging the final release folder or uploading to Steam Workshop.

Ensure your build pipeline or IDE targets all versions so that the respective 1.5/Assemblies, 1.6/Assemblies and 1.7/Assemblies folders are correctly populated.

RimWorld 1.7 runs on the Waylith engine (.NET 10), so `Release-1.7` targets `net10.0` and needs the .NET 10 SDK; `Release-1.5` and `Release-1.6` still target `net472`.

## 5. Merge and Tag

Once the script finishes, inspect the staged changes, add the changelog date, build all supported versions and commit on `develop`:

```bash

# Commit the version bump
git commit -m "Release v1.12.0"
```

Push `develop` after approval and open a `develop` → `master` PR titled `Release v1.12.0`. Squash-merge the PR. Fetch the merged result and, after approval, create the annotated tag **on `origin/master`** and push it:

```bash
git fetch origin
git tag -a v1.12.0 -m "Release v1.12.0" origin/master
git push origin v1.12.0
```

Check the release workflow and download its ZIP. Verify `1.5/Assemblies`, `1.6/Assemblies` and `1.7/Assemblies`; the 1.7 folder must include RIMAPI, Newtonsoft.Json, WatsonWebserver, IpMatcher, RegexMatcher, UrlMatcher, Timestamps and Caching DLLs. Steam Workshop uploads are performed manually by the maintainer.

## 6. Synchronize develop

After the release, merge the squash commit back into `develop` so the next release has a shared base. Merge and push after approval:

```bash
git switch develop
git merge origin/master --no-ff -m "chore: sync develop with squash merge v1.12.0"
git push origin develop
```
