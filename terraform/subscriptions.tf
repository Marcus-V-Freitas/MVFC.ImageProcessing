resource "google_pubsub_subscription" "image_converter_sub" {
  name                 = "mvfc-image-converter-worker-sub"
  topic                = google_pubsub_topic.topic.name
  ack_deadline_seconds = 60

  push_config {
    push_endpoint = "http://mvfc-image-converter-worker:8080/pubsub/push"
  }
}

resource "google_pubsub_subscription" "push_sub" {
  name                 = "mvfc-image-thumbnail-worker-sub"
  topic                = google_pubsub_topic.file_normalized_topic.name
  ack_deadline_seconds = 600

  push_config {
    push_endpoint = "http://mvfc-image-thumbnail-worker:8080/pubsub/push"
  }
}

resource "google_pubsub_subscription" "image_analysis_sub" {
  name                 = "mvfc-image-analysis-worker-sub"
  topic                = google_pubsub_topic.thumbnail_created_topic.name
  ack_deadline_seconds = 600

  push_config {
    push_endpoint = "http://mvfc-image-analysis-worker:8080/pubsub/push"
  }
}

resource "google_pubsub_subscription" "delete_worker_sub" {
  name                 = "mvfc-image-delete-worker-sub"
  topic                = google_pubsub_topic.file_delete_requested_topic.name
  ack_deadline_seconds = 30

  push_config {
    push_endpoint = "http://mvfc-image-delete-worker:8080/pubsub/push"
  }
}
