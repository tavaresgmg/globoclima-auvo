variable "project_name" {
  description = "Nome do projeto"
  type        = string
  default     = "GloboClima"
}

variable "environment" {
  description = "Ambiente de deploy (dev, staging, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment deve ser dev, staging ou prod."
  }
}

variable "aws_region" {
  description = "Região AWS para deploy"
  type        = string
  default     = "us-east-1"
}

variable "jwt_secret" {
  description = "JWT secret para autenticação"
  type        = string
  sensitive   = true
}

variable "openweathermap_api_key" {
  description = "API key do OpenWeatherMap"
  type        = string
  sensitive   = true
}

variable "lambda_timeout" {
  description = "Timeout da função Lambda em segundos"
  type        = number
  default     = 30
}

variable "lambda_memory_size" {
  description = "Memória da função Lambda em MB"
  type        = number
  default     = 512
}

variable "enable_xray_tracing" {
  description = "Habilitar AWS X-Ray tracing"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags padrão para todos os recursos"
  type        = map(string)
  default = {
    Project     = "GloboClima"
    ManagedBy   = "Terraform"
    Repository  = "globoclima-api"
  }
}