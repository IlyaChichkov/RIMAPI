import os
import yaml

# Ensure the output directory exists
output_dir = "docs/_api_macroses/controllers"
os.makedirs(output_dir, exist_ok=True)

base_dir = "docs/_api_macroses"

# Scan for any file that looks like api.yml, api.ja.yml, etc.
for filename in os.listdir(base_dir):
    if filename.startswith("api") and filename.endswith(".yml"):
        filepath = os.path.join(base_dir, filename)

        # Determine the language suffix (e.g., '', '.ja', '.ru')
        parts = filename.split(".")
        lang_suffix = ""
        if len(parts) == 3:  # it's something like api.ja.yml
            lang_suffix = f".{parts[1]}"

        print(f"Slicing {filename}...")

        with open(filepath, "r", encoding="utf-8") as f:
            data = yaml.safe_load(f)

        if not data or "api" not in data:
            continue

        api_data = data["api"]

        # Save the top-level stuff (page_title, section) into a _meta file
        meta = {}
        if "page_title" in api_data:
            meta["page_title"] = api_data["page_title"]
        if "section" in api_data:
            meta["section"] = api_data["section"]

        if meta:
            meta_path = os.path.join(output_dir, f"_meta{lang_suffix}.yml")
            with open(meta_path, "w", encoding="utf-8") as mf:
                yaml.dump(meta, mf, allow_unicode=True, sort_keys=False)

        # 2. Slice the controllers
        if "controllers" in api_data:
            for ctrl_name, ctrl_data in api_data["controllers"].items():
                ctrl_path = os.path.join(output_dir, f"{ctrl_name}{lang_suffix}.yml")
                with open(ctrl_path, "w", encoding="utf-8") as cf:
                    yaml.dump(ctrl_data, cf, allow_unicode=True, sort_keys=False)

        print(f"Finished migrating {filename}!")

print("\nMigration complete! You can now safely delete the old api*.yml files.")
