var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//using
app.UseWebSockets();

//Router
app.MapGet("/health", () => BaseController.Health());
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var user_id = int.Parse(context.Request.Headers["user_id"].ToString());
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var controller = new WebSocketController();
        await controller.HandleAsync(webSocket, context.RequestAborted, user_id);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});



app.Run();