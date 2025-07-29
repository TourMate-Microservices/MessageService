using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using TourMate.MessageService.Repositories.Context;
using TourMate.MessageService.Repositories.IRepositories;
using TourMate.MessageService.Repositories.Repositories;
using TourMate.MessageService.Services.GrpcClients;
using TourMate.MessageService.Services.IServices;
using TourMate.MessageService.Services.Services;
using TourMate.SignalRHub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost.UseUrls("http://0.0.0.0:5001");

// Đăng ký CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins(
            "http://localhost:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddSignalR();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MessageService API",
        Version = "v1"
    });

    // Thêm dòng này để Swagger hiểu base URL là /user-service
    c.AddServer(new OpenApiServer
    {
        Url = "/message-service"
    });
});





builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();

builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();


// Đăng ký DbContext
builder.Services.AddDbContext<TourMateMessageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add gRPC client
builder.Services.AddScoped<IUserServiceGrpcClient, UserServiceGrpcClient>();

// Add gRPC server and UserGrpcService
builder.Services.AddGrpc();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

var app = builder.Build();

app.UsePathBase("/message-service");

app.UseRouting();

app.UseCors("AllowReactApp");


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/message-service/swagger/v1/swagger.json", "MessageService API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<AppHub>("/appHub");
    endpoints.MapControllers();
});

app.MapControllers();

app.Run();
