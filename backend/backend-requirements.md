# REQUIRED SETTINGS FOR THIS APPLICATION 

- use .NET 10 
- use project template API 
- use DDD arch
    - API layer
    - Domain layer
    - Application layer
    - Infrastructure layer

- API Layer
    - Controllers
    - Program
    - appsettings for setting enviroments 
    - DI from Application and Infrastructure services

- Application Layer
    - DTOs 
    - Services/interfaces
    - Application Module (inject all services and interfaces in this class to be imported into api program file)

- Infrasctructure Layer 
    - Repository / interfaces
    - Infra Module (inject all repositories and interfaces in this class to be imported into api program file)
    - DatabaseSeeder
    - DatabaseContext
    - For every Entity **MUST HAVE** his own databaseConfiguration like (ProdutDatabaseSettings) to be imported into DatabaseContext
    - Use EntityFramework

- Database 
    - Postgres Docker

- RabbitMq -> for processing the orders of our application


**Work flow**
- Controllers (API) -> IService (Application) -> IRepository (Infra) -> Database


# ABOUT THE PROJECT
Context: 
    - This application is for a restaurant called **SOFTWARE HAMGUERIA** basicly we must be able to create, update, get an Dish or get all Dishers
    - create, update, get an Order or get all Orders.
    - creating/update/canceling an order needs to be updated in rabbitmq queue. 
    - create a table (where the users will use to create an order)

    OBS:
        - For all entities if the user is anonymous (customer) dont be able to edit crucial things like disable an Dish, disable an Order etc, only with some admin user can do this ok. 
        - anonoymous users can - create, ask to update, ask to cancel. 
        - admin user can do everything 

- DISH Entity 
    - Guid id 
    - String name 
    - String description
    - DataTime createdAt
    - DateTime updatedAt 
    - Double Price
    - Enum Category 
    - Bool isActive

- Order Entity 
    - Guid id
    - Guid TableId 
    - List<Dishes>
    - DateTime createdAt
    - DateTime createdAt
    - Enum Status

- Table Entity 
    - Guid id
    - String tableName
    - DateTime createdAt
    - DateTime updatedAt
    - Bool isActive

- Category Enum 
    - Starters
    - Hamburgers
    - Salads
    - Drinks
    - Dessert

- Status Enum 
    - Created
    - Draft
    - Cancelled 
    - Done
