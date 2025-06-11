# Contributing to GloboClima

Thank you for your interest in contributing to GloboClima! This document provides guidelines for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/GloboClima.git`
3. Add upstream remote: `git remote add upstream https://github.com/guilhermetavares/GloboClima.git`
4. Create a feature branch: `git checkout -b feature/your-feature-name`

## Development Setup

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop
- AWS CLI (configured)
- Terraform >= 1.7.0
- Visual Studio 2022 / VS Code / Rider

### Local Development

1. **Install dependencies:**
   ```bash
   dotnet restore
   dotnet tool restore
   ```

2. **Set up pre-commit hooks:**
   ```bash
   pip install pre-commit
   pre-commit install
   ```

3. **Configure environment:**
   ```bash
   cp src/GloboClima.Api/appsettings.json src/GloboClima.Api/appsettings.Development.json
   # Edit appsettings.Development.json with your API keys
   ```

4. **Run locally:**
   ```bash
   # API
   cd src/GloboClima.Api
   dotnet run

   # Web (in another terminal)
   cd src/GloboClima.Web
   dotnet run
   ```

## Code Standards

### C# Coding Standards

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Keep methods small and focused
- Write XML documentation for public APIs
- Use async/await for I/O operations

### Commit Messages

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer(s)]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Test additions or modifications
- `chore`: Maintenance tasks
- `perf`: Performance improvements
- `ci`: CI/CD changes
- `build`: Build system changes

Examples:
```
feat(auth): add refresh token support
fix(weather): handle API rate limit errors
docs(api): update swagger documentation
```

### Pull Request Process

1. **Before submitting:**
   - Ensure all tests pass: `dotnet test`
   - Check code coverage: `dotnet test --collect:"XPlat Code Coverage"`
   - Run linting: `dotnet format --verify-no-changes`
   - Update documentation if needed

2. **PR Guidelines:**
   - Keep PRs focused and small (< 500 lines preferred)
   - Write a clear PR description
   - Link related issues
   - Add screenshots for UI changes
   - Ensure CI checks pass

3. **PR Title Format:**
   Follow the same convention as commit messages

### Testing

- Write unit tests for new functionality
- Maintain minimum 50% code coverage
- Use descriptive test names: `MethodName_StateUnderTest_ExpectedBehavior`
- Mock external dependencies
- Test edge cases and error scenarios

Example:
```csharp
[Fact]
public async Task GetWeather_WithInvalidCity_ReturnsNotFound()
{
    // Arrange
    var city = "InvalidCity123";
    
    // Act
    var result = await _controller.GetWeather(city);
    
    // Assert
    Assert.IsType<NotFoundResult>(result);
}
```

## CI/CD Pipeline

### Automated Checks

When you submit a PR, the following checks run automatically:

1. **Build and Test**: Compiles code and runs all tests
2. **Code Quality**: Checks formatting and runs static analysis
3. **Security Scan**: Scans for vulnerabilities
4. **Coverage Check**: Ensures 50% minimum coverage
5. **PR Validation**: Checks commit format and PR size

### Manual Testing

For infrastructure changes:
1. Deploy to your AWS account
2. Test all endpoints
3. Verify monitoring and logs
4. Document any new AWS resources

## Project Structure

```
GloboClima/
├── src/
│   ├── GloboClima.Api/          # Web API
│   ├── GloboClima.Application/  # Use cases
│   ├── GloboClima.Domain/       # Domain models
│   ├── GloboClima.Infrastructure/ # External services
│   └── GloboClima.Web/          # Blazor frontend
├── tests/                       # Test projects
├── infrastructure/              # Terraform IaC
├── .github/workflows/           # CI/CD pipelines
└── docs/                        # Documentation
```

## Common Tasks

### Adding a New Feature

1. Create feature branch
2. Implement with TDD approach
3. Update documentation
4. Submit PR with tests

### Fixing a Bug

1. Create issue describing the bug
2. Write failing test
3. Fix the bug
4. Ensure test passes
5. Submit PR referencing issue

### Updating Dependencies

1. Use Dependabot PRs when available
2. Test thoroughly after updates
3. Check for breaking changes
4. Update documentation if needed

## Getting Help

- Check existing issues and PRs
- Read the documentation in `/docs`
- Ask questions in PR comments
- Contact maintainers if blocked

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers
- Provide constructive feedback
- Focus on what's best for the project

## Recognition

Contributors are recognized in:
- GitHub contributors page
- Release notes
- Project documentation

Thank you for contributing to GloboClima!