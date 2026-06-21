# 📊 RIMAPI Capabilities Matrix

Welcome to the RIMAPI feature roadmap! This matrix provides a high-level overview of which RimWorld game mechanics are currently accessible via the REST API. 

Use this page to quickly discover what you can build, check our current development progress across the DLCs, and navigate directly to the relevant API endpoints.

---

## 🏕️ Base Game Core (~15% Covered)
*The fundamental building blocks of colony survival, including map management, pawn jobs, health, architecture, work bills and etc.*

??? abstract "View Core Game Capabilities"
    **🌍 Map & Environment**
    Interact with the physical world, weather, and terrain.
    
    * [Manage Map Zones (Growing, Stockpiles)](../api/map.md)
    * [Change Weather & Time](../api/map.md)
    * [Spawn Drop Pods & Items](../api/map.md)
    * [Read Terrain & Fog of War Grids](../api/map.md)

    **🧠 Pawns & AI**
    Control your colonists, their health, and their daily tasks.
    
    * [Edit Pawn Skills, Traits, & Passions](../api/pawns.md)
    * [Force Jobs & Interrupt Current Actions](../api/pawns.md)
    * [Manage Medical Needs & Bed Rest](../api/pawns.md)
    * [Spawn New Pawns & Animals](../api/pawns.md)

    **🏭 Buildings & Production**
    Control the colony's infrastructure and manufacturing.
    
    * [Toggle Power Grids & Read Batteries](../api/map.md)
    * [Queue, Reorder, and Edit Production Bills](../api/map.md)
    * [Query Available Work Table Recipes](../api/map.md)

    **📦 Items & Economy**
    Manage physical items, inventories, and player designations.
    
    * [Spawn and Manage Physical Things](../api/things.md)
    * [Manage Player Orders (Mine, Haul, Cut, etc.)](../api/things.md)
    * [Trade with Factions](../api/things.md)

    **⚙️ Game State & UI**
    Control the game engine itself.
    
    * [Read Colony Wealth & Statistics](../api/game.md)
    * [Control Game Speed & Pausing](../api/game.md)
    * [Trigger Main Menu & App Shutdown](../api/game.md)
    * [Move & Zoom the In-Game Camera](../api/ui.md)

    **🛠️ System & Developer Tools**
    Utility endpoints for tooling, caching, and discovery.
    
    * [List all REST Endpoints (Discovery)](../api/system.md)
    * [Manage Server Cache](../api/system.md)

---

## 👁️ Ideology DLC (0% Covered)
*Control the belief systems, moral guides, and rituals of your colony.*

---

## 🌑 Anomaly DLC (0% Covered)
*Manage containment facilities, dark research, and entity attacks.*

---

## 👑 Royalty DLC (0% Covered)
*Interact with the fallen empire, royal titles, and psychic powers.*

---

## 🧬 Biotech DLC (0% Covered)
*Control mechanitors, xenogenetics, and pollution management.*

---

*Don't see a feature you need? [Open an issue on our GitHub](#) to request a new endpoint!*