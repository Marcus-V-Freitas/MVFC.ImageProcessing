variable "is_local" {
  description = "Se true, roda contra emuladores locais (pula recursos não suportados)"
  type        = bool
  default     = true
}

# uploads → file-uploaded-topic
resource "google_storage_notification" "uploads_notification" {
  count          = var.is_local ? 0 : 1
  bucket         = google_storage_bucket.uploads.name
  payload_format = "JSON_API_V1"
  topic          = google_pubsub_topic.topic.id
  event_types    = ["OBJECT_FINALIZE"]
}

# converted → file-converted-topic
resource "google_storage_notification" "converted_notification" {
  count          = var.is_local ? 0 : 1
  bucket         = google_storage_bucket.converted.name
  payload_format = "JSON_API_V1"
  topic          = google_pubsub_topic.file_converted_topic.id
  event_types    = ["OBJECT_FINALIZE"]
}

# thumbnails → thumbnail-created-topic
resource "google_storage_notification" "thumbnails_notification" {
  count          = var.is_local ? 0 : 1
  bucket         = google_storage_bucket.thumbnails.name
  payload_format = "JSON_API_V1"
  topic          = google_pubsub_topic.thumbnail_created_topic.id
  event_types    = ["OBJECT_FINALIZE"]
}

# analysis-results → analysis-completed-topic
resource "google_storage_notification" "analysis_results_notification" {
  count          = var.is_local ? 0 : 1
  bucket         = google_storage_bucket.analysis_results.name
  payload_format = "JSON_API_V1"
  topic          = google_pubsub_topic.analysis_completed_topic.id
  event_types    = ["OBJECT_FINALIZE"]
}
