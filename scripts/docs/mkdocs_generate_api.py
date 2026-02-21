import os
import yaml


def define_env(env):
    api_data = {}
    controllers_dir = os.path.join(
        env.project_dir, "docs", "_api_macroses", "controllers"
    )

    if os.path.exists(controllers_dir):
        # 1. Load the English (Base) files from the root controllers directory
        for item in sorted(os.listdir(controllers_dir)):
            filepath = os.path.join(controllers_dir, item)

            if os.path.isfile(filepath) and item.endswith(".yml"):
                name = item.split(".")[
                    0
                ]  # e.g., BuilderController.yml -> BuilderController
                lang = "en"

                if lang not in api_data:
                    api_data[lang] = {"meta": {}, "controllers": {}}

                with open(filepath, "r", encoding="utf-8") as f:
                    content = yaml.safe_load(f)

                if name == "_meta":
                    api_data[lang]["meta"] = content
                else:
                    api_data[lang]["controllers"][name] = content

        # 2. Load the Translations from subdirectories (like /ja, /ru)
        for lang_folder in sorted(os.listdir(controllers_dir)):
            lang_path = os.path.join(controllers_dir, lang_folder)

            if os.path.isdir(lang_path):
                lang = lang_folder

                if lang not in api_data:
                    api_data[lang] = {"meta": {}, "controllers": {}}

                for item in sorted(os.listdir(lang_path)):
                    if item.endswith(".yml"):
                        name = item.split(".")[
                            0
                        ]  # e.g., BuilderController.ja.yml -> BuilderController
                        filepath = os.path.join(lang_path, item)

                        with open(filepath, "r", encoding="utf-8") as f:
                            content = yaml.safe_load(f)

                        if name == "_meta":
                            api_data[lang]["meta"] = content
                        else:
                            api_data[lang]["controllers"][name] = content

    # Register the multi-language dictionary to MkDocs
    env.variables["api_data"] = api_data
