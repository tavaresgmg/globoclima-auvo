output "function_name" {
  description = "Nome da função Lambda"
  value       = aws_lambda_function.api.function_name
}

output "function_arn" {
  description = "ARN da função Lambda"
  value       = aws_lambda_function.api.arn
}

output "invoke_arn" {
  description = "ARN de invocação da função Lambda"
  value       = aws_lambda_function.api.invoke_arn
}

output "function_version" {
  description = "Versão da função Lambda"
  value       = aws_lambda_function.api.version
}