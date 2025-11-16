# Todozra

A full-stack todo application built with Vue.js and ASP.NET Core.

## Project Structure

```
Todozra/
├── Todozra.Api/          # ASP.NET Core backend API
│   ├── Features/         # Feature-based organization
│   │   └── Todo/         # Todo feature endpoints
│   ├── Program.cs        # Application entry point
│   └── wwwroot/          # Static files (serves frontend in production)
├── Todozra.Client/       # Vue.js frontend
│   ├── src/              # Vue source code
│   └── vite.config.ts    # Vite configuration
└── Todozra.Db/          # Database layer
    ├── Migrations/       # EF Core migrations
    └── Todo/             # Todo data models
```

## Prerequisites

### For Docker
- Docker
- Docker Compose

### For Local Development
- .NET 10.0 SDK
- Node.js (or Bun)
- SQLite

## Running the Application

### Option 1: Docker Compose (Recommended)

Build and run the application with Docker Compose:

```bash
docker compose up --build
```

The application will be available at `http://localhost:8080`

To stop:
```bash
docker compose down
```

### Option 2: Local Development

#### Backend (.NET API)

1. Navigate to the API directory:
```bash
cd Todozra.Api
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Run the API:
```bash
dotnet run
```

The API will start at:
- HTTP: `http://localhost:5202`
- HTTPS: `https://localhost:7123`

#### Frontend (Vite/Vue)

1. Navigate to the client directory:
```bash
cd Todozra.Client
```

2. Install dependencies:
```bash
npm install
# or
bun install
```

3. Run the development server:
```bash
npm run dev
# or
bun run dev
```

The frontend will be available at `http://localhost:5173`

## Technology Stack

- **Frontend**: Vue.js 3, TypeScript, TailwindCSS, TanStack Query
- **Backend**: ASP.NET Core 10.0, Minimal APIs
- **Database**: SQLite with Entity Framework Core
- **Build Tools**: Vite (frontend), Docker (containerization)

## Features

- Create, read, update, and delete todos
- Priority toggling
- Persistent storage with SQLite
- OpenTelemetry support for observability

## Testing

Api endpoints can be tested using the provided api.http file.

## Comments

- Will need to add users and authentication.
- Currently cannot support multiple instances of the API due to SQLite.
- Will need to add server side pagination and/or virtualization client side if performance becomes an issue for long lists.
- Playwright testing could be added to test frontend functionality.
- Will need to add tests for backend functionality once business logic becomes more complex.
- Feature set is limited, local first would improve UX, maybe with CRDTs and a sync server.
- Design could be more cohesive, actually set up real themes and colors without hardcoding colours. Get a real favicon.
- CDN for frontend assets could be added for better performance.