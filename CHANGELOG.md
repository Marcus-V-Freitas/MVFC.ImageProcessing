# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.7.0] - 2026-05-26

### Added

- **Architecture**: Added `analysis-completed-topic` Pub/Sub topic and `mvfc-dashboard-analysis-sub` push subscription so the Dashboard is notified when AI analysis finishes.
- **Domain**: Created `AnalysisCompletedRequest` record in `MVFC.Image.Shareable.Requests` representing the AI analysis completion event.
- **Domain**: `ImageAnalysisHandler` now injects `IPublishService` and publishes an `AnalysisCompletedRequest` to `analysis-completed-topic` after saving the analysis JSON to GCS.
- **Configuration**: Added `AnalysisCompletedTopic` property to `PubSubConfig` and `PubSubConfig` parameter to `AppConfigAnalysis`, applied to all `appsettings.json` files and test fixtures.
- **IoC**: `AppAnalysisDependencies` now registers `IPublisherClientFactory`, `IPublishService`, and `PubSubConfig` for the Analysis Worker.
- **Testing**: `ImageAnalysisHandlerTests` updated with `IPublishService` mock and an assertion verifying the `analysis-completed` event is dispatched on the success path.

### Changed

- **Dashboard**: Replaced polling-based gallery refresh with real-time **Server-Sent Events (SSE)**. The Dashboard now subscribes to `/events/stream` (EventSource) and refreshes the gallery only when a Pub/Sub push arrives at `/pubsub/notify`.
- **Dashboard**: `SseClientManager` now implements `IDisposable`, completing all active channels on application shutdown for clean test teardown.
- **Domain**: `ImageThumbnailHandler` generates thumbnails in **PNG** format (previously documented as JPEG). Thumbnail filenames follow the pattern `thumb-{baseName}.png`.
- **Frontend**: Fixed thumbnail URL resolution in `app.js` — strips the original file extension and appends `.png` to match the backend naming convention.
- **Validation**: `FileUploadRequestValidator` now accepts any `image/*` content type instead of a fixed whitelist, supporting all formats handled by Magick.NET (200+ formats). Filename validation regex updated to reject OS-reserved characters (`< > : " / \ | ? *`) while allowing accented characters, parentheses, and other common naming patterns.
- **Code Quality**: Centralized `using` statements via `Usings.cs` files across all projects and test projects. Applied AAA (`// Arrange / // Act / // Assert`) comment structure to all unit and integration tests.

### Fixed

- **Frontend**: Dashboard thumbnail placeholder now correctly derives the `.png` filename from the original upload name, resolving the "Waiting..." state that persisted even after thumbnail generation succeeded.
- **Validation**: Fixed `NullReferenceException` in the Content-Type validator when `ContentType` is `null`.

---

## [2.6.0] - 2026-05-24

### Added

- **Observability**: Added High-Performance Logging using `LoggerMessage` source generators across all Domain Handlers (`ImageThumbnailHandler`, `ImageDeleteHandler`, `ImageGalleryHandler`, etc.), introducing structured logging and gracefully catching exceptions as `Result.Fail`.
- **Observability**: Tests were extended to verify proper `ILogger` invocations.

### Changed

- **Domain**: Refactored `ImageThumbnailHandler` to generate `.png` thumbnails while preserving the aspect ratio, and handling exceptions resiliently.
- **Configuration**: Refactored `PubSubConfig` and other application configurations to use strongly typed properties instead of dictionary lookups for topic names.
- **Architecture**: Moved `ConfigurationExtensions.cs` to the `Extensions` namespace for better grouping.

### Removed

- Removed obsolete `ShareableTypesTests.cs`.

---

## [2.5.1] - 2026-05-22

### Fixed

- **CI/CD**: Fixed GitHub Actions workflow failing due to dangling `needs` references to the removed `build-and-publish` job.

---

## [2.5.0] - 2026-05-22

### Changed

- **Architecture**: Refactored `PublishService` to use Dependency Injection via `IPublisherClientFactory`, removing direct dependency on the emulator and enabling full unit test coverage.
- **Code Quality**: Centralized `using` statements via `Usings.cs` in `MVFC.Image.Infra` and `MVFC.Image.Infra.Tests`, and corrected `StorageService` namespaces.
- **Documentation**: Simplified test execution commands in readmes to support the new solution-wide `dotnet test` architecture.
- **CI/CD**: Customized GitHub Issue and PR templates specifically for the `MVFC.ImageProcessing` project.

### Removed

- Removed outdated NuGet package publishing actions from `.github/workflows/ci.yml` as this is an application project.

---

## [2.4.0] - 2026-05-22

### Added

- **Architecture**: Implemented **Dead-Letter Queues (DLQ)** via Terraform. Messages failing 5 times are now safely routed to `dead-letter-topic` across all worker subscriptions.
- **Architecture**: Added HTTP Resilience to the Vision API client using `Microsoft.Extensions.Http.Resilience`, introducing Circuit Breakers and Retries against transient AI failures.
- **Testing**: Added `MVFC.Image.Infra.Tests` to cover infrastructure services (`StorageService`, `PublishService`).
- **Testing**: Introduced WebApplicationFactory patterns (`AnalysisWorkerFactory`, `UploadApiFactory`, etc.) for robust integration testing across all workers and APIs.
- **Documentation**: Updated `README.md` and `README.pt-BR.md` with advanced architecture diagrams, DLQ flows, and project badges.

### Changed

- **Architecture**: Extracted `ConfigurationExtensions` into the Shareable layer to centralize configuration binding and validation.
- **Testing**: Refactored domain handler tests to properly isolate dependencies.

### Removed

- Removed unused `IDomainEntrypoint.cs` and `IShareableEntrypoint.cs` as they are no longer necessary for test discovery.

---

## [2.3.0] - 2026-05-19

### Added

- **Testing**: Achieved **100% Code Coverage** (Line and Branch) across the entire domain and shareable library.
- **Testing**: Achieved **100% Line Coverage** for all worker and API `Program.cs` entry points.
- **Testing**: Separated unit tests into `MVFC.Image.Domain.Tests` and `MVFC.Image.Shareable.Tests` to mirror the source project structure cleanly.
- **Testing**: Created integration test projects for every worker and API (`MVFC.ImageAnalysis.Worker.Tests`, `MVFC.ImageConverter.Worker.Tests`, etc.) resolving configuration and DI cleanly.

### Changed

- **Code Quality**: Replaced `#pragma warning disable CA2012` hacks with global `.editorconfig` rules.
- **Infrastructure**: Updated `docker-compose.yml` fake-gcs-server healthcheck to use Alpine's native `wget` instead of `curl`.

---

## [2.2.0] - 2026-05-19

### Added

- **Architecture**: Migrated `MVFC.ImageThumbnail.Worker` and `MVFC.ImageDashboard.UI` to Clean Architecture and CQRS pattern using `MVFC.Mediator`.
- **Architecture**: Created `ImageThumbnailHandler`, `ImageGalleryHandler`, and `ImageDeletePublisherHandler` to handle thumbnail generation, gallery queries, and deletion event publication.
- **Domain**: Expanded `IStorageService` contract with `ListObjectsAsync` method and implemented it in `StorageService`.
- **IoC**: Added `AppThumbnailDependencies` and `AppDashboardDependencies` to encapsulate dependency injection registration for the Thumbnail worker and Dashboard UI.
- **Configuration**: Introduced `AppConfigThumbnail` and `AppConfigDashboard` configurations to manage settings for the Thumbnail worker and Dashboard UI.
- **Domain**: Added `FileBaseRequest` base record and new requests/responses (`FileThumbnailRequest`, `FileGalleryRequest`, `FileDeletePublisherRequest`, `FileGalleryResponse`).

### Changed

- **Domain**: Refactored request and event models (`FileConvertedRequest`, `FileDeleteRequest`, `FileUploadRequest`, `FileUploadedRequest`, `PubSubRequest`, `PubSubMessageRequest`) into records.
- **Infrastructure**: Updated Dockerfiles for `MVFC.ImageThumbnail.Worker` and `MVFC.ImageDashboard.UI` to include Clean Architecture project references (`Domain`, `Infra`, `IoC`).
- **Dashboard / Worker**: Configured `Program.cs` and `appsettings.json` in both the worker and UI projects to run via the new Mediator setup and structured configurations.

### Removed

- Removed legacy services `ThumbnailService.cs` from `MVFC.ImageThumbnail.Worker`.
- Removed legacy services `FileDeletePublisher.cs` and `FileGalleryService.cs` from `MVFC.ImageDashboard.UI`.

---

## [2.1.0] - 2026-05-18

### Added

- **Architecture**: Migrated `MVFC.ImageDelete.Worker` to Clean Architecture and CQRS pattern using `MVFC.Mediator`.
- **Architecture**: Created `ImageDeleteHandler` to handle cascading deletions, replacing the legacy delete service.
- **Domain**: Expanded `IStorageService` contract with `DeleteImageAsync` method and implemented it in `StorageService`.
- **IoC**: Added `AppDeleteDependencies` to encapsulate dependency injection registration for the Delete worker.

### Changed

- **Domain**: Refactored `FileDeleteRequest` into a strongly-typed record implementing `ICommand<Result>`.
- **Dashboard**: Updated `FileDeletePublisher` in UI to align with the new `FileDeleteRequest` record constructor.
- **Infrastructure**: Updated `MVFC.ImageDelete.Worker` `Dockerfile` to include Clean Architecture project references (`Domain`, `Infra`, `IoC`).

### Removed

- Removed legacy fat service `DeleteService.cs`.

---

## [2.0.0] - 2026-05-18

### Added

- **Architecture**: Introduced Clean Architecture principles by creating `MVFC.Image.Domain`, `MVFC.Image.Infra`, and `MVFC.Image.IoC` projects.
- **Architecture**: Adopted the CQRS pattern using `MVFC.Mediator`, replacing fat services with dedicated handlers (`ImageUploadHandler`, `ImageConverterHandler`, `ImageAnalysisHandler`).
- **Domain**: Created `IStorageService` and `IPublishService` contracts to abstract away Google Cloud dependencies from the core domain logic.
- **Events**: Added `FileConvertedRequest` event to better separate concerns between upload and conversion stages.

### Changed

- **Infrastructure**: Optimized all C# `Dockerfile`s to use multi-stage `COPY` strategies with explicit project references, significantly improving Docker build caching and speeding up local development.
- **Infrastructure**: Refactored `scripts/start.sh` to preserve container and Terraform state by default, introducing a `--clean` flag for when a full environment reset is needed.
- **Domain**: Refactored terminology across the board, renaming "Normalize" references to "Converter" (e.g., `AppNormalizedDependencies` to `AppConverterDependencies`).
- **Configuration**: Centralized configuration management by extracting `AppConfigAnalysis`, `AppConfigConverter`, `AppConfigUpload`, and a unified `PubSubConfig`.
- **Infrastructure**: Renamed Pub/Sub topic `file-normalized-topic` to `file-converted-topic`.
- **Codebase**: Centralized global usings across the new layers via `Usings.cs` files.

### Removed

- Removed legacy fat services (`UploadService`, `ImageNormalizerService`, `ImageAnalysisService`) in favor of the new CQRS Handlers.

### Fixed

- Fixed `MA0004` (ConfigureAwait) analyzer errors breaking Docker builds by correctly including `.editorconfig` in the Docker build context.

---
## [1.0.0] - 2026-05-17

### Added

- Initial release of the Event-Driven Media Processing Pipeline.
- **Upload API** (`mvfc-image-upload-api`): Endpoint to receive image uploads and save them to Google Cloud Storage.
- **Converter Worker** (`mvfc-image-converter-worker`): Normalizes various image formats (JPEG, PNG, AVIF, HEIC, TIFF, WebP) to PNG using Magick.NET.
- **Thumbnail Worker** (`mvfc-image-thumbnail-worker`): Generates 200x200 JPEG thumbnails.
- **Analysis Worker** (`mvfc-image-analysis-worker`): Orchestrates AI analysis by calling the Vision API via Refit.
- **Vision API** (`mvfc-image-vision-api`): Python/Flask service running the Salesforce BLIP model (PyTorch CPU-only) for natural language image captioning.
- **Delete Worker** (`mvfc-image-delete-worker`): Handles cascading deletion of images, thumbnails, and analysis results across all buckets.
- **Dashboard UI** (`mvfc-image-dashboard-ui`): Web interface for uploading, viewing, and managing images.
- **Infrastructure**: Terraform scripts to provision Pub/Sub topics, subscriptions, and GCS buckets.
- **Emulators**: Local environment setup using `google-cloud-cli:emulators` for Pub/Sub and `fake-gcs-server` for Cloud Storage.
- Convenience shell scripts (`start.sh`, `stop.sh`) for managing the Docker Compose and Terraform lifecycle.

---

[2.5.1]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.5.1
[2.5.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.5.0
[2.4.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.4.0
[2.3.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.3.0
[2.2.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.2.0
[2.1.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.1.0
[2.0.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.0.0
[1.0.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v1.0.0