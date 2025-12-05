![alt text](About/preview.png)

![Status](https://img.shields.io/badge/Status-In_Progress-blue.svg)
![RimWorld Version](https://img.shields.io/badge/RimWorld-v1.5+-blue.svg)
![API Version](https://img.shields.io/badge/API-v0.1.0-green.svg)
![Build](https://github.com/IlyaChichkov/RIMAPI/actions/workflows/release_build.yml/badge.svg)
![Release](https://img.shields.io/github/v/release/IlyaChichkov/RIMAPI)

# RIMAPI

RIMAPI is a RimWorld mod that gives you an API Server to interact with your current game.

RIMAPI exposes a comprehensive REST API from inside RimWorld.
The API listens on `http://localhost:8765/` by default once the
game reaches the main menu. The port can be changed in the mod settings.

[  Documentation  ](https://ilyachichkov.github.io/RIMAPI/index.html)|
[  Discord Server  ](https://discord.gg/Css9b9BgnM)

## 🚀 Features

### Monitor current game state
- **Real-time colony status** - Get current game time, weather, storyteller, and difficulty
- **Colonist management** - Track health, mood, skills, inventory, and work priorities
- **Resource tracking** - Monitor food, medicine, materials, and storage utilization
- **Research progress** - Check current projects and completed research
- **Quests & incidents** - Get list of quests and incidents

### Game world manipulation

- **Camera Controls** - Set position, zoom, stream output
- **Interface Controls** - Select objects, open tabs
- **In development**</br>
  *item spawning, event triggering, zone management*

### Performance optimizations
- **Caching** - Efficient data updates without game lag
- **Field filtering** - Request only the data you need
- **ETag support** - Intelligent caching with 304 Not Modified responses
- **Non-blocking operations** - Game non-blocking API operations

## 🔍 Integrations

Share your projects - send integrations to be featured here

| Name | Link |
|---   |---   |
|Rimworld Dashboard | https://github.com/IlyaChichkov/rimapi-dashboard |
|Food Analysis Script (Python) | https://gist.github.com/IlyaChichkov/1c4455c9f797a277693ee5a3e016ac3d |

## 🛠️ Usage
1. Start new RimWorld game or load one from saves with the mod enabled. When game map is loaded the API server will begin listening.
2. The default address is `http://localhost:8765/`. You can change the port from the RIMAPI mod settings.
3. Use any HTTP client (curl, Postman, etc.) to call the endpoints.

## 📄 License
This project is licensed under the GNU GPLv3 License - see the [LICENSE](https://github.com/IlyaChichkov/RIMAPI/blob/main/LICENSE) file for details.

## 👥 Credits and Acknowledgments

Thanks to MasterPNJ and his project for insipiration: [ARROM](https://github.com/MasterPNJ/API-REST-RimwOrld-Mod)

Thanks to @braasdas and his [RatLab](https://github.com/braasdas/ratlab-mod-github)
for code reference

## 📋 Changelog

[CHANGELOG](https://github.com/IlyaChichkov/RIMAPI/blob/main/CHANGELOG)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.
