# GloboClima - Infraestrutura AWS com Terraform

## Estrutura

```
terraform/
├── modules/              # Módulos reutilizáveis
│   ├── dynamodb/        # Tabelas DynamoDB
│   ├── lambda/          # Função Lambda
│   ├── api-gateway/     # API Gateway
│   └── iam/             # Roles e políticas IAM
├── environments/        # Variáveis por ambiente
│   ├── dev.tfvars
│   ├── staging.tfvars
│   └── prod.tfvars
├── scripts/            # Scripts auxiliares
│   └── deploy.sh
├── main.tf             # Configuração principal
├── variables.tf        # Definição de variáveis
├── outputs.tf          # Outputs do Terraform
└── versions.tf         # Versões do Terraform e providers
```

## Pré-requisitos

1. **AWS CLI configurado**
   ```bash
   aws configure
   ```

2. **Terraform instalado** (>= 1.7.0)
   ```bash
   brew install terraform
   ```

3. **Variáveis de ambiente**
   ```bash
   export TF_VAR_jwt_secret="your-jwt-secret-here"
   export TF_VAR_openweathermap_api_key="your-api-key-here"
   ```

4. **S3 Bucket para state** (criar manualmente)
   ```bash
   aws s3 mb s3://globoclima-terraform-state
   ```

## Deploy

### Deploy Automático

```bash
cd infrastructure/terraform/scripts
./deploy.sh dev    # Para desenvolvimento
./deploy.sh staging # Para staging
./deploy.sh prod   # Para produção
```

### Deploy Manual

1. **Build da aplicação**
   ```bash
   cd src/GloboClima.Api
   dotnet publish -c Release -r linux-x64 --self-contained -o bin/Release/net8.0/linux-x64/publish
   ```

2. **Inicializar Terraform**
   ```bash
   cd infrastructure/terraform
   terraform init
   ```

3. **Criar workspace**
   ```bash
   terraform workspace new dev
   terraform workspace select dev
   ```

4. **Aplicar configuração**
   ```bash
   terraform apply -var-file="environments/dev.tfvars"
   ```

## Recursos Criados

### DynamoDB Tables
- `GloboClima-{env}-Users`
- `GloboClima-{env}-WeatherFavorites`
- `GloboClima-{env}-CountryFavorites`

### Lambda Function
- Runtime: .NET 8
- Handler: ASP.NET Core
- Timeout: 30-60s (configurável)
- Memory: 256-1024 MB (configurável)

### API Gateway
- Type: REST API
- Stage: {environment}
- Tracing: X-Ray (opcional)

### CloudWatch
- Log Groups com retenção automática
- Alarms para erros e throttling
- Metrics habilitadas

## Ambientes

### Development (dev)
- Lambda Memory: 256 MB
- X-Ray: Desabilitado
- Log Retention: 7 dias

### Staging
- Lambda Memory: 512 MB
- X-Ray: Habilitado
- Log Retention: 7 dias

### Production (prod)
- Lambda Memory: 1024 MB
- X-Ray: Habilitado
- Log Retention: 30 dias
- Point-in-time recovery: Habilitado

## Custos Estimados

### DynamoDB (PAY_PER_REQUEST)
- $0.25 por milhão de requests de leitura
- $1.25 por milhão de requests de escrita

### Lambda
- Free tier: 1M requests/mês
- $0.20 por 1M requests adicionais

### API Gateway
- $3.50 por milhão de chamadas
- $0.09/GB de transferência

## Destruir Infraestrutura

```bash
terraform destroy -var-file="environments/dev.tfvars"
```

## Troubleshooting

### Lambda não atualiza
```bash
# Forçar nova build
rm -rf modules/lambda/lambda_function.zip
terraform apply -var-file="environments/dev.tfvars"
```

### State lock
```bash
terraform force-unlock <LOCK_ID>
```

### Logs da Lambda
```bash
aws logs tail /aws/lambda/GloboClima-dev-api --follow
```