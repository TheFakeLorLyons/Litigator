# 📚 Litigator 🐊

Litigator is a mock legal case management system designed to demonstrate basic compentance with full stack C# devlopment. 
It demonstrates a full-stack web application using ASP.NET Core, Entity Framework Core, and an Angular frontend.

---

## 🧰 Tech Stack

- **Backend**: ASP.NET Core Web API (.NET 8)
- **ORM**: Entity Framework Core (code-first)
- **Database**: SQL Server
- **Testing**: xUnit, Bogus for seeding/mock data
- **Documentation**: Swagger (OpenAPI)
- **Frontend**: (WIP) Angular with TypeScript

---

## ⚖️ Features

- Manage clients, attorneys, courts, cases, deadlines, and documents
- RESTful API with full CRUD support
- Search functionality for clients and cases
- Deadline tracking with upcoming and overdue filters
- Swagger UI for API testing
- Seed data support using `Bogus`

---

## 🛠️ Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- (Optional) [Angular CLI](https://angular.io/cli)

### Backend Setup

1. **Clone the repo:**
   ```bash
   git clone https://github.com/yourname/litigator.git
   cd litigator
   ```

### 🧪 Running Tests
- `dotnet test`
- Tests live in /Tests, separated by service and controller concerns using xUnit.

### 📁 Project Structure
```
Litigator/
├── Controllers/         # API endpoints
├── Services/            # Business logic & interfaces
├── DataAccess/          # DbContext, Entities, Seeder
├── Models/              # DTOs and ECF message models
├── Tests/               # xUnit test projects
└── Program.cs           # Entry point and DI setup
```

### 📦 Future Enhancements
- Angular frontend with form management and UI validation
- Role-based authentication (JWT or Identity)
- Document upload and file storage support
- Audit logging

#### Lorelai Lyons 2025