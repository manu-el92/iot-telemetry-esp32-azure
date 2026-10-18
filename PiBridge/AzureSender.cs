using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Text.Json;
public class AzureSender
{
    public async Task TestConnection()
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("AZURE_IOT_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Azure-anslutningssträngen saknas.");
            return;
        }

        try
        {
            using var deviceClient = DeviceClient.CreateFromConnectionString(
                connectionString,
                TransportType.Mqtt);

            await deviceClient.OpenAsync();

            Console.WriteLine("Ansluten till Azure IoT Hub!");

            await deviceClient.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Azure-fel: " + ex.Message);
        }
    }

    public async Task SendReading(SensorReading sensor)
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("AZURE_IOT_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Azure-anslutningssträngen saknas.");
            return;
        }

        try
        {
            using var client = DeviceClient.CreateFromConnectionString(
                connectionString,
                TransportType.Mqtt);

            await client.OpenAsync();

            // Gör om sensordatan till JSON
            string json = JsonSerializer.Serialize(sensor, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Skapa meddelandet som ska skickas till Azure
            using var message = new Message(Encoding.UTF8.GetBytes(json));
            message.ContentType = "application/json";
            message.ContentEncoding = "utf-8";

            await client.SendEventAsync(message);

            Console.WriteLine("Skickat till Azure: " + sensor.DeviceId);

            await client.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Kunde inte skicka till Azure: " + ex.Message);
        }
    }
}