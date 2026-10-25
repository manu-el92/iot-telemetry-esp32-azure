#include <WiFi.h>
#include <PubSubClient.h>
#include <DHT.h>

// DHT11
#define DHTPIN 4
#define DHTTYPE DHT11

DHT dht(DHTPIN, DHTTYPE);

// Wi-Fi
const char* wifi_ssid = " ";
const char* wifi_password = " ";

// MQTT - Raspberry Pi
const char* mqtt_server = "172.20.10.2";
const int mqtt_port = 1883;
const char* mqtt_user = "esp32";
const char* mqtt_password = " ";

WiFiClient espClient;
PubSubClient mqtt(espClient);

void connectWiFi() {
  WiFi.begin(wifi_ssid, wifi_password);

  Serial.print("Ansluter till Wi-Fi");

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("\nWi-Fi anslutet!");
  Serial.println(WiFi.localIP());
}

void connectMQTT() {
  while (!mqtt.connected()) {
    Serial.print("Ansluter till MQTT... ");

    if (mqtt.connect("esp32-01", mqtt_user, mqtt_password)) {
      Serial.println("Ansluten!");
    } else {
      Serial.print("Misslyckades, kod: ");
      Serial.println(mqtt.state());
      delay(5000);
    }
  }
}

void setup() {
  Serial.begin(115200);
  dht.begin();

  connectWiFi();
  mqtt.setServer(mqtt_server, mqtt_port);
}

void loop() {
  if (!mqtt.connected()) {
    connectMQTT();
  }

  mqtt.loop();

  float temperature = dht.readTemperature();
  float humidity = dht.readHumidity();

  if (!isnan(temperature) && !isnan(humidity)) {
    char tempJson[100];
    char humidityJson[100];

    snprintf(tempJson, sizeof(tempJson),
             "{\"deviceId\":\"esp32-01\",\"value\":%.1f,\"unit\":\"C\"}",
             temperature);

    snprintf(humidityJson, sizeof(humidityJson),
             "{\"deviceId\":\"esp32-01\",\"value\":%.1f,\"unit\":\"%%\"}",
             humidity);

    mqtt.publish("sensors/esp32-01/telemetry", tempJson);
    mqtt.publish("sensors/esp32-01/telemetry", humidityJson);

    Serial.println(tempJson);
    Serial.println(humidityJson);
  } else {
    Serial.println("Kunde inte läsa DHT11!");
  }

  delay(5000);
}