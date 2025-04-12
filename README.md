MediGuard
MediGuard is a healthcare application designed to assist users in managing medication schedules, tracking adherence, and processing payments securely. This README highlights my contributions to the project, specifically in database design, ASP.NET Core Entity Framework integration, and controller development.

My Contributions
As a key contributor to MediGuard, I was responsible for designing and implementing the database, integrating it with ASP.NET Core using Entity Framework Core, and developing the controllers to handle application logic. Below is a summary of my achievements:

1. Database Design and Implementation
Database Creation: Designed and implemented the db_ab6bf8_mediguard database using Microsoft SQL Server, optimized for performance and scalability.
Key Tables:
AdherenceRecords: Tracks medication adherence with fields like UserId, MedicationId, ScheduledDoseTime, ActualDoseTime, IsDoseTaken, and AlertTriggered.
AspNetUsers: Manages user authentication and profile data (e.g., FullName, Email, DateOfBirth) using ASP.NET Identity.
Medications: Stores medication details such as ScientificName, BrandName, Price, and Description.
MedicationRecommendations: Links recommendations to users with fields like Dosage, Timing, and RecommendationReason.
PaymentTransactions: Handles payment data with TransactionId, OrderId, Amount, and PaymentStatus.
Additional tables for roles, claims, and migrations (__EFMigrationsHistory) to support ASP.NET Identity and Entity Framework.
Optimization: Configured database settings (e.g., RECOVERY SIMPLE, QUERY_STORE = ON) for efficient querying and maintenance.
Relationships: Established foreign key constraints (e.g., between AspNetUsers and AspNetUserRoles) to ensure data integrity.
2. Entity Framework Core Integration
Models: Created C# entity classes corresponding to database tables, ensuring proper mapping with data annotations and fluent API configurations.
DbContext: Developed the MediGuardDbContext class to manage database connections and entity relationships.
Migrations: Implemented Entity Framework Core migrations to version-control the database schema, ensuring seamless updates (__EFMigrationsHistory table).
Data Access: Wrote efficient LINQ queries for CRUD operations, integrated with the ASP.NET Core application.
3. Controllers Development
API Controllers: Built RESTful API controllers using ASP.NET Core to handle requests for:
User management (e.g., registration, login, profile updates).
Medication tracking (e.g., adding schedules, recording adherence).
Payment processing (e.g., initiating transactions, updating statuses).
Dependency Injection: Utilized ASP.NET Core’s dependency injection to inject services like MediGuardDbContext into controllers.
Validation: Implemented model validation and error handling to ensure robust API endpoints.
Authentication: Integrated ASP.NET Identity for secure user authentication and role-based authorization.
Technologies Used
Backend: ASP.NET Core
Database: Microsoft SQL Server
ORM: Entity Framework Core
Authentication: ASP.NET Identity
