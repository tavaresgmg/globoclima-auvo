environment = "prod"
aws_region  = "us-east-1"

lambda_timeout     = 60
lambda_memory_size = 1024
enable_xray_tracing = true

tags = {
  Project     = "GloboClima"
  ManagedBy   = "Terraform"
  Environment = "Production"
  CostCenter  = "Engineering"
}