# GloboClima - Teste Técnico AUVO

Sistema completo de consulta de clima e países com gerenciamento de favoritos.

## 🚀 Demonstração Online

- **API (AWS Lambda)**: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod
- **Swagger**: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod/swagger
- **Frontend Demo**: http://auvo-globoclima-frontend-071e8010.s3-website-us-east-1.amazonaws.com

## ✅ Requisitos Implementados

### Backend
- [x] API RESTful com .NET 8 e Clean Architecture
- [x] AWS Lambda Serverless (100% Free Tier)
- [x] DynamoDB para persistência
- [x] JWT Authentication com BCrypt
- [x] Integração com WeatherAPI e REST Countries
- [x] Swagger/OpenAPI documentation
- [x] Testes unitários com xUnit
- [x] CI/CD com GitHub Actions
- [x] Terraform para Infrastructure as Code

### Frontend
- [x] Blazor Server com Bootstrap 5
- [x] Interface totalmente responsiva
- [x] Autenticação JWT integrada
- [x] Gerenciamento de favoritos
- [x] Consumo da API REST

## 🏗️ Arquitetura

```
src/
├── GloboClima.Domain/        # Entidades e interfaces do domínio
├── GloboClima.Application/   # Casos de uso e lógica de negócio
├── GloboClima.Infrastructure/# Implementações e integrações externas
├── GloboClima.Lambda/        # Function handler AWS Lambda
└── GloboClima.Web/          # Frontend Blazor Server
```

## 🔧 Como Executar

### Backend (já está online)
A API está hospedada na AWS e funcionando 24/7.

### Frontend Blazor
```bash
# Clone o repositório
git clone [url-do-repositorio]

# Execute o Blazor
cd src/GloboClima.Web
dotnet run

# Acesse
https://localhost:7282
```

## 📚 Endpoints da API

### Autenticação
- `POST /api/auth/register` - Registro de usuário
- `POST /api/auth/login` - Login

### Clima
- `GET /api/weather/city/{city}` - Consultar clima
- `GET /api/weather/favorites` - Listar favoritos (auth)
- `POST /api/weather/favorites` - Adicionar favorito (auth)
- `DELETE /api/weather/favorites/{id}` - Remover favorito (auth)

### Países
- `GET /api/countries/{name}` - Consultar país
- `GET /api/countries/favorites` - Listar favoritos (auth)
- `POST /api/countries/favorites` - Adicionar favorito (auth)
- `DELETE /api/countries/favorites/{id}` - Remover favorito (auth)

## 🧪 Testes

```bash
# Executar testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## 🚀 Deploy

### Infrastructure (Terraform)
```bash
cd infrastructure
terraform init
terraform apply
```

### Lambda
O deploy é feito automaticamente via GitHub Actions ao fazer push para main.

## 💡 Decisões Técnicas

1. **Serverless com Lambda**: Escolhido para garantir custo zero e escalabilidade automática
2. **DynamoDB**: Banco NoSQL para melhor performance e custo no modelo serverless
3. **Clean Architecture**: Separação clara de responsabilidades e testabilidade
4. **Blazor Server**: Rica experiência de usuário com C# no frontend

## 📝 Observações

- O frontend Blazor Server requer hospedagem com suporte .NET para produção
- Uma versão simplificada em HTML está disponível no S3 para demonstração
- Todos os requisitos do teste foram implementados e estão funcionais