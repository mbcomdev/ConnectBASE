# Projekt ConnectBASE

REST-API für das Warenwirtschaftssystem microtech büro+. Zugriff lesend wie schreibend auf relevante Daten des Warenwirtschaftsprogramms über eine moderne REST-API mittels sicherer Authentifizierungsmechanismen.

ConnectBASE stellt folgende Features bereit:
- REST Schnittstelle für lesenden und schreibenden Zugriff auf die Microtech büro+ Datenbank
- Authentifizierung mittels API-Key
- Schnittstellendokumentation mittels Swagger
- Scheduling von Anfragen
- Body-Validierung von eingehenden Anfragen

## Einschränkungen

**Für folgende Tabellen können keine neuen Objekte angelegt werden:**
- Projekte
- Vorgangsposition
- Vorgang

**Folgende Objekte können nicht geschrieben oder geändert werden:**
- Dokument
- Bild
- Seriennummer
- Lagerbewegungen
- Wandeln
- Zahlungsverkehr
- Offene Posten
- Dublettensuche
- Banking
- Blobs
- String Lists
- Beträge
- Events
- Nachrichten
- Drucken
- Layouts

**Löschen von Daten über die Schnittstelle ist nicht möglich.**


## Für Entwickler

Damit ConnectBASE korrekt ausgeführt und gestartet werden kann, müssen gültige Anmeldedaten in der AppSettings Datei hinterlegt werden (siehe Kapitel Konfiguration).
Beim erstmaligen Ausführen legt ConnectBase Validierungs- und Schemadateien lokal an. Dies kann je nach Verbindungsart einige Zeit in Anspruch nehmen.
Es besteht die Möglichkeit mittels Anwendungsargument `-cleanupDirectories true` bei Anwendungsstart alle Validierungs- und Schemadatein zu löschen. Dadurch werden diese neu erstellt.

### Konfiguration

Im Projektordner befindet sich die Konfigurationsdatei `AppSettings.json`.
Diese kann je nach Bedarf angeapsst werden.
