using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class WebSocketController
{
    public static Dictionary<int, WebSocket> conUsersWS = new();
    public async Task HandleAsync(WebSocket socket, CancellationToken cancellationToken, int user_id)
    {
        WebSocketManager(user_id, socket, true);

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
            await HandleSendMessage(obj);

            await socket.SendAsync(
                new ArraySegment<byte>(response),
                result.MessageType,
                result.EndOfMessage,
                cancellationToken
            );
        }
        WebSocketManager(user_id, socket, false);
    }

    public static async Task HandleSendMessage(dynamic message)
    {
        int userId = (int)message.user_id;

        if (conUsersWS.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
        {
            string json = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
        else
        {
            Console.WriteLine("Пользователь не найден или отключён");
        }
    }


    public static void WebSocketManager(int user_id, WebSocket webSocket, bool isConnect)
    {
        if (isConnect)
        {
            conUsersWS[user_id] = webSocket;
        }
        else
        {
            if (conUsersWS.ContainsKey(user_id))
            {
                conUsersWS.Remove(user_id);
            }
        }
    }

}