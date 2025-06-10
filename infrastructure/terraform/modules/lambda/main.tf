# Preparar o código da Lambda
data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.root}/../../src/GloboClima.Api/bin/Release/net8.0/linux-x64/publish"
  output_path = "${path.module}/lambda_function.zip"
}

# Lambda Function
resource "aws_lambda_function" "api" {
  filename         = data.archive_file.lambda_zip.output_path
  function_name    = "${var.resource_prefix}-api"
  role            = var.lambda_role_arn
  handler         = "GloboClima.Api::Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction::FunctionHandlerAsync"
  runtime         = "dotnet8"
  timeout         = var.lambda_timeout
  memory_size     = var.lambda_memory_size
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  
  environment {
    variables = merge(var.environment_variables, {
      LAMBDA_NET_SERIALIZER_DEBUG = "true"
    })
  }
  
  tracing_config {
    mode = var.enable_xray_tracing ? "Active" : "PassThrough"
  }
  
  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-api"
  })
}

# Lambda Permission for API Gateway
resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_lambda_function.api.arn}/*/*"
}

# CloudWatch Log Stream
resource "aws_cloudwatch_log_stream" "api_log_stream" {
  name           = "api-stream-${var.environment}"
  log_group_name = "/aws/lambda/${aws_lambda_function.api.function_name}"
}