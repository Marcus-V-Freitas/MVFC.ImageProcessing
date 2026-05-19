# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[2.0.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v2.0.0
[1.0.0]: https://github.com/Marcus-V-Freitas/MVFC.ImageProcessing/releases/tag/v1.0.0