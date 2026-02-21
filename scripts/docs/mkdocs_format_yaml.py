import os
import yaml


# Teach PyYAML to use the '|' block style for any string with newlines
def str_presenter(dumper, data):
    if len(data.splitlines()) > 1 or "\n" in data:
        # Removes trailing whitespaces that can confuse the dumper
        cleaned_data = "\n".join([line.rstrip() for line in data.splitlines()])
        return dumper.represent_scalar("tag:yaml.org,2002:str", cleaned_data, style="|")
    return dumper.represent_scalar("tag:yaml.org,2002:str", data)


yaml.add_representer(str, str_presenter)
yaml.representer.SafeRepresenter.add_representer(str, str_presenter)


# Reformat the files
def format_all_yaml(directory):
    for filename in os.listdir(directory):
        if filename.endswith(".yml"):
            filepath = os.path.join(directory, filename)

            # Read the messy data
            with open(filepath, "r", encoding="utf-8") as f:
                try:
                    data = yaml.safe_load(f)
                except yaml.YAMLError as exc:
                    print(f"Failed to read {filename}: {exc}")
                    continue

            # Write it back cleanly
            with open(filepath, "w", encoding="utf-8") as f:
                yaml.dump(
                    data,
                    f,
                    allow_unicode=True,
                    sort_keys=False,
                    default_flow_style=False,
                    width=float("inf"),  # Prevents random line wrapping
                )
            print(f"Cleaned up {filename}")


# Run the formatter on the controllers directory
controllers_dir = os.path.join("docs", "_api_macroses", "controllers")
if os.path.exists(controllers_dir):
    format_all_yaml(controllers_dir)
    print("\nFormatting complete! Your YAML files are now readable.")
else:
    print(f"Could not find directory: {controllers_dir}")
