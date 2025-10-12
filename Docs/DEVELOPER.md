# RIMAPI Developer Guide

## Prerequisites

- RimWorld 1.4+
- Visual Studio 2019+ or VSCode
- .NET Framework 4.7.2
- Basic C# and RimWorld modding knowledge

## Project Structure

```
RimworldRestApi/
├── Core/                    # Server core components
│   ├── ApiServer.cs        # Main HTTP server
│   ├── Router.cs           # Request routing
│   └── ResponseBuilder.cs  # HTTP response formatting
├── Controllers/            # API endpoint handlers
│   ├── BaseController.cs   # Common controller functionality
│   ├── GameController.cs   # Game state endpoints
│   └── ColonistsController.cs # Colonist data endpoints
├── Services/               # Business logic layer
│   ├── IGameDataService.cs # Data access interface
│   └── GameDataService.cs  # RimWorld data implementation
├── Models/                 # Data transfer objects
│   ├── DTOs/              # Request/response models
│   └── Entities/          # Internal data models
├── WebSockets/            # Real-time communication
│   └── WebSocketManager.cs # WebSocket connection management
└── Utilities/             # Helper classes
    └── HashUtility.cs     # ETag generation
```

## Adding New API Endpoints

### Step 1: Create Data Transfer Object (DTO)

Create a new model in `Models/DTOs/`:

```csharp
namespace RimworldRestApi.Models
{
    public class ResearchProjectDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public float Progress { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
    }
}
```

### Step 2: Extend Data Service

Add method to `IGameDataService`:

```csharp
public interface IGameDataService
{
    // Existing methods...
    List<ResearchProjectDto> GetResearchProjects();
    ResearchProjectDto GetResearchProject(string projectId);
}
```

Implement in `GameDataService`:

```csharp
public List<ResearchProjectDto> GetResearchProjects()
{
    var projects = new List<ResearchProjectDto>();
    
    try
    {
        var researchManager = Find.ResearchManager;
        if (researchManager == null) return projects;

        // Access RimWorld research data
        foreach (var project in DefDatabase<ResearchProjectDef>.AllDefs)
        {
            var progress = researchManager.GetProgress(project);
            projects.Add(new ResearchProjectDto
            {
                Id = project.defName,
                Name = project.label,
                Progress = progress / project.baseCost,
                Description = project.description,
                Cost = project.baseCost
            });
        }
    }
    catch (Exception ex)
    {
        Log.Error($"RIMAPI: Error getting research projects - {ex.Message}");
    }
    
    return projects;
}
```

### Step 3: Create Controller

Create new controller or extend existing one:

```csharp
using System.Threading.Tasks;
using System.Net;

namespace RimworldRestApi.Controllers
{
    public class ResearchController : BaseController
    {
        private readonly IGameDataService _gameDataService;

        public ResearchController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        public async Task GetResearchProjects(HttpListenerContext context)
        {
            var projects = _gameDataService.GetResearchProjects();
            
            await HandleETagCaching(context, projects, data =>
                GenerateHash(data.Count, data.Max(p => p.Progress))
            );
        }

        public async Task GetResearchProject(HttpListenerContext context)
        {
            var projectId = context.Request.QueryString["id"];
            if (string.IsNullOrEmpty(projectId))
            {
                await ResponseBuilder.Error(context.Response, 
                    HttpStatusCode.BadRequest, "Project ID required");
                return;
            }

            var project = _gameDataService.GetResearchProject(projectId);
            if (project == null)
            {
                await ResponseBuilder.Error(context.Response, 
                    HttpStatusCode.NotFound, "Research project not found");
                return;
            }

            await HandleETagCaching(context, project, data =>
                GenerateHash(data.Id, data.Progress)
            );
        }
    }
}
```

### Step 4: Register Routes

Add routes in `ApiServer.RegisterRoutes()`:

```csharp
private void RegisterRoutes()
{
    // Existing routes...
    
    // Research endpoints
    _router.AddRoute("GET", "/api/v1/research/projects", context => 
        new ResearchController(_gameDataService).GetResearchProjects(context));
        
    _router.AddRoute("GET", "/api/v1/research/projects/{id}", context => 
        new ResearchController(_gameDataService).GetResearchProject(context));
}
```

## Adding Real-time Events

### Step 1: Define Event Type

```csharp
public class ResearchEvent
{
    public string Type { get; set; } // "researchStarted", "researchCompleted"
    public string ProjectId { get; set; }
    public float Progress { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Step 2: Extend WebSocket Manager

Add broadcast method to `WebSocketManager`:

```csharp
public void BroadcastResearchEvent(ResearchEvent researchEvent)
{
    var message = new
    {
        type = "researchUpdate",
        data = researchEvent,
        timestamp = DateTime.UtcNow
    };

    _ = BroadcastMessageAsync(message);
}
```

### Step 3: Hook into RimWorld Events

Extend `GameDataService` to monitor research changes:

```csharp
public class GameDataService : IGameDataService
{
    private readonly WebSocketManager _webSocketManager;
    private string _currentResearchProject;
    private float _lastResearchProgress;

    public void MonitorResearchChanges()
    {
        // This would be called periodically or from GameComponentTick
        var currentProject = Find.ResearchManager?.currentProj;
        var currentProgress = Find.ResearchManager?.GetProgress(currentProject) ?? 0;
        
        if (currentProject?.defName != _currentResearchProject)
        {
            // Research project changed
            _webSocketManager.BroadcastResearchEvent(new ResearchEvent
            {
                Type = "researchStarted",
                ProjectId = currentProject?.defName,
                Progress = currentProgress
            });
            
            _currentResearchProject = currentProject?.defName;
        }
        else if (Math.Abs(currentProgress - _lastResearchProgress) > 0.01f)
        {
            // Research progress updated
            _webSocketManager.BroadcastResearchEvent(new ResearchEvent
            {
                Type = "researchProgress",
                ProjectId = currentProject?.defName,
                Progress = currentProgress
            });
            
            _lastResearchProgress = currentProgress;
        }
    }
}
```

## Implementing Field Filtering

### Basic Field Filtering

The `BaseController` automatically handles field filtering:

```csharp
// Client request with fields
GET /api/v1/colonists?fields=name,health,skills

// Controller automatically filters response to only include specified fields
```

### Custom Field Processing

For complex objects, override field filtering:

```csharp
public async Task GetComplexData(HttpListenerContext context)
{
    var complexData = _gameDataService.GetComplexData();
    var fields = context.Request.QueryString["fields"];
    
    // Custom filtering logic
    var filteredData = ApplyCustomFieldFiltering(complexData, fields);
    
    await HandleETagCaching(context, filteredData, data => 
        GenerateHash(data.Version, data.Timestamp));
}

private object ApplyCustomFieldFiltering(ComplexDto data, string fields)
{
    if (string.IsNullOrEmpty(fields)) return data;
    
    // Implement custom field selection logic
    var fieldList = fields.Split(',');
    var result = new ExpandoObject();
    
    foreach (var field in fieldList)
    {
        switch (field.Trim())
        {
            case "summary":
                ((IDictionary<string, object>)result).Add("summary", data.GetSummary());
                break;
            case "details":
                ((IDictionary<string, object>)result).Add("details", data.GetDetails());
                break;
            // Add more custom field mappings
        }
    }
    
    return result;
}
```

## Error Handling Best Practices

### Controller Error Handling

```csharp
public async Task GetDataWithValidation(HttpListenerContext context)
{
    try
    {
        // Validate parameters
        var idParam = context.Request.QueryString["id"];
        if (!int.TryParse(idParam, out int id) || id <= 0)
        {
            await ResponseBuilder.Error(context.Response, 
                HttpStatusCode.BadRequest, "Invalid ID parameter");
            return;
        }

        // Business logic validation
        var data = _gameDataService.GetData(id);
        if (data == null)
        {
            await ResponseBuilder.Error(context.Response, 
                HttpStatusCode.NotFound, "Data not found");
            return;
        }

        // Success response
        await HandleETagCaching(context, data, d => GenerateHash(d.Id, d.Version));
    }
    catch (Exception ex)
    {
        Log.Error($"RIMAPI: Error in GetDataWithValidation - {ex}");
        await ResponseBuilder.Error(context.Response, 
            HttpStatusCode.InternalServerError, "Internal server error");
    }
}
```

### Custom Exception Types

```csharp
public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    
    public ApiException(HttpStatusCode statusCode, string message) 
        : base(message)
    {
        StatusCode = statusCode;
    }
}

// Usage in service layer
public ColonistDto GetColonist(int id)
{
    var colonist = FindColonistById(id);
    if (colonist == null)
        throw new ApiException(HttpStatusCode.NotFound, "Colonist not found");
        
    return colonist;
}
```

## Testing Your Extensions

### Unit Testing Controllers

```csharp
[TestClass]
public class ResearchControllerTests
{
    private ResearchController _controller;
    private Mock<IGameDataService> _mockGameDataService;

    [TestInitialize]
    public void Setup()
    {
        _mockGameDataService = new Mock<IGameDataService>();
        _controller = new ResearchController(_mockGameDataService.Object);
    }

    [TestMethod]
    public async Task GetResearchProjects_ReturnsProjects()
    {
        // Arrange
        var projects = new List<ResearchProjectDto>
        {
            new ResearchProjectDto { Id = "project1", Progress = 0.5f }
        };
        _mockGameDataService.Setup(x => x.GetResearchProjects()).Returns(projects);
        
        var context = CreateMockContext("/api/v1/research/projects");

        // Act
        await _controller.GetResearchProjects(context);

        // Assert
        // Verify response contains expected data
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class ApiIntegrationTests
{
    private ApiServer _server;

    [TestInitialize]
    public void Setup()
    {
        _server = new ApiServer(8765, new GameDataService());
        _server.Start();
    }

    [TestMethod]
    public async Task GetVersion_ReturnsValidResponse()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("http://localhost:8765/api/v1/version");
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var version = JsonSerializer.Deserialize<VersionDto>(content);
        
        Assert.IsNotNull(version);
        Assert.IsFalse(string.IsNullOrEmpty(version.ApiVersion));
    }

    [TestCleanup]
    public void Cleanup()
    {
        _server?.Dispose();
    }
}
```

## Performance Optimization

### Efficient Data Access

```csharp
public List<ColonistDto> GetColonistsOptimized()
{
    // Use RimWorld's optimized access patterns
    var map = Find.CurrentMap;
    if (map == null) return new List<ColonistDto>();

    return map.mapPawns.FreeColonists
        .Where(pawn => pawn != null && pawn.Spawned)
        .Select(pawn => new ColonistDto
        {
            Id = pawn.thingIDNumber,
            Name = pawn.Name?.ToStringShort ?? "Unknown",
            // Only access necessary properties
            Health = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
            Position = new PositionDto 
            { 
                X = pawn.Position.x, 
                Z = pawn.Position.z 
                // Skip Y if not needed
            }
        })
        .ToList();
}
```

### Cache Strategy

```csharp
public class OptimizedGameDataService : IGameDataService
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(5);
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private GameStateDto _cachedGameState;

    public GameStateDto GetGameState()
    {
        if (DateTime.UtcNow - _lastCacheUpdate > _cacheDuration)
        {
            RefreshCache();
        }
        return _cachedGameState;
    }

    public void RefreshCache()
    {
        // Only update if significant changes occurred
        var newState = CreateGameState();
        if (ShouldUpdateCache(_cachedGameState, newState))
        {
            _cachedGameState = newState;
            _lastCacheUpdate = DateTime.UtcNow;
        }
    }

    private bool ShouldUpdateCache(GameStateDto oldState, GameStateDto newState)
    {
        return oldState == null || 
               Math.Abs(oldState.ColonyWealth - newState.ColonyWealth) > 100 ||
               oldState.ColonistCount != newState.ColonistCount;
    }
}
```

## Common Pitfalls

### 1. RimWorld API Thread Safety
```csharp
// WRONG - Accessing RimWorld API from background thread
public void BadMethod()
{
    Task.Run(() => {
        var colonists = Find.CurrentMap.mapPawns.FreeColonists; // Thread unsafe!
    });
}

// CORRECT - Process on main thread
public void GoodMethod()
{
    // Use MainThreadDispatcher or process during tick
}
```

### 2. Memory Leaks
```csharp
// WRONG - Not disposing WebSocket connections
_sockets.Add(webSocket); // Never removed

// CORRECT - Proper cleanup
try
{
    // Use socket
}
finally
{
    lock (_lock) { _sockets.Remove(webSocket); }
    webSocket?.Dispose();
}
```

### 3. Performance Issues
```csharp
// WRONG - Inefficient data access every request
public ColonistDto GetColonist(int id)
{
    // This scans all pawns every call
    return Find.CurrentMap.mapPawns.AllPawns
        .FirstOrDefault(p => p.thingIDNumber == id);
}

// CORRECT - Cached access
public ColonistDto GetColonist(int id)
{
    return GetColonists() // Uses cached list
        .FirstOrDefault(c => c.Id == id);
}
