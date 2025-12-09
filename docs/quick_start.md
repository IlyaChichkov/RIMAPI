# Quick Start Guide

Welcome to RIMAPI! This guide will help you install the mod, configure it, and make your first API call in under 5 minutes.

## 1. Prerequisites

- RimWorld version 1.5 or later.
- The **[Harmony](https://steamcommunity.com/workshop/filedetails/?id=2009463077)** mod must be installed and enabled.
- A basic understanding of what a REST API is.

## 2. Installation

You can install the mod from the Steam Workshop or manually.

### Method 1: Steam Workshop (Recommended)

1.  Subscribe to **[RIMAPI](https://steamcommunity.com/sharedfiles/filedetails/?id=3593423732)** on the Steam Workshop.
2.  Launch RimWorld, go to the `Mods` menu.
3.  Enable the **RIMAPI** mod.
4.  Ensure RIMAPI is loaded **after** Harmony.

### Method 2: Manual Installation

1.  Download the latest release from the [GitHub Releases](https://github.com/IlyaChichkov/RIMAPI/releases) page.
2.  Extract the ZIP file into your RimWorld `Mods` folder.
3.  Launch RimWorld and enable the mod in the `Mods` menu.

## 3. Configuration

After installing, you can configure the API server's port.

1.  In RimWorld's main menu, go to `Options` > `Mod Settings`.
2.  Select `RIMAPI` from the list of mods.
3.  Here you can change the **Server Port** (default is `8765`).

The API server starts automatically once you load into a colony.

## 4. Making Your First API Call

With the mod running and a colony loaded, you can now interact with the API. The following examples check the current game state.

=== "Bash (cURL)"

    ```bash
    curl http://localhost:8765/api/v1/game/state
    ```

=== "Python"

    ```python
    import requests

    try:
        response = requests.get('http://localhost:8765/api/v1/game/state')
        response.raise_for_status()  # Raises an exception for bad status codes
        print(response.json())
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
    ```

=== "JavaScript (Node.js)"

    ```javascript
    fetch('http://localhost:8765/api/v1/game/state')
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
      })
      .then(data => console.log(data))
      .catch(error => console.error('Error:', error));
    ```

=== "C#"

    ```csharp
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new HttpClient();
            try
            {
                var response = await client.GetStringAsync("http://localhost:8765/api/v1/game/state");
                Console.WriteLine(response);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
    ```

If successful, you'll receive a JSON response like this:

```json
{
  "success": true,
  "data": {
    "game_tick": 2236,
    "colony_wealth": 13442.50,
    "colonist_count": 3,
    "storyteller": "Cassandra Classic",
    "is_paused": false
  },
  "errors": [],
  "warnings": [],
  "timestamp": "2025-11-28T10:33:26.8675876Z"
}
```

## 5. Another Example: Get Colonists

Let's try a more practical example: fetching a list of your colonists.

```bash
curl http://localhost:8765/api/v1/colonists
```

This will return a list of your colonists with their basic information, such as ID, name, health, and mood.

## 6. Troubleshooting

-   **Connection Refused**:
    -   Ensure RimWorld is running and you have loaded into a colony. The API server does not run on the main menu.
    -   Check that the port in your API call matches the port configured in the mod settings (default is `8765`).
    -   Make sure a firewall is not blocking the connection.

-   **404 Not Found**:
    -   Double-check that the endpoint URL is correct.
    -   Ensure the RIMAPI mod is enabled and loaded correctly.

## 7. Integrating with Language Models

For developers looking to integrate RIMAPI with LLMs, there is file with documentation as simple txt file. This file can help streamline your development process.

<a href="https://ilyachichkov.github.io/RIMAPI/llms-full.txt" download="llms-full.txt" class="md-button md-button--primary">Download llms-full.txt</a>

## Next Steps

You're all set! Now you can start building amazing applications and tools that interact with RimWorld.

-   Explore the full **[API Reference](./api.md)** for all available endpoints.
-   Join our **[Discord](https://discord.gg/Css9b9BgnM)** to ask questions and share what you're building.