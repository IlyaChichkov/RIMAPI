# RIMAPI
RIMAPI is a Rimworld mod that give you an API Server to interact with your current game.

RIMAPI exposes a small REST API from inside RimWorld. The API listens on `http://localhost:8765/` by default once the
game reaches the main menu. The port can be changed in the mod settings.

## Usage
1. Start RimWorld with the mod enabled. When the main menu loads the API server will begin listening.
2. The default address is `http://localhost:8765/`. You can change the port from the RIMAPI mod settings.
3. Use any HTTP client (curl, Postman, etc.) to call the endpoints.

Example:
```bash
curl http://localhost:8765/colony
```
This returns a JSON summary of your colony.

## Endpoints
- `/colony`  summary information about the current colony
- `/letters`  recent in-game letters
- `/colonists`  list of free colonists
- `/colonists/<id>`  detail about a specific colonist
- `/mods`  list of loaded mods
- `/factions`  relations with other factions
- `/research`  current research project and finished list
- `/buildings`  list colony buildings (filter with `?type=`)
- `/map`  current map status (weather, temperature...)
- `/animals`  animals owned by the colony
- `/storage`  stockpiles and storage contents
- `/alerts`  active alerts
- `/jobs`  colonist jobs and queues
- `/map/tiles`  list of all map tile coordinates
- `/map/tiles/<x>/<y>`  details of a map tile

## Endpoint details

### `/colony`
Returns basic information about the currently loaded colony:

```json
{
  "colonyName": "Colony name",
  "colonistCount": 3,
  "wealth": 12345.0
}
```

### `/letters`
List of recent letters shown in game:

```json
[
  {
    "label": "Raid!",
    "type": "NegativeEvent",
    "arrivalTime": 123456.0
  }
]
```

### `/colonists`
Summary for all free colonists on the current map:

```json
[
  {
    "id": 1,
    "name": "Pawn",
    "age": 24,
    "gender": "Male",
    "position": { "x": 0, "y": 0 },
    "mood": 75.0,
    "health": 1.0,
    "currentJob": "CutPlant",
    "traits": ["Industrious"],
    "workPriorities": [
      { "workType": "Doctor", "priority": 2 }
    ]
  }
]
```

### `/colonists/<id>`
Detailed data for a single colonist including needs, skills and inventory:

```json
{
  "id": 1,
  "name": "Pawn",
  "backstory": "Colony settler",
  "gender": "Male",
  "age": 24,
  "lifeStage": "Adult",
  "mood": 75.0,
  "needs": [ { "need": "Food", "level": 80.0 } ],
  "health": 1.0,
  "hediffs": [],
  "bleedingRate": 0.0,
  "isDowned": false,
  "isDrafted": false,
  "currentJob": "CutPlant",
  "skills": [ { "skill": "Plants", "level": 6, "passion": "None" } ],
  "equipment": [],
  "apparel": [],
  "inventory": [],
  "relations": []
}
```

### `/mods`
List of all currently loaded mods:

```json
[
  { "name": "Core", "packageId": "Ludeon.RimWorld" }
]
```

### `/factions`
List of non-player factions and their relation to the colony:

```json
[
  {
    "name": "Pirates",
    "def": "Pirate",
    "relation": "Hostile",
    "goodwill": -100
  }
]
```

### `/research`
Current research progress and completed projects:

```json
{
  "currentProject": "Hydroponics",
  "progress": 0.25,
  "finishedProjects": ["Stonecutting"]
}
```

### `/buildings`
Returns the colony buildings. You can filter by type with `?type=`:

```json
[
  { "id": 1, "def": "Battery", "position": { "x": 10, "y": 5 } }
]
```

### `/map`
Information about the current map:

```json
{
  "weather": "Rain",
  "temperature": 22.5,
  "hour": 13,
  "season": "Summer"
}
```

### `/animals`
List of colony animals:

```json
[
  {
    "id": 3,
    "name": "Muffalo",
    "def": "Muffalo",
    "position": { "x": 5, "y": 30 },
    "trainer": 1,
    "trainings": { "Tameness": 3 },
    "pregnant": false
  }
]
```

### `/storage`
Content of stockpiles and storage buildings:

```json
[
  {
    "name": "Stockpile Zone 1",
    "items": { "Steel": 75, "WoodLog": 100 }
  }
]
```

### `/alerts`
Active in-game alerts:

```json
[
  { "label": "Need colonist beds", "priority": "High" }
]
```

### `/jobs`
Current job and queued jobs for each colonist:

```json
[
  {
    "id": 1,
    "name": "Pawn",
    "current": "CutPlant",
    "queue": ["Haul", "Research"]
  }
]
```

### `/map/tiles`
List all map tile coordinates:

```json
[
  { "x": 0, "y": 0 },
  { "x": 0, "y": 1 }
]
```

### `/map/tiles/<x>/<y>`
Detail about a single map tile:

```json
{
  "terrain": "Soil",
  "zone": "Growing Zone",
  "things": ["PlantPotato"]
}
```


For 1.5:

Newtonsoft.Json - Portable40/5.0.8