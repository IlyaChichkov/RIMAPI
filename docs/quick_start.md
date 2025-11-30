# Quick Start Guide

Get up and running with RIMAPI in under 5 minutes.
This guide will help you install the mod, verify it's working, and make your first API call.

## Prerequisites

- RimWorld version 1.5 or later
- [Harmony mod](https://steamcommunity.com/workshop/filedetails/?id=2009463077)
- Basic understanding of REST APIs

## Installation

### Method 1: Steam Workshop
1. Subscribe to [RIMAPI](https://steamcommunity.com/sharedfiles/filedetails/?id=3593423732) on the Steam Workshop
2. Launch RimWorld and enable the mod in the mod list
3. Ensure it's loaded after Harmony and before any mods that depend on it

### Method 2: Manual Installation (Latest releases)
1. Download the latest release from [GitHub](https://github.com/IlyaChichkov/RIMAPI/releases)
2. Extract the ZIP file into your RimWorld `Mods` folder
3. Launch RimWorld and enable the mod

## Check API Status

Once the mod is installed and enabled:

1. Start a new game or load an existing colony
2. Check the server status

Call the GET endpoint with one of the examples below:

=== "Bash"

    ```bash
    curl http://localhost:8765/api/v1/game/state
    ```

=== "Python"

    ```python
    import requests

    response = requests.get('http://localhost:8765/api/v1/game/state')
    print(response.json())
    ```

=== "JavaScript"

    ```javascript
    fetch('http://localhost:8765/api/v1/game/state')
      .then(response => response.json())
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

Example response:

```json
{
  "success": true,
  "data": {
    "game_tick": 2236,
    "colony_wealth": 13442,
    "colonist_count": 3,
    "storyteller": "Cassandra",
    "is_paused": false
  },
  "errors": [],
  "warnings": [],
  "timestamp": "2025-11-28T10:33:26.8675876Z"
}
```

## Next Steps

Explore the full [API](api.md) reference for all available endpoints

