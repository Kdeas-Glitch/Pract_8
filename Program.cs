using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using Pract_8_Demin.Models;
using Pract_8_Demin.Service;

using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

DeminDbContext _db = DBService.Instance.Context;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<List<User>>();

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var key = builder.Configuration["Jwt:Key"]
?? throw new Exception("Не задан секретный ключ JWT");

if (Encoding.UTF8.GetByteCount(key) < 32)
    throw new Exception("Ключ JWT слишком короткий");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ExampleApi"
        ,
            ValidateAudience = true,
            ValidAudience = "ExampleApiClient"
        ,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(key)
        ),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Получаем идентификатор проверенного JWT
                var jti = context.Principal?
                .FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    context.Fail("В JWT отсутствует jti");
                    return System.Threading.Tasks.Task.CompletedTask;
                }
                // Получаем общее хранилище через DI
                var revokedTokens = _db.RevokedTokens.FirstOrDefault(i => i.Jti == jti);
                // Если jti присутствует в списке, отклоняем JWT
                if (revokedTokens != null)
                {
                    context.Fail("JWT отозван");
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
