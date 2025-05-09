using EMC_DotNetCore_Report_Backend.Class;
using EMC_DotNetCore_Report_Backend.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMysqlConnection, mysqlConnection>(); // Add MySQL connection service

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowOrigins",
                    policy =>
                    {
                        policy.WithOrigins("*")
                        .AllowAnyHeader()
                        .WithMethods();
                    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigins"); // Use the CORS policy

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
