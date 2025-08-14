# Social Media Backend API - Phase 5

A high-performance .NET 8 social media backend with enterprise-grade security and scalability.

## 🔐 Security-First Configuration

**NO STATIC PASSWORDS** - All secrets are managed through environment variables and secret managers.

### Required Environment Variables

```bash
# Database Configuration (REQUIRED)
export DB_HOST=localhost
export DB_NAME=socialapp
export DB_USERNAME=your_db_username
export DB_PASSWORD=your_secure_db_password

# Optional: Override defaults
export DB_PORT=5432
```

### For Development Setup

1. **Create a `.env` file** (never commit this):
```bash
DB_HOST=localhost
DB_NAME=socialapp_dev
DB_USERNAME=postgres
DB_PASSWORD=your_local_password
```

2. **Load environment variables**:
```bash
# Using dotenv (recommended)
dotenv -f .env -- dotnet run

# Or export manually
source .env
dotnet run
```

### For Production

Use your cloud provider's secret manager:

- **Azure**: Azure Key Vault
- **AWS**: AWS Secrets Manager  
- **GCP**: Google Secret Manager
- **Kubernetes**: Kubernetes Secrets

## 🚀 Performance Optimizations

### Database Performance Features

✅ **Connection Pooling**: 5-100 concurrent connections  
✅ **Retry Logic**: 3 retries with exponential backoff  
✅ **No Virtual Properties**: Explicit loading for better performance  
✅ **Strategic Indexes**: Optimized for social media query patterns  
✅ **No Change Tracking**: Read-heavy workload optimization  

### Entity Relationship Strategy

- **Removed virtual navigation properties** for better performance
- **Explicit loading** of related data when needed
- **Optimized for social media patterns** (timeline queries, user lookups)
- **Minimal N+1 query problems**

## 📊 Database Schema

### Core Entities
- **User**: Profile management with privacy controls
- **Post**: Multi-media content (text, image, video, audio)
- **Follow**: Follower/following relationships
- **Like/Comment/Share**: Engagement tracking
- **Message**: Direct messaging system
- **MediaAttachment**: File handling with metadata

### Performance Features
- **Unique constraints** prevent duplicate engagements
- **Composite indexes** for timeline queries
- **Foreign key relationships** with proper cascade rules
- **Optimized for read-heavy workloads**

## 🛠 Development Commands

```bash
# Build the solution
dotnet build

# Run migrations (with env vars set)
dotnet ef migrations add MigrationName --project WebApp.Infrastructure --startup-project WebApp.Api

# Update database
dotnet ef database update --project WebApp.Infrastructure --startup-project WebApp.Api

# Run the API
dotnet run --project WebApp.Api
```

## 🔄 Migration Guide

Since we removed virtual navigation properties for performance:

```csharp
// ❌ OLD (Poor Performance)
var user = await context.Users
    .Include(u => u.Posts)
    .ThenInclude(p => p.Likes)
    .FirstAsync(u => u.Id == userId);

// ✅ NEW (High Performance)
var user = await context.Users
    .AsNoTracking()
    .FirstAsync(u => u.Id == userId);

var posts = await context.Posts
    .AsNoTracking()
    .Where(p => p.AuthorId == userId)
    .ToListAsync();

var likeCounts = await context.Likes
    .AsNoTracking()
    .Where(l => postIds.Contains(l.PostId))
    .GroupBy(l => l.PostId)
    .ToDictionaryAsync(g => g.Key, g => g.Count());
```

## 🌐 Multi-Cloud Support

The application is designed to work with:
- **PostgreSQL** on any cloud provider
- **Connection string** built from environment variables
- **Secret managers** integrated for production workloads
- **Container-ready** for Kubernetes deployment

## 📈 Next Phase: API Services

- Repository Pattern implementation
- Service layer with business logic  
- REST API controllers
- JWT authentication
- Unit tests with TDD approach

---

**Security Note**: Never commit database passwords or connection strings. Always use environment variables or secret management services.
