using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

class WebSocketController
{
    public async Task HandleAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            dynamic? obj = JsonConvert.DeserializeObject<dynamic>(message);

            if (obj == null)
            {
                Console.WriteLine("Ошибка: не удалось распарсить JSON");
                return;
            }
            Console.WriteLine(obj);
            var response = Encoding.UTF8.GetBytes(message);

            await socket.SendAsync(
                new ArraySegment<byte>(response),
                result.MessageType,
                result.EndOfMessage,
                cancellationToken
            );
        }
    }
}