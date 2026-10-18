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
        SensorReading? sensor = JsonSerializer.Deserialize<SensorReading>(
            message,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (sensor != null)
        {
            Console.WriteLine("Enhet: " + sensor.DeviceId);
            Console.WriteLine("Värde: " + sensor.Value + " " + sensor.Unit);
            Console.WriteLine("-------------------");
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("Felaktigt JSON-meddelande.");
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