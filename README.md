A tiny, practical console app that models products and a simple inventory

What it does

- Let you add, list, search, update (price/quantity), and delete products.
- Calculates total inventory value and shows the highest-priced product.

Key design notes

- Product is immutable with: Id (Guid), Name, Price (decimal), Quantity (int).
- The service exposes async methods (Task / Task<T>) so we can swap in a real async data store later.
- Current implementation is an in-memory, plain service (`PlainInventoryService`) using LINQ for queries.
