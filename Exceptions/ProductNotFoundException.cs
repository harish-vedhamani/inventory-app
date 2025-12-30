using System;

namespace Playground.Exceptions
{
    public class ProductNotFoundException : Exception
    {
      public Guid ProductId { get; }

      public ProductNotFoundException(Guid id)
          : base($"Product with id '{id}' was not found.")
      {
          ProductId = id;
      }
    }
}
