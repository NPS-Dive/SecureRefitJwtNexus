# 🛡️ SecureRefitJwtNexus

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-5C2D91?style=for-the-badge&logo=dotnet)
![Refit](https://img.shields.io/badge/Refit-API_Client-FF6F00?style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT-Secure-000000?style=for-the-badge&logo=JSON%20web%20tokens)
![Visual Studio 2026](https://img.shields.io/badge/Visual_Studio_2026-5C2D91?style=for-the-badge&logo=visual-studio)
![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)

**SecureRefitJwtNexus** is a highly structured .NET 10 ASP.NET Core solution demonstrating enterprise-grade microservice integration. Designed from a CTO and QA/QC Lead perspective, this repository emphasizes clean architecture, strict dependency management, and secure API-to-API communication.

## 🎯 Main Goal

The primary objective is to demonstrate how one API (`ListReader.Api`) can securely and efficiently consume data from another API (`ListMaker.Api`) using:

- **ASP.NET Core Web APIs** (Controller-based)
- **JWT Authentication** (Cross-service token validation)
- **Swagger/OpenAPI** (With Bearer token support)
- **`IHttpClientFactory` & Refit** (For typed, resilient HTTP clients)
- **Token Caching** (Downstream JWTs are cached until near expiration)
- **Monorepo Architecture** (Clean separation of concerns)
- **QA/QC Ready** (Unit-testable, dependency-injected structure)

---

## 🏗️ Architecture & Strict Constraints

This project adheres to strict architectural boundaries. **`ListReader.Api` must never reference `ListMaker.Api` directly.**
```text
[ListReader.Api] ---> [ListMaker.Client (Refit)] ---> [HTTP/Refit] ---> [ListMaker.Api]

```

All shared Request/Response DTOs reside in a separate, isolated contract library to ensure decoupling.

### 📂 Project Structure

```text
SecureRefitJwtNexus/
│
├── ApiIntegration.sln
├── README.md
│
└── src/
├── BuildingBlocks/
│   └── ListMaker.Contracts/ (Shared DTOs & Models)
│
├── Clients/
│   └── ListMaker.Client/ (Refit Interfaces & Token Handlers)
│
└── Services/
├── ListMaker/
│   └── ListMaker.Api/ (Data Provider - Port 7001)
│
└── ListReader/
└── ListReader.Api/ (Data Consumer - Port 7002)

```

---

## ⚙️ Services Overview

### 1. `ListMaker.Api` 🛠️
- **Base URL:** `https://localhost:7001`
- **Responsibilities:**
  - Authenticate machine-to-machine callers.
  - Issue JWT tokens.
  - Expose a stable, seeded list of $N = 50$ records (Name, Family, Age, Gender).
- **Endpoints:**
  - `POST /api/auth/login`
  - `GET /api/persons`

### 2. `ListReader.Api` 📖
- **Base URL:** `https://localhost:7002`
- **Responsibilities:**
  - Authenticate external/demo users.
  - Call `ListMaker.Api` via a strongly-typed **Refit** client.
  - Manage and cache JWT tokens requested from `ListMaker.Api` to minimize latency and authentication overhead.
  - Serve the aggregated data to the end-user.
- **Endpoints:**
  - `POST /api/auth/login`
  - `GET /api/lists/from-maker`

---

## 🚀 Development Roadmap

- [x] **Step 1:** Define strict architecture and rules.
- [x] **Step 2:** Repository initialization and documentation.
- [ ] **Step 3:** Shared Contracts (`ListMaker.Contracts`).
- [ ] **Step 4:** `ListMaker.Api` (Auth, JWT, Swagger, Seeded Data).
- [ ] **Step 5:** `ListMaker.Client` (Refit interfaces).
- [ ] **Step 6:** `ListReader.Api` (Auth, Refit Integration, Token Caching).
- [ ] **Step 7:** Unit Testing (xUnit, Moq).
- [ ] **Step 8:** C4 Modeling and Final Architecture Diagrams.

---

## ⚠️ QA/QC & Production Notes

*   **Static Configured Users:** This project uses static users for demonstration purposes. This allows for isolated integration testing and architectural proof-of-concept without requiring an external database.
*   **Production Readiness:** In a production environment, Identity management should be offloaded to robust providers such as **Microsoft Entra ID, Keycloak, Auth0, or Duende IdentityServer**.
*   **Security:** Never store real production passwords, connection strings, or JWT signing keys in source control. Always use Azure Key Vault, AWS Secrets Manager, or Environment Variables.

---
*Developed with a focus on Clean Code, SOLID principles, and enterprise quality assurance.*
