output "api_gateway_url" {
  description = "URL do API Gateway"
  value       = module.api_gateway.api_url
}

output "lambda_function_name" {
  description = "Nome da função Lambda"
  value       = module.lambda.function_name
}

output "lambda_function_arn" {
  description = "ARN da função Lambda"
  value       = module.lambda.function_arn
  sensitive   = true
}

output "dynamodb_table_names" {
  description = "Nomes das tabelas DynamoDB"
  value = {
    users             = module.dynamodb.users_table_name
    weather_favorites = module.dynamodb.weather_favorites_table_name
    country_favorites = module.dynamodb.country_favorites_table_name
  }
}

output "cloudwatch_log_group" {
  description = "CloudWatch Log Group para a Lambda"
  value       = aws_cloudwatch_log_group.api_logs.name
}

output "deployment_info" {
  description = "Informações de deployment para CI/CD"
  value = {
    environment     = var.environment
    region         = var.aws_region
    api_url        = module.api_gateway.api_url
    lambda_name    = module.lambda.function_name
  }
}