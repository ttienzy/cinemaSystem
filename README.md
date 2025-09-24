# cinemaSystem

# Cinema Booking System

A comprehensive cinema management and online ticket booking platform built with modern technologies and Domain-Driven Design principles.

## Table of Contents

- [Overview](#overview)
- [Business Logic & Core Features](#business-logic--core-features)
- [System Architecture](#system-architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [API Documentation](#api-documentation)
- [Real-time Features](#real-time-features)
- [Payment Integration](#payment-integration)

## Overview

This Cinema Booking System is designed to handle the complete lifecycle of cinema operations, from movie management to ticket sales and concession operations. The system supports both online booking for customers and point-of-sale operations for cinema staff.

### Key Capabilities

- **Multi-cinema Management**: Support for multiple cinema locations with individual screen and seat configurations
- **Real-time Seat Selection**: Live seat availability updates using SignalR
- **Dual Sales Channels**: Online booking platform and in-person POS system
- **Comprehensive Movie Management**: Complete movie lifecycle from upcoming to archived
- **Staff & Equipment Management**: Full operational management for cinema operations
- **Payment Processing**: Integrated with VNPay for secure online transactions

## Business Logic & Core Features

### Core Business Domains

#### 1. Online Ticket Booking (Core #1)

The primary customer-facing booking system that enables:

**Customer Journey:**

```
Movie Selection → Showtime Selection → Seat Selection → Payment → E-Ticket Generation
```

**Key Features:**

- Browse movies with detailed information (cast, ratings, trailers)
- View showtimes across multiple cinemas and screens
- Interactive seat map with real-time availability
- Secure payment processing via VNPay
- Digital ticket generation with QR codes
- Booking history and management

**Business Rules:**

- Seat hold time: 10-15 minutes during payment process
- Maximum seats per booking: 8 seats
- Dynamic pricing based on seat type, time slots, and special events
- Automatic booking expiration and seat release

#### 2. Point-of-Sale Operations (Core #2)

Comprehensive POS system for cinema staff handling:

**Staff Operations:**

```
Walk-in Sales → Concession Sales → Inventory Management → Daily Reporting
```

**Key Features:**

- Quick ticket sales for walk-in customers
- Concession item sales with inventory tracking
- Staff authentication and role-based permissions
- Real-time inventory updates and low-stock alerts
- Daily sales reporting and analytics
- Cash and card payment processing

**Business Rules:**

- Staff-level access controls for different operations
- Automatic inventory deduction on sales
- End-of-shift reconciliation requirements
- Integration with online bookings for unified reporting

### Supporting Business Domains

#### Cinema & Screen Management

- Multi-location cinema support
- Flexible screen configurations (Standard, IMAX, 4DX, VIP)
- Dynamic seat layout management
- Equipment tracking and maintenance scheduling

#### Movie & Content Management

- Complete movie lifecycle management
- Cast and crew information
- Certification and rating management
- Copyright and licensing tracking
- Genre and classification systems

#### Staff & Operations Management

- Employee management with role-based access
- Shift scheduling and time tracking
- Equipment maintenance logging
- Comprehensive reporting and analytics

## System Architecture

### Domain-Driven Design (DDD)

The system is built using DDD principles with clear aggregate boundaries:

**Aggregates:**

- **Cinema Aggregate**: Cinema → Screens → Seats
- **Movie Aggregate**: Movie + Cast/Crew + Certifications + Copyrights
- **Booking Aggregate**: Booking → BookingTickets → Payments
- **Showtime Aggregate**: Showtime + Pricing configurations
- **Staff Aggregate**: Staff + Shifts + Roles
- **Concession Aggregate**: Sales + Items + Inventory tracking
- **Equipment Aggregate**: Equipment + Maintenance logs

### Clean Architecture Layers

```
├── Presentation Layer (API Controllers, SignalR Hubs)
├── Application Layer (Use Cases, DTOs, Interfaces)
├── Domain Layer (Entities, Aggregates, Domain Services)
└── Infrastructure Layer (Data Access, External Services)
```

### Real-time Communication

- **SignalR Integration**: Real-time seat availability updates
- **Event-Driven Architecture**: Domain events for system integration
- **Caching Strategy**: Redis for high-performance seat queries

## Technology Stack

### Backend Technologies

- **Framework**: ASP.NET Core 8.0
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core 8.0
- **Real-time**: SignalR for WebSocket communication
- **Caching**: Redis for performance optimization
- **Authentication**: JWT-based authentication
- **Payment**: VNPay integration for Vietnamese market

### Frontend Technologies

- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite for fast development and building
- **State Management**: React Hooks and Context API
- **UI Styling**: Tailwind CSS for responsive design
- **Real-time**: SignalR JavaScript client
- **HTTP Client**: Axios for API communication

### Development Tools

- **API Documentation**: Swagger/OpenAPI 3.0
- **Database Migrations**: Entity Framework Code-First
- **Version Control**: Git with conventional commits
- **Testing**: xUnit for backend, Jest for frontend

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- SQL Server 2019+ or SQL Server Express
- Redis Server (for caching)
- Visual Studio 2022 or VS Code

### Backend Setup

1. **Clone the repository**

```bash
git clone https://github.com/your-repo/cinema-booking-system.git
cd cinema-booking-system
```

2. **Configure Database Connection**

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CinemaBookingDB;Trusted_Connection=true;"
  }
}
```

3. **Run Database Migrations**

```bash
cd src/Infrastructure
dotnet ef database update
```

4. **Configure Redis (Optional but recommended)**

```json
// appsettings.json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

5. **Start the API**

```bash
cd src/WebAPI
dotnet run
```

The API will be available at `https://localhost:7001`

### Frontend Setup

1. **Navigate to frontend directory**

```bash
cd src/ClientApp
```

2. **Install dependencies**

```bash
npm install
```

3. **Configure environment variables**

```env
# .env
VITE_API_BASE_URL=https://localhost:7001/api
VITE_SIGNALR_HUB_URL=https://localhost:7001/seatHub
VITE_VNPAY_RETURN_URL=http://localhost:3000/payment/callback
```

4. **Start development server**

```bash
npm run dev
```

The frontend will be available at `http://localhost:3000`

### Database Seeding

Run the seeding script to populate initial data:

```bash
dotnet run --project src/WebAPI -- --seed
```

This will create:

- Sample cinemas and screens
- Movie catalog with current releases
- Seat configurations and pricing tiers
- Admin user accounts

## Real-time Features

### SignalR Implementation

The system uses SignalR for real-time communication:

**Seat Availability Updates**

```typescript
// Client-side subscription
signalRService.subscribeToSeatUpdates(showtimeId, (seatUpdate) => {
  // Handle real-time seat status changes
  updateSeatMap(seatUpdate);
});
```

**Hub Methods**

- `JoinShowtimeGroup(showtimeId)`: Subscribe to seat updates
- `LeaveShowtimeGroup(showtimeId)`: Unsubscribe from updates
- `SeatUpdated`: Real-time seat status broadcast
- `SeatHeld`: Temporary seat hold notifications
- `SeatReleased`: Seat release notifications

## Payment Integration

### VNPay Configuration

1. **Register with VNPay**

   - Obtain Merchant ID and Secret Key
   - Configure return URLs for success/failure scenarios

2. **Configure Payment Settings**

```json
{
  "VNPay": {
    "TmnCode": "YOUR_MERCHANT_ID",
    "HashSecret": "YOUR_SECRET_KEY",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://yoursite.com/payment/callback"
  }
}
```

3. **Payment Flow**

```
Customer Booking → VNPay Redirect → Payment Processing → Callback Handling → Ticket Generation
```

### Development Workflow

1. **Feature Development**

   - Create feature branch from `develop`
   - Implement feature following DDD principles
   - Write comprehensive tests
   - Update documentation

2. **Code Standards**

   - Follow C# coding conventions
   - Use meaningful variable and method names
   - Implement proper error handling
   - Add XML documentation for public APIs

3. **Testing Requirements**

   - Unit tests for domain logic
   - Integration tests for API endpoints
   - Frontend component tests
   - End-to-end booking flow tests

4. **Pull Request Process**
   - Ensure all tests pass
   - Include API documentation updates
   - Provide clear PR description
   - Request code review from team members

### Environment Configuration

**Development Environment**

- Use SQL Server LocalDB
- Redis running on default port (6379)
- VNPay sandbox environment

**Production Considerations**

- Configure proper SSL certificates
- Set up Redis cluster for high availability
- Implement proper logging and monitoring
- Configure production VNPay credentials

## License

This project is proprietary software. All rights reserved.

## Support

For technical support or questions about the system:

- Create an issue in the project repository
- Contact the development team
- Refer to the API documentation at `/swagger`
