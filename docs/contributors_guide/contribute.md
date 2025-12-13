# Contributing to RIMAPI

Thank you for your interest in contributing to RIMAPI! This guide provides everything you need to know to contribute code, documentation, or ideas to the project. Your contributions help make RIMAPI a more powerful tool for the RimWorld community.

## Ways to Contribute

- **Code**: Implement new features, fix bugs, or improve performance.
- **Documentation**: Enrich our guides, add examples, or fix typos. Every clarification helps.
- **Testing**: Help us find and squash bugs by testing new features in-game.
- **Feature Ideas**: Suggest new functionality or improvements by opening an issue.
- **Community Support**: Assist other users in GitHub Discussions or on Discord.

## Getting Started: Your First Contribution

### Prerequisites

- A licensed copy of RimWorld (1.5 or later).
- An IDE for .NET development:
    - **Visual Studio**: Install the `.NET desktop development` workload.
    - **VSCode**: Install the `C# Dev Kit` extension (`ms-dotnettools.csdevkit`).
- The **.NET Framework 4.8** Developer Pack (or the version targeted by the project).
- **Git** for version control.

### Development Setup

1.  **Fork & Clone**:
    -   **Fork** the repository on GitHub to create your own copy.
    -   **Clone** your fork to your local machine:
        ```bash
        git clone https://github.com/Your-Username/RIMAPI.git
        cd RIMAPI
        ```

2.  **Configure RimWorld Path**:
    -   The project needs to know where your RimWorld installation is to reference its assemblies.
    -   Locate the `Source/Directory.Build.props` file.
    -   Update the `<RimWorldPath>` property to point to your RimWorld installation directory.

3.  **Build the Project**:
    -   Open the `Source/RimApi.sln` solution in your IDE.
    -   Build the solution to ensure all dependencies are resolved and the project compiles correctly.

4.  **Create a Feature Branch**:
    -   Create a new branch for your changes. This keeps the `main` branch clean.
        ```bash
        git checkout -b feature/your-awesome-feature
        ```

## Creating a New Endpoint

Adding a new endpoint is a great way to start contributing. We have a detailed, step-by-step guide for this.

- **Read the [Creating Endpoints](./creating_endpoints.md) guide** to learn about the architecture and the process from DTO creation to controller implementation.

## Development Workflow

### Coding & Architecture Standards

-   **Follow Existing Patterns**: Strive for consistency with the existing codebase.
-   **Controllers are Thin**: Controllers should only parse requests and return responses. All business logic belongs in the **service layer**.
-   **Services Coordinate, Helpers Execute**: Services orchestrate the workflow, while **helpers** mostly contain reusable code.
-   **Use DTOs**: Never expose RimWorld's internal types in an API response. Map all data to Data Transfer Objects (DTOs) in the `Models` directory.

### Testing Your Changes

Before submitting your contribution, please test it thoroughly:

1.  The mod loads in RimWorld without errors.
2.  The API server starts up correctly.
3.  Your new endpoint responds with the correct data and status codes.
4.  Run a quick check on a few existing endpoints to ensure you haven't introduced any regressions.
5.  Error handling works as expected (e.g., providing an invalid ID).

### Updating Documentation

Accurate documentation is as important as the code itself.

-   If you add or modify an endpoint, you **must** update the documentation in `docs/_endpoints_examples/examples.yml`.
-   If you add a major new feature, create or update a corresponding guide in the `docs/` directory.

## Submitting a Pull Request

-  **Push to Your Fork**:
    ```bash
    git push origin feature/your-awesome-feature
    ```

-  **Open a Pull Request**:
    -   Go to the original RIMAPI repository on GitHub.
    -   Click "New Pull Request" and select your feature branch.
    -   Fill out the pull request template, explaining what your PR does and how you tested it.

## Getting Help

-   **GitHub Discussions**: For questions, ideas, and general feedback.
-   **GitHub Issues**: To report a bug or request a specific feature.
-   **Discord**: Join the official [RimWorld Modding Discord](https://discord.gg/Css9b9BgnM) for real-time help in the `#rimapi` channel.