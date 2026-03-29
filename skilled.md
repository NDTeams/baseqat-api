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
