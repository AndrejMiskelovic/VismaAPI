using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VismaAPI.Controllers;
using VismaAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); //will show Role enum as text instead int in swagger 

builder.Services.AddDbContext<EmployeeContext>(options => options.UseInMemoryDatabase("VismaEmployeesDB"));

var app = builder.Build();
EmployeeContext.AddCustomerData(app); //test employees
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//
app.UseDeveloperExceptionPage();
//
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
