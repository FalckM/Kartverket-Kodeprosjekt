# Kartverket Kodeprosjekt

A web application for registering and managing aviation obstacles in Norway. Built with ASP.NET Core MVC, Entity Framework, and MariaDB.

**Third Semester Project**

---

## Table of Contents

- [About the Project](#about-the-project)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation Guide](#installation-guide)
  - [First Time Setup](#first-time-setup)
  - [Running the Application](#running-the-application)
- [User Guide](#user-guide)
  - [Test Accounts](#test-accounts)
  - [Pilot Features](#pilot-features)
  - [Registerfører Features](#registerfører-features)
  - [Admin Features](#admin-features)
- [Technical Documentation](#technical-documentation)
  - [Project Structure](#project-structure)
  - [Database Design](#database-design)
  - [Architecture Decisions](#architecture-decisions)
- [Technologies Used](#technologies-used)
- [Known Issues](#known-issues)
- [Future Plans](#future-plans)
- [Troubleshooting](#troubleshooting)
- [License](#license)
- [Credits](#credits)

---

## About the Project

This application allows pilots to register obstacles (masts, poles, antennas, etc.) that could be hazardous to aviation. The system includes a three-tier approval workflow:

1. **Pilots** register obstacles with location data
2. **Registerfører** (Registry Officers) review and approve/reject submissions
3. **Admins** manage users and view system statistics

Key features:
- Interactive map-based obstacle registration (Leaflet.js)
- Quick Register and Full Register workflows
- Role-based access control
- Approval/rejection workflow with audit trail
- Responsive design (works on mobile and desktop)

---

## Getting Started

This guide will help you set up and run the application on your computer, even if you have never written code before.

### Prerequisites

You need to install these programs on your computer:

#### 1. Docker Desktop (Required)
Docker runs the database in a container, so you don't have to install MariaDB manually.

**Installation:**
1. Go to [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Click "Download for Windows"
3. Run the installer
4. Restart your computer when prompted
5. Open Docker Desktop and wait for it to start (you'll see a green icon)

**System Requirements:**
- Windows 10 64-bit: Pro, Enterprise, or Education (Build 19041 or higher)
- OR Windows 11 64-bit
- WSL 2 enabled (installer will help with this)

#### 2. .NET 9.0 SDK (Required)
This is needed to run the web application.

**Installation:**
1. Go to [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Download ".NET 9.0 SDK" (not Runtime - we need the SDK!)
3. Run the installer
4. Open Command Prompt and type: `dotnet --version`
5. You should see something like `9.0.x`

#### 3. Git (Required)
To download the project from GitHub.

**Installation:**
1. Go to [https://git-scm.com/downloads](https://git-scm.com/downloads)
2. Download for Windows
3. Run installer (keep all default settings)

#### 4. Visual Studio Code (Optional but Recommended)
A code editor to view and edit files.

**Installation:**
1. Go to [https://code.visualstudio.com/](https://code.visualstudio.com/)
2. Download for Windows
3. Run installer

---

### Installation Guide

Follow these steps exactly:

#### Step 1: Download the Project

1. Open **Command Prompt** (Press `Windows + R`, type `cmd`, press Enter)

2. Navigate to where you want to save the project:
   ```cmd
   cd Documents
   ```

3. Download the project:
   ```cmd
   git clone https://github.com/FalckM/Kartverket-Kodeprosjekt.git
   ```

4. Go into the project folder:
   ```cmd
   cd Kartverket-Kodeprosjekt
   ```

#### Step 2: Start Docker

1. Open **Docker Desktop** from your Start menu
2. Wait until you see "Docker Desktop is running" (green icon)
3. Keep Docker Desktop open in the background

#### Step 3: Start the Database

In the same Command Prompt window (inside the project folder):

```cmd
docker-compose up -d
```

**What this does:** Starts a MariaDB database in a Docker container.

**Wait 15 seconds** for the database to start properly.

#### Step 4: Setup the Database

1. Go into the application folder:
   ```cmd
   cd FirstWebApplication
   ```

2. Apply database migrations (creates tables):
   ```cmd
   dotnet ef database update
   ```

   **If you get an error** saying `dotnet ef not found`:
   ```cmd
   dotnet tool install --global dotnet-ef
   dotnet ef database update
   ```

#### Step 5: Build the Application

```cmd
dotnet build
```

This compiles the application. Wait for it to complete.

---

### First Time Setup

The application automatically creates three test accounts when you run it for the first time:

| Role | Email | Password |
|------|-------|----------|
| Pilot | pilot@test.com | Pilot123 |
| Registerfører | registerforer@test.com | Register123 |
| Admin | admin@test.com | Admin123 |

These accounts are created by `UserSeederService` during startup.

---

### Running the Application

#### Start the Application

In Command Prompt (inside `FirstWebApplication` folder):

```cmd
dotnet run
```

**You should see:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7286
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5112
```

#### Open in Browser

1. Open your web browser
2. Go to: **https://localhost:7286** (or the HTTPS URL shown in your terminal)
3. If you see a security warning, click "Advanced" then "Proceed" (this is normal for localhost)

#### Log In

Use one of the test accounts:
- **Pilot:** pilot@test.com / Pilot123
- **Registerfører:** registerforer@test.com / Register123
- **Admin:** admin@test.com / Admin123

#### Stop the Application

To stop the server:
1. Go back to Command Prompt
2. Press `Ctrl + C`

To stop the database:
```cmd
cd ..
docker-compose down
```

---

## User Guide

### Test Accounts

The application comes with three pre-configured test accounts:

#### Pilot Account
- **Email:** pilot@test.com
- **Password:** Pilot123
- **Can:** Register obstacles, view own registrations, delete pending obstacles

#### Registerfører Account
- **Email:** registerforer@test.com
- **Password:** Register123
- **Can:** Review pending obstacles, approve/reject submissions, view all obstacles

#### Admin Account
- **Email:** admin@test.com
- **Password:** Admin123
- **Can:** Manage users, assign roles, view system statistics

---

### Pilot Features

After logging in as a pilot, you can:

#### 1. Register an Obstacle

Two registration methods are available:

**Quick Register:**
1. Click "Register Obstacle" in navbar
2. Select "Quick Register"
3. Click on map to mark obstacle location
4. Click "Save and Continue"
5. Later, complete the registration with name, height, and description

**Full Register:**
1. Click "Register Obstacle" in navbar
2. Select "Full Register"
3. Fill in all fields:
   - Name (e.g., "Radio Mast")
   - Height in meters (e.g., 50)
   - Description (e.g., "Red and white striped tower")
4. Click on map to mark location
5. Click "Submit"

#### 2. View Your Registrations

Click "My Registrations" to see:
- **Incomplete:** Quick registers waiting for completion
- **Pending:** Submitted obstacles awaiting approval
- **Approved:** Obstacles approved by Registerfører
- **Rejected:** Obstacles rejected (with reason)

#### 3. Complete Quick Registrations

1. Go to "My Registrations"
2. Find incomplete obstacles
3. Click "Complete Registration"
4. Fill in name, height, and description
5. Click "Submit"

#### 4. Delete Pending Obstacles

You can only delete obstacles that are **pending** (not yet approved/rejected):
1. Go to "My Registrations"
2. Click "Delete" on a pending obstacle
3. Confirm deletion

---

### Registerfører Features

After logging in as Registerfører, you can:

#### 1. View Dashboard

Shows statistics:
- Pending obstacles (awaiting review)
- Approved obstacles (total)
- Rejected obstacles (total)

#### 2. Review Pending Obstacles

1. Click "Pending" in navbar
2. See list of completed obstacle registrations
3. Click "Review" on any obstacle

**Note:** Incomplete quick registrations do NOT appear here. Only completed obstacles (with name, height, description) are shown.

#### 3. Approve an Obstacle

1. Review obstacle details and location
2. Scroll to "Approve Obstacle" section
3. Add optional comments
4. Click "Approve This Obstacle"

The pilot will see their obstacle marked as "Approved".

#### 4. Reject an Obstacle

1. Review obstacle details
2. Scroll to "Reject Obstacle" section
3. Enter rejection reason (required)
4. Click "Reject This Obstacle"
5. Confirm the action

The pilot will see their obstacle marked as "Rejected" with your reason.

#### 5. View Approved/Rejected Obstacles

- Click "Approved" to see all approved obstacles
- Click "Rejected" to see all rejected obstacles
- Click "View Details" to see obstacle information

---

### Admin Features

After logging in as Admin, you can:

#### 1. View Dashboard

Shows system-wide statistics:
- Total users
- Total obstacles
- Approved/Pending counts
- User role distribution

#### 2. Manage Users

1. Click "Users" in navbar
2. See all registered users with their roles
3. Click "Manage Roles" to modify a user

#### 3. Assign/Remove Roles

1. Select a user
2. See current roles
3. Click "Assign" to add a role
4. Click "Remove" to remove a role

Available roles:
- **Pilot:** Can register obstacles
- **Registerfører:** Can approve/reject obstacles
- **Admin:** Can manage users

#### 4. Delete Users

1. Go to "Manage Users"
2. Click "Delete" next to a user
3. Confirm deletion

**Note:** You cannot delete your own account.

#### 5. View Statistics

Click "Statistics" to see:
- Obstacle statistics (total, approved, pending, rejected)
- User statistics by role
- Recent activity

---

## Technical Documentation

### Project Structure

```
Kartverket-Kodeprosjekt/
├── FirstWebApplication/
│   ├── Controllers/              # MVC Controllers
│   │   ├── AccountController.cs  # Login, Register, Logout
│   │   ├── AdminController.cs    # User management, statistics
│   │   ├── HomeController.cs     # Landing page
│   │   ├── PilotController.cs    # Obstacle registration
│   │   └── RegisterforerController.cs  # Approval workflow
│   │
│   ├── Models/                   # View Models
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   └── ErrorViewModel.cs
│   │
│   ├── Entities/                 # Database Models
│   │   └── ObstacleData.cs       # Main obstacle entity
│   │
│   ├── Data/                     # Database Context
│   │   └── ApplicationDbContext.cs
│   │
│   ├── Services/                 # Business Logic
│   │   ├── RoleInitializerService.cs   # Creates roles on startup
│   │   ├── UserSeederService.cs        # Creates test users
│   │   └── UserRoleService.cs          # Role management helpers
│   │
│   ├── Views/                    # Razor Views (HTML)
│   │   ├── Account/              # Login/Register forms
│   │   ├── Admin/                # Admin dashboard and user management
│   │   ├── Home/                 # Landing page, privacy
│   │   ├── Pilot/                # Obstacle registration forms
│   │   ├── Registerforer/        # Approval/rejection views
│   │   └── Shared/               # Layout, navigation
│   │
│   ├── wwwroot/                  # Static files
│   │   ├── css/                  # Stylesheets (Tailwind CSS)
│   │   ├── js/                   # JavaScript
│   │   └── lib/                  # Third-party libraries
│   │       ├── leaflet/          # Map library
│   │       ├── leaflet-draw/     # Drawing tools
│   │       ├── jquery/
│   │       └── bootstrap/
│   │
│   ├── Migrations/               # Database migrations
│   ├── Properties/               # Launch settings
│   ├── Program.cs                # Application startup
│   ├── appsettings.json          # Configuration
│   └── FirstWebApplication.csproj
│
├── docker-compose.yml            # Docker configuration
├── README.md                     # This file
└── .gitignore
```

---

### Controllers Explained

#### AccountController.cs
**Purpose:** Handles user authentication

**Key Methods:**
- `Register()` - Creates new user accounts
- `Login()` - Authenticates users and redirects based on role
- `Logout()` - Signs out current user

**How it works:**
1. User submits login form
2. `SignInManager` validates credentials
3. If successful, gets user's roles
4. Redirects to appropriate dashboard:
   - Admin → AdminDashboard
   - Registerfører → RegisterforerDashboard
   - Pilot → RegisterType

#### PilotController.cs
**Purpose:** Obstacle registration and management

**Key Methods:**
- `RegisterType()` - Choose Quick or Full Register
- `QuickRegister()` - Save location first, complete later
- `FullRegister()` - Complete registration in one step
- `MyRegistrations()` - View all obstacles by current pilot
- `CompleteQuickRegister()` - Finish incomplete quick registers
- `DeleteRegistration()` - Delete pending obstacles

**Authorization:** `[Authorize(Roles = "Pilot")]`

**How Quick Register works:**
1. Pilot clicks map → saves geometry
2. Obstacle saved with NULL name/height/description
3. Appears in "Incomplete" list
4. Pilot completes later → becomes "Pending"

#### RegisterforerController.cs
**Purpose:** Review and approve/reject obstacles

**Key Methods:**
- `RegisterforerDashboard()` - Statistics overview
- `PendingObstacles()` - List all complete obstacles awaiting review
- `ReviewObstacle()` - Detailed view with approve/reject forms
- `ApproveObstacle()` - Mark obstacle as approved
- `RejectObstacle()` - Mark obstacle as rejected with reason
- `ApprovedObstacles()` - List all approved obstacles
- `RejectedObstacles()` - List all rejected obstacles
- `ViewObstacle()` - View details of any obstacle

**Authorization:** `[Authorize(Roles = "Registerfører")]`

**How approval works:**
1. Query filters: `IsApproved=false AND IsRejected=false AND has name/height/description`
2. Registerfører reviews obstacle
3. On approve: Sets `IsApproved=true`, saves who/when/comments
4. On reject: Sets `IsRejected=true`, saves who/when/reason

#### AdminController.cs
**Purpose:** User and system management

**Key Methods:**
- `AdminDashboard()` - System statistics
- `AdminUsers()` - List all users with roles
- `AdminManageUser()` - Manage a specific user's roles
- `AssignRole()` - Add role to user
- `RemoveRole()` - Remove role from user
- `DeleteUser()` - Delete user account
- `AdminStatistics()` - Detailed system metrics

**Authorization:** `[Authorize(Roles = "Admin")]`

---

### Models and Entities

#### ObstacleData.cs (Entity)
**Purpose:** Represents an obstacle in the database

**Properties:**
```csharp
// Basic Information
public int Id { get; set; }                    // Primary key
public string ObstacleName { get; set; }       // e.g., "Radio Tower"
public double ObstacleHeight { get; set; }     // In meters
public string ObstacleDescription { get; set; } // Details
public string ObstacleGeometry { get; set; }   // WKT format (POINT, LINESTRING, POLYGON)
public string? ObstacleType { get; set; }      // e.g., "Mast", "Pole"

// Registration Info
public DateTime RegisteredDate { get; set; }   // When created
public string RegisteredBy { get; set; }       // Email of pilot

// Approval Status
public bool IsApproved { get; set; }           // true = approved
public string? ApprovedBy { get; set; }        // Email of registerfører
public DateTime? ApprovedDate { get; set; }    // When approved
public string? ApprovalComments { get; set; }  // Optional notes

// Rejection Status
public bool IsRejected { get; set; }           // true = rejected
public string? RejectedBy { get; set; }        // Email of registerfører
public DateTime? RejectedDate { get; set; }    // When rejected
public string? RejectionReason { get; set; }   // Required explanation
```

**Validation:**
- `ObstacleName`: Required, max 100 characters
- `ObstacleHeight`: Required, range 0-200 meters
- `ObstacleDescription`: Required, max 1000 characters
- `ObstacleGeometry`: Required, stored as WKT string

#### View Models
**LoginViewModel.cs:**
```csharp
public string Email { get; set; }
public string Password { get; set; }
public bool RememberMe { get; set; }
```

**RegisterViewModel.cs:**
```csharp
public string Email { get; set; }
public string Password { get; set; }
public string ConfirmPassword { get; set; }
```

---

### Services

#### RoleInitializerService.cs
**Purpose:** Creates the three roles on application startup

**When it runs:** Once at startup in `Program.cs`

**What it does:**
1. Checks if "Pilot" role exists → creates if missing
2. Checks if "Registerfører" role exists → creates if missing
3. Checks if "Admin" role exists → creates if missing

#### UserSeederService.cs
**Purpose:** Creates test accounts for development

**When it runs:** Once at startup (only in Development environment)

**What it does:**
1. Creates pilot@test.com with Pilot role
2. Creates registerforer@test.com with Registerfører role
3. Creates admin@test.com with Admin role

**Note:** Only runs if `IWebHostEnvironment.IsDevelopment()` is true

#### UserRoleService.cs
**Purpose:** Helper methods for role management

**Methods:**
- `AssignRoleToUserAsync()` - Assigns role to user
- `RemoveRoleFromUserAsync()` - Removes role from user
- `GetUserRolesAsync()` - Gets all roles for a user
- `GetUsersInRoleAsync()` - Gets all users with a specific role
- `IsInRoleAsync()` - Checks if user has a role

**Used by:** AdminController

---

### Database Design

#### Database: MariaDB 10.11
**Why MariaDB?**
- Open-source, free
- MySQL-compatible
- Excellent performance
- Easy to run in Docker

#### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mariadb;Port=3306;Database=ObstacleDB;User=obstacleuser;Password=1234;"
  }
}
```

#### Tables

**AspNetUsers** (Identity Framework)
- Stores user accounts
- Email, password hash, etc.

**AspNetRoles** (Identity Framework)
- Stores roles: Pilot, Registerfører, Admin

**AspNetUserRoles** (Identity Framework)
- Links users to roles (many-to-many)

**Obstacles** (Our custom table)
- Stores all obstacle data
- One table for all states (pending, approved, rejected, incomplete)

#### Obstacles Table Schema

```sql
CREATE TABLE Obstacles (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    
    -- Basic Info
    ObstacleName VARCHAR(100),
    ObstacleHeight DOUBLE,
    ObstacleDescription VARCHAR(1000),
    ObstacleGeometry LONGTEXT NOT NULL,
    ObstacleType VARCHAR(50),
    
    -- Registration
    RegisteredDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    RegisteredBy VARCHAR(100),
    
    -- Approval
    IsApproved BOOLEAN DEFAULT 0,
    ApprovedBy VARCHAR(100),
    ApprovedDate DATETIME,
    ApprovalComments TEXT,
    
    -- Rejection
    IsRejected BOOLEAN DEFAULT 0,
    RejectedBy VARCHAR(100),
    RejectedDate DATETIME,
    RejectionReason TEXT,
    
    UNIQUE INDEX IX_Obstacles_ObstacleName (ObstacleName)
);
```

#### Why One Table? (Soft State Pattern)

**Decision:** Store all obstacle states in a single table using boolean flags

**Alternatives considered:**
1. Separate tables (PendingObstacles, ApprovedObstacles, RejectedObstacles)
2. Status table with foreign keys
3. State machine pattern with history

**Why we chose soft states:**

**Pros:**
- Simple to understand and query
- No complex joins needed
- Complete audit trail in one place
- Fast lookups (indexed boolean flags)
- Easy to add new states
- No data movement between tables
- Perfect for simple workflows

**Cons:**
- Some NULL fields (sparse data)
- No change history if status is modified
- Can't track "re-submissions"

**Verdict:** For a simple 3-state workflow (Pending → Approved/Rejected), soft states are the industry-standard solution. Used by GitHub, Trello, Jira, and most modern applications.

#### State Detection Logic

**Incomplete Quick Register:**
```csharp
string.IsNullOrEmpty(ObstacleName) || 
ObstacleHeight == 0 || 
string.IsNullOrEmpty(ObstacleDescription)
```

**Pending (awaiting approval):**
```csharp
!IsApproved && !IsRejected && 
!string.IsNullOrEmpty(ObstacleName) && 
ObstacleHeight > 0 && 
!string.IsNullOrEmpty(ObstacleDescription)
```

**Approved:**
```csharp
IsApproved == true
```

**Rejected:**
```csharp
IsRejected == true
```

---

### Architecture Decisions

#### 1. MVC Pattern
**Decision:** Use Model-View-Controller architecture

**Why:**
- Separation of concerns (data, logic, presentation)
- Easy to test individual components
- Industry standard for web applications
- Built-in support from ASP.NET Core

**Structure:**
- **Model:** ObstacleData, ViewModels
- **View:** Razor .cshtml files
- **Controller:** Logic and routing

#### 2. Role-Based Access Control
**Decision:** Use ASP.NET Identity with custom roles

**Why:**
- Built-in authentication/authorization
- Secure password hashing (PBKDF2)
- Cookie-based authentication
- Easy role management

**Roles:**
- Pilot: Register and manage own obstacles
- Registerfører: Approve/reject submissions
- Admin: Manage users and view stats

#### 3. Entity Framework Core
**Decision:** Use EF Core as ORM (Object-Relational Mapper)

**Why:**
- No raw SQL needed (type-safe queries)
- Automatic database migrations
- Change tracking
- LINQ support

**Example:**
```csharp
// Instead of SQL:
// SELECT * FROM Obstacles WHERE RegisteredBy = 'user@email.com'

// We write:
var obstacles = await _context.Obstacles
    .Where(o => o.RegisteredBy == userEmail)
    .ToListAsync();
```

#### 4. Dependency Injection
**Decision:** Use built-in DI container

**Services registered:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<UserRoleService>();
builder.Services.AddScoped<RoleInitializerService>();
builder.Services.AddScoped<UserSeederService>();
```

**Benefits:**
- Loose coupling
- Easy to test (mock services)
- Automatic lifecycle management

#### 5. Docker for Database
**Decision:** Use Docker Compose for MariaDB

**Why:**
- No manual database installation
- Consistent environment across developers
- Easy to reset/rebuild
- Portable (works on any OS with Docker)

**Configuration:** `docker-compose.yml`

#### 6. Tailwind CSS for Styling
**Decision:** Use Tailwind CSS utility classes

**Why:**
- Rapid development (no custom CSS needed)
- Consistent design system
- Small final bundle size
- Responsive by default

**Example:**
```html
<button class="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg">
  Click Me
</button>
```

#### 7. Leaflet.js for Maps
**Decision:** Use Leaflet with OpenStreetMap

**Why:**
- Free and open-source
- Lightweight (39 KB)
- Great documentation
- Supports drawing tools (Leaflet.draw)

**Features used:**
- Point markers
- Polylines
- Polygons
- Drawing controls

---

### Views Structure

#### Shared Layout (_Layout.cshtml)
**Purpose:** Common navbar and footer for all pages

**Features:**
- Role-based navigation (different links for Pilot/Registerfører/Admin)
- User email display
- Logout button
- Responsive design

#### Pilot Views

**RegisterType.cshtml**
- Choose Quick or Full Register
- Two card buttons

**QuickRegister.cshtml**
- Map with drawing tools
- Save location only

**FullRegister.cshtml**
- Complete form with map
- Name, height, description, location

**MyRegistrations.cshtml**
- Four tabs: Incomplete, Pending, Approved, Rejected
- Color-coded status badges
- Action buttons (Complete, Delete, View)

**CompleteQuickRegister.cshtml**
- Form to fill in missing details
- Map shows existing location

**Overview.cshtml**
- Success page after registration
- Shows all obstacle details
- Action buttons (Register another, View all, Done)

#### Registerfører Views

**RegisterforerDashboard.cshtml**
- Statistics cards (pending, approved, rejected)
- Quick action buttons

**PendingObstacles.cshtml**
- Table of completed obstacles awaiting review
- Review button for each

**ReviewObstacle.cshtml**
- Detailed obstacle information
- Map showing location
- Approve form (with optional comments)
- Reject form (with required reason)

**ApprovedObstacles.cshtml**
- Table of approved obstacles
- Shows who approved and when
- View details link

**RejectedObstacles.cshtml**
- Table of rejected obstacles
- Shows rejection reason
- View details link

**ViewObstacle.cshtml**
- Read-only obstacle details
- Map showing location
- Status badge

#### Admin Views

**AdminDashboard.cshtml**
- System statistics (users, obstacles)
- Role distribution
- Quick action cards

**AdminUsers.cshtml**
- Table of all users
- Role badges
- Manage and Delete buttons

**AdminManageUser.cshtml**
- Current roles section
- Available roles section
- Assign/Remove buttons
- Delete user (danger zone)

**AdminStatistics.cshtml**
- Detailed metrics
- Obstacle statistics breakdown
- User statistics by role
- Recent activity

#### Account Views

**Index.cshtml** (Home)
- Login form
- Register form (toggled with JavaScript)
- Welcome message

**Privacy.cshtml**
- Privacy policy placeholder

---

## Technologies Used

### Backend

**ASP.NET Core 9.0**
- Web framework
- [https://dotnet.microsoft.com/](https://dotnet.microsoft.com/)

**Entity Framework Core 8.0**
- ORM (database access)
- [https://docs.microsoft.com/ef/core/](https://docs.microsoft.com/ef/core/)

**ASP.NET Identity**
- Authentication and authorization
- [https://docs.microsoft.com/aspnet/core/security/authentication/identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)

**Pomelo.EntityFrameworkCore.MySql 8.0**
- EF Core provider for MySQL/MariaDB
- [https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)

### Frontend

**Leaflet.js 1.9.4**
- Interactive maps
- [https://leafletjs.com/](https://leafletjs.com/)
- License: BSD-2-Clause

**Leaflet.draw 1.0.4**
- Drawing tools for Leaflet
- [https://github.com/Leaflet/Leaflet.draw](https://github.com/Leaflet/Leaflet.draw)
- License: MIT

**OpenStreetMap**
- Map tiles
- [https://www.openstreetmap.org/](https://www.openstreetmap.org/)
- License: ODbL

**Tailwind CSS 4.1.13**
- Utility-first CSS framework
- [https://tailwindcss.com/](https://tailwindcss.com/)
- License: MIT

**jQuery 3.7.1**
- JavaScript library
- [https://jquery.com/](https://jquery.com/)
- License: MIT

**Bootstrap 5.3.3**
- UI components (minimal use)
- [https://getbootstrap.com/](https://getbootstrap.com/)
- License: MIT

### Database

**MariaDB 10.11**
- Relational database
- [https://mariadb.org/](https://mariadb.org/)
- License: GPL v2

### DevOps

**Docker 24.x**
- Containerization
- [https://www.docker.com/](https://www.docker.com/)

**Docker Compose 2.x**
- Multi-container orchestration

### Development Tools

**Visual Studio 2022** or **Visual Studio Code**
- IDE
- [https://visualstudio.microsoft.com/](https://visualstudio.microsoft.com/)

**.NET CLI**
- Command-line tools for .NET

**Git**
- Version control
- [https://git-scm.com/](https://git-scm.com/)

---

## Known Issues

### 1. Authentication Redirect Loop
**Issue:** After database reset, normal browser may show redirect loop

**Cause:** Old authentication cookies conflict with new database

**Solution:** Clear browser cookies or use Private/Incognito mode

**How to fix:**
- Press F12 → Storage → Cookies → Delete All
- Or use Private/Incognito mode

### 2. Incomplete Quick Registers Visible to Pilot
**Issue:** Incomplete quick registrations appear in "My Registrations"

**Cause:** This is intentional - pilots need to see what needs completion

**Not a bug:** Working as designed

### 3. Database Connection on First Startup
**Issue:** Sometimes `dotnet ef database update` fails immediately after `docker-compose up`

**Cause:** MariaDB takes 10-15 seconds to fully start

**Solution:** Wait 20 seconds after starting Docker before running database commands

### 4. Unique Constraint on ObstacleName
**Issue:** Cannot register two obstacles with the same name

**Cause:** Database has unique index on `ObstacleName`

**Solution:** Use descriptive, unique names (e.g., "Radio Tower A", "Radio Tower B")

**To remove constraint:** Run migration to drop the unique index (if needed)

---

## Future Plans

### Planned Features

#### Phase 1: Enhanced Obstacle Management
- Edit obstacle details after submission (before approval)
- Bulk approval/rejection
- Export obstacles to CSV/Excel
- Advanced search and filtering
- Obstacle categories/tags

#### Phase 2: Improved Mapping
- Multiple geometry types per obstacle (e.g., base + height zone)
- 3D visualization of obstacles
- Terrain elevation data integration
- Distance measurement tools
- Export map as PDF

#### Phase 3: Workflow Improvements
- Multi-level approval (Level 1 → Level 2 → Final)
- Comment system (discussions on obstacles)
- Notification system (email when obstacle is approved/rejected)
- Approval history (see who changed status and when)
- Re-submission after rejection

#### Phase 4: Reporting & Analytics
- Advanced statistics dashboard
- Obstacle density heat maps
- Historical data trends
- Custom report generation
- Data visualization charts

#### Phase 5: User Experience
- Mobile app (React Native or Flutter)
- Offline mode (Progressive Web App)
- Multi-language support (Norwegian, English)
- Dark mode
- Accessibility improvements (WCAG 2.1 compliance)

#### Phase 6: Integration
- REST API for external systems
- WebSocket real-time updates
- Integration with aviation databases
- GIS system integration
- Automated data import from other sources

#### Phase 7: Security & Performance
- Two-factor authentication (2FA)
- API rate limiting
- Redis caching for performance
- CDN for static assets
- Database indexing optimization
- Audit logging (track all changes)

---

## Troubleshooting

### Docker Issues

**Problem:** "Docker is not running"
```
Error: Cannot connect to Docker daemon
```

**Solution:**
1. Open Docker Desktop
2. Wait for green icon
3. Try command again

---

**Problem:** "Port 3306 is already in use"
```
Error: bind: address already in use
```

**Solution:**
1. Another MySQL/MariaDB is running
2. Stop it: `net stop MySQL` (or close XAMPP/WAMP)
3. Or change port in `docker-compose.yml`

---

### .NET Issues

**Problem:** "dotnet: command not found"

**Solution:**
1. Install .NET SDK (see Prerequisites)
2. Restart Command Prompt
3. Verify: `dotnet --version`

---

**Problem:** "Entity Framework tools not found"
```
Could not execute because the specified command or file was not found.
Possible reasons for this include:
  * You misspelled a built-in dotnet command.
  * You intended to execute a .NET program, but dotnet-ef does not exist.
```

**Solution:**
```cmd
dotnet tool install --global dotnet-ef
```

---

**Problem:** "Build failed" errors

**Solution:**
1. Check you're in `FirstWebApplication` folder
2. Run: `dotnet clean`
3. Run: `dotnet restore`
4. Run: `dotnet build`
5. Check error messages - might be missing packages

---

### Database Issues

**Problem:** "Unable to connect to database"
```
MySqlConnector.MySqlException: Unable to connect to any of the specified MySQL hosts.
```

**Solution:**
1. Is Docker running? Check Docker Desktop
2. Is MariaDB started? Run: `docker ps` (should see `mariadbcontainer`)
3. Wait 20 seconds after `docker-compose up`
4. Check connection string in `appsettings.json`

---

**Problem:** "Table 'ObstacleDB.Obstacles' doesn't exist"

**Solution:**
Run migrations:
```cmd
cd FirstWebApplication
dotnet ef database update
```

---

**Problem:** "Database migration failed"

**Solution:**
Nuclear reset:
```cmd
docker-compose down -v
docker-compose up -d
# Wait 20 seconds
cd FirstWebApplication
dotnet ef database update
```

---

### Application Issues

**Problem:** "Login fails" or "Invalid credentials"

**Solution:**
1. Check you're using correct test account:
   - pilot@test.com / Pilot123
   - registerforer@test.com / Register123
   - admin@test.com / Admin123
2. Make sure database was seeded (check console output when app started)
3. Try nuclear reset

---

**Problem:** "View not found" error
```
InvalidOperationException: The view 'RegisterType' was not found
```

**Solution:**
1. Check view file exists: `Views/Pilot/RegisterType.cshtml`
2. Check action name matches view name exactly
3. Check controller name matches folder name

---

**Problem:** "Redirect loop" after login

**Solution:**
1. Clear browser cookies (F12 → Storage → Delete all)
2. Use Private/Incognito mode
3. See [Known Issues](#known-issues) section

---

### Map Issues

**Problem:** Map doesn't load (gray area)

**Solution:**
1. Check internet connection (needs OpenStreetMap tiles)
2. Check browser console (F12) for errors
3. Verify Leaflet.js and Leaflet.draw are loaded
4. Check if Leaflet CSS is included in `_Layout.cshtml`

---

**Problem:** Can't draw on map

**Solution:**
1. Check Leaflet.draw.js is loaded
2. Check draw control is initialized in JavaScript
3. Look for JavaScript errors in console (F12)

---

### General Tips

**Always check:**
1. Docker is running
2. You're in the correct folder
3. No typos in commands
4. Browser console for JavaScript errors (F12)
5. Application console output for server errors

**Nuclear reset (fixes 90% of issues):**
```cmd
docker-compose down -v
docker-compose up -d
# Wait 20 seconds
cd FirstWebApplication
dotnet clean
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

---

## License

This project is licensed under the MIT License - see below for details:

```
MIT License

Copyright (c) 2024 Kartverket Kodeprosjekt

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## Credits

**Developed by:** Third Semester Students

**Third-Party Libraries:**
- [Leaflet.js](https://leafletjs.com/) - BSD-2-Clause License
- [Leaflet.draw](https://github.com/Leaflet/Leaflet.draw) - MIT License
- [OpenStreetMap](https://www.openstreetmap.org/) - ODbL License
- [Tailwind CSS](https://tailwindcss.com/) - MIT License
- [Bootstrap](https://getbootstrap.com/) - MIT License
- [jQuery](https://jquery.com/) - MIT License

**Map Data:**
- © OpenStreetMap contributors
- [https://www.openstreetmap.org/copyright](https://www.openstreetmap.org/copyright)

**Frameworks:**
- ASP.NET Core - Microsoft
- Entity Framework Core - Microsoft
- MariaDB - MariaDB Foundation

**Special Thanks:**
- Kartverket for project inspiration
- OpenStreetMap community for map data
- All open-source contributors

---

## Repository

**GitHub:** [https://github.com/FalckM/Kartverket-Kodeprosjekt](https://github.com/FalckM/Kartverket-Kodeprosjekt)

**Issues:** Report bugs or request features at [GitHub Issues](https://github.com/FalckM/Kartverket-Kodeprosjekt/issues)

**Contributions:** Not accepting contributions at this time (student project)

---

**Last Updated:** 2024

**Project Status:** Active Development

**Contact:** Via GitHub Issues only

---

*Made with ASP.NET Core, Entity Framework, MariaDB, and Leaflet.js*
