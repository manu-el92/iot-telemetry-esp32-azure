using MQTTnet;
using System.Text.Json;

var azure = new AzureSender();
await azure.TestConnection();

// Skapa MQTT-klienten
var factory = new MqttClientFactory();
var mqttClient = factory.CreateMqttClient();

// Vad ska hända när ett meddelande kommer?
mqttClient.ApplicationMessageReceivedAsync += async e =>
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

            await azure.SendReading(sensor);
        }
    }
    catch (JsonException)
    {
        Console.WriteLine("Felaktigt JSON-meddelande.");
    }


};

// Inställningar för Mosquitto


var mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD");

if (string.IsNullOrWhiteSpace(mqttPassword))
{
    throw new InvalidOperationException("MQTT_PASSWORD saknas.");
}

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithCredentials("esp32", mqttPassword)
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