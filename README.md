# POS Application CI/CD Pipeline

This project includes a comprehensive Woodpecker CI pipeline for automated building, testing, and deployment of the ASP.NET Core POS application.

## Pipeline Overview

The Woodpecker CI pipeline includes the following stages:

### 1. Build Stage
- **Purpose**: Restore NuGet packages and build the application
- **Image**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Commands**:
  - `dotnet restore POS/POS.csproj`
  - `dotnet build POS/POS.csproj --configuration Release --no-restore`

### 2. Test Stage
- **Purpose**: Run unit tests (if available)
- **Image**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Commands**:
  - `dotnet test POS/POS.csproj --configuration Release --no-build --logger trx`
- **Condition**: Only runs if build stage succeeds

### 3. Lint Stage
- **Purpose**: Check code style and formatting
- **Image**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Commands**:
  - `dotnet format --verify-no-changes --verbosity normal`
- **Condition**: Only runs if test stage succeeds

### 4. Security Scan Stage
- **Purpose**: Scan for security vulnerabilities
- **Image**: `owasp/dependency-check:latest`
- **Commands**:
  - Generate JSON and HTML security reports
- **Condition**: Only runs if lint stage succeeds

### 5. Publish Stage
- **Purpose**: Publish the application for deployment
- **Image**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Commands**:
  - `dotnet publish POS/POS.csproj --configuration Release --output ./publish --no-build`
- **Condition**: Only runs if security scan succeeds

### 6. Docker Build Stage
- **Purpose**: Build container image for deployment
- **Image**: `docker:20.10.16`
- **Commands**:
  - Build Docker image with commit SHA tag
  - Tag image as latest
- **Condition**: Only runs if publish stage succeeds

### 7. Docker Push Stage
- **Purpose**: Push container image to registry
- **Image**: `docker:20.10.16`
- **Commands**:
  - Login to Docker registry
  - Tag image with registry URL
  - Push both commit SHA and latest tags
- **Condition**: Only runs if Docker build succeeds and on push events
- **Environment Variables Required**:
  - `DOCKER_USERNAME`: Registry username
  - `DOCKER_PASSWORD`: Registry password
  - `DOCKER_REGISTRY`: Registry URL (e.g., `your-registry.com:5000`)

### 8. Manual Deployment Stage
- **Purpose**: Manual deployment to production environment
- **Image**: `docker:20.10.16`
- **Commands**:
  - Pull latest image from registry
  - Stop and remove existing container
  - Deploy new container with latest image
- **Condition**: Only runs if all previous stages succeed and triggered manually
- **Environment Variables Required**:
  - `DOCKER_USERNAME`: Registry username
  - `DOCKER_PASSWORD`: Registry password
  - `DOCKER_REGISTRY`: Registry URL (e.g., `your-registry.com:5000`)

## Pipeline Triggers

The pipeline runs automatically on:
- Push to any branch
- Pull request creation/update

## Docker Support

The project includes a multi-stage Dockerfile for containerized deployment:

- **Build Stage**: Uses .NET SDK to build and publish the application
- **Runtime Stage**: Uses .NET ASP.NET runtime for production deployment
- **Port**: Exposes port 80
- **Environment**: Configured for production with optimized settings

## Usage

### Local Development

1. **Build the application**:
   ```bash
   cd POS
   dotnet build --configuration Release
   ```

2. **Run tests** (if available):
   ```bash
   dotnet test --configuration Release
   ```

3. **Check code formatting**:
   ```bash
   dotnet format --verify-no-changes
   ```

4. **Publish the application**:
   ```bash
   dotnet publish --configuration Release --output ./publish
   ```

### Docker Development

1. **Build Docker image**:
   ```bash
   docker build -t pos-application:latest .
   ```

2. **Run container**:
   ```bash
   docker run -p 8080:80 pos-application:latest
   ```

3. **Access application**:
   Open http://localhost:8080 in your browser

## Project Structure

```
calculator/
├── .woodpecker.yml          # CI/CD pipeline configuration
├── Dockerfile              # Multi-stage Docker build
├── POS/                    # ASP.NET Core Web API project
│   ├── POS.csproj
│   ├── Program.cs
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── Data/
└── README.md               # This file
```

## Dependencies

- **.NET 10.0**: Target framework for the application
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Database provider
- **Microsoft.AspNetCore.OpenApi**: API documentation

## Security

The pipeline includes security scanning using OWASP Dependency Check to identify known vulnerabilities in dependencies.

## Monitoring

Pipeline execution can be monitored through:
- Woodpecker CI web interface
- Build logs and reports
- Security scan reports (JSON and HTML formats)

## Troubleshooting

### Common Issues

1. **Build failures**: Check .NET SDK version compatibility
2. **Test failures**: Ensure test projects are properly configured
3. **Docker build issues**: Verify Docker daemon is running
4. **Security scan failures**: Review dependency versions and update if needed

### Logs and Reports

- Build logs: Available in Woodpecker CI interface
- Security reports: Generated in project root as `dependency-check-report.json` and `dependency-check-report.html`
- Test results: Available as TRX files in test output directory

## Contributing

1. Create feature branches from `master`
2. Ensure all pipeline stages pass before merging
3. Update this documentation for significant changes
4. Follow the established code formatting and security standards