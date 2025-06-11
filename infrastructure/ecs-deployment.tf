# ECS/Fargate deployment configuration as alternative to Lambda

# Variables
variable "deploy_to_ecs" {
  description = "Deploy to ECS Fargate instead of Lambda"
  type        = bool
  default     = false
}

# VPC Configuration
resource "aws_vpc" "main" {
  count                = var.deploy_to_ecs ? 1 : 0
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "globoclima-vpc"
  }
}

# Public Subnets
resource "aws_subnet" "public" {
  count                   = var.deploy_to_ecs ? 2 : 0
  vpc_id                  = aws_vpc.main[0].id
  cidr_block              = cidrsubnet(aws_vpc.main[0].cidr_block, 8, count.index)
  availability_zone       = data.aws_availability_zones.available.names[count.index]
  map_public_ip_on_launch = true

  tags = {
    Name = "globoclima-public-subnet-${count.index + 1}"
  }
}

# Private Subnets
resource "aws_subnet" "private" {
  count             = var.deploy_to_ecs ? 2 : 0
  vpc_id            = aws_vpc.main[0].id
  cidr_block        = cidrsubnet(aws_vpc.main[0].cidr_block, 8, count.index + 10)
  availability_zone = data.aws_availability_zones.available.names[count.index]

  tags = {
    Name = "globoclima-private-subnet-${count.index + 1}"
  }
}

# Internet Gateway
resource "aws_internet_gateway" "main" {
  count  = var.deploy_to_ecs ? 1 : 0
  vpc_id = aws_vpc.main[0].id

  tags = {
    Name = "globoclima-igw"
  }
}

# Route Table
resource "aws_route_table" "public" {
  count  = var.deploy_to_ecs ? 1 : 0
  vpc_id = aws_vpc.main[0].id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.main[0].id
  }

  tags = {
    Name = "globoclima-public-rt"
  }
}

# Route Table Association
resource "aws_route_table_association" "public" {
  count          = var.deploy_to_ecs ? 2 : 0
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public[0].id
}

# Security Group
resource "aws_security_group" "ecs" {
  count       = var.deploy_to_ecs ? 1 : 0
  name        = "globoclima-ecs-sg"
  description = "Security group for ECS tasks"
  vpc_id      = aws_vpc.main[0].id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "globoclima-ecs-sg"
  }
}

# Application Load Balancer
resource "aws_lb" "main" {
  count              = var.deploy_to_ecs ? 1 : 0
  name               = "globoclima-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.ecs[0].id]
  subnets            = aws_subnet.public[*].id

  tags = {
    Name = "globoclima-alb"
  }
}

# ALB Target Groups
resource "aws_lb_target_group" "api" {
  count       = var.deploy_to_ecs ? 1 : 0
  name        = "globoclima-api-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main[0].id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name = "globoclima-api-tg"
  }
}

resource "aws_lb_target_group" "web" {
  count       = var.deploy_to_ecs ? 1 : 0
  name        = "globoclima-web-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = aws_vpc.main[0].id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 2
  }

  tags = {
    Name = "globoclima-web-tg"
  }
}

# ALB Listeners
resource "aws_lb_listener" "main" {
  count             = var.deploy_to_ecs ? 1 : 0
  load_balancer_arn = aws_lb.main[0].arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type = "fixed-response"
    fixed_response {
      content_type = "text/plain"
      message_body = "404: Not Found"
      status_code  = "404"
    }
  }
}

# ALB Listener Rules
resource "aws_lb_listener_rule" "api" {
  count        = var.deploy_to_ecs ? 1 : 0
  listener_arn = aws_lb_listener.main[0].arn
  priority     = 100

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api[0].arn
  }

  condition {
    path_pattern {
      values = ["/api/*", "/swagger/*", "/health"]
    }
  }
}

resource "aws_lb_listener_rule" "web" {
  count        = var.deploy_to_ecs ? 1 : 0
  listener_arn = aws_lb_listener.main[0].arn
  priority     = 200

  action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.web[0].arn
  }

  condition {
    path_pattern {
      values = ["/*"]
    }
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  count = var.deploy_to_ecs ? 1 : 0
  name  = "globoclima-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name = "globoclima-cluster"
  }
}

# ECS Task Execution Role
resource "aws_iam_role" "ecs_task_execution" {
  count = var.deploy_to_ecs ? 1 : 0
  name  = "globoclima-ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution" {
  count      = var.deploy_to_ecs ? 1 : 0
  role       = aws_iam_role.ecs_task_execution[0].name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# ECS Task Role
resource "aws_iam_role" "ecs_task" {
  count = var.deploy_to_ecs ? 1 : 0
  name  = "globoclima-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

# Add DynamoDB permissions to ECS Task Role
resource "aws_iam_role_policy" "ecs_task_dynamodb" {
  count = var.deploy_to_ecs ? 1 : 0
  name  = "ecs-task-dynamodb-policy"
  role  = aws_iam_role.ecs_task[0].id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
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

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "ecs" {
  count             = var.deploy_to_ecs ? 1 : 0
  name              = "/ecs/globoclima"
  retention_in_days = 7
}

# ECS Task Definition - API
resource "aws_ecs_task_definition" "api" {
  count                    = var.deploy_to_ecs ? 1 : 0
  family                   = "globoclima-api"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_task_execution[0].arn
  task_role_arn            = aws_iam_role.ecs_task[0].arn

  container_definitions = jsonencode([
    {
      name  = "api"
      image = "globoclima-api:latest"
      portMappings = [
        {
          containerPort = 80
          protocol      = "tcp"
        }
      ]
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:80"
        },
        {
          name  = "AWS_REGION"
          value = "us-east-1"
        },
        {
          name  = "JWT_SECRET"
          value = "CHANGE_THIS_IN_PRODUCTION_USE_SECRETS_MANAGER"
        },
        {
          name  = "USE_OPENWEATHERMAP"
          value = "true"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs[0].name
          "awslogs-region"        = "us-east-1"
          "awslogs-stream-prefix" = "api"
        }
      }
    }
  ])
}

# ECS Task Definition - Web
resource "aws_ecs_task_definition" "web" {
  count                    = var.deploy_to_ecs ? 1 : 0
  family                   = "globoclima-web"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_task_execution[0].arn

  container_definitions = jsonencode([
    {
      name  = "web"
      image = "globoclima-web:latest"
      portMappings = [
        {
          containerPort = 80
          protocol      = "tcp"
        }
      ]
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "ASPNETCORE_URLS"
          value = "http://+:80"
        },
        {
          name  = "ApiSettings__BaseUrl"
          value = "http://${aws_lb.main[0].dns_name}/api"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.ecs[0].name
          "awslogs-region"        = "us-east-1"
          "awslogs-stream-prefix" = "web"
        }
      }
    }
  ])
}

# ECS Service - API
resource "aws_ecs_service" "api" {
  count           = var.deploy_to_ecs ? 1 : 0
  name            = "globoclima-api-service"
  cluster         = aws_ecs_cluster.main[0].id
  task_definition = aws_ecs_task_definition.api[0].arn
  desired_count   = 2
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.ecs[0].id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api[0].arn
    container_name   = "api"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.main[0]]
}

# ECS Service - Web
resource "aws_ecs_service" "web" {
  count           = var.deploy_to_ecs ? 1 : 0
  name            = "globoclima-web-service"
  cluster         = aws_ecs_cluster.main[0].id
  task_definition = aws_ecs_task_definition.web[0].arn
  desired_count   = 2
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.ecs[0].id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.web[0].arn
    container_name   = "web"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.main[0]]
}

# Auto Scaling - API
resource "aws_appautoscaling_target" "api" {
  count              = var.deploy_to_ecs ? 1 : 0
  max_capacity       = 10
  min_capacity       = 2
  resource_id        = "service/${aws_ecs_cluster.main[0].name}/${aws_ecs_service.api[0].name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "api_cpu" {
  count              = var.deploy_to_ecs ? 1 : 0
  name               = "api-cpu-scaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.api[0].resource_id
  scalable_dimension = aws_appautoscaling_target.api[0].scalable_dimension
  service_namespace  = aws_appautoscaling_target.api[0].service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 70.0
  }
}

# Auto Scaling - Web
resource "aws_appautoscaling_target" "web" {
  count              = var.deploy_to_ecs ? 1 : 0
  max_capacity       = 10
  min_capacity       = 2
  resource_id        = "service/${aws_ecs_cluster.main[0].name}/${aws_ecs_service.web[0].name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "web_cpu" {
  count              = var.deploy_to_ecs ? 1 : 0
  name               = "web-cpu-scaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.web[0].resource_id
  scalable_dimension = aws_appautoscaling_target.web[0].scalable_dimension
  service_namespace  = aws_appautoscaling_target.web[0].service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 70.0
  }
}

# Outputs
output "ecs_alb_url" {
  value       = var.deploy_to_ecs ? "http://${aws_lb.main[0].dns_name}" : null
  description = "The URL of the Application Load Balancer"
}

output "ecs_cluster_name" {
  value       = var.deploy_to_ecs ? aws_ecs_cluster.main[0].name : null
  description = "The name of the ECS cluster"
}

data "aws_availability_zones" "available" {
  state = "available"
}