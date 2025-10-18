# 🔗 UrlShortener

**UrlShortener** — a training project built with **.NET 8 + Angular 20**, demonstrating a complete URL shortening service with basic authentication (admin/user).

---

## 🏗️ Technologies

### Backend
- ASP.NET Core 8 (Web API + Razor Pages)
- Entity Framework Core (InMemory Database)
- Microsoft.Extensions.Logging
- Swagger / OpenAPI
- Basic Authentication with cookies
- CORS support for Angular
- Razor Page `About` with Markdown file support

### Frontend
- Angular 20 (Standalone Components)
- TailwindCSS (dark theme)
- RxJS + BehaviorSubject
- HTTP Interceptors + AuthService
- SPA structure with login, URL table, and API integration

---

## 🚀 Project Setup

### 1️⃣ Run backend
```bash
cd UrlShortener.WebApi
dotnet run
```
> 🔸 Default API runs on:  
> `https://localhost:7025`  
> Swagger UI available at:  
> 👉 https://localhost:7025/swagger

### 2️⃣ Run frontend
```bash
cd UrlShortener.WebClient
npm install
ng serve
```
> 🔹 Angular client runs on:  
> http://localhost:4200  
> (automatically connected to backend on https://localhost:7025)

---

## 🔐 Test Users

| Role | Username | Password |
|------|-----------|-----------|
| 🛠️ Admin | `admin` | `admin` |
| 👤 User | `user` | `user` |

---

## 📡 API Endpoints

### 🔑 Auth (`/api/auth`)
| Method | Route | Description |
|--------|--------|-------------|
| `POST` | `/api/auth/login` | Log in the user and return `auth_basic` cookie |
| `POST` | `/api/auth/logout` | Log out and clear cookie |
| `GET` | `/api/auth/me` | Get current user info |
| `GET` | `/api/auth/whoami` | Debug endpoint (optional) |

**Example request:**
```json
POST /api/auth/login
{
  "userName": "admin",
  "password": "admin"
}
```

---

### 🔗 URLs (`/api/urls`)
| Method | Route | Description |
|--------|--------|-------------|
| `GET` | `/api/urls` | Get all shortened URLs |
| `GET` | `/api/urls/{id}` | Get URL by ID |
| `POST` | `/api/urls` | Create a new short URL |
| `DELETE` | `/api/urls/{id}` | Delete a URL (admin or owner only) |

**Example create request:**
```json
POST /api/urls
{
  "originalUrl": "https://youtube.com/..."
}
```

**Example response:**
```json
{
  "id": 1,
  "originalUrl": "https://youtube.com/...",
  "shortCode": "T2bXlkf7",
  "shortUrl": "https://localhost:7025/T2bXlkf7",
  "createdBy": "admin",
  "createdDate": "2025-10-18T22:22:34Z"
}
```

---

### 🚦 Redirect
| Method | Route | Description |
|--------|--------|-------------|
| `GET` | `/{shortCode}` | Redirect to the original URL |

**Example:**  
🔗 `https://localhost:7025/T2bXlkf7` → 🔁 redirects to `https://youtube.com/...`

---

### 📄 Razor Page `/about`
- Displays content from the Markdown file `AboutContent.md`
- Accessible at: 👉 https://localhost:7025/About  
- If the logged-in user is Admin, the content can be edited directly

---

## 📁 Project Structure

```
UrlShortener/
 ├── UrlShortener.Domain/           # Entities
 ├── UrlShortener.Application/      # DTOs, services, interfaces
 ├── UrlShortener.Infrastructure/   # Repositories, EF context
 ├── UrlShortener.WebApi/           # API controllers, Razor Pages, Middleware
 ├── UrlShortener.WebClient/        # Angular SPA frontend
 └── UrlShortener.Tests/            # Unit tests (InMemoryDatabase)
```

---

## 🧠 Additional Notes
- CORS enabled for `http://localhost:4200`
- Cookie `auth_basic` is `HttpOnly`, `Secure`, `SameSite=None`
- Using EF InMemoryDatabase — data resets on restart

---

## 🧩 Swagger Authentication Example
1. Open Swagger UI: https://localhost:7025/swagger  
2. Click **Authorize**
3. Enter credentials:
   ```
   admin:admin
   ```
4. Now all `/api/urls` requests are authorized as Admin.

---

## ✨ Author
**Oleksandr** — .NET / Angular Developer  
