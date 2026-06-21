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
    """Scans all C# files recursively and returns a dict of {controller: {path: method}}."""
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


def _load_yaml_file(filepath):
    """Returns parsed YAML data or None on parse failure; prints the error."""
    with open(filepath, "r", encoding="utf-8") as f:
        try:
            return yaml.safe_load(f)
        except Exception as e:
            rel = os.path.relpath(filepath, YAML_DIR)
            print(f"❌ YAML ERROR in {rel}: {e}")
            return None


def _extract_routes(data):
    """Given a parsed YAML dict, return {path: method} for all route keys."""
    if not data:
        return {}
    return {
        key: val.get("method", "GET").upper()
        for key, val in data.items()
        if isinstance(key, str) and key.startswith("/")
    }


def get_yaml_routes():
    """
    Walks the entire YAML_DIR tree.
    Returns:
        base_routes:        {controller: {path: method}}   (from *.yml base files)
        translation_routes: {lang: {controller: {path: method}}}  (from *.lang.yml files)
        parse_errors:       count of files that failed to parse
    """
    base_routes = {}
    translation_routes = {}
    parse_errors = 0

    if not os.path.exists(YAML_DIR):
        print(f"❌ Error: YAML directory not found at {YAML_DIR}")
        return base_routes, translation_routes, parse_errors

    for dirpath, _, filenames in os.walk(YAML_DIR):
        for filename in filenames:
            if not filename.endswith(".yml"):
                continue
            if filename.startswith("_"):
                continue  # skip _meta.yml etc.

            filepath = os.path.join(dirpath, filename)
            parts = filename.split(".")  # e.g. ["GameController", "ru", "yml"]

            if len(parts) == 2:
                # Base English file: GameController.yml
                controller = parts[0]
                data = _load_yaml_file(filepath)
                if data is None:
                    parse_errors += 1
                    continue
                base_routes[controller] = _extract_routes(data)

            elif len(parts) == 3:
                # Translation file: GameController.ru.yml
                controller, lang = parts[0], parts[1]
                data = _load_yaml_file(filepath)
                if data is None:
                    parse_errors += 1
                    continue
                translation_routes.setdefault(lang, {})[controller] = _extract_routes(data)

    return base_routes, translation_routes, parse_errors


def run_validation():
    print("🔍 Scanning C# Controllers and YAML Documentation...\n")

    cs_routes = get_csharp_routes()
    base_routes, translation_routes, parse_errors = get_yaml_routes()

    errors_found = parse_errors
    warnings_found = 0

    # ── 1. C# vs base YAML ────────────────────────────────────────────────────

    # Missing docs and method mismatches
    for controller, routes in cs_routes.items():
        if not routes:
            continue

        yml_controller = base_routes.get(controller, {})
        for path, method in routes.items():
            if path not in yml_controller:
                print(f"❌ MISSING DOCS: '{path}' in {controller}.cs is not documented in YAML.")
                errors_found += 1
            else:
                yml_method = yml_controller[path]
                if method != yml_method:
                    print(
                        f"⚠️ METHOD MISMATCH: '{path}' in {controller} is {method} in C# but {yml_method} in YAML."
                    )
                    warnings_found += 1

    # Ghost routes (documented but deleted from C#)
    for controller, routes in base_routes.items():
        cs_controller = cs_routes.get(controller, {})
        for path in routes:
            if path not in cs_controller:
                print(
                    f"👻 GHOST ROUTE: '{path}' is documented in {controller}.yml but does not exist in {controller}.cs."
                )
                warnings_found += 1

    # ── 2. Translation checks ─────────────────────────────────────────────────

    for lang, lang_controllers in translation_routes.items():
        for controller, trans_routes in lang_controllers.items():

            # Orphaned translation: no base file exists
            if controller not in base_routes:
                print(
                    f"👻 ORPHANED TRANSLATION: {controller}.{lang}.yml has no base {controller}.yml."
                )
                warnings_found += 1
                continue

            base = base_routes[controller]

            # Routes in base missing from translation
            for path in base:
                if path not in trans_routes:
                    print(
                        f"🌐 MISSING TRANSLATION: '{path}' from {controller}.yml is not in {controller}.{lang}.yml."
                    )
                    warnings_found += 1

            # Routes in translation that no longer exist in base
            for path in trans_routes:
                if path not in base:
                    print(
                        f"👻 GHOST IN TRANSLATION: '{path}' in {controller}.{lang}.yml has no matching route in {controller}.yml."
                    )
                    warnings_found += 1

    # ── Summary ───────────────────────────────────────────────────────────────
    print("\n" + "=" * 40)
    if errors_found == 0 and warnings_found == 0:
        print("✅ SUCCESS: Documentation perfectly matches C# codebase!")
    else:
        print(f"⚠️ SUMMARY: Found {errors_found} errors and {warnings_found} warnings.")
        print("=" * 40)
        sys.exit(1)


if __name__ == "__main__":
    run_validation()
