# 🔌 Documentação da API

## 📋 Visão Geral

A API REST do GloboClima oferece endpoints para autenticação, consulta meteorológica, informações de países e gerenciamento de favoritos.

**Base URL**: `https://api.globoclima.com`  
**Versão**: `v1.0`  
**Formato**: `JSON`  
**Autenticação**: `JWT Bearer Token`

## 🔐 Autenticação

### **Registrar Usuário**

```http
POST /api/auth/register
```

**Request Body:**
```json
{
  "email": "usuario@exemplo.com",
  "password": "MinhaSenh@123",
  "confirmPassword": "MinhaSenh@123",
  "firstName": "João",
  "lastName": "Silva"
}
```

**Response (201 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "usuario@exemplo.com",
    "firstName": "João",
    "lastName": "Silva"
  },
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

**Possíveis Erros:**
- `400 Bad Request` - Dados inválidos
- `409 Conflict` - Email já existe

---

### **Login**

```http
POST /api/auth/login
```

**Request Body:**
```json
{
  "email": "usuario@exemplo.com",
  "password": "MinhaSenh@123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "usuario@exemplo.com",
    "firstName": "João",
    "lastName": "Silva"
  },
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

**Possíveis Erros:**
- `400 Bad Request` - Dados inválidos
- `401 Unauthorized` - Credenciais incorretas

---

## 🌤️ Clima

### **Consultar Clima por Cidade**

```http
GET /api/weather/{cityName}
```

**Parâmetros:**
- `cityName` (string): Nome da cidade (URL encoded)

**Exemplo:**
```http
GET /api/weather/São%20Paulo
```

**Response (200 OK):**
```json
{
  "name": "São Paulo",
  "country": "Brasil",
  "coord": {
    "lat": -23.5505,
    "lon": -46.6333
  },
  "main": {
    "temp": 25.5,
    "feelsLike": 28.2,
    "tempMin": 22.1,
    "tempMax": 28.9,
    "pressure": 1013,
    "humidity": 65
  },
  "weather": [
    {
      "main": "Nublado",
      "description": "nuvens dispersas",
      "icon": "03d"
    }
  ],
  "wind": {
    "speed": 3.2,
    "deg": 180
  },
  "visibility": 10000,
  "sys": {
    "country": "BR",
    "sunrise": "2024-01-15T08:30:00Z",
    "sunset": "2024-01-15T20:45:00Z"
  }
}
```

**Possíveis Erros:**
- `404 Not Found` - Cidade não encontrada
- `500 Internal Server Error` - Erro da API externa

---

### **Consultar Clima por Coordenadas**

```http
GET /api/weather/coordinates?lat={latitude}&lon={longitude}
```

**Query Parameters:**
- `lat` (double): Latitude (-90 a 90)
- `lon` (double): Longitude (-180 a 180)

**Exemplo:**
```http
GET /api/weather/coordinates?lat=-23.5505&lon=-46.6333
```

**Response:** Mesmo formato da consulta por cidade

**Possíveis Erros:**
- `400 Bad Request` - Coordenadas inválidas

---

### **Listar Favoritos de Clima**

```http
GET /api/weather/favorites
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "cityName": "São Paulo",
    "countryCode": "BR",
    "latitude": -23.5505,
    "longitude": -46.6333,
    "addedAt": "2024-01-15T10:30:00Z",
    "currentWeather": {
      "temperature": 25.5,
      "feelsLike": 28.2,
      "description": "nuvens dispersas",
      "icon": "03d",
      "humidity": 65,
      "windSpeed": 3.2
    }
  }
]
```

**Possíveis Erros:**
- `401 Unauthorized` - Token inválido ou expirado

---

### **Adicionar Favorito de Clima**

```http
POST /api/weather/favorites
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "cityName": "Rio de Janeiro",
  "countryCode": "BR",
  "latitude": -22.9068,
  "longitude": -43.1729
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "cityName": "Rio de Janeiro",
  "countryCode": "BR",
  "latitude": -22.9068,
  "longitude": -43.1729,
  "addedAt": "2024-01-15T11:00:00Z"
}
```

**Possíveis Erros:**
- `400 Bad Request` - Dados inválidos
- `401 Unauthorized` - Token inválido
- `409 Conflict` - Favorito já existe

---

### **Remover Favorito de Clima**

```http
DELETE /api/weather/favorites/{favoriteId}
Authorization: Bearer {token}
```

**Parâmetros:**
- `favoriteId` (GUID): ID do favorito

**Response (204 No Content)**

**Possíveis Erros:**
- `401 Unauthorized` - Token inválido
- `404 Not Found` - Favorito não encontrado

---

## 🌍 Países

### **Buscar Países**

```http
GET /api/countries/search?name={countryName}
```

**Query Parameters:**
- `name` (string): Nome do país (parcial ou completo)

**Exemplo:**
```http
GET /api/countries/search?name=Brasil
```

**Response (200 OK):**
```json
[
  {
    "name": {
      "common": "Brasil",
      "official": "República Federativa do Brasil"
    },
    "cca2": "BR",
    "cca3": "BRA",
    "capital": ["Brasília"],
    "region": "Americas",
    "subregion": "South America",
    "population": 215313498,
    "area": 8515767.0,
    "languages": {
      "por": "Português"
    },
    "currencies": {
      "BRL": {
        "name": "Real brasileiro",
        "symbol": "R$"
      }
    },
    "flags": {
      "svg": "https://flagcdn.com/br.svg",
      "png": "https://flagcdn.com/w320/br.png"
    }
  }
]
```

---

### **Consultar País por Código**

```http
GET /api/countries/{countryCode}
```

**Parâmetros:**
- `countryCode` (string): Código ISO 2 ou 3 letras (BR, BRA)

**Exemplo:**
```http
GET /api/countries/BR
```

**Response:** Mesmo formato da busca

**Possíveis Erros:**
- `404 Not Found` - País não encontrado

---

### **Listar Favoritos de Países**

```http
GET /api/countries/favorites
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "countryCode": "BR",
    "countryName": "Brasil",
    "region": "Americas",
    "addedAt": "2024-01-15T10:30:00Z",
    "countryInfo": {
      "name": "Brasil",
      "capital": "Brasília",
      "region": "Americas",
      "subregion": "South America",
      "population": 215313498,
      "area": 8515767.0,
      "flag": "https://flagcdn.com/br.svg",
      "languages": ["Português"],
      "currencies": ["Real brasileiro"]
    }
  }
]
```

---

### **Adicionar Favorito de País**

```http
POST /api/countries/favorites
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "countryCode": "JP",
  "countryName": "Japão",
  "region": "Asia"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "countryCode": "JP",
  "countryName": "Japão",
  "region": "Asia",
  "addedAt": "2024-01-15T11:15:00Z"
}
```

---

### **Remover Favorito de País**

```http
DELETE /api/countries/favorites/{favoriteId}
Authorization: Bearer {token}
```

**Response (204 No Content)**

---

## 💓 Monitoramento

### **Health Check**

```http
GET /health
```

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T12:00:00Z",
  "version": "1.0.0",
  "service": "GloboClima.Api"
}
```

---

## 📚 Swagger/OpenAPI

### **Documentação Interativa**

```http
GET /swagger
```

Acesse a documentação interativa da API com todos os endpoints, schemas e possibilidade de teste direto na interface.

---

## 🔧 Códigos de Status HTTP

| Código | Significado | Quando Usar |
|--------|-------------|-------------|
| `200` | OK | Requisição bem-sucedida |
| `201` | Created | Recurso criado com sucesso |
| `204` | No Content | Operação bem-sucedida sem retorno |
| `400` | Bad Request | Dados de entrada inválidos |
| `401` | Unauthorized | Token inválido ou ausente |
| `403` | Forbidden | Acesso negado |
| `404` | Not Found | Recurso não encontrado |
| `409` | Conflict | Recurso já existe |
| `422` | Unprocessable Entity | Entidade inválida |
| `429` | Too Many Requests | Rate limit excedido |
| `500` | Internal Server Error | Erro interno do servidor |
| `502` | Bad Gateway | Erro de gateway |
| `503` | Service Unavailable | Serviço indisponível |

---

## 🔒 Autenticação JWT

### **Formato do Token**

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6InVzdWFyaW9AZXhlbXBsby5jb20iLCJpYXQiOjE2MDk0NTkyMDAsImV4cCI6MTYwOTQ2MjgwMH0.signature
```

### **Claims do Token**

```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "usuario@exemplo.com",
  "iat": 1609459200,
  "exp": 1609462800
}
```

### **Validação**
- ✅ Token deve estar presente no header `Authorization`
- ✅ Token deve ter o prefixo `Bearer `
- ✅ Token deve ser válido e não expirado
- ✅ Usuário deve existir no sistema

---

## 📊 Rate Limiting

| Endpoint | Limite | Janela |
|----------|--------|--------|
| **Autenticação** | 10 req/min | Por IP |
| **Clima** | 100 req/min | Por usuário |
| **Países** | 50 req/min | Por usuário |
| **Favoritos** | 30 req/min | Por usuário |

---

## 🌐 CORS

### **Origins Permitidas**
- `https://globoclima.com`
- `https://www.globoclima.com`
- `http://localhost:3000` (desenvolvimento)
- `http://localhost:5001` (desenvolvimento)

### **Headers Permitidos**
- `Authorization`
- `Content-Type`
- `X-Requested-With`

### **Métodos Permitidos**
- `GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`

---

## 🔧 SDKs e Bibliotecas

### **JavaScript/TypeScript**

```typescript
import { GloboClimaClient } from '@globoclima/sdk';

const client = new GloboClimaClient({
  baseURL: 'https://api.globoclima.com',
  apiKey: 'seu-token-jwt'
});

// Consultar clima
const weather = await client.weather.getByCity('São Paulo');

// Adicionar favorito
await client.weather.addFavorite({
  cityName: 'Rio de Janeiro',
  countryCode: 'BR',
  latitude: -22.9068,
  longitude: -43.1729
});
```

### **C#**

```csharp
using GloboClima.SDK;

var client = new GloboClimaClient("https://api.globoclima.com", "seu-token-jwt");

// Consultar clima
var weather = await client.Weather.GetByCityAsync("São Paulo");

// Adicionar favorito
await client.Weather.AddFavoriteAsync(new WeatherFavoriteRequest
{
    CityName = "Rio de Janeiro",
    CountryCode = "BR",
    Latitude = -22.9068,
    Longitude = -43.1729
});
```

---

## 🚨 Tratamento de Erros

### **Formato Padrão de Erro**

```json
{
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Email ou senha incorretos",
    "details": {
      "field": "email",
      "reason": "Usuário não encontrado"
    },
    "timestamp": "2024-01-15T12:00:00Z",
    "path": "/api/auth/login"
  }
}
```

### **Códigos de Erro Comuns**

| Código | Descrição |
|--------|-----------|
| `INVALID_CREDENTIALS` | Credenciais inválidas |
| `TOKEN_EXPIRED` | Token JWT expirado |
| `CITY_NOT_FOUND` | Cidade não encontrada |
| `COUNTRY_NOT_FOUND` | País não encontrado |
| `FAVORITE_EXISTS` | Favorito já existe |
| `FAVORITE_NOT_FOUND` | Favorito não encontrado |
| `RATE_LIMIT_EXCEEDED` | Limite de taxa excedido |
| `EXTERNAL_API_ERROR` | Erro na API externa |

---

## 📈 Monitoramento e Métricas

### **Métricas Disponíveis**
- **Request Count**: Número total de requisições
- **Response Time**: Tempo médio de resposta
- **Error Rate**: Taxa de erros por endpoint
- **Active Users**: Usuários ativos
- **API Calls**: Chamadas para APIs externas

### **Logs Estruturados**
```json
{
  "timestamp": "2024-01-15T12:00:00Z",
  "level": "INFO",
  "message": "Weather request completed",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "endpoint": "/api/weather/São Paulo",
  "responseTime": 245,
  "statusCode": 200
}
```