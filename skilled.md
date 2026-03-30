# Baseqat - Commands Reference

## Backend (.NET API)

### Build & Run
```bash
# Kill running API (required before build due to DLL lock)
taskkill //F //IM "Baseqt.API.exe"

# Build project
cd "d:/ND/app/Baseqt" && dotnet build --no-restore

# Run API (foreground)
cd "d:/ND/app/Baseqt/Baseqt.API" && dotnet run

# Run API (background)
cd "d:/ND/app/Baseqt/Baseqt.API" && nohup dotnet run > /dev/null 2>&1 &
```

### EF Migrations
```bash
cd "d:/ND/app/Baseqt"

# Add migration
dotnet ef migrations add MigrationName --project Baseqat.EF --startup-project Baseqt.API

# Update database
dotnet ef database update --project Baseqat.EF --startup-project Baseqt.API

# Remove last migration
dotnet ef migrations remove --project Baseqat.EF --startup-project Baseqt.API
```

### Test API Endpoints
```bash
# Course Stats (public)
curl -s http://localhost:5139/api/Course/GetCourseStats

# Course Categories (public)
curl -s http://localhost:5139/api/CourseCategory/GetAllHome

# Active Courses (public)
curl -s http://localhost:5139/api/Course/GetActive

# Active Instructors (public)
curl -s http://localhost:5139/api/Instructor/GetActive

# Home Statistics (public)
curl -s http://localhost:5139/api/HomeStatistic/GetAll

# Public Calendar (public)
curl -s "http://localhost:5139/api/ConsultationRequest/GetPublicCalendar?startDate=2026-01-01&endDate=2026-12-31"

# Media Center (public)
curl -s http://localhost:5139/api/MediaCenter/GetAllHome

# Consultants (public)
curl -s http://localhost:5139/api/Consultant/GetActive

# Consultation Categories (public)
curl -s http://localhost:5139/api/ConsultationCategory/GetAllHome

# Auth endpoints (POST)
curl -s -X POST http://localhost:5139/api/Auth/Login -H "Content-Type: application/json" -d '{"email":"admin@test.com","password":"123456"}'
```

## Frontend (Next.js)

### Dev Server
```bash
cd "d:/2026/baseqat/app/Basqat-main"

# Start dev server (port 3001)
npm run dev

# Start in background
nohup npm run dev > /dev/null 2>&1 &

# Build for production
npm run build

# Install dependencies
npm install
```

### Test Frontend Pages
```bash
# Check if running
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001

# Public pages
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001/courses-categories
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001/courses-archive
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001/consultation-calendar
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001/consultation-request
```

## Git
```bash
cd "d:/ND/app/Baseqt"

git status
git add -A
git commit -m "message"
git log --oneline -10
git diff
git diff --cached
```

## Start Both Projects
```bash
# Kill & restart API
taskkill //F //IM "Baseqt.API.exe" 2>/dev/null
cd "d:/ND/app/Baseqt/Baseqt.API" && nohup dotnet run > /dev/null 2>&1 &

# Start frontend
cd "d:/2026/baseqat/app/Basqat-main" && nohup npm run dev > /dev/null 2>&1 &

# Verify both running
sleep 10
curl -s http://localhost:5139/api/Course/GetCourseStats
curl -s -o /dev/null -w "%{http_code}" http://localhost:3001
```

## Project Paths
| Component | Path |
|-----------|------|
| Backend Root | `d:\ND\app\Baseqt\` |
| API Project | `d:\ND\app\Baseqt\Baseqt.API\` |
| Controllers | `d:\ND\app\Baseqt\Baseqt.API\Controllers\` |
| DTOs | `d:\ND\app\Baseqt\Baseqat.CORE\DTOs\` |
| Models | `d:\ND\app\Baseqt\Baseqat.EF\Models\` |
| Frontend Root | `d:\2026\baseqat\app\Basqat-main\` |
| Services | `d:\2026\baseqat\app\Basqat-main\services\` |
| Admin Pages | `d:\2026\baseqat\app\Basqat-main\app\(dashboard)\` |
| Public Pages | `d:\2026\baseqat\app\Basqat-main\app\(site)\` |
| Components | `d:\2026\baseqat\app\Basqat-main\components\` |

## Recent Updates (March 2026)

### Complete Logout System Implementation

#### Backend Changes
```bash
# Files Modified
- Baseqat.CORE/Services/IAuthServices.cs
- Baseqat.CORE/Services/AuthServices.cs
- Baseqt.API/Controllers/AccountController.cs

# New Endpoint
POST /api/Account/logout
Authorization: Bearer {token}
```

**Implementation Details:**
```csharp
// IAuthServices.cs
Task<ApiBaseResponse<bool>> Logout(string token);

// AuthServices.cs
public async Task<ApiBaseResponse<bool>> Logout(string token)
{
    await _signInManager.SignOutAsync();
    // Future: Add token to blacklist
    return ApiBaseResponse<bool>.Success(true, "تم تسجيل الخروج بنجاح");
}

// AccountController.cs
[HttpPost("logout")]
public async Task<ActionResult<ApiBaseResponse<bool>>> Logout()
{
    var authHeader = Request.Headers["Authorization"].ToString();
    var token = authHeader.Replace("Bearer ", "");
    var result = await _authServices.Logout(token);
    return Ok(result);
}
```

#### Frontend Changes
```bash
# Files Modified
- services/auth/auth.service.ts
- components/site/header.tsx
- components/dashboard/header.tsx
- components/student-dashboard/mobile-bottom-nav.tsx
- components/client-dashboard/mobile-bottom-nav.tsx
- components/client-dashboard/sidebar.tsx
```

**Enhanced Logout Flow:**
```typescript
// auth.service.ts
async logout(): Promise<void> {
  try {
    // 1. Send logout request to backend
    const token = Cookies.get("auth_token");
    if (token) {
      await api.post("/auth/logout", {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
    }
  } catch (error) {
    console.error("Logout error:", error);
  } finally {
    // 2. Clear all storage
    Cookies.remove("auth_token", { path: "/" });
    Cookies.remove("auth_token");
    localStorage.removeItem("user");
    localStorage.removeItem("authToken");
    localStorage.removeItem("token");
    sessionStorage.clear();

    // 3. Force full page reload
    window.location.href = "/login";
  }
}
```

### Hero Section Redesign

#### Changes Made
```bash
# Files Modified
- components/site/hero.tsx
- public/site/slide1.png (added)
- public/site/basqat.ico (added)
- app/favicon.ico (updated)
```

**Design Improvements:**
- **Font Sizes:** Reduced from 7xl to 5xl (h1), 5xl to 3xl (h2), 2xl to lg (description)
- **Image Dimensions:** Fixed max-w-lg, h-300px with proper centering
- **Padding:** Added pt-[75px] to prevent header overlap
- **Pattern:** Replaced plus (+) pattern with subtle dots
- **Colors:** Unified emerald gradient across all slides
- **Removed:** A+ quality badge for cleaner design

### Authentication & Access Control

#### Route Changes
```bash
# Changed /dashboard to /index throughout the app
- Updated middleware.ts for role-based access
- Updated all navigation links and redirects
- Added support for roles: admin, BASEQATEMPLOYEE, SUPERADMIN
```

#### Header Improvements
```typescript
// When NOT logged in: Show login + register buttons
<Link href="/register">التسجيل</Link>
<Link href="/login">تسجيل الدخول</Link>

// When logged in: Show user icon dropdown
<button onClick={userMenu}>
  <FontAwesomeIcon icon={faUser} />
</button>
```

#### Debug Tools
```bash
# New debug page for JWT troubleshooting
/debug-token

# Displays:
- Token payload
- All token keys
- Role information
- Allowed admin roles
```

### Testing Logout Flow

```bash
# 1. Login
curl -X POST http://localhost:5139/api/Account/LoginByEmail \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'

# 2. Get token from response
TOKEN="eyJhbGc..."

# 3. Test logout
curl -X POST http://localhost:5139/api/Account/logout \
  -H "Authorization: Bearer $TOKEN"

# Expected Response:
{
  "succeeded": true,
  "message": "تم تسجيل الخروج بنجاح",
  "data": true
}
```

### Git Commits

#### Frontend
```bash
cd "d:\2026\baseqat\app\Basqat-main"
git add .
git commit -m "feat: Enhance hero section, implement complete logout system, and improve auth flow"
git push
```

#### Backend
```bash
cd "D:\ND\app\Baseqt"
git add Baseqat.CORE/Services/ Baseqt.API/Controllers/AccountController.cs
git commit -m "feat: Add logout endpoint with token revocation support"
git push
```

### Token Blacklist Implementation (March 30, 2026)

**Database Migration:**
```bash
cd "D:\ND\app\Baseqt"

# Kill API process before migration (if running)
taskkill //F //IM "Baseqt.API.exe"

# Create migration
dotnet ef migrations add AddRevokedTokensTable --project Baseqat.EF --startup-project Baseqt.API

# Apply to database
dotnet ef database update --project Baseqat.EF --startup-project Baseqt.API
```

**Complete Implementation:**
```csharp
// RevokedToken model (Baseqat.EF/Models/Auth/RevokedToken.cs)
public class RevokedToken
{
    public int Id { get; set; }
    [Required] [MaxLength(2000)] public string Token { get; set; }
    [Required] [MaxLength(450)] public string UserId { get; set; }
    [Required] public DateTime RevokedAt { get; set; }
    [Required] public DateTime ExpiresAt { get; set; }
    [MaxLength(50)] public string? RevokedFrom { get; set; }
    public virtual ApplicationUser User { get; set; }
}

// ITokenService methods
Task<bool> RevokeTokenAsync(string token, string userId, string? revokedFrom = null);
Task<bool> IsTokenRevokedAsync(string token);
Task<int> CleanupExpiredTokensAsync();

// JwtBlacklistMiddleware checks all requests
// Returns 401 if token is in blacklist
```

### Key Files Reference

| Feature | Frontend File | Backend File |
|---------|--------------|--------------|
| Logout Service | `services/auth/auth.service.ts` | `Baseqat.CORE/Services/AuthServices.cs` |
| Logout Interface | - | `Baseqat.CORE/Services/IAuthServices.cs` |
| Logout Endpoint | - | `Baseqt.API/Controllers/AccountController.cs` |
| Token Blacklist Model | - | `Baseqat.EF/Models/Auth/RevokedToken.cs` |
| Token Service | - | `Baseqat.CORE/Services/TokenService.cs` |
| Blacklist Middleware | - | `Baseqt.API/Middleware/JwtBlacklistMiddleware.cs` |
| Hero Design | `components/site/hero.tsx` | - |
| Site Header | `components/site/header.tsx` | - |
| Dashboard Header | `components/dashboard/header.tsx` | - |
| Middleware | `middleware.ts` | - |
| Debug Page | `app/debug-token/page.tsx` | - |
