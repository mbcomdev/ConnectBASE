# Projekt ConnectBASE

REST-API für das Warenwirtschaftssystem microtech büro+. Zugriff lesend wie schreibend auf relevante Daten des Warenwirtschaftsprogramms über eine moderne REST-API mittels sicherer Authentifizierungsmechanismen.

ConnectBASE stellt folgende Features bereit:
- REST Schnittstelle für lesenden und schreibenden Zugriff auf die Microtech büro+ Datenbank
- Authentifizierung mittels JSON Web Token
- Schnittstellendokumentation mittels Swagger
- Scheduling von Anfragen
- Body-Validierung von eingehenden Anfragen

## Für Entwickler

Damit ConnectBASE korrekt ausgeführt und gestartet werden kann, müssen gültige Anmeldedaten in der AppSettings Datei hinterlegt werden (siehe Kapitel Konfiguration).
Beim erstmaligen Ausführen legt ConnectBase Validierungs- und Schemadateien lokal an. Dies kann je nach Verbindungsart einige Zeit in Anspruch nehmen.
Es besteht die Möglichkeit mittels Anwendungsargument `-cleanupDirectories true` bei Anwendungsstart alle Validierungs- und Schemadatein zu löschen. Dadurch werden diese neu erstellt.

### Konfiguration

Im Projektordner `Server/Helper` befindet sich die Konfigurationsdatei `AppSettings.cs`.
Diese kann je nach Bedarf angeapsst werden.
```
        // Secret for our JSON-WEB-TOKEN
        // We should change this to an external source (azure key vault, ...)
        private string _secret = "we-should-change-this";

        // We need this user for creating our scheme and validation files on startup
        // We should change this to a real service user so we don´t block other users connection
        private readonly User _serviceUser = new User("username", "password", MANDANT);
        
        // Initial mandant setup 
        public static readonly string MANDANT = "1";

        // Configuration data for our scheduler to ensure user can only perform action after action
        // How often should we retry after waiting 
        private readonly int retryCount = 3;
        // How long do we wait after one retry (ms)
        private readonly int waitUntilRetry = 1000;
```
