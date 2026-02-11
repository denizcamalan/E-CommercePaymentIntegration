# E-Commerce Payment Integration

E-ticaret platformu için ödeme entegrasyonu sağlayan backend projesi. Balance Management servisi ile entegre çalışarak ödeme işlemlerini yönetir.

## 🏗️ Proje Mimarisi

Bu proje **Clean Architecture** ve **Domain-Driven Design (DDD)** prensiplerine uygun olarak geliştirilmiştir. Katmanlı mimari yapısı sayesinde bağımlılıklar tek yönlüdür ve iş mantığı altyapı detaylarından bağımsızdır.

### Katmanlar

```
├── ECommercePayment.API          # Sunum Katmanı (Presentation Layer)
├── ECommercePayment.Application  # Uygulama Katmanı (Application Layer)
├── ECommercePayment.Domain       # Domain Katmanı (Domain Layer)
├── ECommercePayment.Infrastructure # Altyapı Katmanı (Infrastructure Layer)
├── ECommercePayment.Integrations # Dış Servis Entegrasyonları
└── EcommercePayment.Scheduler    # Zamanlanmış İşler (Background Jobs)
```

#### 1. **ECommercePayment.Domain** (Domain Layer)
- İş mantığının kalbi, hiçbir dış bağımlılığı yok
- **Entities**: Veritabanı varlıkları (Orders, OrderedProducts, ECommercePaymentIdentity)
- **DTOs**: Veri transfer nesneleri (Request/Response modelleri)
- **Enums**: İş mantığı enumları (OrderStatus, Currency, Category, UserRoles)
- **AppSettings**: Yapılandırma modelleri

#### 2. **ECommercePayment.Application** (Application Layer)
- İş akışlarını ve kullanım senaryolarını (use cases) içerir
- **Services**: İş mantığı servisleri (OrderService, ProductService, IdentityService)
- **Middleware**: HTTP pipeline middleware'leri (Exception, Logging)
- **Validation**: FluentValidation ile veri doğrulama
- **Mappings**: DTO dönüşüm extension'ları
- **Custom Attributes**: ECommerceIdentityAttribute (JWT token bazlı kimlik doğrulama)

#### 3. **ECommercePayment.Infrastructure** (Infrastructure Layer)
- Teknik altyapı ve veri erişim katmanı
- **EF Core DbContext**: PostgreSQL veritabanı erişimi
- **Migrations**: Veritabanı şema yönetimi
- **UoW (Unit of Work)**: Transaction yönetimi
- **Cache**: Redis cache implementasyonu
- **Configurations**: Entity Framework yapılandırmaları

#### 4. **ECommercePayment.Integrations** (External Integrations)
- Dış servislere bağlantılar
- **BalanceManagementService**: Ödeme servisi entegrasyonu
- **Polly**: Retry politikaları ile dayanıklılık (resilience)

#### 5. **ECommercePayment.API** (Presentation Layer)
- REST API endpoint'leri
- **Controllers**: HTTP endpoint'leri (Orders, Products, Identity)
- **Program.cs**: Dependency Injection ve middleware yapılandırması
- **Swagger**: API dokümantasyonu

#### 6. **EcommercePayment.Scheduler** (Background Jobs)
- Zamanlanmış arkaplan görevleri
- **Worker Services**: UserSyncWorker (kullanıcı senkronizasyonu)

## 🛠️ Kullanılan Teknolojiler

### Core Framework
- **.NET 8.0**: En son LTS versiyonu
- **C# 12**: Modern C# özellikleri ile

### Veritabanı ve ORM
- **PostgreSQL**: Ana veritabanı
- **Entity Framework Core 8.0**: ORM ve migration yönetimi
- **Npgsql**: PostgreSQL provider
- **Redis**: Caching (StackExchange.Redis)

### Güvenlik ve Kimlik Doğrulama
- **JWT (JSON Web Token)**: Token bazlı kimlik doğrulama
- **Microsoft.AspNetCore.Authentication.JwtBearer**: JWT middleware
- **Custom Identity Service**: Özel kimlik doğrulama servisi

### Validation ve Mapping
- **FluentValidation**: Veri doğrulama (CreateOrderRequestValidator, CompleteOrderRequestValidator)
- **Extension Methods**: DTO mapping için özel extension'lar

### Logging
- **Serilog**: Yapılandırılabilir loglama
- **Serilog.Sinks.File**: Dosyaya log yazma
- **Custom Logging Middleware**: HTTP request/response loglama

### API Dokümantasyonu
- **Swagger/OpenAPI**: API dokümantasyonu ve test arayüzü
- **Bearer Authentication Support**: Swagger'da JWT token desteği

### Resilience (Dayanıklılık)
- **Polly**: Retry, circuit breaker politikaları
- **Connection Retry**: PostgreSQL bağlantı yeniden deneme mekanizması

### Dependency Injection
- **Microsoft.Extensions.DependencyInjection**: Built-in DI container
- **Scoped Services**: Request başına yaşam döngüsü
- **Singleton Services**: Uygulama genelinde tek instance

### Background Services
- **Worker Services**: Zamanlanmış arkaplan görevleri
- **Hosted Services**: .NET Generic Host ile

## 📋 Tasarım Desenleri ve Prensipler

### Design Patterns
- **Repository Pattern**: Veri erişim soyutlaması
- **Unit of Work (UoW)**: Transaction yönetimi
- **Dependency Injection**: Gevşek bağlılık (loose coupling)
- **Service Layer Pattern**: İş mantığı katmanı
- **Middleware Pattern**: HTTP pipeline'ı için

### SOLID Principles
- **Single Responsibility**: Her sınıf tek bir sorumluluğa sahip
- **Dependency Inversion**: Interface'lere bağımlılık
- **Interface Segregation**: Ayrıştırılmış interface'ler

### Best Practices
- **Async/Await**: Asenkron programlama
- **Exception Handling Middleware**: Merkezi hata yönetimi
- **Custom Error Responses**: Standart hata formatı
- **Model Validation**: FluentValidation ile
- **Configuration Management**: appsettings.json yapılandırması
- **Health Checks**: Servis sağlık kontrolü (`/health`)
- **Connection String Management**: Environment bazlı yapılandırma

## 🔧 Proje Yapısı Özellikleri

### Global Features
- **Nullable Reference Types**: Null safety
- **Implicit Usings**: Otomatik using direktifleri
- **Custom Base Response**: Standart yanıt formatı
- **Custom Error Handling**: BaseErrorResponse ile tutarlı hata mesajları

### API Features
- **JWT Authentication**: Token bazlı güvenlik
- **Custom Identity Attribute**: Controller bazında kimlik bilgisi çekme
- **Model State Validation**: Otomatik validasyon ve özel hata yanıtları
- **JSON Serialization Options**: Null değerleri ignore etme

### Database Features
- **Code-First Migrations**: Entity Framework migrations
- **Schema Support**: Özel schema tanımı (OrderTable)
- **Connection Resiliency**: Otomatik yeniden bağlanma (5 retry)
- **Migration History Table**: Custom migration tablosu

### Caching Strategy
- **Redis Cache**: Dağıtık cache mekanizması
- **ICacheService Interface**: Cache soyutlaması
- **CacheKeys Constants**: Merkezi cache key yönetimi

## 🚀 Başlangıç

### Gereksinimler
- .NET 8.0 SDK
- PostgreSQL
- Redis

### Kurulum

```bash
# Bağımlılıkları yükle
dotnet restore

# Veritabanı migration'larını uygula
dotnet ef database update --project ECommercePayment.Infrastructure --startup-project ECommercePayment.API

# API'yi çalıştır
dotnet run --project ECommercePayment.API

# Scheduler'ı çalıştır (opsiyonel)
dotnet run --project EcommercePayment.Scheduler
```

### Yapılandırma

`appsettings.json` dosyasında aşağıdaki ayarları yapılandırın:
- **ConnectionStrings**: PostgreSQL ve Redis bağlantı stringleri
- **JwtSettings**: JWT token ayarları (Secret, Issuer, Audience)
- **BalanceManagement**: Dış ödeme servisi ayarları
- **Serilog**: Log yapılandırması

### API Dokümantasyonu

API çalıştığında Swagger UI'a erişim:
```
https://localhost:5001/swagger
```

## 📝 Lisans

Bu proje [LICENSE](LICENSE) dosyasında belirtilen lisans altında lisanslanmıştır.
