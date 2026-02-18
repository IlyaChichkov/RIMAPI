![alt text](../media/media/banner_title.jpg)

![Status](https://img.shields.io/badge/Status-In_Progress-blue.svg)
![RimWorld Version](https://img.shields.io/badge/RimWorld-v1.5+-blue.svg)
![API Version](https://img.shields.io/badge/API-v0.1.0-green.svg)
![Build](https://github.com/IlyaChichkov/RIMAPI/actions/workflows/release_build.yml/badge.svg)
![Release](https://img.shields.io/github/v/release/IlyaChichkov/RIMAPI)

# RimAPI

RimAPI is a RimWorld mod that embeds a high-performance REST API server directly into the game, allowing external applications to read and interact with your colony in real-time.

RimAPI exposes over **120+ comprehensive endpoints**. The API starts listening on `http://localhost:8765/` by default as soon as the game reaches the main menu.

<table>
  <tr>
    <td align="center">
      <a href="https://ilyachichkov.github.io/RIMAPI/index.html" target="_blank">
        <img src="../media/media/banner_documentation.jpg" alt="Documentation" width="520"/>
      </a>
    </td>
    <td align="center">
      <a href="https://discord.gg/Css9b9BgnM" target="_blank">
        <img src="../media/media/banner_discord.jpg" alt="Discord Server" width="520"/>
      </a>
    </td>
  </tr>
</table>

## 🚀 Features

### Monitor current game state
- **Game Controls** - Change game settings, modlist, start a new RimWorld game or load one from saves
- **Global & Colony map** - Get a list of caravans or items inside your storage
- **Real-time colony status** - Get current game time, weather, storyteller, and difficulty
- **Colonist management** - Track health, mood, skills, inventory, and work priorities
- **Resource tracking** - Monitor food, medicine, materials, and storage utilization
- **Research progress** - Check current projects and completed research
- **Quests & incidents** - Get list of quests and incidents

[  API Reference  ](https://ilyachichkov.github.io/RIMAPI/api.html)

### Performance optimizations
- **Caching** - Efficient data updates without game lag
- **Non-blocking operations** - Game non-blocking API operations
- **Field filtering [todo]** - Request only the data you need
- **ETag support [todo]** - Intelligent caching with 304 Not Modified responses

![alt text](../media/media/banner_get_colonist.jpg)

## 🔍 Integrations

Share your projects - send integrations on discord server

## 🛠️ Usage

1. Start new RimWorld game or load one from saves with the mod enabled. When the game map is loaded the API server will begin listening.
2. The default address is `http://localhost:8765/`. You can change the port from the RIMAPI mod settings.
3. Use any HTTP client (curl, Postman, etc.) to call the endpoints.

> More information in the documentation.

![alt text](../media/media/banner_post_edit_pawn.jpg)

## 📄 License
This project is licensed under the GNU GPLv3 License - see the [LICENSE](https://github.com/IlyaChichkov/RIMAPI/blob/main/LICENSE) file for details.

## 👥 Credits and Acknowledgments

Thanks to MasterPNJ and his project for inspiration.

Thanks to @braasdas and his RatLab project for code reference.

Contributors: @braasdas, @M4x28

## 📌 Links

- [RimWorld Dashboard [#Web]](https://github.com/IlyaChichkov/rimapi-dashboard)
- [RatLab [#Mod]](https://github.com/braasdas/ratlab-mod-github)
- [RimAPI MCP [#AI]](https://github.com/M4x28/RimAPI_MCP_Server)
- [ARROM [#Analogs]](https://github.com/MasterPNJ/API-REST-RimwOrld-Mod)

## 📋 Changelog

[CHANGELOG](https://github.com/IlyaChichkov/RIMAPI/blob/main/CHANGELOG)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

[  Contributing  ](https://ilyachichkov.github.io/RIMAPI/contributors_guide/contribute.html)
