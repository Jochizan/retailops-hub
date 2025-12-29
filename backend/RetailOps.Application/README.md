# Application Layer

This layer contains the core business logic of the RetailOps application. It is independent of external frameworks, UI, or database implementation details (following Clean Architecture principles).

## Key Components

### Services
Services implement the business use cases. They orchestrate domain objects and interact with the database via the `IRetailOpsDbContext` interface.

*   **`OrderService`**: The heart of the order processing system.
    *   **Responsibility**: Validates orders, reserves stock, and creates order records.
    *   **Transactionality**: Uses explicit database transactions to ensure that Order Creation and Stock Reservation happen atomically. If stock cannot be reserved, the order is not created.
    *   **Concurrency**: Uses Optimistic Concurrency Control (OCC) via `Inventory.RowVersion` to prevent race conditions when multiple users try to buy the last unit of an item simultaneously.
*   **`InventoryService`**: Handles read-only operations for inventory (viewing stock). Stock modifications are currently centralized in `OrderService` to strictly control the "Reservation" lifecycle.
*   **`ProductService`**: Manages product catalog retrieval and searching.

### DTOs (Data Transfer Objects)
Define the contracts for data input/output, decoupling the domain entities from the API contract.
*   `OrderDtos.cs`: Requests/Responses for Order operations.
*   `InventoryDto.cs`: View model for stock levels.

### Interfaces
*   **`IRetailOpsDbContext`**: An abstraction of the EF Core DbContext. This allows the Application layer to use the database without depending on the `Infrastructure` layer directly (Dependency Inversion). It exposes a `BeginTransactionAsync` method for services that need fine-grained transaction control.

## Business Rules Highlights

1.  **Stock Reservation**: You cannot sell what you don't have. Logic: `Available = OnHand - Reserved`.
2.  **Atomic Orders**: An order is either completely accepted (all items reserved) or completely rejected. No partial orders.
3.  **Attributes**: Product attributes are split between standard columns (e.g., Brand) and dynamic JSON (e.g., Manufacturer), handled seamlessly by the Domain entities.
