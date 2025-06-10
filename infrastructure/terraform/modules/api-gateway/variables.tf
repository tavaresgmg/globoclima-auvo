variable "resource_prefix" {
  description = "Prefixo para nomes dos recursos"
  type        = string
}

variable "environment" {
  description = "Ambiente de deploy"
  type        = string
}

variable "lambda_function_arn" {
  description = "ARN da função Lambda"
  type        = string
}

variable "lambda_function_name" {
  description = "Nome da função Lambda"
  type        = string
}

variable "enable_xray_tracing" {
  description = "Habilitar AWS X-Ray tracing"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags para os recursos"
  type        = map(string)
  default     = {}
}