# Inventory Management System

## Description
ASP.NET Core RESTful API for managing inventory, invoices, 
stock movements, and financial operations.
Refactored and extended from a .NET Framework 4.8 desktop 
application to a modern .NET 8 Web API.

## Architecture
- API Layer
- Business Layer (BL)
- Data Access Layer (DAL)
- Shared Layer (DTOs and utilities)

## Technologies
- C# / .NET 8
- ASP.NET Core Web API
- SQL Server
- ADO.NET

## Features
- Inventory management using product batches
- Manual active batch selection per product
- Automatic quantity deduction from batches
- Manages purchase, sales, return, and expense invoices
- Transactional consistency between invoices and batches
- Stock movement tracking with filtering support
- Dynamic invoice filtering and financial reporting
- JWT-based authentication
- Dynamic permission-based authorization using bitwise flags
- Rate limiting on authentication endpoint
- Centralized exception handling middleware
- Security logging for failed login attempts and forbidden access

## Deployment
1. Clone the repository
2. Execute the database scripts in SQL Server
3. Set environment variable: `ConnectionStrings__InventoryDB`
4. Set environment variable: `JWT__KEY`
5. Run the application