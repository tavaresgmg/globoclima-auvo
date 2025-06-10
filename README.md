# 🌍 GloboClima

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge&logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Server-purple?style=for-the-badge&logo=blazor)
![AWS](https://img.shields.io/badge/AWS-Cloud-orange?style=for-the-badge&logo=amazonaws)
![Docker](https://img.shields.io/badge/Docker-Containerized-blue?style=for-the-badge&logo=docker)
![Tests](https://img.shields.io/badge/Coverage-31.6%25-yellow?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**Sistema completo de informações meteorológicas e geográficas com arquitetura serverless na AWS**

[🚀 Demo Live](https://globoclima.exemplo.com) • [📖 Documentação](./docs/) • [🐛 Reportar Bug](https://github.com/seu-usuario/globoclima/issues) • [💡 Solicitar Feature](https://github.com/seu-usuario/globoclima/issues)

</div>

---

## 🎯 **Sobre o Projeto**

GloboClima é uma aplicação web moderna desenvolvida para o teste técnico da **AUVO**, demonstrando as melhores práticas de desenvolvimento full-stack com .NET, arquitetura serverless e DevOps. 

### 🏆 **Destaques Técnicos**
- ✅ **Clean Architecture** com separação clara de responsabilidades
- ✅ **Microserviços** com containerização Docker
- ✅ **CI/CD** completo com GitHub Actions
- ✅ **Infraestrutura como Código** com Terraform
- ✅ **Monitoramento** com CloudWatch e alertas
- ✅ **Testes automatizados** (unitários e integração)
- ✅ **Segurança** com JWT e escaneamento de vulnerabilidades

---

## 🚀 **Funcionalidades**

| Feature | Descrição | Status |
|---------|-----------|--------|
| 🌡️ **Consulta Meteorológica** | Informações climáticas em tempo real via WeatherAPI | ✅ |
| 🗺️ **Dados Geográficos** | Informações de países via REST Countries API | ✅ |
| ⭐ **Sistema de Favoritos** | CRUD completo para cidades e países favoritos | ✅ |
| 🔐 **Autenticação JWT** | Login/registro seguro com tokens | ✅ |
| 🌐 **Interface Responsiva** | UI moderna e adaptável com Bootstrap | ✅ |
| 🇧🇷 **Tradução Automática** | Conteúdo localizado para português | ✅ |
| 📊 **Dashboard de Monitoramento** | Métricas em tempo real com CloudWatch | ✅ |
| 🔄 **Auto-scaling** | Escalonamento automático baseado em carga | ✅ |

---

## 🏗️ **Arquitetura**

### 🎯 **Estrutura do Projeto**

```
📦 GloboClima/
├── 🔧 .github/workflows/       # CI/CD Pipelines
├── 📋 docs/                    # Documentação
├── 🏗️ infrastructure/         # Terraform (AWS)
├── 🧪 tests/                   # Testes Automatizados
│   ├── 🔬 *.Tests/            # Testes Unitários
│   └── 🔍 *.Integration.Tests/ # Testes de Integração
└── 💻 src/                     # Código Fonte
    ├── 🌐 GloboClima.Api/      # Web API (.NET 8)
    ├── 🎨 GloboClima.Web/      # Frontend (Blazor)
    ├── 📋 GloboClima.Application/ # Casos de Uso
    ├── 🏛️ GloboClima.Domain/   # Entidades de Negócio
    └── 🔌 GloboClima.Infrastructure/ # Infraestrutura
```

---

## 🛠️ **Stack Tecnológica**

### **Backend**
- ![.NET](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet) **ASP.NET Core 8** - API REST
- ![JWT](https://img.shields.io/badge/JWT-Authentication-green) **JWT Bearer** - Autenticação
- ![Swagger](https://img.shields.io/badge/Swagger-Documentation-orange) **OpenAPI/Swagger** - Documentação

### **Frontend**
- ![Blazor](https://img.shields.io/badge/Blazor-Server-purple) **Blazor Server** - UI Interativa
- ![Bootstrap](https://img.shields.io/badge/Bootstrap-5-purple) **Bootstrap 5** - Design System
- ![Icons](https://img.shields.io/badge/Bootstrap-Icons-blue) **Bootstrap Icons** - Iconografia

### **Banco de Dados**
- ![DynamoDB](https://img.shields.io/badge/AWS-DynamoDB-orange) **DynamoDB** - NoSQL (Produção)
- ![Memory](https://img.shields.io/badge/In--Memory-Development-green) **In-Memory** - Desenvolvimento

### **Infraestrutura**
- ![AWS](https://img.shields.io/badge/AWS-Cloud-orange) **ECS Fargate** - Container Orchestration
- ![ALB](https://img.shields.io/badge/AWS-ALB-orange) **Application Load Balancer** - Balanceamento
- ![CloudWatch](https://img.shields.io/badge/AWS-CloudWatch-orange) **CloudWatch** - Monitoramento
- ![Terraform](https://img.shields.io/badge/Terraform-IaC-purple) **Terraform** - Infrastructure as Code

### **DevOps**
- ![Docker](https://img.shields.io/badge/Docker-Containerization-blue) **Docker** - Containerização
- ![GitHub](https://img.shields.io/badge/GitHub-Actions-black) **GitHub Actions** - CI/CD
- ![Trivy](https://img.shields.io/badge/Trivy-Security-red) **Trivy** - Security Scanning

---

## 🚀 **Quick Start**

### **Pré-requisitos**

- ![.NET](https://img.shields.io/badge/.NET-8.0+-blue) [.NET 8 SDK](https://dotnet.microsoft.com/download)
- ![Docker](https://img.shields.io/badge/Docker-Latest-blue) [Docker](https://docker.com) (opcional)
- ![AWS](https://img.shields.io/badge/AWS-CLI-orange) [AWS CLI](https://aws.amazon.com/cli/) (para deploy)

### **🔥 Execução Local (Modo Rápido)**

```bash
# 1️⃣ Clone o repositório
git clone https://github.com/seu-usuario/globoclima.git
cd globoclima

# 2️⃣ Execute o script de desenvolvimento
chmod +x run-dev.sh
./run-dev.sh
```

### **⚙️ Configuração Manual**

```bash
# 1️⃣ Restaurar dependências
dotnet restore

# 2️⃣ Terminal 1: API
dotnet run --project src/GloboClima.Api

# 3️⃣ Terminal 2: Web App
dotnet run --project src/GloboClima.Web

# 4️⃣ Acessar aplicação
# Web: http://localhost:5001
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

---

## 🧪 **Testes**

### **📊 Cobertura Atual: 31.6%**

```bash
# 🧪 Executar todos os testes
dotnet test

# 📊 Testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# 📈 Gerar relatório de cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

### **🎯 Tipos de Teste**

| Tipo | Quantidade | Cobertura | Status |
|------|------------|-----------|--------|
| 🔬 **Unitários** | 39 testes | Domínio + Application | ✅ Passando |
| 🔍 **Integração** | 12/27 testes | Controllers + API | ⚠️ Parcial |
| 🛡️ **Segurança** | Trivy Scan | Vulnerabilidades | ✅ Automatizado |

---

## 🚀 **Deploy na AWS**

### **☁️ Deploy Automatizado (Recomendado)**

O deploy acontece automaticamente via **GitHub Actions** quando você faz push para `main`:

1. ✅ **Testes** executam automaticamente
2. 🏗️ **Build** das imagens Docker
3. 📦 **Push** para AWS ECR
4. 🚀 **Deploy** via Terraform
5. 📊 **Monitoramento** ativo

### **🔧 Deploy Manual**

```bash
# 1️⃣ Configurar AWS
aws configure

# 2️⃣ Deploy da infraestrutura
cd infrastructure
terraform init
terraform plan
terraform apply
```

---

## 📊 **Monitoramento**

### **🎯 Métricas Principais**

| Métrica | Descrição | Alertas |
|---------|-----------|---------|
| 💻 **CPU Usage** | Utilização de CPU do ECS | > 80% |
| 🧠 **Memory Usage** | Uso de memória | > 80% |
| ⏱️ **Response Time** | Tempo de resposta da API | > 2s |
| 📊 **Request Count** | Número de requisições | - |
| ❌ **Error Rate** | Taxa de erros HTTP | > 5% |

### **📈 Dashboard CloudWatch**

O projeto inclui dashboards automatizados com:

- 📊 **Métricas de Performance** (CPU, Memória, Rede)
- 🌐 **Load Balancer** (Requisições, Latência, Erros)
- 🗃️ **DynamoDB** (Leituras, Escritas, Throttles)
- 🚨 **Alertas SNS** para eventos críticos

---

## 🔒 **Segurança**

### **🛡️ Medidas Implementadas**

- ✅ **JWT Authentication** com tokens seguros
- ✅ **HTTPS** obrigatório em produção  
- ✅ **Secrets Management** via AWS Systems Manager
- ✅ **Security Scanning** automatizado com Trivy
- ✅ **CORS** configurado adequadamente
- ✅ **Input Validation** em todas as APIs
- ✅ **Error Handling** sem exposição de dados sensíveis

---

## 🔌 **API Endpoints**

### **🔐 Autenticação**
```http
POST /api/auth/register    # Registrar usuário
POST /api/auth/login       # Login
```

### **🌤️ Clima**
```http
GET  /api/weather/{city}                    # Clima por cidade
GET  /api/weather/coordinates?lat=&lon=     # Clima por coordenadas
GET  /api/weather/favorites                 # Favoritos de clima
POST /api/weather/favorites                 # Adicionar favorito
DELETE /api/weather/favorites/{id}          # Remover favorito
```

### **🌍 Países**
```http
GET  /api/countries/search?name=           # Buscar países
GET  /api/countries/{code}                 # País por código
GET  /api/countries/favorites              # Favoritos de países
POST /api/countries/favorites              # Adicionar favorito
DELETE /api/countries/favorites/{id}       # Remover favorito
```

### **💓 Monitoramento**
```http
GET  /health              # Health check
GET  /swagger             # Documentação Swagger
```

---

## 🤝 **Contribuição**

### **📋 Como Contribuir**

1. 🍴 **Fork** o projeto
2. 🌟 **Clone** seu fork
3. 🔀 **Crie** uma branch (`git checkout -b feature/MinhaFeature`)
4. ✨ **Commit** suas mudanças (`git commit -m 'feat: adiciona MinhaFeature'`)
5. 📤 **Push** para a branch (`git push origin feature/MinhaFeature`)
6. 🔃 **Abra** um Pull Request

### **📝 Padrões de Commit**

Utilizamos [Conventional Commits](https://conventionalcommits.org/):

```bash
feat: nova funcionalidade
fix: correção de bug
docs: atualização da documentação
style: formatação, sem mudança de lógica
refactor: refatoração de código
test: adição ou modificação de testes
chore: tarefas de manutenção
```

---

## 📄 **Licença**

Este projeto está licenciado sob a **MIT License** - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 📞 **Suporte**

<div align="center">

**Encontrou um problema? Precisa de ajuda?**

[![Issues](https://img.shields.io/badge/🐛_Reportar_Bug-GitHub_Issues-red?style=for-the-badge)](https://github.com/seu-usuario/globoclima/issues)
[![Email](https://img.shields.io/badge/📧_Email-Contato_Direto-green?style=for-the-badge)](mailto:seu.email@exemplo.com)

</div>

---

<div align="center">

**⭐ Se este projeto foi útil, por favor deixe uma estrela! ⭐**

---

**Desenvolvido com ❤️ para o teste técnico da AUVO**

</div>