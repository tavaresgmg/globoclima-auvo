#!/bin/bash

# Script de deploy para GloboClima
set -e

# Verificar parâmetros
if [ "$#" -ne 1 ]; then
    echo "Uso: $0 <environment>"
    echo "Ambientes válidos: dev, staging, prod"
    exit 1
fi

ENVIRONMENT=$1

# Validar ambiente
if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
    echo "Erro: Ambiente inválido. Use: dev, staging ou prod"
    exit 1
fi

echo "🚀 Iniciando deploy para ambiente: $ENVIRONMENT"

# Verificar se as variáveis de ambiente estão definidas
if [ -z "$TF_VAR_jwt_secret" ]; then
    echo "❌ Erro: TF_VAR_jwt_secret não está definido"
    exit 1
fi

if [ -z "$TF_VAR_openweathermap_api_key" ]; then
    echo "❌ Erro: TF_VAR_openweathermap_api_key não está definido"
    exit 1
fi

# Build da aplicação
echo "📦 Building aplicação .NET..."
cd ../../src/GloboClima.Api
dotnet publish -c Release -r linux-x64 --self-contained -o bin/Release/net8.0/linux-x64/publish

# Voltar para o diretório do Terraform
cd ../../infrastructure/terraform

# Inicializar Terraform
echo "🔧 Inicializando Terraform..."
terraform init

# Validar configuração
echo "✅ Validando configuração Terraform..."
terraform validate

# Criar workspace se não existir
terraform workspace select $ENVIRONMENT 2>/dev/null || terraform workspace new $ENVIRONMENT

# Aplicar Terraform
echo "🏗️ Aplicando configuração Terraform..."
terraform apply -var-file="environments/$ENVIRONMENT.tfvars" -auto-approve

# Capturar outputs
API_URL=$(terraform output -raw api_gateway_url)
echo "✅ Deploy concluído!"
echo "🌐 API URL: $API_URL"
echo "📝 Para testar: curl $API_URL/swagger"