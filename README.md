# Full-Stack Authentication Template

A production-ready, secure authentication template with JWT access tokens and refresh tokens using httpOnly cookies. Perfect starting point for any web application requiring robust authentication.

## 🌟 Why This Template?

### Security First
- **JWT Access Tokens** stored in memory (never localStorage)
- **Refresh Tokens** stored in httpOnly cookies (immune to XSS attacks)
- **Automatic Token Refresh** on 401 errors with retry logic
- **CORS Protection** with configurable origins
- **Password Hashing** using BCrypt
- **HTTPS Ready** for both development and production

### Modern Tech Stack
- **Backend**: .NET 10 + Entity Framework Core + PostgreSQL
- **Architecture**: Vertical Slice Architecture with Rich Domain Model
- **Frontend**: React 18 + TypeScript + Vite + TailwindCSS
- **API**: RESTful Minimal APIs with CQRS pattern and MediatR

### Developer Experience
- Vertical Slice Architecture - each feature is self-contained
- Rich Domain Model with business logic encapsulation
- Type-safe API client with Axios interceptors
- Centralized token management
- Comprehensive error handling
- Ready-to-use login/dashboard UI
- Unit tests included

### Production Ready
- Environment-based configuration
- Database migrations
- Proper separation of concerns
- Scalable architecture
- HTTPS configuration

---

## 🏗️ Architecture Overview

### Backend - Vertical Slice Architecture with Rich Domain Model

This template uses **Vertical Slice Architecture** where each feature is a complete, self-contained vertical slice through all layers. Unlike traditional layered architecture, each slice contains everything it needs (command/query, handler, validation, endpoint).

**Why Vertical Slices?**
- Each feature is independent and cohesive
- Easy to add/remove features without affecting others
- No forced abstractions or shared layers
- Teams can work on different slices simultaneously
- Changes are localized to a single feature folder

**Rich Domain Model** - Business logic lives in domain entities, not scattered across services.

### Backend Structure
```
backend/Veterinary/src/VeterinaryApi/
│
├── Common/                          # Shared building blocks
│   ├── Abstractions/                # Core interfaces
│   │   ├── IApplicationDbContext.cs # Database contract
│   │   ├── ICurrentUser.cs          # User context
│   │   ├── IJwtProvider.cs          # JWT generation
│   │   └── IPasswordHasher.cs       # Password hashing
│   ├── CQRS/                        # CQRS interfaces
│   │   ├── ICommand.cs              # Command interface
│   │   ├── ICommandHandler.cs       # Command handler interface
│   │   ├── IQuery.cs                # Query interface
│   │   └── IQueryHandler.cs         # Query handler interface
│   ├── Endpoints/                   # Endpoint registration
│   │   └── IEndpoint.cs             # Minimal API endpoint interface
│   ├── Errors/                      # Error handling
│   │   ├── Error.cs                 # Error model
│   │   └── ErrorType.cs             # Error type enum
│   ├── Results/                     # Result pattern
│   │   ├── GenericResult.cs         # Generic result wrapper
│   │   ├── Result.cs                # Result implementation
│   │   └── ResultExtension.cs       # Helper extensions
│   └── Util/                        # Utility classes
│
├── Domain/                          # Rich Domain Models
│   ├── Common/                      # Shared domain primitives
│   └── Users/                       # User domain entities
│       └── User.cs                  # User aggregate root with business logic
│
├── Features/                        # Vertical Slices (each feature is independent)
│   └── Users/                       # User feature slice
│       ├── Login.cs                 # Login: Command + Handler + Endpoint
│       ├── RefreshToken.cs          # Refresh: Command + Handler + Endpoint
│       ├── Logout.cs                # Logout: Command + Handler + Endpoint
│       ├── ForgetPassword.cs        # Forget: Command + Handler + Endpoint
│       └── ResetPassword.cs         # Reset: Command + Handler + Endpoint
│       # Each file contains: Request, Handler, Validator, Endpoint
│       # Complete vertical slice - no jumping between folders
│
├── Infrastructure/                  # Cross-cutting concerns
│   ├── DependencyInjection.cs       # Service registration
│   ├── Auth/                        # Authentication implementation
│   │   └── JwtProvider.cs           # JWT token generation
│   ├── Interceptors/                # EF Core interceptors
│   ├── Persistence/                 # Database implementation
│   │   └── ApplicationDbContext.cs  # EF Core DbContext
│   └── Services/                    # Infrastructure services
│       └── PasswordHasher.cs        # BCrypt password hashing
│
├── Migrations/                      # EF Core migrations
│   ├── 20251231135144_InitDataBase.cs
│   ├── 20251231135144_InitDataBase.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
│
├── Properties/
│   └── launchSettings.json          # Launch configuration
├── appsettings.json                 # Production settings
├── appsettings.Development.json     # Development settings
├── Dockerfile                       # Container configuration
├── Program.cs                       # Application entry point
└── VeterinaryApi.csproj             # Project file
```

### How Vertical Slices Work

Each feature file (e.g., `Login.cs`) contains:
1. **Request DTO** (Command/Query)
2. **Request Handler** (business logic)
3. **Validators** (FluentValidation)
4. **Endpoint** (Minimal API endpoint)

Example structure in `Features/Users/Login.cs`:
```csharp
// Request
public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

// Response
public record LoginResponse(string Token, DateTime ExpiresAt);

// Handler (business logic)
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    // All login logic in one place
}

// Validator
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    // Validation rules
}

// Endpoint
public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (LoginCommand command, ISender sender) => 
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}
```

### Frontend Structure
```
frontend/
├── src/
│   ├── lib/
│   │   └── api/
│   │       ├── api.ts              # Axios instance with interceptors
│   │       ├── auth.ts             # Authentication API methods
│   │       └── tokenManager.ts     # Token storage & refresh logic
│   ├── assets/                     # Static assets
│   ├── App.tsx                     # Login & Dashboard UI
│   ├── App.css                     # Application styles
│   ├── index.css                   # Global styles (Tailwind)
│   └── main.tsx                    # Application entry point
│
├── public/                         # Public static files
├── .env                            # Environment variables
├── eslint.config.js                # ESLint configuration
├── index.html                      # HTML entry point
├── package.json                    # Dependencies
├── pnpm-lock.yaml                  # Lock file
├── tsconfig.json                   # TypeScript configuration
├── tsconfig.app.json               # App-specific TS config
├── tsconfig.node.json              # Node-specific TS config
└── vite.config.ts                  # Vite configuration
```

### Key Architectural Concepts

**1. Vertical Slice Architecture**
- Each feature folder is completely independent
- Adding a new feature? Just create a new file in `Features/`
- No need to touch multiple layers or folders
- Each slice registers its own endpoint

**2. Rich Domain Model**
- Domain entities in `Domain/` contain business logic
- Entities enforce invariants and rules
- No anemic domain model - behavior with data
- Example: `User` entity handles password validation, status changes

**3. CQRS Pattern**
- Commands change state (Login, Logout)
- Queries read state (if needed)
- Clear separation of concerns
- MediatR handles request pipeline

**4. Result Pattern**
- No exceptions for business logic failures
- Explicit success/failure handling
- Type-safe error handling
- Example: `Result<LoginResponse>` instead of throwing exceptions

**5. Minimal APIs**
- No controllers - just endpoints
- Each feature registers its own route
- Clean, focused endpoint definitions
- Built-in OpenAPI support

---

## 🚀 Quick Start

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and pnpm
- PostgreSQL 14+

### Backend Setup

1. **Navigate to Backend**
```bash
cd backend/Veterinary/src/VeterinaryApi
```

2. **Update Connection String**

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Database=your_db_name;Username=your_user;Password=your_password"
  }
}
```

3. **Run Migrations**
```bash
dotnet ef database update
```

4. **Configure JWT Settings**

In `appsettings.json`:
```json
{
  "JwtOptions": {
    "SecretKey": "your-super-secret-key-min-32-characters-long",
    "Issuer": "YourApp",
    "Audience": "YourApp",
    "ExpirationTimeInMinutes": 15
  }
}
```

5. **Run Backend**
```bash
dotnet run
```

Backend will start on `https://localhost:7256`

### Frontend Setup

1. **Navigate to Frontend**
```bash
cd frontend
```

2. **Install Dependencies**
```bash
pnpm install
```

3. **Configure API URL**

Create/edit `.env`:
```env
VITE_API_URL=https://localhost:7256/api/v1
```

4. **Run Frontend**
```bash
pnpm run dev
```

Frontend will start on `http://localhost:5173` (or `https://localhost:5173` if configured)

---

## 🔐 Authentication Flow

### Login Flow
1. User enters email and password
2. Backend validates credentials and hashes password
3. Backend generates JWT access token (15 min expiry)
4. Backend creates refresh token session (7 days expiry)
5. Access token sent in response body
6. Refresh token sent as httpOnly cookie
7. Frontend stores access token in memory only

### Automatic Token Refresh
1. API request receives 401 Unauthorized
2. Axios interceptor catches error
3. Automatically calls refresh endpoint
4. Refresh token sent via cookie (httpOnly)
5. New access token received and stored
6. Original request retried with new token
7. Seamless user experience - no re-login needed

### Security Features
- ✅ Access tokens never stored in localStorage (XSS protection)
- ✅ Refresh tokens in httpOnly cookies (XSS immune)
- ✅ Automatic token rotation
- ✅ Single concurrent refresh (prevents race conditions)
- ✅ Password hashing with BCrypt
- ✅ CORS protection

---

## 📝 How to Use This Template

### For Your Own Project

1. **Clone and Customize Branding**
   - The UI is already generic and ready to use
   - Update `appsettings.json` with your app name
   - Customize colors in `App.tsx` (Tailwind classes)

2. **Add Your Domain Models (Rich Domain Model)**
   - Add entities to `Domain/` folder
   - Put business logic in domain entities, not services
   - Example: User validation, status transitions, business rules
   ```csharp
   // Domain/Orders/Order.cs
   public class Order
   {
       public void Cancel()
       {
           if (Status == OrderStatus.Shipped)
               throw new InvalidOperationException("Cannot cancel shipped order");
           Status = OrderStatus.Cancelled;
       }
   }
   ```

3. **Add New Features (Vertical Slices)**
   - Create new feature file in `Features/` folder
   - Each file contains Command/Query, Handler, Validator, Endpoint
   - Example: `Features/Orders/CreateOrder.cs`
   ```csharp
   // All in one file - complete vertical slice
   public record CreateOrderCommand(...) : ICommand<OrderResponse>;
   public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, OrderResponse> { }
   public class CreateOrderValidator : AbstractValidator<CreateOrderCommand> { }
   public class CreateOrderEndpoint : IEndpoint { }
   ```
   - Feature automatically discovered and registered

4. **Extend Authentication**
   - Template includes: Login, Refresh, Logout, ForgetPassword, ResetPassword
   - Add role-based authorization as needed
   - Add user registration by creating `Features/Users/Register.cs`

5. **Configure Production**
   - Set proper CORS origins in `Program.cs`
   - Use production database
   - Configure HTTPS certificates
   - Set secure JWT secret keys
   - Update cookie SameSite settings for your domain setup

### Testing the Template

1. **Test Login**
   - Create a user in the database
   - Navigate to `http://localhost:5173`
   - Enter credentials and login

2. **Test Token Refresh**
   - Click "Test Refresh Token" button
   - Check browser console for new token
   - Token should refresh automatically on 401 errors

3. **Test Logout**
   - Click Logout button
   - Refresh token cookie should be cleared
   - User returned to login page

---

## ⚙️ Configuration

### Backend Configuration

**Cookie Settings (for cross-domain)**
```csharp
new CookieOptions
{
    HttpOnly = true,        // Prevents JavaScript access
    Expires = expiryDate,   // Cookie expiration
    SameSite = SameSiteMode.None,  // For cross-domain (dev)
    Secure = true           // HTTPS only
}
```

**CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Frontend Configuration

**Token Manager Settings**

Located in `frontend/src/lib/api/tokenManager.ts`:
- Prevents concurrent refresh requests
- Automatic retry with new token
- Memory-only storage

**API Base URL**

In `.env`:
```env
VITE_API_URL=https://localhost:7256/api/v1
```

**HTTPS for Development** (Optional)

In `vite.config.ts`:
```typescript
server: {
  https: true,  // Self-signed cert
  port: 5173
}
```

---

## 🔧 Available Endpoints

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/login` | Login with email/password |
| POST | `/auth/refresh` | Refresh access token |
| POST | `/auth/logout` | Logout and clear session |
| POST | `/auth/forget-password` | Request password reset |
| POST | `/auth/reset-password` | Reset password with token |

### Request/Response Examples

**Login Request**
```json
POST /api/v1/auth/login
{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

**Login Response**
```json
{
  "isSuccess": true,
  "value": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-01-01T12:30:00Z"
  }
}
```

**Refresh Request**
```json
POST /api/v1/auth/refresh
// No body needed - refresh token sent via httpOnly cookie
```

---

## 🧪 Testing

### Backend Tests

Located in `Tests/Application.Tests/`:

```bash
cd backend/Veterinary
dotnet test
```

Tests include:
- Login validation
- Token refresh logic
- Password reset flow
- Unit tests with mocked DbContext

### Frontend Manual Testing

1. **Token Refresh Test**
   - Login successfully
   - Wait for token to expire (or manually trigger)
   - Make API request
   - Check console for automatic refresh

2. **Logout Test**
   - Verify cookie cleared in browser DevTools
   - Verify access token cleared from memory

---

## 🚀 Deployment Considerations

### Production Checklist

**Backend**
- [ ] Use production database with proper backup
- [ ] Set strong JWT secret keys (32+ characters)
- [ ] Configure CORS with specific domain (no wildcards)
- [ ] Enable HTTPS with valid certificates
- [ ] Set `SameSite=Strict` or `Lax` if same domain
- [ ] Use environment variables for secrets
- [ ] Enable logging and monitoring
- [ ] Set proper token expiration times

**Frontend**
- [ ] Update `VITE_API_URL` to production API
- [ ] Build for production: `pnpm run build`
- [ ] Enable HTTPS
- [ ] Configure CDN if needed
- [ ] Set up proper error tracking

**Security**
- [ ] Never commit `.env` files
- [ ] Rotate JWT secrets regularly
- [ ] Implement rate limiting
- [ ] Add CAPTCHA for login if needed
- [ ] Monitor for suspicious activity
- [ ] Keep dependencies updated

---

## 📚 Technology Documentation

- [.NET 10](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [React](https://react.dev/)
- [Vite](https://vitejs.dev/)
- [TailwindCSS](https://tailwindcss.com/)
- [MediatR](https://github.com/jbogard/MediatR)

---

## 📄 License

This template is provided as-is for use in your projects. Customize as needed.

---

## 🤝 Contributing

This is a template repository. Fork it and make it your own! Feel free to adapt the architecture, add features, or modify the tech stack to fit your needs.

---

## 💡 Tips

1. **Start Small**: The template provides authentication - add features incrementally
2. **Keep Security Updated**: Regularly update dependencies for security patches
3. **Customize UI**: The frontend uses TailwindCSS - easy to customize colors and styling
4. **Add Features**: Follow the CQRS pattern in `Features/` folder for consistency
5. **Environment Variables**: Always use environment variables for sensitive data
6. **Testing**: Write tests as you add features - the template includes test examples

---

**Ready to build something amazing? Start customizing this template for your next project! 🚀**
