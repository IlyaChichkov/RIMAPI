import os
import re
import sys
import yaml

# Configuration paths
CS_DIRS = [
    os.path.join("Source", "RIMAPI", "RimworldRestApi", "BaseControllers"),
    os.path.join("Source", "RIMAPI", "RimworldRestApi", "Controllers"),
]
YAML_DIR = os.path.join("docs", "_api_macroses", "controllers")

# Regex to find C# route attributes like [Post("/api/v1/builder/copy")]
# Group 1 = HTTP Method (Get, Post, etc), Group 2 = Route Path
ROUTE_REGEX = re.compile(
    r'\[(Get|Post|Put|Delete|Patch)\(\s*"([^"]+)"\s*\)\]', re.IGNORECASE
)


def get_csharp_routes():
    """Scans all C# files recursively and returns a dictionary of controllers and their routes."""
    csharp_data = {}
    for cs_dir in CS_DIRS:
        if not os.path.exists(cs_dir):
            print(f"❌ Error: C# directory not found at {cs_dir}")
            continue

        for dirpath, _, filenames in os.walk(cs_dir):
            for filename in filenames:
                if not filename.endswith(".cs"):
                    continue
                controller_name = filename[:-3]  # Remove .cs
                filepath = os.path.join(dirpath, filename)
                with open(filepath, "r", encoding="utf-8") as f:
                    content = f.read()

                matches = ROUTE_REGEX.findall(content)
                if matches:
                    routes = csharp_data.setdefault(controller_name, {})
                    for method, path in matches:
                        routes[path] = method.upper()

    return csharp_data


def get_yaml_routes():
    """Scans base English YAML files and returns a dictionary of documented routes."""
    yaml_data = {}
    if not os.path.exists(YAML_DIR):
        print(f"❌ Error: YAML directory not found at {YAML_DIR}")
        return yaml_data

    for filename in os.listdir(YAML_DIR):
        # Only parse base files (e.g., BuilderController.yml), ignore _meta and translations (.ja.yml)
        if (
            filename.endswith(".yml")
            and len(filename.split(".")) == 2
            and not filename.startswith("_meta")
        ):
            controller_name = filename[:-4]  # Remove .yml
            yaml_data[controller_name] = {}

            filepath = os.path.join(YAML_DIR, filename)
            with open(filepath, "r", encoding="utf-8") as f:
                try:
                    data = yaml.safe_load(f)
                except Exception as e:
                    print(f"❌ Error reading {filename}: {e}")
                    continue

            if not data:
                continue

            for key, val in data.items():
                if key.startswith("/"):
                    # Default to GET if method isn't specified in YAML
                    method = val.get("method", "GET").upper()
                    yaml_data[controller_name][key] = method

    return yaml_data


def run_validation():
    print("🔍 Scanning C# Controllers and YAML Documentation...\n")

    cs_routes = get_csharp_routes()
    yml_routes = get_yaml_routes()

    errors_found = 0
    warnings_found = 0

    # Check C# against YAML (Missing docs or Method mismatches)
    for controller, routes in cs_routes.items():
        if not routes:
            continue  # Skip controllers with no API routes (e.g., base classes)

        yml_controller = yml_routes.get(controller, {})

        for path, method in routes.items():
            if path not in yml_controller:
                print(
                    f"❌ MISSING DOCS: '{path}' in {controller}.cs is not documented in YAML."
                )
                errors_found += 1
            else:
                yml_method = yml_controller[path]
                if method != yml_method:
                    print(
                        f"⚠️ METHOD MISMATCH: '{path}' in {controller} is {method} in C# but {yml_method} in YAML."
                    )
                    warnings_found += 1

    # Check YAML against C# (Stale/Deleted routes that are still in the docs)
    for controller, routes in yml_routes.items():
        cs_controller = cs_routes.get(controller, {})

        for path, method in routes.items():
            if path not in cs_controller:
                # Note: It might exist in a different controller, but we flag it for organization mismatch
                print(
                    f"👻 GHOST ROUTE: '{path}' is documented in {controller}.yml but does not exist in {controller}.cs."
                )
                warnings_found += 1

    # Summary
    print("\n" + "=" * 40)
    if errors_found == 0 and warnings_found == 0:
        print("✅ SUCCESS: Documentation perfectly matches C# codebase!")
    else:
        print(f"⚠️ SUMMARY: Found {errors_found} errors and {warnings_found} warnings.")
        print("=" * 40)
        sys.exit(1)


if __name__ == "__main__":
    run_validation()
