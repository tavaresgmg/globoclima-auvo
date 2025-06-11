# 🚀 GloboClima - Guia de Deploy Sem Custos

Este guia fornece instruções detalhadas para implantar o GloboClima sem custos, utilizando recursos gratuitos e alternativas locais.

## 📋 Índice

1. [Deploy Local com Docker](#deploy-local-com-docker)
2. [Deploy com LocalStack](#deploy-com-localstack)
3. [Deploy na AWS Free Tier](#deploy-na-aws-free-tier)
4. [Deploy com ECS/Fargate](#deploy-com-ecsfargate)
5. [Alternativas Gratuitas](#alternativas-gratuitas)

## 🐳 Deploy Local com Docker

### Pré-requisitos
- Docker Desktop instalado
- Docker Compose instalado
- 4GB de RAM disponível

### Passo a Passo

1. **Clone o repositório**
```bash
git clone https://github.com/seu-usuario/globoclima.git
cd globoclima
```

2. **Configure as variáveis de ambiente**
```bash
cp .env.example .env
# Edite o arquivo .env com suas configurações
```

3. **Inicie os containers**
```bash
docker-compose up -d
```

4. **Acesse a aplicação**
- API: http://localhost:7001
- Web: http://localhost:5000
- Swagger: http://localhost:7001/swagger
- DynamoDB Admin: http://localhost:8001

5. **Para parar os containers**
```bash
docker-compose down
```

## 🧪 Deploy com LocalStack

O LocalStack simula serviços AWS localmente, permitindo testar a infraestrutura sem custos.

### Instalação

1. **Instale o LocalStack**
```bash
pip install localstack
```

2. **Configure o arquivo `docker-compose.localstack.yml`**
```yaml
version: '3.8'
services:
  localstack:
    image: localstack/localstack
    ports:
      - "4566:4566"
    environment:
      - SERVICES=dynamodb,lambda,apigateway,s3
      - DEBUG=1
      - DATA_DIR=/tmp/localstack/data
    volumes:
      - "${TMPDIR:-/tmp}/localstack:/tmp/localstack"
      - "/var/run/docker.sock:/var/run/docker.sock"
```

3. **Inicie o LocalStack**
```bash
docker-compose -f docker-compose.localstack.yml up -d
```

4. **Configure o AWS CLI para LocalStack**
```bash
aws configure set aws_access_key_id test
aws configure set aws_secret_access_key test
aws configure set region us-east-1

# Configure o endpoint
export AWS_ENDPOINT_URL=http://localhost:4566
```

5. **Execute o Terraform**
```bash
cd infrastructure
terraform init
terraform plan -var="aws_endpoint=http://localhost:4566"
terraform apply -var="aws_endpoint=http://localhost:4566"
```

## ☁️ Deploy na AWS Free Tier

### Limites do Free Tier (12 meses)
- **Lambda**: 1 milhão de requests/mês
- **DynamoDB**: 25 GB de armazenamento
- **API Gateway**: 1 milhão de chamadas/mês
- **S3**: 5 GB de armazenamento
- **CloudWatch**: 10 métricas personalizadas

### Configuração

1. **Crie uma conta AWS**
   - Acesse: https://aws.amazon.com/free
   - Forneça um cartão de crédito (não será cobrado se ficar nos limites)

2. **Configure as credenciais**
```bash
aws configure
# AWS Access Key ID: [sua-access-key]
# AWS Secret Access Key: [sua-secret-key]
# Default region name: us-east-1
# Default output format: json
```

3. **Deploy com Terraform**
```bash
cd infrastructure
terraform init
terraform plan
terraform apply
```

4. **Monitoramento de custos**
   - Configure alertas de billing
   - Use AWS Cost Explorer
   - Revise os recursos regularmente

## 🚢 Deploy com ECS/Fargate

### Opção 1: ECS com EC2 (mais barato)

1. **Configure a variável no Terraform**
```bash
cd infrastructure
terraform apply -var="deploy_to_ecs=true"
```

2. **Use instâncias Spot para economia**
```hcl
# Adicione ao ecs-deployment.tf
resource "aws_ecs_capacity_provider" "spot" {
  name = "spot-capacity-provider"
  
  auto_scaling_group_provider {
    auto_scaling_group_arn = aws_autoscaling_group.ecs_spot.arn
    
    managed_scaling {
      status          = "ENABLED"
      target_capacity = 100
    }
  }
}
```

### Opção 2: Deploy Manual no ECS

1. **Build das imagens**
```bash
# API
docker build -t globoclima-api:latest -f src/GloboClima.Api/Dockerfile .

# Web
docker build -t globoclima-web:latest -f src/GloboClima.Web/Dockerfile .
```

2. **Push para ECR**
```bash
# Crie os repositórios
aws ecr create-repository --repository-name globoclima-api
aws ecr create-repository --repository-name globoclima-web

# Login no ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [sua-conta].dkr.ecr.us-east-1.amazonaws.com

# Tag e push
docker tag globoclima-api:latest [sua-conta].dkr.ecr.us-east-1.amazonaws.com/globoclima-api:latest
docker push [sua-conta].dkr.ecr.us-east-1.amazonaws.com/globoclima-api:latest
```

## 🆓 Alternativas Gratuitas

### 1. **Railway.app**
```bash
# Instale a CLI
npm install -g @railway/cli

# Login
railway login

# Deploy
railway up
```

### 2. **Fly.io**
```bash
# Instale a CLI
curl -L https://fly.io/install.sh | sh

# Login
fly auth login

# Deploy
fly launch
fly deploy
```

### 3. **Render.com**
1. Conecte seu repositório GitHub
2. Configure as variáveis de ambiente
3. Deploy automático a cada push

### 4. **Heroku (limitado)**
```bash
# Login
heroku login

# Crie a aplicação
heroku create globoclima-api

# Deploy
git push heroku main
```

## 📊 Comparação de Custos

| Opção | Custo Mensal | Limitações |
|-------|--------------|------------|
| Docker Local | $0 | Apenas desenvolvimento |
| LocalStack | $0 | Simulação local |
| AWS Free Tier | $0* | Limites de uso |
| Railway | $0-5 | 500 horas/mês |
| Fly.io | $0-5 | 3 apps gratuitos |
| Render | $0-7 | Build lento no free |

*Dentro dos limites do Free Tier

## 🔧 Configuração de CI/CD Gratuito

### GitHub Actions (já configurado)
```yaml
# .github/workflows/ci-cd.yml já está configurado
# Apenas configure os secrets:
# - AWS_ACCESS_KEY_ID
# - AWS_SECRET_ACCESS_KEY
# - JWT_SECRET
# - OPENWEATHERMAP_API_KEY
```

### Alternativa: GitLab CI
```yaml
# .gitlab-ci.yml
stages:
  - build
  - test
  - deploy

variables:
  DOCKER_DRIVER: overlay2

build:
  stage: build
  script:
    - docker build -t globoclima-api .
```

## 🛠️ Troubleshooting

### Problema: "Insufficient memory"
```bash
# Aumente a memória do Docker Desktop
# Settings > Resources > Memory: 4GB+
```

### Problema: "Port already in use"
```bash
# Encontre o processo usando a porta
lsof -i :7001
# Mate o processo
kill -9 [PID]
```

### Problema: "AWS credentials not found"
```bash
# Configure as credenciais
aws configure
# Ou use variáveis de ambiente
export AWS_ACCESS_KEY_ID=xxx
export AWS_SECRET_ACCESS_KEY=yyy
```

## 📝 Scripts Úteis

### Script de Deploy Completo
```bash
#!/bin/bash
# deploy.sh

echo "🚀 Iniciando deploy do GloboClima..."

# Build
docker-compose build

# Testes
docker-compose run --rm api dotnet test

# Deploy
if [ "$1" == "production" ]; then
    terraform apply -auto-approve
else
    docker-compose up -d
fi

echo "✅ Deploy concluído!"
```

### Script de Backup
```bash
#!/bin/bash
# backup.sh

# Backup do DynamoDB local
aws dynamodb scan \
    --table-name GloboClima-Users \
    --endpoint-url http://localhost:8000 \
    > backup-users.json

echo "✅ Backup concluído!"
```

## 🎯 Próximos Passos

1. Configure monitoramento com Prometheus/Grafana
2. Implemente cache com Redis
3. Configure CDN com CloudFlare (gratuito)
4. Adicione testes de carga com K6

## 📚 Recursos Adicionais

- [AWS Free Tier](https://aws.amazon.com/free/)
- [LocalStack Docs](https://docs.localstack.cloud/)
- [Docker Compose Docs](https://docs.docker.com/compose/)
- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/)