📌 Mini ERP System

ASP.NET Core Web API + MySQL + React

A full-stack Mini ERP system built to demonstrate real-world enterprise application concepts such as authentication, inventory management, and invoicing.

✨ Key Features

🔐 JWT-based Authentication (Register & Login)

👤 Secure user profile endpoint

📦 Inventory (Items) Management

🧾 Sales Invoicing with Line Items

📉 Automatic stock reduction on invoicing

📘 API documentation using Swagger

🛠️ Tools & Technologies
Backend

ASP.NET Core Web API (.NET 9)

C#

MySQL

JWT Authentication

Swagger / OpenAPI

MySqlConnector

Frontend

React

JavaScript

Axios

React Router

Development Tools

Visual Studio Code

.NET CLI

Node.js & npm

Git

📂 Project Folder Structure
erpdotnet/
├── MiniErp.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── UsersController.cs
│   │   ├── ItemsController.cs
│   │   └── InvoicesController.cs
│   │
│   ├── Models/
│   │   ├── AuthDtos.cs
│   │   ├── ItemDtos.cs
│   │   └── InvoiceDtos.cs
│   │
│   ├── Properties/
│   │   └── launchSettings.json
│   │
│   ├── Program.cs
│   ├── appsettings.json
│   └── MiniErp.Api.csproj
│
├── mini-erp-ui/
│   ├── src/
│   │   ├── api.js
│   │   ├── App.js
│   │   ├── Login.js
│   │   ├── Register.js
│   │   ├── Me.js
│   │   └── index.js
│   │
│   ├── assets/
│   │   ├── ss1.jpg   ← Swagger API overview
│   │   └── ss2.jpg   ← Swagger JWT authorization
│   │
│   └── package.json
│
└── README.md

🗄️ Database Setup (MySQL)
CREATE DATABASE mini_erp;
USE mini_erp;

CREATE TABLE erp_users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(200) UNIQUE,
  password_hash VARCHAR(255),
  password_salt VARCHAR(255),
  full_name VARCHAR(200),
  role VARCHAR(50) DEFAULT 'Clerk'
);

CREATE TABLE items (
  id INT AUTO_INCREMENT PRIMARY KEY,
  sku VARCHAR(50) UNIQUE,
  name VARCHAR(200),
  unit_price DECIMAL(10,2),
  qty_on_hand INT
);

CREATE TABLE sales_invoices (
  id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_no VARCHAR(30),
  customer_name VARCHAR(200),
  invoice_date DATE,
  total DECIMAL(12,2),
  created_by_email VARCHAR(200)
);

CREATE TABLE sales_invoice_lines (
  id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_id INT,
  item_id INT,
  qty INT,
  unit_price DECIMAL(10,2),
  line_total DECIMAL(12,2)
);

⚙️ Backend Configuration

Edit MiniErp.Api/appsettings.json:

{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=mini_erp;User=root;Password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "LONG_RANDOM_SECRET_KEY",
    "Issuer": "MiniErp",
    "Audience": "MiniErpClient",
    "ExpiresMinutes": 60
  }
}

▶️ How to Run the Project
1️⃣ Start Backend
cd MiniErp.Api
dotnet run


API URL: http://localhost:5000

Swagger: http://localhost:5000/swagger

2️⃣ Start Frontend
cd mini-erp-ui
npm install
npm start


UI URL: http://localhost:3000

🧪 API Testing with Swagger
Swagger UI
http://localhost:5000/swagger

📸 Swagger Screenshots

Swagger API Overview


JWT Authorization in Swagger


🔐 Authentication Flow

Call POST /api/auth/register

Or POST /api/auth/login

Copy the JWT token

Click Authorize in Swagger

Paste:

Bearer <JWT_TOKEN>


Access protected APIs

📦 Available API Endpoints
Auth

POST /api/auth/register

POST /api/auth/login

User

GET /api/users/me 🔒

Items

GET /api/items

POST /api/items

PUT /api/items/{id}

DELETE /api/items/{id}

Invoices

POST /api/invoices

GET /api/invoices/{id}

🧠 ERP Workflow Summary

User registers/logs in

Items are created

Invoice is generated

Stock is reduced automatically

Invoice total is calculated

🔮 Future Enhancements

Customers & suppliers

Purchase orders

Invoice listing & search

Role-based access (Admin / Clerk)

Reports & dashboards

👨‍💻 Author

Mini ERP System
ASP.NET Core + MySQL + React
Academic / learning project


