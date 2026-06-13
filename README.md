# Attendance Management Desktop Client 💻

A native Windows desktop application built with **.NET 8**, serving as the dedicated administrative workstation for the Attendance System. This client is engineered using a structured **N-Tier architecture** within a unified project, ensuring that UI rendering, business rules, and data access remain strictly isolated for optimal maintainability.

## 🚀 Technical Specifications
* **Framework:** .NET 8.0 Windows (C#)
* **UI Paradigm:** Windows Presentation / Event-Driven Architecture
* **Data Access:** SQL Server (via T-SQL & internal Repositories)
* **Design Pattern:** Layered Monolith (N-Tier)

## 📂 Component Architecture & Directory Deep-Dive

Unlike the distributed Clean Architecture of the web portal, this desktop client utilizes a highly cohesive, layered structure optimized for rapid local execution, statefulness, and tight operating system integration. Every directory has a strictly defined role in the application's lifecycle.

### 🖥️ Presentation Layer (UI)
* **`Forms/`**: Contains the primary window definitions and top-level UI lifecycle management. These forms act as the main containers for user interaction. They are deliberately kept "thin," delegating complex event handling and data processing to the service layer to prevent the "Fat UI" anti-pattern.
* **`UserControls/`**: Encapsulates reusable, modular UI components. By isolating complex or repetitive UI elements (such as specialized data grids, metric panels, or dashboard widgets) into User Controls, the project strictly adheres to the DRY (Don't Repeat Yourself) principle, ensuring visual and functional consistency across the entire application.

### 🧠 Business & Domain Logic
* **`Models/` & `Models/Dashboard/`**: The core data structures representing application state and system entities. These POCOs (Plain Old CLR Objects) are responsible for carrying typed data safely between the database, the services, and the UI without exposing internal implementations.
* **`Services/`**: The operational brain of the client. This module intercepts commands from the UI, applies domain business rules, performs input validation, and coordinates with the repositories. By isolating this logic, the application becomes significantly easier to unit test.

### 💾 Infrastructure & Data Access
* **`Repositories/`**: The dedicated data access layer. This directory encapsulates all database querying logic, ensuring that SQL Server interactions (T-SQL) and data hydration mechanisms do not leak into the business or presentation layers.
* **`database/`**: Houses localized database assets, schema definitions, connection configurations, or seeding mechanisms required for the desktop environment to interface seamlessly with the backend.

### 🛠️ Shared Resources
* **`Utilities/`**: A centralized module for cross-cutting concerns. This includes essential helper classes, state managers, formatting extensions, and cryptographic functions that are universally required across the application, keeping the core layers free of boilerplate utility code.
