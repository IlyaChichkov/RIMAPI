# Contributing to RIMAPI

Thank you for your interest in contributing to RIMAPI! This guide will help you get started with contributing code, documentation, or ideas to the project.

## Ways to Contribute

There are many ways to contribute, even if you're not a C# expert:

- **Code Contributions**: Bug fixes, new features, performance improvements
- **Documentation**: Improving guides, adding examples, fixing typos
- **Testing**: Testing in game new features, reporting bugs
- **Feature Ideas**: Suggesting new functionality or improvements
- **Community Support**: Helping other users in discussions and issues

## Getting Started

### Prerequisites

- RimWorld 1.5 or later
- .NET Framework 4.7.2
- Git
- IDE

??? note "VSCode Extensions"

    C# Dev Kit: ms-dotnettools.csdevkit
    CSharpier - Code formatter: csharpier.csharpier-vscode

### Development Setup

1. **Clone the repository** locally:

    ```bash
    git clone https://github.com/your-username/RIMAPI.git
    cd RIMAPI
    ```

2. **Create a feature branch**:

    ```bash
    git checkout -b feature/your-feature-name
    ```

## Development Workflow

### Coding Standards

- Follow the existing code style and patterns
- Use meaningful variable and method names
- Keep methods focused and single-purpose
- Try to document large functions with comments

### Architecture Guidelines

- **Controllers should be thin** - delegate logic to services
- **Use dependency injection** for all service dependencies
- **Keep RimWorld API calls isolated** in service layer
- **Use DTOs for API responses** - don't expose RimWorld types directly

### Testing

#### Testing Checklist

- [x] Mod loads without errors in RimWorld
- [x] API server starts correctly
- [x] New endpoints respond as expected
- [x] Existing endpoints still work
- [x] SSE events fire correctly
- [x] Error handling works properly
- [x] No performance regressions

### Update Documentation

If your changes affect:

- **API endpoints**: Update the auto-generated API docs if needed
- **Configuration**: Update configuration guides
- **Extension system**: Update developer documentation
- **New features**: Add usage examples and guides

## Areas Needing Contribution

### Beginner-Friendly

- Documentation improvements and examples
- Additional API endpoint examples
- Test case development
- Code comments and documentation

### Intermediate

- New service implementations (IInventoryService, IResearchService)
- Additional controller endpoints
- SSE event implementations
- Error handling improvements

### Advanced

- Performance optimizations
- Authentication system
- Advanced extension system features
- Protocol enhancements

## Getting Help

- **Discussions**: Use GitHub Discussions for questions and ideas
- **Issues**: Open an issue for bugs or feature requests
- **Discord**: Join the [RimWorld Modding Discord](https://discord.gg/Css9b9BgnM) for real-time help
- **Code Review**: Ask for early feedback on incomplete work

## Recognition

All contributors are recognized in:

- GitHub contributor graph
- Release notes
- Project documentation
- Community acknowledgements

Thank you for contributing to RIMAPI and helping build a better modding ecosystem for RimWorld!
