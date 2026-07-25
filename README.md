# Smart Monitoring System for Employee Performance Evaluation
### Predictive Analytics Based on Deep Learning (LSTM)

![Angular](https://img.shields.io/badge/Angular-20-red)
![.NET](https://img.shields.io/badge/.NET-8-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-blue)
![Python](https://img.shields.io/badge/Python-3.10-yellow)
![TensorFlow](https://img.shields.io/badge/TensorFlow-LSTM-orange)

## Overview

Smart Monitoring System is a web-based application developed as a final undergraduate project to improve employee performance evaluation through predictive analytics.

Unlike traditional performance evaluation systems that rely on manual assessment, this system integrates **Deep Learning (Long Short-Term Memory / LSTM)** with a modern web architecture to provide objective, adaptive, and data-driven performance analysis.

The application combines:

- Angular Frontend
- ASP.NET Core Web API Backend
- Microsoft SQL Server Database
- Python Flask Deep Learning API

The system enables organizations to visualize employee performance, monitor daily productivity, and predict future performance using historical data.

---

# Features

## Authentication

- JWT Authentication
- Role-based Authorization
- Employee Dashboard
- Engineer Dashboard

---

## Employee Features

- Daily Performance Summary
- Finished Today
- Average Workmanship
- Total Workmanship
- AI Efficiency Score
- Task Log History
- Performance Charts
- Development Activity
- Daily Prediction Result

---

## Engineer Features

- Team Performance Dashboard
- Employee Performance Table
- Team Visualization
- Employee Detail
- AI Prediction
- Future Workload Prediction

---

# Deep Learning Features

The prediction service is developed using Long Short-Term Memory (LSTM).

Model Outputs:

- Efficiency Score
- Performance Category
- Next Day Workload Prediction

Model Pipeline:

Dataset

↓

Data Cleaning

↓

Normalization

↓

Sequence Generation

↓

LSTM Training

↓

Prediction API (Flask)

↓

.NET Core API

↓

Angular Dashboard

---

# System Architecture

```
                 +----------------------+
                 |    Angular Frontend  |
                 +----------+-----------+
                            |
                     REST API (JWT)
                            |
                            v
              +---------------------------+
              | ASP.NET Core Web API (.NET 8) |
              +-------------+--------------+
                            |
        +-------------------+--------------------+
        |                                        |
        |                                        |
        v                                        v
 SQL Server Database                   Flask Prediction API
                                               |
                                               |
                                         TensorFlow LSTM
```

---

# Technology Stack

## Frontend

- Angular 20
- Standalone Components
- TypeScript
- HTML
- CSS

---

## Backend

- ASP.NET Core 8 Web API
- C#
- Dapper ORM
- JWT Authentication
- REST API

---

## Database

- Microsoft SQL Server 2022

Main Tables

- Users
- WorkLogNew
- DailyUserSummary2

---

## Deep Learning

- Python 3.10
- TensorFlow
- Scikit-Learn
- Flask API

---

# Folder Structure

```
SmartMonitoringSystem
│
├── frontend/
│   ├── src/
│   │   └── app/
│   │       ├── components/
│   │       ├── models/
│   │       ├── services/
│   │       └── shared/
│   └── angular.json
│
├── backend/
│   ├── Controllers/
│   ├── Services/
│   ├── Repository/
│   ├── Models/
│   ├── Middleware/
│   ├── Helpers/
│   ├── Program.cs
│   └── appsettings.json
│
├── flask-api/
│   ├── model/
│   ├── dataset/
│   ├── train.py
│   ├── predict.py
│   └── app.py
│
├── database/
│   └── SQL Scripts
│
└── README.md
```

---

# Application Flow

```
Employee

↓

Login

↓

JWT Authentication

↓

Dashboard

↓

Submit Daily Activity

↓

Store into SQL Server

↓

Backend requests Flask API

↓

LSTM Prediction

↓

Prediction Result

↓

Dashboard Visualization

↓

Engineer Dashboard

↓

Decision Support
```

---

# Functional Requirements

- User Authentication
- Role-based Authorization
- Daily Performance Monitoring
- Work Log Management
- Employee Dashboard
- Engineer Dashboard
- Team Visualization
- AI Prediction
- Performance Classification
- Future Workload Prediction

---

# Non Functional Requirements

- Responsive Web Application
- RESTful Architecture
- Secure Authentication (JWT)
- SQL Server Database
- Real-Time Prediction
- Modular Architecture
- Cross Platform
- User Friendly Interface

---

# Development Methodology

### Design Science Research (DSR)

- Problem Identification
- Define Objectives
- Design & Development
- Demonstration
- Evaluation

### Agile Development

- Kanban
- To Do
- In Progress
- Done

---

# Testing

## Functional Testing

- Black Box Testing

## Usability Testing

- System Usability Scale (SUS)

Final SUS Score:

```
74 / 100
```

Category:

```
Good
Acceptable
```

---

# Dataset

Dataset Source:

- Kaggle Employee Performance Dataset

Main Features:

- WorkDate
- FinishedToday
- AvgWorkmanship
- TotalWorkmanship
- EfficiencyScore
- PerformanceCategory
- UserId

---

# Future Improvements

- Docker Deployment
- Kubernetes Support
- CI/CD Pipeline
- Microservices Architecture
- Real-time Notification
- Power BI Integration
- Azure Deployment
- Multi-Factory Support

---

# Author

**Dany Setiawan Maulana Azis**

Software Engineering Technology

Batam State Polytechnic

2026

---

# License

This project was developed for academic purposes as an undergraduate final project at Batam State Polytechnic.
