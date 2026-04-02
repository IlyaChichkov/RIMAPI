# 📦 Items & Economy API

This section covers spawning physical items, managing inventories, player designations, and trading with factions.

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'ThingsController',
    'TradeController',
    'OrderController'
]) }}