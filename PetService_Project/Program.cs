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
using PetService_Project_Api.Service.Cart;
using PetService_Project_Api.Service.Service;
using PetService_Project_Api.Hubs;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetService_Project_Api.WebSockets;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using PetService_Project.Partials;
using PetService_Project_Api.Service.OrderEmail;
using Microsoft.Extensions.DependencyInjection;
using PetService_Project_Api.Options;
using PetService_Project_Api.Service.Payment;

var builder = WebApplication.CreateBuilder(args);

// ✅ 加入資料庫連線
builder.Services.AddDbContext<dbPetService_ProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// ✅ 設定 Identity 使用者登入管理
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<dbPetService_ProjectContext>()
    .AddDefaultTokenProviders();

// ✅ 設定密碼規則
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
});
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache(); // 使用記憶體快取作為 session 儲存
//builder.Services.AddSession();

// ✅ JWT 驗證設定
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session 過期時間
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ CORS 設定（允許 Vue 前端跨域）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ✅ 加入 SignalR 服務
builder.Services.AddSignalR();


// ✅ 記憶體或 Redis 快取
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IEmailService, SendGridService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
//SMTP 寄送訂單通知信件
builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<SmtpOptions>(opt =>
{
    builder.Configuration.GetSection("Smtp").Bind(opt);
    opt.User = builder.Configuration["Smtp:User"] ?? opt.User;
    opt.Password = builder.Configuration["Smtp:Password"] ?? opt.Password;
    opt.SenderEmail = builder.Configuration["Smtp:SenderEmail"] ?? opt.SenderEmail;
});
builder.Services.AddScoped<IOrderNotificationEmailService,SmtpEmailService>();
//Ecpay設置
builder.Services.Configure<EcpayOptions>(
    builder.Configuration.GetSection("Ecpay"));
builder.Services.AddScoped<IEcpayService, EcpayService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));
builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
{
    opt.TokenLifespan = TimeSpan.FromMinutes(20);
});

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

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddHostedService<MemberSourceBroadcaster>();

var app = builder.Build();

app.UseWebSockets();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseCors("AllowVueClient"); //¨Ï¥Î¸ó°ì½Ð¨D
// ✅ 中介軟體設定
app.UseSession();


app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
    context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.Map("/ws/membersource", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var config = app.Services.GetRequiredService<IConfiguration>(); // 取得 config
        var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
        await WebSocketHandler.Handle(context, config, scopeFactory); // 傳入 context, config, scopeFactory
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});


app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// 初始化角色 (同步等待)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await InitializeRolesAsync(roleManager);
    await AssignUserRoleToExistingAdminAsync(userManager);
}

await app.RunAsync();

async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "User" }; // Define your roles

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                Console.Error.WriteLine($"Error creating role {roleName}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
            else
            {
                Console.WriteLine($"Role {roleName} created successfully.");
            }
        }
        else
        {
            Console.WriteLine($"Role {roleName} already exists.");
        }
    }
}

async Task AssignUserRoleToExistingAdminAsync(UserManager<ApplicationUser> userManager)
{
    // 建立預設 admin 使用者
    //var adminEmail = "chris@skz.com";
    
    string[] adminEmails = { "chris@skz.com", "fuen41t2@gmail.com" }; // 指定多個管理員電子郵件

    foreach(var adminEmail in adminEmails)
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(adminUser, "Admin@123");
            if (!createResult.Succeeded)
            {
                Console.Error.WriteLine($"Error creating default admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return; // 如果建立失敗，直接返回
            }
            Console.WriteLine($"Default admin user '{adminEmail}' created successfully.");
        }
        // 加入 Admin 角色
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                Console.Error.WriteLine($"Error adding user '{adminEmail}' to role 'Admin': {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
            }
            else
            {
                Console.WriteLine($"User '{adminEmail}' added to role 'Admin' successfully.");
            }
        }
        else
        {
            Console.WriteLine($"User '{adminEmail}' is already in role 'Admin'.");
        }
    }
}


