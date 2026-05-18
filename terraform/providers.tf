terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "4.33.0"
    }
  }
}

provider "google" {
  project = "local-project"
  region  = "us-central1"

  access_token = "fake-token"
}
