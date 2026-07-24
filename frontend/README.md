# SmartAttendance Frontend

A modern, high-performance frontend for the SmartAttendance ecosystem. Built meticulously using React, Vite, TanStack Query, and Shadcn UI, this application follows a strict **Feature-Based Clean Architecture**.

## 🚀 Features Overview

- **Authentication & Security:** JWT based auth, role-based access control, secure routing, and global error boundaries.
- **Dashboard:** Interactive overview with real-time metric cards.
- **Organization Management:** Full CRUD operations for Companies, Branches, Departments, and Employees.
- **Attendance & Leaves:** Advanced policy configuration, leave requests, attendance anomaly tracking, and manual logging capabilities.
- **Identity & Access:** Secure management of Users, Roles, and granular Permissions.
- **Biometric Device Integration:** Live device monitoring, heartbeat checks, and sync configuration.
- **Reports & Analytics:** Dashboard generation, late employee tracking, and comprehensive statistical overviews.
- **System Traceability:** A comprehensive Notification system with a persistent unread counter and an Audit Logs module to track all administrative actions.
- **Global Settings:** Centralized configuration UI for modifying attendance rules, security thresholds, email/SMS endpoints, and database backup routines.

## 📁 Architecture Overview

The codebase strictly adheres to a domain-driven, feature-first structure. All code lives inside modular `src/features/*` directories:

```
src/
├── features/
│   ├── [feature-name]/        # Domain-driven feature directory
│   │   ├── api/               # API clients and endpoints (mocked where necessary)
│   │   ├── components/        # Feature-specific React components
│   │   ├── hooks/             # Feature-specific custom hooks (TanStack Query)
│   │   ├── pages/             # Route-level page components
│   │   ├── schemas/           # Zod validation schemas
│   │   ├── types/             # TypeScript DTOs and Interfaces (Strict)
│   │   └── utils/             # Helper functions specific to this domain
├── layouts/                   # Global page wrappers (Auth, MainLayout)
├── router/                    # Centralized React Router configuration (Code Split via Lazy/Suspense)
├── shared/                    # Reusable, cross-feature code
│   ├── components/ui/         # Generic design system components (Shadcn UI)
│   ├── utils/                 # Global utility functions (e.g. `cn`)
└── main.tsx                   # Entry point and global providers (QueryClient, Suspense)
```

## 🛠 Developer Guide

### Environment Configuration

Before running the application, ensure you have a valid `.env` file based on the provided `.env.example`:

```bash
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_NAME=SmartAttendance
VITE_ENVIRONMENT=development
```

### Installation & Setup

1. Install dependencies using npm (ensure Node.js >= 18):
   ```bash
   npm install
   ```
2. Start the development server (HMR enabled):
   ```bash
   npm run dev
   ```

### Code Quality & TypeScript Rules

- **Strict TypeScript:** `any` is strictly prohibited. Use explicit DTO interfaces (`features/*/types/`).
- **Enums Prohibited:** Do not use `export enum`. Instead, use `export const Object = {} as const` coupled with a type union (`type Object = typeof Object[keyof typeof Object]`).
- **Zero Linting Warnings:** All features are expected to compile with zero warnings from ESLint or `tsc`.

### Building for Production

We use route-level code splitting (`React.lazy`) to ensure optimal chunk sizes and rapid Time-to-Interactive (TTI). 

Run the production build:
```bash
npm run build
```

The output will be placed in the `/dist` directory, ready to be served via Nginx, Vercel, or AWS S3.

## 🤝 Contribution Guidelines

1. **New Features:** Must be placed in a new directory under `src/features/`.
2. **Data Fetching:** Always use `@tanstack/react-query` through custom hooks in the feature's `hooks/` directory.
3. **Forms:** Must be built using `react-hook-form` paired with a `@hookform/resolvers/zod` schema from the `schemas/` directory.