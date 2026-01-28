# Mini ERP System (ASP.NET Core + MySQL + React)

A simple **Mini ERP system** built using:

- **ASP.NET Core Web API (.NET 9)**
- **MySQL**
- **JWT Authentication**
- **React (Frontend)**
- **Swagger (API testing & documentation)**

This project demonstrates a basic ERP workflow including:
- User Registration & Login
- Role-based authentication (JWT)
- Items (Inventory) management
- Sales Invoices with invoice lines
- Stock updates on invoicing

---

## 🚀 Tech Stack

### Backend
- ASP.NET Core Web API (.NET 9)
- MySQL
- JWT Authentication
- Swagger / OpenAPI
- MySqlConnector

### Frontend
- React
- Axios
- React Router

---

## 📂 Folder Structure

erpdotnet/
├── MiniErp.Api/
│ ├── Controllers/
│ │ ├── AuthController.cs
│ │ ├── UsersController.cs
│ │ ├── ItemsController.cs
│ │ └── InvoicesController.cs
│ │
│ ├── Models/
│ │ ├── AuthDtos.cs
│ │ ├── ItemDtos.cs
│ │ └── InvoiceDtos.cs
│ │
│ ├── Properties/
│ │ └── launchSettings.json
│ │
│ ├── Program.cs
│ ├── appsettings.json
│ └── MiniErp.Api.csproj
│
├── mini-erp-ui/
│ ├── src/
│ │ ├── api.js
│ │ ├── App.js
│ │ ├── Login.js
│ │ ├── Register.js
│ │ ├── Me.js
│ │ └── index.js
│ │
│ ├── assets/
│ │ ├── ss1.jpg <-- Swagger screenshot
│ │ └── ss2.jpg <-- Swagger authorize screenshot
│ │
│ └── package.json
│
└── README.md


---

## 🗄️ Database Setup (MySQL)

Create the database and tables:

```sql
CREATE DATABASE mini_erp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE mini_erp;

CREATE TABLE erp_users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(200) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  password_salt VARCHAR(255) NOT NULL,
  full_name VARCHAR(200),
  role VARCHAR(50) DEFAULT 'Clerk',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE items (
  id INT AUTO_INCREMENT PRIMARY KEY,
  sku VARCHAR(50) NOT NULL UNIQUE,
  name VARCHAR(200) NOT NULL,
  unit_price DECIMAL(10,2) NOT NULL,
  qty_on_hand INT NOT NULL,
  is_active TINYINT(1) DEFAULT 1,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE sales_invoices (
  id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_no VARCHAR(30) NOT NULL UNIQUE,
  customer_name VARCHAR(200) NOT NULL,
  invoice_date DATE NOT NULL,
  total DECIMAL(12,2) NOT NULL,
  created_by_email VARCHAR(200),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE sales_invoice_lines (
  id INT AUTO_INCREMENT PRIMARY KEY,
  invoice_id INT NOT NULL,
  item_id INT NOT NULL,
  qty INT NOT NULL,
  unit_price DECIMAL(10,2) NOT NULL,
  line_total DECIMAL(12,2) NOT NULL,
  FOREIGN KEY (invoice_id) REFERENCES sales_invoices(id) ON DELETE CASCADE,
  FOREIGN KEY (item_id) REFERENCES items(id)
);
⚙️ Backend Configuration
Edit MiniErp.Api/appsettings.json:

{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Port=3306;Database=mini_erp;User=root;Password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY",
    "Issuer": "MiniErp",
    "Audience": "MiniErpClient",
    "ExpiresMinutes": 60
  }
}
▶️ How to Run the Project
1️⃣ Run Backend (ASP.NET Core)
cd MiniErp.Api
dotnet run
Backend will start at:

http://localhost:5000
Swagger UI:

http://localhost:5000/swagger
2️⃣ Run Frontend (React)
cd mini-erp-ui
npm install
npm start
Frontend will start at:

http://localhost:3000
🧪 Testing the Backend with Swagger
Swagger UI
Open:

http://localhost:5000/swagger
Swagger Screenshots
Swagger API Overview

JWT Authorization in Swagger

🔐 Authentication Flow
Call POST /api/auth/register

Or call POST /api/auth/login

Copy the returned JWT token

Click Authorize in Swagger

Paste:

Bearer <JWT_TOKEN>
You can now access protected endpoints

📦 Available API Endpoints
Auth
POST /api/auth/register

POST /api/auth/login

User
GET /api/users/me 🔒

Items (Inventory)
GET /api/items

POST /api/items

PUT /api/items/{id}

DELETE /api/items/{id}

Invoices
POST /api/invoices

GET /api/invoices/{id}

🧠 ERP Flow Summary
Register/Login user

Create items (inventory)

Create invoice with items

Stock is reduced automatically

Invoice total is calculated

🔮 Future Enhancements
Customers module

Purchase orders

Invoice listing & search

Role-based authorization (Admin / Clerk)

UI pages for Items & Invoices

Reporting & dashboards

👨‍💻 Author
Mini ERP Project – .NET + MySQL + React
Built for learning and academic purposes.


