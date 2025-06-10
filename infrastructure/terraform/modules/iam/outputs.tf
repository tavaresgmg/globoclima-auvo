output "lambda_role_arn" {
  description = "ARN da role do Lambda"
  value       = aws_iam_role.lambda_role.arn
}

output "lambda_role_name" {
  description = "Nome da role do Lambda"
  value       = aws_iam_role.lambda_role.name
}