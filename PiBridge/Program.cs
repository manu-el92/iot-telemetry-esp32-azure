using MQTTnet;

// Skapa MQTT-klienten
var factory = new MqttClientFactory();
var mqttClient = factory.CreateMqttClient();

// Inställningar för Mosquitto
string mqttHost = "192.168.1.100";
int mqttPort = 1883;

var options = new MqttClientOptionsBuilder()
    .WithTcpServer(mqttHost, mqttPort)
    .Build();

try
{
    Console.WriteLine("Försöker ansluta till Mosquitto...");

    await mqttClient.ConnectAsync(options);

    Console.WriteLine("Ansluten till Mosquitto!");
}
catch (Exception ex)
{
    Console.WriteLine("Kunde inte ansluta: " + ex.Message);
}