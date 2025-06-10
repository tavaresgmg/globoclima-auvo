variable "resource_prefix" {
  description = "Prefixo para nomes dos recursos"
  type        = string
}

variable "environment" {
  description = "Ambiente de deploy"
  type        = string
}

variable "lambda_role_arn" {
  description = "ARN da role do Lambda"
  type        = string
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

variable "environment_variables" {
  description = "Variáveis de ambiente para a Lambda"
  type        = map(string)
  default     = {}
}

variable "tags" {
  description = "Tags para os recursos"
  type        = map(string)
  default     = {}
}