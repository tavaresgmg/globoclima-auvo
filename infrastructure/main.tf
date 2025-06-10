terraform {
  required_providers {
    aws = { source = "hashicorp/aws", version = "~> 5.0" }
  }
}

provider "aws" {
  region = "us-east-1"
}

# DynamoDB Tables
resource "aws_dynamodb_table" "users" {
  name = "GloboClima-Users"
  billing_mode = "PAY_PER_REQUEST"
  hash_key = "Id"
  attribute {
    name = "Id"
    type = "S"
  }
  attribute {
    name = "Email"
    type = "S"
  }
  global_secondary_index {
    name = "email-index"
    hash_key = "Email"
    projection_type = "ALL"
  }
}

resource "aws_dynamodb_table" "weather_favorites" {
  name = "GloboClima-WeatherFavorites"
  billing_mode = "PAY_PER_REQUEST"
  hash_key = "Id"
  attribute {
    name = "Id"
    type = "S"
  }
  attribute {
    name = "UserId"
    type = "S"
  }
  global_secondary_index {
    name = "userId-index"
    hash_key = "UserId"
    projection_type = "ALL"
  }
}

resource "aws_dynamodb_table" "country_favorites" {
  name = "GloboClima-CountryFavorites"
  billing_mode = "PAY_PER_REQUEST"
  hash_key = "Id"
  attribute {
    name = "Id"
    type = "S"
  }
  attribute {
    name = "UserId"
    type = "S"
  }
  global_secondary_index {
    name = "userId-index"
    hash_key = "UserId"
    projection_type = "ALL"
  }
}

# IAM Role for Lambda
resource "aws_iam_role" "lambda_role" {
  name = "auvo-lambda-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy" "lambda_policy" {
  name = "auvo-lambda-policy"
  role = aws_iam_role.lambda_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "arn:aws:logs:*:*:*"
      },
      {
        Effect = "Allow"
        Action = [
          "dynamodb:PutItem",
          "dynamodb:GetItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:DescribeTable"
        ]
        Resource = [
          aws_dynamodb_table.users.arn,
          "${aws_dynamodb_table.users.arn}/index/*",
          aws_dynamodb_table.weather_favorites.arn,
          "${aws_dynamodb_table.weather_favorites.arn}/index/*",
          aws_dynamodb_table.country_favorites.arn,
          "${aws_dynamodb_table.country_favorites.arn}/index/*"
        ]
      }
    ]
  })
}

# Lambda Function
resource "aws_lambda_function" "api" {
  filename = "lambda-deployment.zip"
  function_name = "auvo-globoclima-api"
  role = aws_iam_role.lambda_role.arn
  handler = "GloboClima.Lambda::GloboClima.Lambda.Function::FunctionHandler"
  runtime = "dotnet8"
  timeout = 30
  memory_size = 512
  source_code_hash = filebase64sha256("lambda-deployment.zip")

  environment {
    variables = {
      DYNAMODB_USERS_TABLE = aws_dynamodb_table.users.name
      DYNAMODB_WEATHER_TABLE = aws_dynamodb_table.weather_favorites.name
      DYNAMODB_COUNTRIES_TABLE = aws_dynamodb_table.country_favorites.name
    }
  }

  depends_on = [aws_iam_role_policy.lambda_policy]
}

# API Gateway
resource "aws_api_gateway_rest_api" "api" {
  name = "auvo-globoclima-api"
  description = "GloboClima API for AUVO Technical Test"
}

resource "aws_api_gateway_resource" "api_resource" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  parent_id = aws_api_gateway_rest_api.api.root_resource_id
  path_part = "{proxy+}"
}

resource "aws_api_gateway_method" "api_method" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_resource.api_resource.id
  http_method = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "api_integration" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_resource.api_resource.id
  http_method = aws_api_gateway_method.api_method.http_method
  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = aws_lambda_function.api.invoke_arn
}

resource "aws_api_gateway_method" "api_method_root" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_rest_api.api.root_resource_id
  http_method = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "api_integration_root" {
  rest_api_id = aws_api_gateway_rest_api.api.id
  resource_id = aws_api_gateway_rest_api.api.root_resource_id
  http_method = aws_api_gateway_method.api_method_root.http_method
  integration_http_method = "POST"
  type = "AWS_PROXY"
  uri = aws_lambda_function.api.invoke_arn
}

resource "aws_api_gateway_deployment" "api_deployment" {
  depends_on = [
    aws_api_gateway_integration.api_integration,
    aws_api_gateway_integration.api_integration_root
  ]
  rest_api_id = aws_api_gateway_rest_api.api.id
  stage_name = "prod"
}

resource "aws_lambda_permission" "api_gateway" {
  statement_id = "AllowExecutionFromAPIGateway"
  action = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api.function_name
  principal = "apigateway.amazonaws.com"
  source_arn = "${aws_api_gateway_rest_api.api.execution_arn}/*/*"
}

# S3 for frontend hosting
resource "aws_s3_bucket" "frontend" {
  bucket = "auvo-globoclima-frontend-${random_id.bucket_suffix.hex}"
}

resource "random_id" "bucket_suffix" {
  byte_length = 4
}

resource "aws_s3_bucket_website_configuration" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  index_document { suffix = "index.html" }
  error_document { key = "error.html" }
}

resource "aws_s3_bucket_public_access_block" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  block_public_acls = false
  block_public_policy = false
  ignore_public_acls = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_policy" "frontend" {
  bucket = aws_s3_bucket.frontend.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Principal = "*"
      Action = "s3:GetObject"
      Resource = "${aws_s3_bucket.frontend.arn}/*"
    }]
  })
  depends_on = [aws_s3_bucket_public_access_block.frontend]
}

output "api_gateway_url" {
  value = "https://${aws_api_gateway_rest_api.api.id}.execute-api.us-east-1.amazonaws.com/prod"
}

output "s3_website_url" {
  value = "http://${aws_s3_bucket.frontend.bucket}.s3-website-us-east-1.amazonaws.com"
}

output "dynamodb_tables" {
  value = {
    users = aws_dynamodb_table.users.name
    weather = aws_dynamodb_table.weather_favorites.name
    countries = aws_dynamodb_table.country_favorites.name
  }
}
