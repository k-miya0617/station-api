var builder = WebApplication.CreateBuilder(args);

// Cors�I�u�W�F�N�g���쐬����
var AllowLocalhost = "_allowLocalhost";
var AllowDeployServer = "_allowDeployServer";

// Cors�̏ڍׂ��`����
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowLocalhost,
        builder =>
        {
            builder.WithOrigins("http://localhost:13125")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    options.AddPolicy(name: AllowDeployServer,
        builder =>
        {
            builder.WithOrigins("http://192.168.1.125:13125")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Cors��K�p������
app.UseCors(AllowLocalhost);
app.UseCors(AllowDeployServer);

app.UseAuthorization();

app.MapControllers();

app.Run();
