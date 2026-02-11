# E-Commerce Payment API - Authentication Guide

Bu dokuman, E-Commerce Payment API'de Bearer Authentication sistemininin nasıl kullanılacağını açıklar.

## Genel Bakış

API, JWT (JSON Web Token) tabanlı Bearer Authentication kullanmaktadır. Korumalı endpointlere erişmek için önce bir access token almanız gerekmektedir.

## Yapılandırma

### 1. Database'e Identity Kaydı Ekleme

Öncelikle `ECommercePaymentIdentity` tablosuna client bilgilerinizi eklemeniz gerekiyor:

```sql
INSERT INTO "ECommercePaymentIdentity" 
("Id", "Client_ID", "Client_Secret", "Grant_Type", "Scope", "CreatedDate", "UpdatedDate", "Status")
VALUES 
(gen_random_uuid(), 'your_client_id', 'your_client_secret', 'client_credentials', 'api.read api.write', NOW(), NOW(), 1);
```

### 2. JWT Ayarları (appsettings.json)

JWT ayarları `appsettings.json` dosyasında yapılandırılmıştır:

```json
{
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyForJWTTokenGeneration2024!",
    "Issuer": "ECommercePaymentAPI",
    "Audience": "ECommercePaymentClients",
    "ExpirationMinutes": 60
  }
}
```

**ÖNEMLİ:** Production ortamında `Secret` değerini güvenli bir şekilde (örn: Azure Key Vault, AWS Secrets Manager) saklayın!

## Kullanım

### 1. Token Alma

Token almak için `/api/identity/token` endpointine POST isteği gönderin:

**Endpoint:** `POST /api/identity/token`

**Request Body:**
```json
{
  "client_ID": "your_client_id",
  "client_Secret": "your_client_secret",
  "grant_Type": "client_credentials",
  "scope": "api.read api.write"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "expiresAt": "2026-02-11T15:30:00Z",
  "scope": "api.read api.write"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "errorCode": "AuthenticationFailed",
  "message": "Invalid credentials"
}
```

### 2. Token ile API Çağrısı

Aldığınız token'ı HTTP Header'da gönderin:

**Header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Örnek cURL:**
```bash
curl -X GET "https://your-api.com/api/products" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

**Örnek C# HttpClient:**
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);

var response = await client.GetAsync("https://your-api.com/api/products");
```

### 3. Token Doğrulama (Test)

Token'ınızın geçerliliğini test etmek için:

**Endpoint:** `GET /api/identity/validate`

**Header:**
```
Authorization: Bearer your_token_here
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Token is valid",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "clientId": "your_client_id",
  "claims": [
    {
      "type": "nameid",
      "value": "123e4567-e89b-12d3-a456-426614174000"
    },
    {
      "type": "client_id",
      "value": "your_client_id"
    }
  ]
}
```

## Controller'lara Attribute Ekleme

### Class Seviyesinde (Tüm Methodlar Korumalı)

```csharp
using ECommercePayment.Application.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ECommerceIdentity] // ← Bu attribute tüm controller'ı korur
public class OrdersController : BaseController
{
    // Tüm methodlar authentication gerektirir
}
```

### Method Seviyesinde (Seçici Koruma)

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    // Bu method authentication GEREKTIRMEZ
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProducts()
    {
        // ...
    }

    // Bu method authentication GEREKTIRIR
    [HttpPost("create")]
    [ECommerceIdentity]
    public async Task<IActionResult> CreateProduct()
    {
        // ...
    }
}
```

### AllowAnonymous ile İstisna

Class seviyesinde attribute varken belirli methodları açmak için:

```csharp
[ApiController]
[Route("api/[controller]")]
[ECommerceIdentity] // Controller korumalı
public class OrdersController : BaseController
{
    [HttpGet("status")]
    [AllowAnonymous] // ← Bu method authentication GEREKTIRMEZ
    public IActionResult GetStatus()
    {
        return Ok("Service is running");
    }

    [HttpPost("create")] // ← Bu method authentication GEREKTIRIR
    public async Task<IActionResult> CreateOrder()
    {
        // ...
    }
}
```

## User Bilgilerine Erişim

Controller içinde authenticated user bilgilerine erişebilirsiniz:

```csharp
[ECommerceIdentity]
public class OrdersController : BaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder()
    {
        // User ID
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Client ID
        var clientId = User.FindFirst("client_id")?.Value;
        
        // Grant Type
        var grantType = User.FindFirst("grant_type")?.Value;
        
        // Scope
        var scope = User.FindFirst("scope")?.Value;
        
        // Tüm claims
        var allClaims = User.Claims.ToList();
        
        // ...
    }
}
```

## Hata Durumları

### 401 Unauthorized

Token eksik, geçersiz veya expired olduğunda döner:

```json
{
  "message": "Invalid or expired token"
}
```

**Çözüm:** Yeni token alın ve tekrar deneyin.

### 400 Bad Request

Login request'inde gerekli alanlar eksik olduğunda:

```json
{
  "errors": {
    "Client_ID": ["Client ID is required"],
    "Client_Secret": ["Client Secret is required"]
  }
}
```

## Güvenlik Notları

1. **Secret Key:** Production'da `JwtSettings.Secret` değerini environment variable veya secret manager ile yönetin
2. **HTTPS:** Production'da sadece HTTPS kullanın
3. **Token Storage:** Client tarafında token'ı güvenli şekilde saklayın (HTTP-only cookies, secure storage)
4. **Expiration:** Token expiration süresini ihtiyacınıza göre ayarlayın
5. **Rate Limiting:** `/api/identity/token` endpointine rate limiting ekleyin
6. **Logging:** Failed authentication attempt'leri loglayın

## Özet

1. Database'e client credentials ekleyin
2. `/api/identity/token` ile token alın
3. Her istekte `Authorization: Bearer {token}` header'ı gönderin
4. Controller'lara `[ECommerceIdentity]` attribute'u ekleyin
5. Token expire olduğunda yeni token alın

## Swagger Kullanımı

Swagger UI'da authentication için:

1. Swagger sayfasını açın: `https://your-api.com/swagger`
2. Sağ üstteki "Authorize" butonuna tıklayın
3. Token'ınızı girin: `Bearer your_token_here`
4. "Authorize" butonuna tıklayın
5. Artık korumalı endpointleri test edebilirsiniz

## Örnek Senaryo

```bash
# 1. Token al
curl -X POST "https://your-api.com/api/identity/token" \
  -H "Content-Type: application/json" \
  -d '{
    "client_ID": "my_client",
    "client_Secret": "my_secret",
    "grant_Type": "client_credentials",
    "scope": "api.read api.write"
  }'

# Response:
# {
#   "success": true,
#   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "expiresIn": 3600
# }

# 2. Token ile API çağrısı yap
curl -X GET "https://your-api.com/api/products" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# 3. Order oluştur
curl -X POST "https://your-api.com/api/orders/create" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "products": [...]
  }'
```

## Sorun Giderme

### Token alınamıyor
- Database'de client credentials'ın doğru olduğunu kontrol edin
- `Grant_Type` değerinin doğru olduğunu kontrol edin
- Logları kontrol edin

### 401 Unauthorized alıyorum
- Token'ın expire olmadığını kontrol edin
- Authorization header'ın doğru formatı: `Bearer {token}`
- JWT Secret değerinin appsettings'te doğru olduğunu kontrol edin

### Swagger'da token çalışmıyor
- Token'ı `Bearer ` prefix'i ile birlikte girin
- Token'ın expire olmadığını kontrol edin
