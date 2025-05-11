var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache(); // �ϥΰO����֨��@�� session �x�s
//builder.Services.AddSession();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session �L���ɶ�
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//���\���ШD
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5174/") // ���w���\���e�ݨӷ�
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // ���\����
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSession();

app.UseCors(); //�ϥθ��ШD
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
