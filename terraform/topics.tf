resource "google_pubsub_topic" "topic" {
  name = "file-uploaded-topic"
}

resource "google_pubsub_topic" "file_converted_topic" {
  name = "file-converted-topic"
}

resource "google_pubsub_topic" "thumbnail_created_topic" {
  name = "thumbnail-created-topic"
}

resource "google_pubsub_topic" "file_delete_requested_topic" {
  name = "file-delete-requested-topic"
}

resource "google_pubsub_topic" "dead_letter_topic" {
  name = "dead-letter-topic"
}

resource "google_pubsub_topic" "analysis_completed_topic" {
  name = "analysis-completed-topic"
}
