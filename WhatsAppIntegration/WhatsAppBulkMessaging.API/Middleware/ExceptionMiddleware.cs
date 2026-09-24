//using System.Net;
//using System.Text.Json;

//namespace WhatsAppBulkMessaging.API.Middleware;

//public class ExceptionMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<ExceptionMiddleware> _logger;

//    public ExceptionMiddleware(
//        RequestDelegate next,
//        ILogger<ExceptionMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(
//                ex,
//                "Unhandled exception occurred. Path: {Path}, Method: {Method}",
//                context.Request.Path,
//                context.Request.Method);

//            await HandleExceptionAsync(context);
//        }
//    }

//    private static async Task HandleExceptionAsync(
//        HttpContext context)
//    {
//        context.Response.ContentType = "application/json";
//        context.Response.StatusCode =
//            (int)HttpStatusCode.InternalServerError;

//        var response = new
//        {
//            StatusCode = context.Response.StatusCode,
//            Message = "An unexpected error occurred. Please try again later."
//        };

//        await context.Response.WriteAsync(
//            JsonSerializer.Serialize(response));
//    }
//}