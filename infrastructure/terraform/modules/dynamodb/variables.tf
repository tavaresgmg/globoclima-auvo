variable "resource_prefix" {
  description = "Prefixo para nomes dos recursos"
  type        = string
}

variable "environment" {
  description = "Ambiente de deploy"
  type        = string
}

variable "enable_point_in_time_recovery" {
  description = "Habilitar point-in-time recovery"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags para os recursos"
  type        = map(string)
  default     = {}
}