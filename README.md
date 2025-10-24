![alt text](About/preview.png)

![Status](https://img.shields.io/badge/Status-In_Progress-blue.svg)
![RimWorld Version](https://img.shields.io/badge/RimWorld-v1.5+-blue.svg)
![API Version](https://img.shields.io/badge/API-v0.1.0-green.svg)
![Build](https://github.com/IlyaChichkov/RIMAPI/actions/workflows/build.yml/badge.svg)
![Release](https://img.shields.io/github/v/release/IlyaChichkov/RIMAPI)

# RIMAPI
RIMAPI is a RimWorld mod that gives you an API Server to interact with your current game.

RIMAPI exposes a comprehensive REST API from inside RimWorld.
The API listens on `http://localhost:8765/` by default once the
game reaches the main menu. The port can be changed in the mod settings.

[  API Documentation  ](https://github.com/IlyaChichkov/RIMAPI/blob/main/Docs/API.md)|
[  For developers  ](https://github.com/IlyaChichkov/RIMAPI/blob/main/Docs/DEVELOPER.md)

## 🚀 Features

### Monitor current game state
- **Real-time colony status** - Get current game time, weather, storyteller, and difficulty
- **Colonist management** - Track health, mood, skills, inventory, and work priorities
- **Resource tracking** - Monitor food, medicine, materials, and storage utilization
- **Research progress** - Check current projects and completed research

### Game world manipulation

- **In development**</br>
  *camera control, item spawning, event triggering, zone management*

### Performance optimizations
- **Caching** - Efficient data updates without game lag
- **Field filtering** - Request only the data you need
- **ETag support** - Intelligent caching with 304 Not Modified responses
- **Non-blocking operations** - Game non-blocking API operations

## 🛠️ Usage
1. Start RimWorld with the mod enabled. When the main menu loads the API server will begin listening.
2. The default address is `http://localhost:8765/`. You can change the port from the RIMAPI mod settings.
3. Use any HTTP client (curl, Postman, etc.) to call the endpoints.

### 🎯 Example
**Request:**
```bash
curl http://localhost:8765/api/v1/colonists
```

**Response:**
```json
{
  "colonists": [
    {
      "id": 123,
      "name": "John",
      "age": 32,
      "health": 1.0
    }
  ]
}
```

> Note: This mod is under active development. API endpoints may change between versions.
  Always check /api/v1/version for compatibility information.

## 🔍 Integrations

Share your projects - send integrations to be featured here

| Name | Link |
|---   |---   |
|Rimworld Dashboard | Upcoming soon|

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](https://github.com/IlyaChichkov/RIMAPI/blob/main/LICENSE) file for details.

## Credits and Acknowledgments

This project started as a fork of ARROM by MasterPNJ.
- Original Repository: [ARROM](https://github.com/MasterPNJ/API-REST-RimwOrld-Mod)

A significant portion of the code has been rewritten and new features have been added, but the initial inspiration and base came from the aforementioned project, which is released under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.
