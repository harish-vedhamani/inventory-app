using System;

namespace Playground.Domain
{
  public sealed class Product
  {
        public Guid Id { get; }
        public string Name { get; }
        public decimal Price { get; }
        public int Quantity { get; }

      public Product(Guid id, string name, decimal price, int quantity)
      {
          if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
          if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
          if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");
          if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative");

          Id = id;
          Name = name.Trim();
          Price = price;
          Quantity = quantity;
      }


      public Product WithPrice(decimal price) => new Product(Id, Name, price, Quantity);
      public Product WithQuantity(int quantity) => new Product(Id, Name, Price, quantity);
  }
}
