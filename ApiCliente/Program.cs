
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<MinimalDbContext>(p => p.UseInMemoryDatabase("InMemoryDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Clientes - Minimal API",
        Version = "v1",
        Description = "API utilizada a fim de testes para a empresa Emccamp, não tendo necessidades de adicionar o id , a api ja add"
    });
});

// Adicionando CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilitando o CORS
app.UseCors("AllowAnyOrigin");

app.MapGet("/clientes", async (MinimalDbContext context) =>
{
    return Results.Ok(await context.clientes.ToListAsync());
});

app.MapPost("/clients", async (Cliente cliente, MinimalDbContext context) =>
{

    var clienteComMesmoEmail = await context.clientes.FirstOrDefaultAsync(c => c.Email == cliente.Email);
    if (clienteComMesmoEmail != null)
        return Results.BadRequest("Já existe um cliente com o mesmo email.");


    await context.clientes.AddAsync(cliente);
    await context.SaveChangesAsync();

    return Results.Ok(cliente);
});


app.MapPut("/clientes/{id}", async (int id, Cliente cliente, MinimalDbContext context) =>
{
    var clienteDb = await context.clientes.FindAsync(id);

    if (clienteDb == null)
        return Results.NotFound();

    var clienteComMesmoEmail = await context.clientes.FirstOrDefaultAsync(c => c.Email == cliente.Email && c.Id != id);
    if (clienteComMesmoEmail != null)
        return Results.BadRequest("Já existe um cliente com o mesmo email.");

    clienteDb.Nome = cliente.Nome;
    clienteDb.Email = cliente.Email;

    await context.SaveChangesAsync();

    return Results.Ok(clienteDb);
});

app.MapDelete("/clientes/{id}", async (int id, MinimalDbContext context) =>
{
    var clienteDb = await context.clientes.FindAsync(id);

    if (clienteDb == null)
        return Results.NotFound();

    context.clientes.Remove(clienteDb);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class MinimalDbContext : DbContext
{
    public MinimalDbContext(DbContextOptions<MinimalDbContext> options)
        : base(options) { }

    public DbSet<Cliente> clientes { get; set; }
}
