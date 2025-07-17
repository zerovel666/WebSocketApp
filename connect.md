composer require textalk/websocket

## CONNECT FROM LARAVEL

public function webSocketSendMessage(Request $request)
{
    $data = json_encode([
        "user_id" => 1742,
        "message" => "STRING",
        "data" => [
            "block" => false,
            "lara" => 11
        ]
    ]);
    $client = new Client("URL/ws", [
        "headers" => [
            "user_id" => 2
        ],
    ]);
    $client->send($data);

    $client->receive();
    $client->close();

    return "Succefully";
}