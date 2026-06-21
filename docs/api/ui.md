# 🖥️ UI & Camera API

This section provides endpoints for manipulating the in-game camera, opening game windows, and rendering visual overlays or images.

{% from 'api/_api_template.md' import render_controllers with context %}
{{ render_controllers([
    'CameraController',
    'WindowController',
    'OverlayController',
    'ImagesController'
]) }}