provider "aws" {
  region = var.aws_region
  
  default_tags {
    tags = merge(var.tags, {
      Environment = var.environment
      Terraform   = "true"
    })
  }
}

locals {
  common_tags = merge(var.tags, {
    Environment = var.environment
  })
  
  resource_prefix = "${var.project_name}-${var.environment}"
}

# DynamoDB Tables
module "dynamodb" {
  source = "./modules/dynamodb"
  
  resource_prefix     = local.resource_prefix
  environment         = var.environment
  enable_point_in_time_recovery = var.environment == "prod"
  tags               = local.common_tags
}

# IAM Roles and Policies
module "iam" {
  source = "./modules/iam"
  
  resource_prefix      = local.resource_prefix
  dynamodb_tables_arns = module.dynamodb.table_arns
  tags                = local.common_tags
}

# Lambda Function
module "lambda" {
  source = "./modules/lambda"
  
  resource_prefix        = local.resource_prefix
  environment           = var.environment
  lambda_role_arn       = module.iam.lambda_role_arn
  lambda_timeout        = var.lambda_timeout
  lambda_memory_size    = var.lambda_memory_size
  enable_xray_tracing   = var.enable_xray_tracing
  
  environment_variables = {
    AWS_REGION             = var.aws_region
    JWT_SECRET            = var.jwt_secret
    OPENWEATHERMAP_API_KEY = var.openweathermap_api_key
    ASPNETCORE_ENVIRONMENT = var.environment == "prod" ? "Production" : "Development"
  }
  
  tags = local.common_tags
}

# API Gateway
module "api_gateway" {
  source = "./modules/api-gateway"
  
  resource_prefix         = local.resource_prefix
  environment            = var.environment
  lambda_function_arn    = module.lambda.function_arn
  lambda_function_name   = module.lambda.function_name
  enable_xray_tracing    = var.enable_xray_tracing
  tags                   = local.common_tags
}

# CloudWatch Log Groups
resource "aws_cloudwatch_log_group" "api_logs" {
  name              = "/aws/lambda/${module.lambda.function_name}"
  retention_in_days = var.environment == "prod" ? 30 : 7
  
  tags = local.common_tags
}

# CloudWatch Alarms
resource "aws_cloudwatch_metric_alarm" "lambda_errors" {
  alarm_name          = "${local.resource_prefix}-lambda-errors"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "2"
  metric_name         = "Errors"
  namespace           = "AWS/Lambda"
  period              = "60"
  statistic           = "Sum"
  threshold           = "10"
  alarm_description   = "This metric monitors lambda errors"
  treat_missing_data  = "notBreaching"

  dimensions = {
    FunctionName = module.lambda.function_name
  }
  
  tags = local.common_tags
}

resource "aws_cloudwatch_metric_alarm" "lambda_throttles" {
  alarm_name          = "${local.resource_prefix}-lambda-throttles"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = "1"
  metric_name         = "Throttles"
  namespace           = "AWS/Lambda"
  period              = "60"
  statistic           = "Sum"
  threshold           = "5"
  alarm_description   = "This metric monitors lambda throttles"
  treat_missing_data  = "notBreaching"

  dimensions = {
    FunctionName = module.lambda.function_name
  }
  
  tags = local.common_tags
}