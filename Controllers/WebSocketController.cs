using System.Net.WebSockets;
using System.Text;
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
            if (!string.IsNullOrEmpty(message))
            {
                dynamic? obj = null;
                try
                {
                    obj = JsonConvert.DeserializeObject<dynamic>(message);
                }
                catch (JsonReaderException)
                {
                    continue;
                }

                if (obj == null)
                {
                    continue;
                }

                Console.WriteLine(obj);
                await HandleSendMessage(obj);
            }

            var response = Encoding.UTF8.GetBytes(message);

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
        int? userId = (int?)message.user_id;

        if (userId is int id)
        {
            if (conUsersWS.TryGetValue(id, out var socket) && socket.State == WebSocketState.Open)
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
        else
        {
            string json = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            foreach (var item in conUsersWS)
            {
                var socket = item.Value;

                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
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