using AutoFixture;
using Lab7.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5433;Database=Lab7;Username=postgres;Password=postgres";

var seedCount = builder.Configuration.GetValue<int?>("Seed:Count") ?? 10000;

builder.Services.AddDbContext<StudentContext>(options =>
    options.UseNpgsql(connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StudentContext>();
    context.Database.Migrate();
    
    if (!context.Students.Any())
    {
        SeedDatabase(context, seedCount);
    }
}

app.Run();

void SeedDatabase(StudentContext context, int count)
{
    var random = new Random();
    var fixture = new Fixture();
    var students = Enumerable.Range(1, count)
        .Select(i => fixture.Build<Lab7.Api.Models.Student>()
            .Without(s => s.Id)
            .With(s => s.Email, $"student{i:D5}@university.edu")
            .With(s => s.StudentNumber, $"STU{i:D6}")
            .With(s => s.CourseYear, random.Next(1, 5))
            .With(s => s.GPA, decimal.Round((decimal)(random.NextDouble() * 4.0), 2))
            .With(s => s.EnrollmentDate, DateTime.UtcNow.AddDays(-random.Next(0, 365 * 4)))
            .Create())
        .ToList();

    context.Students.AddRange(students);
    context.SaveChanges();
    Console.WriteLine($"Database seeded with {count} students.");
}

public partial class Program
{
}
