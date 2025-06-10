variable "resource_prefix" {
  description = "Prefixo para nomes dos recursos"
  type        = string
}

variable "dynamodb_tables_arns" {
  description = "ARNs das tabelas DynamoDB"
  type        = list(string)
}

variable "tags" {
  description = "Tags para os recursos"
  type        = map(string)
  default     = {}
}