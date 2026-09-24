//using Serilog;
//using WhatsAppBulkMessaging.API.Configuration;
//using WhatsAppBulkMessaging.API.Middleware;

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File(
//        "Logs/whatsapp-api-.log",
//        rollingInterval: RollingInterval.Day,
//        retainedFileCountLimit: 30,
//        shared: true)
//    .CreateLogger();

//var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseSerilog();

//builder.Services.AddControllers();

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.Services.AddApplicationServices(builder.Configuration);

//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using WhatsAppBulkMessaging.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();