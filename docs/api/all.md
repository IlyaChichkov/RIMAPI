# 📚 Complete API Reference (All Endpoints)

This page contains the complete, unabridged list of all RIMAPI endpoints.

{% from 'api/_api_template.md' import render_controllers with context %}
{% set lang = config.theme.language if config.theme.language in api_data else 'en' %}
{{ render_controllers(api_data.get(lang, api_data['en']).controllers.keys()) }}