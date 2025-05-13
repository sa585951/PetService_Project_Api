using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PetService_Project.Models;
using PetService_Project_Api.Models;
using PetService_Project_Api.Service;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<dbPetService_ProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// 設定 Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<dbPetService_ProjectContext>()
    .AddDefaultTokenProviders();
// Program.cs - 配置 Identity 密碼規則
builder.Services.Configure<IdentityOptions>(options =>
{
    // 密碼設置
    options.Password.RequiredLength = 4; // 最小長度設為4
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // 不需要特殊字符
});
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache(); // 使用記憶體快取作為 session 儲存
//builder.Services.AddSession();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session 過期時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//允許跨域請求
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vue 開發時的網址
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddScoped<IEmailService, SendGridService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromMinutes(20);
});

// 可切換的注入（用 Redis 或 Memory）
var useRedis = builder.Configuration.GetValue<bool>("UseRedis");

if (useRedis)
{
    builder.Services.AddScoped<ICodeService, RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ICodeService, MemoryCacheService>();
}
// 設定 JWT
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // 使用 Convert.FromBase64String 來處理 Base64 字符串
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))

    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSession();

app.UseCors("AllowVueClient"); //¨Ï¥Î¸ó°ì½Ð¨D
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
    context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
    await next();
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();
