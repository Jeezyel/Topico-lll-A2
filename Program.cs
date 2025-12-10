using A2.Data;
using A2.Models;
using A2.Service;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<A2Context>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpClient();

builder.Services.AddScoped<IGeocodingService, NominatimGeocodingService>();

builder.Services.AddScoped<IWeatherService, OpenWeatherService>();

// Add services to the container.


// Configurar a autenticao JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddControllers().AddJsonOptions(x =>
{
    // Esta  a linha mgica que impede o loop infinito
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()  // Permite qualquer origem (React)
               .AllowAnyMethod()  // Permite GET, POST, PUT, DELETE...
               .AllowAnyHeader(); // Permite enviar o Token no cabealho
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Reset admin password on startup in development for consistency
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<A2Context>();
            
            // 1. Reset Admin User
            var adminUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@logifleet.com");
            if (adminUser != null)
            {
                bool needsUpdate = true;
                try { needsUpdate = !BCrypt.Net.BCrypt.Verify("123456", adminUser.SenhaHash); }
                catch { needsUpdate = true; } // Hash is invalid format

                if(needsUpdate)
                {
                    adminUser.SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    await context.SaveChangesAsync();
                    Console.WriteLine(">>>> Senha do usuário 'admin@logifleet.com' resetada para '123456'.");
                }
            }

            // 2. Reset Test User
            var testUser = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            if (testUser != null)
            {
                bool needsUpdate = true;
                try { needsUpdate = !BCrypt.Net.BCrypt.Verify("123456", testUser.SenhaHash); }
                catch { needsUpdate = true; } // Hash is invalid format

                if(needsUpdate)
                {
                    testUser.SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456");
                    await context.SaveChangesAsync();
                    Console.WriteLine(">>>> Senha do usuário 'test@example.com' resetada para '123456'.");
                }
            }

            // 3. Reset all Client Users
            var clientRole = await context.Roles.FirstOrDefaultAsync(r => r.Nome == "Cliente");
            if (clientRole != null)
            {
                var clientUsers = await context.Usuarios.Where(u => u.RoleId == clientRole.Id).ToListAsync();
                foreach (var clientUser in clientUsers)
                {
                    bool needsUpdate = true;
                    try { needsUpdate = !BCrypt.Net.BCrypt.Verify("123456", clientUser.SenhaHash); }
                    catch { needsUpdate = true; } // Hash is invalid format

                    if (needsUpdate)
                    {
                        clientUser.SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456");
                        Console.WriteLine($">>>> Senha do usuário cliente '{clientUser.Email}' resetada para '123456'.");
                    }
                }
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocorreu um erro ao tentar resetar senhas de desenvolvimento.");
        }
    }
}

app.UseHttpsRedirection();

app.UseRouting(); // Adiciona o middleware de roteamento

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controllers para os endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();