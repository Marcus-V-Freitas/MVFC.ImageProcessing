resource "google_storage_bucket" "uploads" {
  name          = "uploads"
  location      = "US"
  force_destroy = true
}

resource "google_storage_bucket" "thumbnails" {
  name          = "thumbnails"
  location      = "US"
  force_destroy = true
}

resource "google_storage_bucket" "analysis_results" {
  name          = "analysis-results"
  location      = "US"
  force_destroy = true
}

resource "google_storage_bucket" "converted" {
  name          = "converted"
  location      = "US"
  force_destroy = true
}
