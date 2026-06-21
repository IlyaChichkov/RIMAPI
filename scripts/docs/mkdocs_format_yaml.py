import os
from ruamel.yaml import YAML


def make_yaml():
    y = YAML()
    y.preserve_quotes = True
    # Keep block style for multi-line strings instead of collapsing to flow style
    y.default_flow_style = False
    y.width = float("inf")  # Prevents random line wrapping
    # YAML keys must be unique; when a file has duplicate route keys (e.g. GET
    # and POST on the same path), don't crash — keep the last occurrence, which
    # matches pyyaml's silent behaviour and what the checker already relies on.
    y.allow_duplicate_keys = True
    return y


def format_all_yaml(directory):
    y = make_yaml()
    for filename in os.listdir(directory):
        if filename.endswith(".yml"):
            filepath = os.path.join(directory, filename)

            with open(filepath, "r", encoding="utf-8") as f:
                try:
                    data = y.load(f)
                except Exception as exc:
                    print(f"Failed to read {filename}: {exc}")
                    continue

            if data is None:
                continue

            with open(filepath, "w", encoding="utf-8", newline="\n") as f:
                y.dump(data, f)

            print(f"Cleaned up {filename}")


controllers_dir = os.path.join("docs", "_api_macroses", "controllers")
if os.path.exists(controllers_dir):
    format_all_yaml(controllers_dir)
    print("\nFormatting complete! Your YAML files are now readable.")
else:
    print(f"Could not find directory: {controllers_dir}")
