# 🧠 Pawns & AI API

This section covers reading, spawning, editing, and controlling pawns, as well as managing group AI behavior (Lords).

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'PawnController',
    'PawnInfoController',
    'PawnSocialController',
    'PawnEditController',
    'PawnJobController',
    'PawnSpawnController',
    'LordController'
]) }}