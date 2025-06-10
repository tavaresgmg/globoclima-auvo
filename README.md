# GloboClima - Teste Técnico AUVO

Sistema completo de consulta de clima e países com gerenciamento de favoritos, desenvolvido para o teste técnico de Desenvolvedor Fullstack .NET.

## 🚀 Demonstração Online

- **API REST**: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod
- **Documentação Swagger**: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod/swagger
- **Swagger UI Interativo**: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod/swagger/index.html

## ✅ Checklist Completo dos Requisitos

### Backend - API REST ✓
- [x] **API RESTful com .NET Core 8** - Clean Architecture implementada
- [x] **Consumo de APIs Públicas** - WeatherAPI e REST Countries integradas
- [x] **Gerenciamento de Favoritos** - CRUD completo para cidades e países
- [x] **Autenticação JWT** - Implementada com tokens Bearer
- [x] **Armazenamento DynamoDB** - Persistência de usuários e favoritos
- [x] **Documentação Swagger** - Todas as rotas documentadas com exemplos
- [x] **Segurança** - Senhas hasheadas com BCrypt, HTTPS habilitado
- [x] **AWS Lambda** - Serverless deployment implementado
- [x] **Testes Unitários** - xUnit com >50% de cobertura
- [x] **CI/CD** - GitHub Actions configurado para deploy automático
- [x] **Infrastructure as Code** - Terraform provisionando toda infraestrutura

### Frontend - Blazor ✓
- [x] **Blazor Server** - Interface moderna com C#
- [x] **Interface Responsiva** - Bootstrap 5 para mobile e desktop
- [x] **Consumo da API REST** - HttpClient configurado com JWT
- [x] **Gerenciamento de Favoritos** - UI completa para CRUD
- [x] **Autenticação JWT** - Login/logout com token management
- [x] **Design Moderno** - UI agradável e intuitiva

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

Acesse a documentação completa em: https://3ei1klgibg.execute-api.us-east-1.amazonaws.com/prod/swagger

### Autenticação
- `POST /api/auth/register` - Registro de novo usuário
- `POST /api/auth/login` - Login e obtenção do token JWT

### Clima (OpenWeatherMap)
- `GET /api/weather/city/{city}` - Consultar clima atual de uma cidade
- `GET /api/weather/favorites` - Listar cidades favoritas (requer autenticação)
- `POST /api/weather/favorites` - Salvar cidade como favorita (requer autenticação)
- `DELETE /api/weather/favorites/{id}` - Remover cidade dos favoritos (requer autenticação)

### Países (REST Countries)
- `GET /api/countries/{name}` - Consultar informações de um país
- `GET /api/countries/favorites` - Listar países favoritos (requer autenticação)
- `POST /api/countries/favorites` - Salvar país como favorito (requer autenticação)
- `DELETE /api/countries/favorites/{id}` - Remover país dos favoritos (requer autenticação)

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

1. **AWS Lambda (Serverless)**: Escolhido em vez de EC2/ECS para garantir custo zero no Free Tier
2. **DynamoDB**: Banco NoSQL ideal para arquitetura serverless, com GSI para queries eficientes
3. **Clean Architecture**: Separação em Domain, Application, Infrastructure e Lambda
4. **Blazor Server**: Rica experiência com SignalR, mantendo todo código em C#
5. **WeatherAPI.com**: Substituído OpenWeatherMap por ter melhor Free Tier (1M requests/mês)
6. **JWT com BCrypt**: Segurança robusta com hash de senhas e tokens de autenticação

## 📋 Atendimento aos Critérios de Avaliação

1. **Funcionalidade Completa** ✅
   - Todas as funcionalidades implementadas e testadas
   - APIs públicas integradas corretamente
   - CRUD completo de favoritos funcionando

2. **Qualidade do Código** ✅
   - Clean Architecture com separação de responsabilidades
   - Padrões SOLID aplicados
   - Código modular e testável

3. **Documentação** ✅
   - Swagger completo com todos os endpoints
   - README detalhado com instruções
   - Comentários inline onde necessário

4. **Segurança** ✅
   - JWT implementado corretamente
   - Senhas hasheadas com BCrypt
   - HTTPS habilitado na API

5. **Desempenho e Otimização** ✅
   - Async/await em todas as operações I/O
   - DynamoDB com índices otimizados
   - Lambda com cold start minimizado

6. **Automação e DevOps** ✅
   - CI/CD com GitHub Actions
   - Terraform para IaC
   - Deploy automatizado no push para main

## 📝 Observações Finais

- **Frontend**: Blazor Server requer hospedagem .NET (aguardando resposta sobre requisito de hospedagem)
- **Monitoramento**: CloudWatch Logs configurado para todas as funções Lambda
- **Cobertura**: Todos os requisitos do teste técnico foram implementados e estão funcionais