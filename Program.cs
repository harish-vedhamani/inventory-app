using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Playground.Services;
using Playground.Data;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var mysqlConn = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(mysqlConn))
{
		throw new InvalidOperationException("Connection string 'DefaultConnection' is required in configuration.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseMySql(mysqlConn, ServerVersion.AutoDetect(mysqlConn)));

builder.Services.AddScoped<IInventoryService, EfInventoryService>();
builder.Services.AddScoped<IAuthService, EfAuthService>();
builder.Services.AddControllers();

var jwtKey = configuration.GetValue<string>("Jwt:Key");
var jwtIssuer = configuration.GetValue<string>("Jwt:Issuer");
if (!string.IsNullOrEmpty(jwtKey))
{
	var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
	builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = "Bearer";
		options.DefaultChallengeScheme = "Bearer";
	}).AddJwtBearer("Bearer", opts =>
	{
		opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtIssuer,
			ValidAudience = jwtIssuer,
			IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
		};
	});

	builder.Services.AddAuthorization(options =>
	{
		options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	});
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseMiddleware<Playground.Middleware.ExceptionMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
