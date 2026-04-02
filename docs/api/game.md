# ⚙️ Game State & Mechanics API

This section handles the core game lifecycle (save/load/pause), triggering random events, managing factions, and research.

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'GameController',
    'GameEventsController',
    'FactionController',
    'ResearchController'
]) }}