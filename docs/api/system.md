# 🛠️ System & Developer Tools API

This section contains utility endpoints for developers, including cache management, API discovery, and general system diagnostics.

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'DevToolsController',
    'ServerCacheController',
    'DocumentationController',
    'General'
]) }}