import sys
import re
import os
import subprocess

if len(sys.argv) < 2:
    print("Usage: python bump_version.py <new_version>")
    sys.exit(1)

new_version = sys.argv[1]

# Define the files and the specific regex patterns to replace safely
files_to_update = [
    {
        "path": "About/About.xml",
        "pattern": r"(<modVersion>).*?(</modVersion>)",
        "replacement": rf"\g<1>{new_version}\g<2>",
    },
    {
        "path": "About/Manifest.xml",
        "pattern": r"(<version>).*?(</version>)",
        "replacement": rf"\g<1>{new_version}\g<2>",
    },
    {
        "path": "Source/RIMAPI/RimApi.csproj",
        "pattern": r"(<Version>).*?(</Version>)",
        "replacement": rf"\g<1>{new_version}\g<2>",
    },
    {
        "path": ".config/dotnet-tools.json",
        "pattern": r'("version":\s*").*?(")',
        "replacement": rf"\g<1>{new_version}\g<2>",
    },
    {
        "path": "docs/_api_macroses/controllers/_meta.yml",
        "pattern": r'(version:\s*").*?(")',
        "replacement": rf"\g<1>{new_version}\g<2>",
    },
    {
        "path": "CHANGELOG.md",
        "pattern": r"## \[Unreleased\]",
        "replacement": f"## [Unreleased]\n\n## v{new_version}",
    },
]

print(f"🚀 Bumping RIMAPI to version {new_version}...")

for file_info in files_to_update:
    filepath = file_info["path"]
    if os.path.exists(filepath):
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()

        updated_content = re.sub(
            file_info["pattern"], file_info["replacement"], content
        )

        with open(filepath, "w", encoding="utf-8") as f:
            f.write(updated_content)
        print(f"✅ Updated {filepath}")
    else:
        print(f"⚠️ Warning: {filepath} not found.")

subprocess.run(["git", "add", "-u"])
print("\n✨ Version bumped and files staged! Ready for commit.")
