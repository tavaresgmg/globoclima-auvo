# Users Table
resource "aws_dynamodb_table" "users" {
  name         = "${var.resource_prefix}-Users"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
  attribute {
    name = "Id"
    type = "S"
  }
  
  attribute {
    name = "Email"
    type = "S"
  }
  
  global_secondary_index {
    name            = "EmailIndex"
    hash_key        = "Email"
    projection_type = "ALL"
  }
  
  server_side_encryption {
    enabled = true
  }
  
  point_in_time_recovery {
    enabled = var.enable_point_in_time_recovery
  }
  
  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-Users"
  })
}

# Weather Favorites Table
resource "aws_dynamodb_table" "weather_favorites" {
  name         = "${var.resource_prefix}-WeatherFavorites"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
  attribute {
    name = "Id"
    type = "S"
  }
  
  attribute {
    name = "UserId"
    type = "S"
  }
  
  global_secondary_index {
    name            = "UserIdIndex"
    hash_key        = "UserId"
    projection_type = "ALL"
  }
  
  server_side_encryption {
    enabled = true
  }
  
  point_in_time_recovery {
    enabled = var.enable_point_in_time_recovery
  }
  
  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-WeatherFavorites"
  })
}

# Country Favorites Table
resource "aws_dynamodb_table" "country_favorites" {
  name         = "${var.resource_prefix}-CountryFavorites"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  
  attribute {
    name = "Id"
    type = "S"
  }
  
  attribute {
    name = "UserId"
    type = "S"
  }
  
  global_secondary_index {
    name            = "UserIdIndex"
    hash_key        = "UserId"
    projection_type = "ALL"
  }
  
  server_side_encryption {
    enabled = true
  }
  
  point_in_time_recovery {
    enabled = var.enable_point_in_time_recovery
  }
  
  tags = merge(var.tags, {
    Name = "${var.resource_prefix}-CountryFavorites"
  })
}