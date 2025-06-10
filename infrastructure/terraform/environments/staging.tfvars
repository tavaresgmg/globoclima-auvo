environment = "staging"
aws_region  = "us-east-1"

lambda_timeout     = 30
lambda_memory_size = 512
enable_xray_tracing = true

tags = {
  Project     = "GloboClima"
  ManagedBy   = "Terraform"
  Environment = "Staging"
}