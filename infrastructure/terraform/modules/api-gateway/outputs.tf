output "api_id" {
  description = "ID da API Gateway"
  value       = aws_api_gateway_rest_api.api.id
}

output "api_url" {
  description = "URL da API Gateway"
  value       = aws_api_gateway_stage.api.invoke_url
}

output "api_arn" {
  description = "ARN da API Gateway"
  value       = aws_api_gateway_rest_api.api.arn
}

output "stage_name" {
  description = "Nome do stage da API Gateway"
  value       = aws_api_gateway_stage.api.stage_name
}