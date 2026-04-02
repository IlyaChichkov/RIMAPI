{% macro render_controllers(controller_names) %}
{% set lang = config.theme.language if config.theme.language in api_data else 'en' %}
{% set current_api = api_data.get(lang, api_data['en']) %}

{% for name in controller_names %}
{% set controller = current_api.controllers.get(name) %}
{% if controller %}

{{ controller.title | default('### ' ~ name) }}

{{ controller.desc | default('') }}

{% for path, data in controller.items() %}
{% if path.startswith('/') %}
---

<h4 id="{{ path | replace('/', '') | replace('-', '') | replace('{', '') | replace('}', '') | lower }}">
<div class="doc-api-container">
<div class="doc-api-header">
<div class="doc-api-method doc-api-method-{{ data.method | default('GET') | lower }}">{{ data.method | default('GET') | upper }}</div>
<div class="doc-api-endpoint">
<code>
```text
{{ path }}
```
</code>
<a class="headerlink" href="#{{ path | replace('/', '') | replace('-', '') | replace('{', '') | replace('}', '') | lower }}" title="Permanent link">¶</a>
</div>
</div>
</div>
</h4>

{% if data.tags %}
<div class="doc-api-tags" style="margin-bottom: 15px; display: flex; gap: 8px; flex-wrap: wrap;">
{% for tag in data.tags %}
<span class="doc-tag">{{ tag }}</span>
{% endfor %}
</div>
{% endif %}

{{ data.desc | default('') }}

{{ data.curl | default('') }}

{{ data.request | default('') }}

{{ data.response | default('') }}

{% if data.github_link %}
<div class="doc-github-container">
<a href="{{ data.github_link }}" class="doc-github-link">
{{ data.github_name | default('View Source') }}
</a>
</div>
{% endif %}

{% endif %}
{% endfor %}

{% endif %}
{% endfor %}
{% endmacro %}