# Smart Monitoring System for Employee Performance Evaluation

## Overview

Smart Monitoring System is a web-based application developed to support objective and data-driven employee performance evaluation using Deep Learning. This project was created as a final project (Tugas Akhir) in the Software Engineering Technology Program at Politeknik Negeri Batam.

The system integrates Angular 20 as the frontend framework, ASP.NET Core Web API as the backend service, Microsoft SQL Server as the database management system, and Flask API with TensorFlow for Deep Learning model integration. The application applies a modular architecture with separation of concerns to ensure scalability, maintainability, and real-time communication between services.

## Deep Learning Models

This project implements multiple Deep Learning models based on Long Short-Term Memory (LSTM), including:

* Regression model for predicting employee efficiency score
* Time series model for predicting future workload
* Classification model for determining employee performance category

## Main Features

### Employee Dashboard

* Daily performance monitoring
* Efficiency score prediction
* Historical task logging
* Development activity visualization

### Engineer Dashboard

* Team performance visualization
* Employee monitoring dashboard
* Workload prediction
* Performance category analysis

## System Features

* JWT-based authentication and role management
* Real-time performance monitoring
* REST API integration between .NET and Flask
* SQL Server database integration
* Responsive Single Page Application (SPA)
* Fallback mechanism when Flask API is unavailable

## Evaluation

The system was evaluated using:

* Black-box functional testing
* Deep Learning evaluation metrics (MAE, RMSE, Accuracy, Precision, Recall)
* System Usability Scale (SUS)

The usability evaluation achieved a SUS score of 74, categorized as **Good**, indicating that the system is acceptable and user-friendly for monitoring and decision-support activities.

## Technologies Used

### Frontend

* Angular 20
* TypeScript
* HTML/CSS

### Backend

* ASP.NET Core Web API (.NET 8)
* Dapper ORM
* JWT Authentication

### Database

* Microsoft SQL Server 2022

### Deep Learning

* Python 3.10
* TensorFlow
* Scikit-learn
* Flask API

## Architecture

The system uses a modular architecture consisting of:

1. Frontend (Angular)
2. Backend (.NET Core Web API)
3. Database (SQL Server)
4. Deep Learning Service (Flask API + TensorFlow)

Communication between services is implemented using REST API to support real-time data exchange and prediction processing.

## Research Methodology

This project uses the Design Science Research (DSR) methodology combined with Agile Kanban development to support iterative and adaptive system development.

## Dataset

This project uses public datasets obtained from Kaggle for training and evaluating Deep Learning models.

Datasets used:

* Factory Workers Daily Performance and Attrition
* Employee Activity and Evaluation Dataset
* Productivity Prediction of Garment Employees

## Author

**Dany Setiawan Maulana Azis**
Software Engineering Technology Program
Politeknik Negeri Batam
