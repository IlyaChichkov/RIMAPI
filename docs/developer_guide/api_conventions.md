# API Conventions

To ensure a consistent and standard RESTful experience, RIMAPI enforces the following rules globally across all endpoints.

## JSON Naming Standard (`snake_case`)
All JSON request bodies, response payloads, and query parameters strictly use **`snake_case`**. 

While the internal C# mod code utilizes standard `PascalCase` for its Data Transfer Objects (DTOs), the API boundary automatically handles the conversion. You must format your requests using `snake_case` or the server will ignore the fields.

**❌ Incorrect Payload (PascalCase/camelCase):**
```json
{
  "ResearchProject": "Electricity",
  "targetPoints": 500
}
```

**✅ Correct Payload (snake_case):**
```json
{
"research_project": "Electricity",
"target_points": 500
}
```
