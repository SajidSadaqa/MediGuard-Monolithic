# MediGuard Backend

This repository contains the backend implementation for the MediGuard application, a comprehensive medication management system built with ASP.NET Core 9 MVC.

## Features

- **User Authentication**: Secure JWT-based authentication with ASP.NET Identity
- **Medication Management**: Complete CRUD operations for medications
- **Medication Conflict Checking**: Intelligent detection of potential medication interactions
- **Order Processing**: Full order flow with dummy payment integration
- **Personalized Medication Assistant**: AI-powered chatbot using DeepSeek API
- **Adaptive Medication Recommendations**: Smart recommendation engine
- **Visual Recognition of Medications**: Simulated pill recognition service
- **Emergency Conflict Alerts**: Real-time conflict detection

## Technology Stack

- **ASP.NET Core 9 MVC**: Modern, high-performance web framework
- **Entity Framework Core**: Code-first approach for database management
- **SQL Server**: Robust relational database
- **JWT Authentication**: Secure token-based authentication
- **Swagger**: Comprehensive API documentation
- **DeepSeek API Integration**: AI-powered chatbot and recommendations

## Project Structure

```
/MediGuard.API
├── Controllers
│   ├── AuthController.cs
│   ├── ChatBotController.cs
│   ├── MedicationController.cs
│   ├── OrderController.cs
│   ├── PillRecognitionController.cs
│   ├── RecommendationController.cs
│   └── UserMedicationController.cs
├── Services
│   ├── AuthService.cs
│   ├── ChatBotService.cs
│   ├── MedicationService.cs
│   ├── OrderService.cs
│   ├── PillRecognitionService.cs
│   ├── RecommendationService.cs
│   └── UserMedicationService.cs
├── Models
│   ├── ApplicationUser.cs
│   ├── ChatMessage.cs
│   ├── Medication.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Recommendation.cs
│   └── UserMedication.cs
├── Data
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── DTOs
│   ├── ChatDto.cs
│   ├── MedicationDto.cs
│   ├── OrderDto.cs
│   ├── RecommendationDto.cs
│   ├── UserDto.cs
│   └── UserMedicationDto.cs
├── Helpers
│   ├── ConflictChecker.cs
│   └── DummyPaymentProcessor.cs
├── Middleware
│   ├── AuthorizeRolesAttribute.cs
│   └── ExceptionHandlerMiddleware.cs
├── Tests
│   ├── TestAuthController.cs
│   ├── TestChatBotController.cs
│   ├── TestConflictAndUserMedication.cs
│   ├── TestMedicationController.cs
│   ├── TestOrderController.cs
│   ├── TestPillRecognitionController.cs
│   └── TestRecommendationController.cs
├── Program.cs
└── appsettings.json
```

## API Endpoints

The backend provides the following API endpoints:

### Authentication
- `POST /api/Auth/register` - Register a new user
- `POST /api/Auth/login` - Login and get JWT token
- `GET /api/Auth/profile` - Get user profile
- `PUT /api/Auth/profile` - Update user profile

### Medications
- `GET /api/Medication` - Get all medications
- `GET /api/Medication/{id}` - Get medication by ID
- `POST /api/Medication` - Create new medication (Admin only)
- `PUT /api/Medication/{id}` - Update medication (Admin only)
- `DELETE /api/Medication/{id}` - Delete medication (Admin only)
- `GET /api/Medication/search` - Search medications

### User Medications
- `GET /api/UserMedication` - Get user's medications
- `GET /api/UserMedication/{id}` - Get user medication by ID
- `POST /api/UserMedication` - Add medication to user's list
- `PUT /api/UserMedication/{id}` - Update user medication
- `DELETE /api/UserMedication/{id}` - Delete user medication
- `PUT /api/UserMedication/{id}/toggle-status` - Toggle medication active status
- `GET /api/UserMedication/conflicts` - Check user medication conflicts
- `GET /api/UserMedication/medication/{medicationId}/conflicts` - Check specific medication conflicts

### Orders
- `GET /api/Order` - Get user's orders
- `GET /api/Order/{id}` - Get order by ID
- `POST /api/Order` - Create new order
- `PUT /api/Order/{id}/status` - Update order status (Admin only)
- `POST /api/Order/{id}/cancel` - Cancel order

### ChatBot
- `POST /api/ChatBot` - Get chat response
- `GET /api/ChatBot/history` - Get chat history

### Recommendations
- `GET /api/Recommendation` - Get user recommendations
- `GET /api/Recommendation/{id}` - Get recommendation by ID
- `POST /api/Recommendation/generate` - Generate new recommendations
- `PUT /api/Recommendation/{id}/view` - Mark recommendation as viewed
- `PUT /api/Recommendation/{id}/accept` - Accept recommendation
- `DELETE /api/Recommendation/{id}` - Delete recommendation

### Pill Recognition
- `POST /api/PillRecognition/recognize` - Recognize pill from image

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server

### Setup
1. Clone the repository
2. Update the connection string in `appsettings.json` to point to your SQL Server instance
3. Run the application using `dotnet run`
4. Access the Swagger documentation at `https://localhost:5001/swagger`

## Testing

The project includes comprehensive unit tests for all controllers and key components. Run the tests using:

```
dotnet test
```

## Security Features

- JWT-based authentication
- Role-based authorization
- Password hashing with ASP.NET Identity
- HTTPS enforcement
- CORS configuration for Flutter frontend
- Exception handling middleware

## Sample Data

The application is seeded with sample data including:
- Test user: `test@mediaguard.com` / `Test@123`
- Admin user: `admin@mediaguard.com` / `Admin@123`
- Sample medications (Advil, Tylenol, Warfarin, etc.)
