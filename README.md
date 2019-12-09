# Building the Bangazon Platform API

Welcome, new Bangazonians!

Your job is to build out a .NET Web API that makes each resource in the Bangazon ERD available to application developers throughout the entire company.

1. Products
1. Product types
1. Customers
1. Orders
1. Payment types
1. Employees
1. Computers
1. Training programs
1. Departments

> **Pro tip:** You do not need to make a Controller for the join tables, because those aren't resources.

Your product owner will provide you with a prioritized backlog of features for you to work on over the development sprint. The first version of the API will be completely open since we have not determined which authentication method we want to use yet.



## Plan

First, you need to plan.

- Your team needs to use the official SQL script (see below) and use the ERD provided.
- Read all of the tickets. They're in order of priority. This is a week long sprint, and you're in charge of deciding how many tickets you want to take on. When you've decided, one of your lead devs will come around for a commitment ceremony.


## Modeling

Next, you need to author the Models needed for your API. Make sure that each model has the approprate foreign key relationship defined on it, either with a custom type or an `List<T>` to store many related things.

## Database Management

You will be using the [Official Bangazon SQL](./bangazon.sql) file to create your database. Name the database `BangazonAPI`. Create the database using the SQL Server Object Explorer in Visual Studio and then run the provided SQL file on the new database. Every team member will need to build the database on their own computer.

## Controllers

Now it's time to build the controllers that handle GET, POST, PUT, and DELETE operations on each resource. Make sure you read and clarify the requirements in the issue tickets to you can use  SQL to return the correct data structure to client requests.

## Test Classes

Each feature ticket your team will work on for this sprint has testing requirements. This boilerplate solution has a testing project includes with some starter code. You must make sure that all tests pass before you submit a PR.


