# Contributing

## Workflow

1. Create a feature branch
2. Make your changes
3. Ensure the solution builds without errors or warnings:
   ```bash
   dotnet build DotnetNiger.slnx --configuration Release /p:TreatWarningsAsErrors=true
   ```
4. Commit with a clear, descriptive message in English
5. Open a pull request

## Code Style

- Follow existing patterns in the codebase
- Use .NET 9 features where appropriate
- Keep controllers thin; logic belongs in services
- Use async/await consistently
- Do not add unnecessary comments

## Commit Messages

Use conventional commits:
```
feat: add new feature
fix: correct bug
refactor: restructure code
docs: update documentation
chore: maintenance tasks
```

## Pull Requests

- Reference any related issues
- Describe what the PR does and why
- Keep PRs focused on a single change
