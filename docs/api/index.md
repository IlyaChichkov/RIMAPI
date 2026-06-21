{% set lang = config.theme.language if config.theme.language in api_data else 'en' %}
{% set current_api = api_data.get(lang, api_data['en']) %}

{# --- Dynamically count endpoints --- #}
{% set ns = namespace(endpoint_count=0) %}
{% for controller_name, controller in current_api.controllers.items() %}
    {% for path in controller.keys() %}
        {% if path.startswith('/') %}
            {% set ns.endpoint_count = ns.endpoint_count + 1 %}
        {% endif %}
    {% endfor %}
{% endfor %}

# 📚 API Reference Overview

**Version**: {{ current_api.meta.version | default('1.8.3') }}  
**Endpoints total count**: {{ ns.endpoint_count }}

!!! warning "Warning"
    Please read our [API Conventions](../developer_guide/api_conventions.md) page to understand our `snake_case` JSON requirements and header rules before using these endpoints.

Welcome to the RIMAPI Reference. Choose a category below to view the available endpoints:

* [**Map & Environment**](map.md) - Weather, zones, terrain, and buildings.
* [**Pawns & AI**](pawns.md) - Pawn spawning, jobs, medical, editing, and Lord behavior.
* [**Items & Economy**](things.md) - Physical items, inventories, trading, and player designations.
* [**Game State & Mechanics**](game.md) - Save/load, random events, factions, and research.
* [**UI & Camera**](ui.md) - Viewports, windows, images, and visual overlays.
* [**System & Developer Tools**](system.md) - Cache management, API discovery, and general system diagnostics.

---

### 🤖 LLMs & Power Users
If you are using an AI coding assistant or simply prefer to search through a single, monolithic document, you can view the [**Complete API Reference (All Endpoints)**](all.md) here.