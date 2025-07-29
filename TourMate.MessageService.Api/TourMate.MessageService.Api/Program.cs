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

builder.WebHost.UseUrls("http://0.0.0.0:5001");

// Register services
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MessageService API", Version = "v1" });
    c.AddServer(new OpenApiServer { Url = "/message-service" });
});

// Repository & service DI
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();

builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IUserServiceGrpcClient, UserServiceGrpcClient>();

builder.Services.AddDbContext<TourMateMessageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGrpc();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

var app = builder.Build();

app.UsePathBase("/message-service");

app.UseRouting();


// ✅ Middleware xử lý CORS động – hỗ trợ origin tự động và credentials
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();

    if (!string.IsNullOrEmpty(origin))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
        context.Response.Headers["Access-Control-Allow-Headers"] =
            "Origin, X-Requested-With, Content-Type, Accept, Authorization, x-signalr-user-agent";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
    }

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }

    await next();
});



// Swagger
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
