# GloboClima CI/CD Pipeline

## Overview

The GloboClima project uses GitHub Actions for continuous integration and deployment. The pipeline ensures code quality, security, and reliable deployments to AWS.

## Workflows

### 1. CI - Build and Test (`ci.yml`)

**Trigger:** Push to `main` or `develop` branches, Pull Requests

**Jobs:**
- **build-and-test**: Compiles code, runs tests, checks coverage
- **code-quality**: Validates code formatting and runs static analysis
- **security-scan**: Performs vulnerability scanning with Trivy
- **docker-build**: Tests Docker image build

**Key Features:**
- NuGet package caching for faster builds
- Code coverage threshold enforcement (50%)
- Test result artifacts upload
- Security vulnerability detection

### 2. Deploy to AWS (`deploy.yml`)

**Trigger:** Push to `main` (staging), Manual dispatch (production)

**Environments:**
- **staging**: Automatic deployment on main branch push
- **production**: Manual approval required, with rollback capability

**Deployment Steps:**
1. Build .NET application for Linux runtime
2. Deploy infrastructure with Terraform
3. Run smoke tests
4. Monitor deployment health
5. Notify via Slack

**Safety Features:**
- State backup before production deployment
- Health checks and error monitoring
- Automatic rollback on failure
- Manual approval for production

### 3. PR Check (`pr-check.yml`)

**Trigger:** Pull Request events

**Validations:**
- Conventional commit format
- PR size limits (max 1000 changes)
- Dependency security review
- Terraform validation (if infrastructure changes)

### 4. Release (`release.yml`)

**Trigger:** Git tags matching `v*`

**Artifacts:**
- Platform-specific binaries (Linux, Windows, macOS)
- Docker images (multi-architecture)
- Automated changelog generation

**Distribution:**
- GitHub Releases
- Docker Hub
- GitHub Container Registry

### 5. Security Scan (`security-scan.yml`)

**Trigger:** Daily at 2 AM UTC, Manual dispatch

**Scans:**
- CodeQL analysis for code vulnerabilities
- OWASP dependency check
- Container security with Trivy
- Secret scanning with TruffleHog
- License compliance verification

## Required Secrets

Configure these in GitHub repository settings:

### AWS Deployment
- `AWS_ACCESS_KEY_ID`: AWS access key for deployment
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `JWT_SECRET`: JWT signing secret
- `OPENWEATHERMAP_API_KEY`: OpenWeatherMap API key

### Integrations
- `SLACK_WEBHOOK`: Slack webhook for notifications
- `CODECOV_TOKEN`: Codecov integration token
- `DOCKER_USERNAME`: Docker Hub username
- `DOCKER_PASSWORD`: Docker Hub password

## Environment Configuration

### Staging
- Auto-deploy from main branch
- Lower resource allocation
- 7-day log retention
- No approval required

### Production
- Manual deployment only
- Higher resource allocation
- 30-day log retention
- Manual approval required
- Point-in-time recovery enabled

## Branch Protection Rules

Recommended settings for `main` branch:

1. **Require PR before merging**
2. **Require status checks:**
   - build-and-test
   - code-quality
   - security-scan
3. **Require up-to-date branches**
4. **Require code review approval**
5. **Dismiss stale reviews**
6. **Require CODEOWNERS review**

## Deployment Process

### Staging Deployment
```bash
# Automatic on push to main
git push origin main
```

### Production Deployment
1. Go to Actions → Deploy to AWS
2. Click "Run workflow"
3. Select "production" environment
4. Approve deployment when prompted

### Emergency Rollback
```bash
# Connect to AWS
aws configure

# Rollback Terraform
cd infrastructure/terraform
terraform workspace select production
terraform apply -var-file="environments/prod.tfvars" -replace="aws_lambda_function.api"
```

## Monitoring

### Build Status
- Check Actions tab for workflow runs
- Review test results and coverage reports
- Monitor security scan results

### Deployment Health
```bash
# View Lambda logs
aws logs tail /aws/lambda/GloboClima-prod-api --follow

# Check API health
curl https://api.globoclima.com/health
```

### Alerts
- Slack notifications for deployment status
- CloudWatch alarms for errors
- Security alerts via GitHub Security tab

## Best Practices

1. **Commit Messages**: Use conventional commits format
   ```
   feat(auth): add JWT refresh token support
   fix(weather): handle API rate limits
   docs(api): update swagger documentation
   ```

2. **PR Size**: Keep PRs under 500 lines of changes

3. **Testing**: Maintain >50% code coverage

4. **Dependencies**: Review Dependabot PRs weekly

5. **Security**: Address security alerts within 48 hours

## Troubleshooting

### Build Failures
1. Check workflow logs in Actions tab
2. Verify all tests pass locally
3. Ensure dependencies are up to date

### Deployment Failures
1. Check Terraform state consistency
2. Verify AWS credentials are valid
3. Review CloudWatch logs
4. Check resource quotas

### Common Issues

**"Code coverage below threshold"**
- Run tests locally with coverage
- Add missing test cases
- Update threshold if justified

**"Terraform state lock"**
```bash
terraform force-unlock <LOCK_ID>
```

**"Docker build fails"**
- Clear Docker cache: `docker system prune -a`
- Check Dockerfile syntax
- Verify base image availability

## Local Development

### Running CI checks locally
```bash
# Format check
dotnet format --verify-no-changes

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Security scan
docker run --rm -v "$PWD":/src aquasec/trivy fs /src
```

### Pre-commit hooks
```bash
# Install pre-commit
pip install pre-commit

# Install hooks
pre-commit install

# Run manually
pre-commit run --all-files
```