using System;
using System.Globalization;
using System.Threading.Tasks;
using Playground.Domain;
using Playground.Services;
using Playground.Exceptions;

class Program
{
	static async Task<int> Main(string[] args)
	{
	IInventoryService inventory = new PlainInventoryService();
		Console.WriteLine("Inventory Console");

		while (true)
		{
			PrintMenu();
			Console.Write("Select> ");
			var choice = Console.ReadLine()?.Trim();
			try
			{
				switch (choice)
				{
					case "1": await AddProductAsync(inventory); break;
					case "2": await ListProductsAsync(inventory); break;
					case "3": await SearchProductsAsync(inventory); break;
					case "4": await UpdatePriceAsync(inventory); break;
					case "5": await UpdateQuantityAsync(inventory); break;
					case "6": await DeleteProductAsync(inventory); break;
					case "7": await ShowTotalsAsync(inventory); break;
					case "0": Console.WriteLine("Exiting"); return 0;
					default: Console.WriteLine("Unknown option"); break;
				}
			}catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}
			Console.WriteLine();
		}
	}

	static void PrintMenu()
	{
		Console.WriteLine("\n1) Add product");
		Console.WriteLine("2) List products");
		Console.WriteLine("3) Search products");
		Console.WriteLine("4) Update price");
		Console.WriteLine("5) Update quantity");
		Console.WriteLine("6) Delete product");
		Console.WriteLine("7) Show totals");
		Console.WriteLine("0) Exit");
	}

	static async Task AddProductAsync(IInventoryService inventory)
	{
    Console.Write("Name: ");
    var name = Console.ReadLine() ?? string.Empty;
		Console.Write("Price: ");
		var priceS = Console.ReadLine();
		Console.Write("Quantity: ");
		var qtyS = Console.ReadLine();

		if (!decimal.TryParse(priceS, out var price))
    throw new ArgumentException("Price must be a decimal number");

    if (!int.TryParse(qtyS, out var qty))
    throw new ArgumentException("Quantity must be an integer");

		var product = new Product(Guid.NewGuid(), name, price, qty);
		await inventory.AddProductAsync(product);
		Console.WriteLine($"Added {product.Name} (Id: {product.Id})");
	}

	static async Task ListProductsAsync(IInventoryService inventory)
	{
		var items = await inventory.ListProductsAsync();
		Console.WriteLine("Products:");

    foreach (var p in items)
    {
      Console.WriteLine($"{p.Id} | {p.Name} | Price: {p.Price} | Qty: {p.Quantity}");
    }
	}

	static async Task SearchProductsAsync(IInventoryService inventory)
	{
    Console.Write("Search query: ");
    var q = Console.ReadLine() ?? string.Empty;
		var results = await inventory.SearchProductsAsync(q);

    Console.WriteLine("Matches:");
		foreach (var p in results)
		{
			Console.WriteLine($"{p.Id} | {p.Name} | Price: {p.Price} | Qty: {p.Quantity}");
		}
	}

	static async Task UpdatePriceAsync(IInventoryService inventory)
	{
		Console.Write("Product Id: ");
		var idS = Console.ReadLine();

    Console.Write("New price: ");
		var priceS = Console.ReadLine();

    if (!Guid.TryParse(idS, out var id)) throw new ArgumentException("Invalid GUID");
		if (!decimal.TryParse(priceS, out var price)) throw new ArgumentException("Price must be a decimal number");

    await inventory.UpdatePriceAsync(id, price);
		Console.WriteLine("Price updated");
	}

	static async Task UpdateQuantityAsync(IInventoryService inventory)
	{
		Console.Write("Product Id: ");
		var idS = Console.ReadLine();

    Console.Write("New quantity: ");
		var qS = Console.ReadLine();

    if (!Guid.TryParse(idS, out var id)) throw new ArgumentException("Invalid GUID");
		if (!int.TryParse(qS, out var q)) throw new ArgumentException("Quantity must be integer");

    await inventory.UpdateQuantityAsync(id, q);
		Console.WriteLine("Quantity updated");
	}

	static async Task DeleteProductAsync(IInventoryService inventory)
	{
		Console.Write("Product Id: ");
		var idS = Console.ReadLine();

    if (!Guid.TryParse(idS, out var id)) throw new ArgumentException("Invalid GUID");

    await inventory.DeleteProductAsync(id);
		Console.WriteLine("Deleted");
	}

	static async Task ShowTotalsAsync(IInventoryService inventory)
	{
		var total = await inventory.GetTotalInventoryValueAsync();
		var highest = await inventory.GetHighestPricedProductAsync();

    Console.WriteLine($"Total inventory value: {total}");

    if (highest is null) Console.WriteLine("No products");
    else Console.WriteLine($"Highest priced product: {highest.Name} ({highest.Price})");
	}
}
