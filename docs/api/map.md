# 🌍 Map & Environment API

This section covers interacting with the physical game world, weather, buildings, and zones.

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'MapController', 
    'GlobalMapController',
    'BuilderController',
    'BillController'
]) }}