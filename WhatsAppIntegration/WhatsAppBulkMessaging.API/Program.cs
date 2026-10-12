using WhatsAppBulkMessaging.API.Configuration;
using WhatsAppBulkMessaging.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Friendly error responses for status codes without a body
app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    response.ContentType = "application/json";

    var message = response.StatusCode switch
    {
        400 => "Your request is invalid. Please check your input.",
        401 => "Authentication is required to access this resource.",
        403 => "You do not have permission to perform this action.",
        404 => "The requested resource was not found.",
        405 => "This HTTP method is not allowed for this endpoint.",
        409 => "The request conflicts with the current data.",
        429 => "Too many requests. Please try again later.",
        _ => "The request could not be completed."
    };

    await response.WriteAsJsonAsync(new
    {
        StatusCode = response.StatusCode,
        Message = message
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();