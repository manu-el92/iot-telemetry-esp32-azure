using MQTTnet;
using System.Text.Json;

// Skapa MQTT-klienten
var factory = new MqttClientFactory();
var mqttClient = factory.CreateMqttClient();

// Vad ska hända när ett meddelande kommer?
mqttClient.ApplicationMessageReceivedAsync += e =>
{
    string message = e.ApplicationMessage.ConvertPayloadToString();

    try
    {
        using JsonDocument json = JsonDocument.Parse(message);

        string deviceId = json.RootElement.GetProperty("deviceId").GetString() ?? "";
        double value = json.RootElement.GetProperty("value").GetDouble();
        string unit = json.RootElement.GetProperty("unit").GetString() ?? "";

        Console.WriteLine("Enhet: " + deviceId);
        Console.WriteLine("Värde: " + value + " " + unit);
        Console.WriteLine("-------------------");
    }
    catch (Exception)
    {
        Console.WriteLine("Kunde inte läsa sensordata.");
    }

    return Task.CompletedTask;
};

// Inställningar för Mosquitto


var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .Build();

try
{


    await mqttClient.ConnectAsync(options);

    Console.WriteLine("Ansluten till Mosquitto!");

    // Lyssna på meddelanden från våra sensorer
    await mqttClient.SubscribeAsync("sensors/+/telemetry");
    Console.WriteLine("Väntar på sensordata...");

    // Håll programmet igång
    Console.ReadLine();

    await mqttClient.DisconnectAsync();
}
catch (Exception ex)
{
    Console.WriteLine("Fel: " + ex.Message);
}