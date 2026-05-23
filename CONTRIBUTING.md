# Contributing

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) running locally
- [Terraform](https://developer.hashicorp.com/terraform/downloads) 1.5+
- Git

## Running locally

```sh
git clone https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing.git
cd MVFC.ImageProcessing

# Start all containers + infrastructure
./scripts/start.sh

# (Optional) Build .NET projects locally
dotnet restore MVFC.ImageProcessing.slnx
dotnet build MVFC.ImageProcessing.slnx --configuration Release
```

## Running tests

```sh
dotnet test --configuration Release
```

## Adding a new service

1. Create a new folder under `src/MVFC.Image{ServiceName}.{Worker|Api}/`
2. Follow the structure of an existing service (e.g. `MVFC.ImageDelete.Worker`)
3. Add the new project to `MVFC.ImageProcessing.slnx` under the `/src/` folder
4. Add any new package versions to `Directory.Packages.props`
5. Add integration tests in `tests/MVFC.ImageProcessing.Tests/`
6. Update `README.md` and `README.pt-BR.md` with the new component entry

## Branch naming

- `feat/` — new feature or service
- `fix/` — bug fix
- `chore/` — dependency update or maintenance
- `docs/` — documentation only
- `test/` — tests only
- `refactor/` — no feature change, no bug fix

Example: `feat/add-watermark-worker`

## Commit convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

- `feat: add watermark worker`
- `fix: fix pubsub connection timeout`
- `docs: update README badges`
- `chore: bump Magick.NET to 14.14.0`
- `test: add upload API integration tests`
- `refactor: simplify converter setup`

## Pull Request process

1. Fork and create your branch from `main`
2. Make your changes and ensure all tests pass locally
3. Open a PR against `main` and fill in the PR template
4. Wait for the CI to pass
5. A maintainer will review and merge