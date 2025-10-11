# RIMAPI Setup Guide

## Installation

### Prerequisites
- RimWorld 1.4 or later
- Windows, Linux, or macOS
- .NET Framework 4.7.2 or later

### Installation Steps

1. **Download the Mod**
   - Obtain the RIMAPI mod files
   - Ensure the folder structure is:
   ```
   RimWorld/Mods/RIMAPI/
   ├── About/
   │   └── About.xml
   ├── Assemblies/
   │   └── RIMAPI.dll
   └── Defs/
       └── (mod configuration files)
   ```

2. **Activate the Mod**
   - Launch RimWorld
   - Go to **Options** → **Mod Settings**
   - Click **Mods**
   - Find "RIMAPI" in the list and check the box
   - Restart RimWorld if prompted

3. **Verify Installation**
   - Start a new game or load an existing save
   - Check the console for the message:
   ```
   RIMAPI: REST API Server started on port 8765
   ```

## Configuration

### Mod Settings

Access settings in-game:
1. Go to **Options** → **Mod Settings**
2. Select **RIMAPI** from the mod list

**Available Settings:**

| Setting | Default | Description |
|---------|---------|-------------|
| Server Port | 8765 | TCP port for the REST API server (1-65535) |
| Refresh Interval (ticks) | 300 | How often to refresh game data cache |

**Example Configuration:**
```csharp
// Default settings in code
public class RIMAPI_Settings : ModSettings
{
    public int serverPort = 8765;
    public int refreshIntervalTicks = 300;
}
```

### Port Configuration

**Choosing a Port:**
- Use ports between 1024-65535
- Avoid well-known ports (80, 443, 8080, etc.)
- Ensure the port is not in use by other applications

**Checking Port Availability:**
```bash
# Windows
netstat -an | find "8765"

# Linux/macOS
netstat -an | grep 8765
lsof -i :8765
```

### Firewall Configuration

**Windows:**
1. Open Windows Defender Firewall
2. Click "Allow an app or feature through Windows Defender Firewall"
3. Find "RimWorld" in the list or click "Allow another app"
4. Navigate to your RimWorld executable and add it
5. Ensure both private and public networks are checked

**Linux/macOS:**
```bash
# If using ufw (Ubuntu/Debian)
sudo ufw allow 8765

# If using firewalld (CentOS/RHEL)
sudo firewall-cmd --add-port=8765/tcp --permanent
sudo firewall-cmd --reload
```

## Testing the Installation

### Basic Connectivity Test

1. **Using Command Line (curl):**
   ```bash
   # Test if server is responding
   curl http://localhost:8765/api/v1/version
   
   # Expected response:
   # {"version":"1.0.0","rimWorldVersion":"1.4.0","modVersion":"1.0.0","apiVersion":"v1"}
   ```

2. **Using Web Browser:**
   - Open browser and navigate to: `http://localhost:8765/api/v1/version`
   - You should see JSON version information

3. **Using PowerShell (Windows):**
   ```powershell
   # Test connectivity
   Invoke-RestMethod -Uri "http://localhost:8765/api/v1/version"
   
   # Test with specific port
   Test-NetConnection -ComputerName localhost -Port 8765
   ```

### Advanced Testing

**Test All Endpoints:**
```bash
# Game state
curl http://localhost:8765/api/v1/game/state

# Colonists list
curl http://localhost:8765/api/v1/colonists

# Specific colonist (replace 1 with actual colonist ID)
curl http://localhost:8765/api/v1/colonists/1

# World information
curl http://localhost:8765/api/v1/world/info
```

**Test WebSocket Connection:**
```javascript
// Using JavaScript in browser console
const socket = new WebSocket('ws://localhost:8765/api/v1/events/stream');

socket.onopen = function(event) {
    console.log('WebSocket connected');
};

socket.onmessage = function(event) {
    console.log('Received:', JSON.parse(event.data));
};

socket.onclose = function(event) {
    console.log('WebSocket disconnected');
};
```

## Usage Examples

### C# Client Example

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class RimWorldApiClient
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public RimWorldApiClient(string baseUrl = "http://localhost:8765")
    {
        _client = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<GameState> GetGameStateAsync()
    {
        var response = await _client.GetAsync($"{_baseUrl}/api/v1/game/state");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GameState>(json);
    }

    public async Task<List<Colonist>> GetColonistsAsync()
    {
        var response = await _client.GetAsync($"{_baseUrl}/api/v1/colonists");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Colonist>>(json);
    }
}

// Usage
var client = new RimWorldApiClient();
var gameState = await client.GetGameStateAsync();
Console.WriteLine($"Game tick: {gameState.GameTick}");
```

### Python Client Example

```python
import requests
import json

class RimWorldClient:
    def __init__(self, base_url="http://localhost:8765"):
        self.base_url = base_url
    
    def get_version(self):
        response = requests.get(f"{self.base_url}/api/v1/version")
        response.raise_for_status()
        return response.json()
    
    def get_colonists(self, fields=None):
        params = {}
        if fields:
            params['fields'] = fields
            
        response = requests.get(f"{self.base_url}/api/v1/colonists", params=params)
        response.raise_for_status()
        return response.json()

# Usage
client = RimWorldClient()
version = client.get_version()
colonists = client.get_colonists(fields="name,health,mood")
```

### JavaScript/Node.js Client Example

```javascript
const axios = require('axios');

class RimWorldClient {
    constructor(baseUrl = 'http://localhost:8765') {
        this.client = axios.create({ baseURL: baseUrl });
    }

    async getGameState() {
        const response = await this.client.get('/api/v1/game/state');
        return response.data;
    }

    async getColonists(fields = null) {
        const params = fields ? { fields } : {};
        const response = await this.client.get('/api/v1/colonists', { params });
        return response.data;
    }
}

// Usage
const client = new RimWorldClient();
client.getGameState().then(state => {
    console.log(`Game tick: ${state.gameTick}`);
});
```

## Troubleshooting

### Common Issues

**1. "Connection Refused" Error**
```
Error: Unable to connect to http://localhost:8765
```
**Solutions:**
- Verify RimWorld is running with the mod loaded
- Check the mod is activated in Mod Settings
- Ensure the correct port is configured
- Check firewall settings

**2. "Port Already in Use" Error**
```
RIMAPI: Failed to start API server - Address already in use
```
**Solutions:**
- Change the server port in mod settings
- Identify and stop the application using the port
- Restart RimWorld after changing port

**3. Mod Not Loading**
```
No RIMAPI initialization message in console
```
**Solutions:**
- Verify mod installation directory structure
- Check RimWorld's Player.log for errors
- Ensure all dependencies are met
- Try reinstalling the mod

**4. API Returns 404 Errors**
```
{"error": "Endpoint not found"}
```
**Solutions:**
- Verify the URL path is correct
- Check that RimWorld has loaded a game
- Ensure the API server started successfully

### Debugging Steps

**1. Check RimWorld Logs**
- Location: `%AppData%\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log`
- Look for RIMAPI-related messages
- Check for any error messages

**2. Verify Server Status**
```bash
# Check if port is listening
netstat -an | findstr 8765

# Should show:
# TCP    0.0.0.0:8765           0.0.0.0:0              LISTENING
```

**3. Test Local Connectivity**
```bash
# Test from the same machine
telnet localhost 8765

# If successful, you should see a connected message
```

**4. Check Mod Loading Order**
- Ensure RIMAPI doesn't have conflicting dependencies
- Try placing RIMAPI later in the load order
- Check for mod conflicts

### Performance Considerations

**1. High CPU Usage**
- Reduce refresh interval in mod settings
- Use ETag caching in client applications
- Consider using WebSocket instead of frequent polling

**2. Memory Usage**
- The mod uses minimal memory for caching
- WebSocket connections consume memory per client
- Monitor with RimWorld's built-in performance tools

**3. Network Impact**
- API designed for local network use
- For remote access, consider reverse proxy
- Use field filtering to reduce payload size

## Security Considerations

### Current Security Model
- API is designed for local development use
- No authentication by default
- Bind to localhost only (not exposed to network)

### Production Deployment
For network exposure, consider:
1. **Reverse Proxy** (nginx, Apache)
2. **Authentication Layer**
3. **SSL/TLS Encryption**
4. **Rate Limiting**

### Example nginx Configuration
```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location /api/ {
        proxy_pass http://localhost:8765;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        
        # Add authentication if needed
        auth_basic "RIMAPI Access";
        auth_basic_user_file /etc/nginx/.htpasswd;
    }
}
```

## Support

### Getting Help
1. **Check Logs**: Always include relevant log entries
2. **Reproduce Issue**: Note steps to reproduce the problem
3. **Environment Details**: 
   - RimWorld version
   - Mod version
   - Operating System
   - Other active mods

### Reporting Issues
When reporting issues, include:
- Error messages from logs
- Steps to reproduce
- Expected vs actual behavior
- RimWorld and mod versions

### Common Solutions
- **Restart RimWorld** after changing settings
- **Verify mod load order** for conflicts
- **Check port availability** before changing settings
- **Update RimWorld** and mod to latest versions

This setup guide should help you get RIMAPI running and troubleshoot common issues. For advanced usage, refer to the API documentation and developer guide.
