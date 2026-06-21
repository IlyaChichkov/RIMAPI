# RIMAPI Compact Endpoint Index
*This file is optimized for LLM context windows and RAG ingestion.*

{# FORCE ENGLISH: Bypasses the i18n overwrite loop and fixes Windows encoding bugs #}
{% set current_api = api_data['en'] %}

{% for controller_name, controller in current_api.controllers.items() %}
{% for path, data in controller.items() %}
{% if path.startswith('/') %}
{{ data.method | default('GET') | upper }} {{ path }} - {{ data.desc | striptags | replace('\n', ' ') | truncate(150) }}
{% endif %}
{% endfor %}
{% endfor %}