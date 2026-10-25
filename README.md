# Wireless Telemetry with ESP32, MQTT and Azure IoT Hub

This individual IoT project demonstrates how an ESP32 sends temperature and humidity measurements over WiFi and MQTT to a Raspberry Pi, which forwards the data to Azure IoT Hub.

## Architecture

```text
ESP32 (temperature and humidity sensor)
    |
    | WiFi / MQTT
    v
Raspberry Pi (Mosquitto MQTT broker)
    |
    | PiBridge (C# / .NET)
    v
Azure IoT Hub
    |
    v
Azure CLI event monitor
```

## Technologies

- ESP32 with temperature and humidity sensor
- WiFi and MQTT
- Raspberry Pi running Mosquitto
- C# / .NET and MQTTnet
- Azure IoT Hub
- Azure CLI

## MQTT Communication

The ESP32 publishes sensor measurements to:

`sensors/esp32-01/telemetry`

### Temperature Example

```json
{
  "deviceId": "esp32-01",
  "value": 22.8,
  "unit": "C"
}
```

### Humidity Example

```json
{
  "deviceId": "esp32-01",
  "value": 51,
  "unit": "%"
}
```

The gateway subscribes to `sensors/+/telemetry`, allowing messages from multiple sensor devices.

## Raspberry Pi Gateway

Mosquitto receives MQTT messages from the ESP32.

The `PiBridge` application reads the JSON messages and forwards them to Azure IoT Hub.

The gateway authenticates using its registered Azure device identity, `pi-bridge-01`, while preserving the original ESP32 device ID, measurement value and unit in the message payload.

The gateway source code is located in the `PiBridge/` directory.

## Configuration

The gateway uses the following environment variables:

- `MQTT_PASSWORD` - password for the local MQTT broker.
- `AZURE_IOT_CONNECTION_STRING` - Azure IoT Hub device connection string.

Credentials and connection strings must not be committed to GitHub.

## Running and Monitoring

The gateway runs as a systemd service on the Raspberry Pi.

### Check Gateway Status

```bash
sudo systemctl status pibridge.service
```

### View Gateway Logs

```bash
sudo journalctl -u pibridge.service -f
```

### Local MQTT Consumer

A local MQTT consumer can use `mosquitto_sub` to monitor incoming sensor measurements.

For example:

```bash
mosquitto_sub -h localhost -p 1883 -u esp32 -t 'sensors/+/telemetry' -v
```

The MQTT broker requires authentication. Supply the configured password securely when connecting.

### Azure Consumer

To monitor incoming telemetry in Azure IoT Hub using Azure CLI:

```bash
az iot hub monitor-events --hub-name Iot2026-Manuel --output table
```

## Verified Results

The complete telemetry flow has been successfully tested using real sensor measurements.

### Temperature Received in Azure

```yaml
event:
  origin: pi-bridge-01
  payload:
    deviceId: esp32-01
    unit: C
    value: 22.8
```

### Humidity Received in Azure

```yaml
event:
  origin: pi-bridge-01
  payload:
    deviceId: esp32-01
    unit: '%'
    value: 51
```

These results confirm that both temperature and humidity measurements are successfully transmitted from the ESP32 through the Raspberry Pi gateway to Azure IoT Hub.

## Repository Structure

- `PiBridge/` - C# source code for the MQTT-to-Azure gateway.
- `README.md` - project architecture, configuration, monitoring instructions and verified results.

## Security

MQTT passwords and Azure IoT Hub connection strings are stored outside the source code using environment variables. No credentials should be uploaded to the public GitHub repository.