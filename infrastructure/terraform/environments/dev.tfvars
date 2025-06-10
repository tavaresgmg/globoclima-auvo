environment = "dev"
aws_region  = "us-east-1"

lambda_timeout     = 30
lambda_memory_size = 256
enable_xray_tracing = false

tags = {
  Project     = "GloboClima"
  ManagedBy   = "Terraform"
  Environment = "Development"
}