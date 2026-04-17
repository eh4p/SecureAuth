# SecureAuth.API — JWT, OAuth 2.0 & ASP.NET Identity Showcase

A backend-only ASP.NET Core Web API demonstrating JWT authentication, OAuth 2.0 (Google), and ASP.NET Core Identity working together in a single project.

---

## Concepts Demonstrated

### ASP.NET Core Identity
ASP.NET Core Identity is a membership system that handles user authentication, authorization, and management. In this project, Identity is used to:
- Store users and roles in an in-memory SQLite database
- Manage password hashing and verification
- Handle role-based authorization (`Admin`, `Manager`, `Employee`)
- Seed initial test users with predefined credentials

### JWT (JSON Web Tokens)
JWT is a compact, URL-safe means of representing claims between parties. This project demonstrates:
- Token generation with user ID, email, and roles as claims
- Token signing using a symmetric key (HMAC-SHA256)
- Token validation on protected endpoints
- Stateless authentication (no server-side session required)

### OAuth 2.0 (Google External Login)
OAuth 2.0 is an authorization framework that enables third-party applications to obtain limited access to user accounts. This project implements:
- Google OAuth 2.0 integration (with fake credentials for demo)
- External login flow: redirect to provider → callback → JWT issuance
- Automatic user creation for new external logins

### How They Connect Together
```
┌─────────────────────────────────────────────────────────────────┐
│                      Authentication Flow                        │
└─────────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
    ┌──────────┐       ┌──────────┐       ┌──────────┐
    │  Local   │       │  OAuth   │       │  Token   │
    │  Login   │       │  2.0     │       │  Issue   │
    │          │       │ (Google) │       │  (JWT)   │
    └────┬─────┘       └────┬─────┘       └────┬─────┘
         │                  │                  │
         ▼                  ▼                  ▼
    ┌─────────────────────────────────────────────────┐
    │              ASP.NET Core Identity               │
    │  - Validate credentials                          │
    │  - Find/create user                              │
    │  - Retrieve roles                                │
    └─────────────────────┬───────────────────────────┘
                          │
                          ▼
    ┌─────────────────────────────────────────────────┐
    │              JWT Token Service                   │
    │  - Generate token with claims                    │
    │  - Sign with secret key                          │
    │  - Set expiration (60 min)                       │
    └─────────────────────┬───────────────────────────┘
                          │
                          ▼
    ┌─────────────────────────────────────────────────┐
    │              Protected API Endpoints             │
    │  - [Authorize] validates token                   │
    │  - [Authorize(Roles="...")] checks roles         │
    └─────────────────────────────────────────────────┘
```

---

## Architecture

```
SecureAuth/
├── SecureAuth.API/
│   ├── Controllers/
│   │   ├── AuthController.cs         # Login, Register, OAuth
│   │   ├── EmployeesController.cs    # CRUD with role auth
│   │   └── DepartmentsController.cs  # CRUD with role auth
│   ├── Data/
│   │   ├── AppDbContext.cs           # EF Core + Identity context
│   │   └── DataSeeder.cs             # Fake data seeding
│   ├── Models/
│   │   ├── Employee.cs               # ERP employee entity
│   │   ├── Department.cs             # ERP department entity
│   │   └── DTOs/
│   │       ├── LoginRequest.cs
│   │       ├── RegisterRequest.cs
│   │       └── AuthResponse.cs
│   ├── Services/
│   │   ├── ITokenService.cs          # JWT interface
│   │   └── TokenService.cs           # JWT implementation
│   ├── Program.cs                    # App configuration
│   └── appsettings.json
└── SecureAuth.sln
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK or later
- Any REST client (Swagger UI, Postman, curl)

### Run the API

```bash
# Clone the repository
git clone <repository-url>
cd SecureAuth

# Restore dependencies
dotnet restore

# Run the API
dotnet run --project SecureAuth.API
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

---

## API Reference

### Auth Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/login` | Public | Login with email/password, returns JWT |
| POST | `/api/auth/register` | Public | Register new user (default role: Employee) |
| GET | `/api/auth/me` | Authenticated | Returns current user info from JWT claims |
| GET | `/api/auth/external-login?provider=Google` | Public | Start Google OAuth flow |
| GET | `/api/auth/external-callback` | Public | OAuth callback, returns JWT |

### Employee Endpoints

| Method | Route | Role Required | Description |
|--------|-------|---------------|-------------|
| GET | `/api/employees` | Any authenticated | List all employees |
| GET | `/api/employees/{id}` | Any authenticated | Get employee by ID |
| POST | `/api/employees` | Admin | Create employee |
| PUT | `/api/employees/{id}` | Manager or Admin | Update employee |
| DELETE | `/api/employees/{id}` | Admin | Delete employee |

### Department Endpoints

| Method | Route | Role Required | Description |
|--------|-------|---------------|-------------|
| GET | `/api/departments` | Any authenticated | List all departments |
| POST | `/api/departments` | Admin | Create department |

---

## Testing with Swagger

### Step 1: Login to Get JWT Token

1. Open Swagger UI: `http://localhost:5000/swagger`
2. Expand `POST /api/auth/login`
3. Click **Try it out**
4. Enter credentials:
   ```json
   {
     "email": "admin@erp.local",
     "password": "Admin@123"
   }
   ```
5. Click **Execute**
6. Copy the `token` from the response

### Step 2: Authorize in Swagger

1. Click the **Authorize** button (lock icon) at the top right
2. Enter the token: `your_jwt_token_here`
3. Click **Authorize**
4. Click **Close**

### Step 3: Call Protected Endpoints

Now you can call any protected endpoint. For example:
- `GET /api/employees` - List employees
- `POST /api/employees` - Create employee (Admin only)
- `GET /api/auth/me` - View your user info

### Test Users (Pre-seeded)

| Email | Password | Role |
|-------|----------|------|
| `admin@erp.local` | `Admin@123` | Admin |
| `manager@erp.local` | `Manager@123` | Manager |
| `employee@erp.local` | `Employee@123` | Employee |

---

## What I Learned

### Authentication vs Authorization
Authentication verifies **who you are** (e.g., JWT proves your identity), while Authorization determines **what you can do** (e.g., roles like Admin, Manager define your permissions). JWT handles authentication, while `[Authorize(Roles="...")]` handles authorization.

### How Identity Manages Users Securely
ASP.NET Core Identity handles password hashing using PBKDF2 with HMAC-SHA256, stores user data securely, and provides `UserManager` and `SignInManager` for common operations. It abstracts away the complexity of secure credential handling.

### OAuth 2.0 Delegated Authentication
OAuth 2.0 allows delegating authentication to trusted external providers (like Google). The user authenticates with the provider, and we receive claims (email, name) that we can use to create or link a local account. This eliminates the need to handle passwords for external users.

### Stateless JWT vs Session-based Auth
JWT is stateless—the server doesn't store session data. All necessary information is in the token itself. This scales better for distributed systems but means tokens can't be easily revoked before expiration. Session-based auth stores state on the server, allowing immediate revocation but requiring server resources.

---

## Security Notes

⚠️ **This is a demonstration project with intentional mock configurations.**

### Fake Credentials (Do NOT use in production)

| Item | Value | Notes |
|------|-------|-------|
| JWT Secret Key | `SecureAuth_FakeSecretKey_32Chars!!` | Use 256-bit key from environment variable |
| Google Client ID | `fake-google-client-id...` | Register at Google Cloud Console |
| Google Client Secret | `fake-google-client-secret` | Store in secure vault |

### Before Production Deployment

1. **Replace all secrets** with values from secure sources (Azure Key Vault, AWS Secrets Manager, environment variables)
2. **Use HTTPS** - Redirect all HTTP traffic
3. **Configure CORS** - Restrict to trusted origins
4. **Add rate limiting** - Prevent brute force attacks
5. **Implement token refresh** - Short-lived access tokens with refresh tokens
6. **Add logging & monitoring** - Track authentication events
7. **Use a real database** - Replace SQLite in-memory with PostgreSQL/SQL Server
8. **Enable account lockout** - Configure lockout after failed attempts
9. **Add MFA** - Multi-factor authentication for sensitive roles