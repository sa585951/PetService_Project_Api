var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5174/") // 指定允許的前端來源
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // 允許憑證
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSession();

app.UseCors(); //使用跨域請求
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
