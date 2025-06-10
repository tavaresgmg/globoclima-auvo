output "users_table_name" {
  description = "Nome da tabela Users"
  value       = aws_dynamodb_table.users.name
}

output "users_table_arn" {
  description = "ARN da tabela Users"
  value       = aws_dynamodb_table.users.arn
}

output "weather_favorites_table_name" {
  description = "Nome da tabela WeatherFavorites"
  value       = aws_dynamodb_table.weather_favorites.name
}

output "weather_favorites_table_arn" {
  description = "ARN da tabela WeatherFavorites"
  value       = aws_dynamodb_table.weather_favorites.arn
}

output "country_favorites_table_name" {
  description = "Nome da tabela CountryFavorites"
  value       = aws_dynamodb_table.country_favorites.name
}

output "country_favorites_table_arn" {
  description = "ARN da tabela CountryFavorites"
  value       = aws_dynamodb_table.country_favorites.arn
}

output "table_arns" {
  description = "Lista de ARNs de todas as tabelas"
  value = [
    aws_dynamodb_table.users.arn,
    aws_dynamodb_table.weather_favorites.arn,
    aws_dynamodb_table.country_favorites.arn
  ]
}