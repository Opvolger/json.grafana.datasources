# JSON.Grafana.datasources

Deze tool is ontstaan omdat ik op mijn werk in grafana een dashboard wilde hebben uit verschillende bronnen welke elke nacht geupdaten moet worden.
We hadden 3 waarheden en deze tool maakte een overzicht of al deze waarheden nog overeenkwamen.

## Werking

Deze tool kan aangeroepen worden met de SimpleJson plugin binnen Grafana [link](https://grafana.com/grafana/plugins/grafana-simple-json-datasource?src=grafana_plugin_list)

installeren op de grafana machine:

```bash
grafana-cli plugins install grafana-simple-json-datasource
```

### Enkele bron

TODO

### Meerdere bronnen

Met deze tool is het mogelijk om onder een noemer (in ons geval een landschap), meerdere key-value lijsten op te sturen. De keys worden bij elkaar gezet en de values worden in een kolom gezet. Een voorbeeld:

input:

lijst Kolom A voor noemer TEST
| key  | value  |
|---|---|
| abc-001  | 1 |
| abc-003  | 1 |
| abc-004  | 2 |

lijst Kolom B voor noemer TEST
| key  | value  |
|---|---|
| abc-001  | A |
| abc-002  | B |
| abc-003  | C |
| abc-004  | C |

lijst Kolom C voor noemer TEST
| key  | value  |
|---|---|
| abc-001  | 12 |
| abc-002  | 14 |
| abc-003  | 17 |
| abc-004  | 12 |
| abc-005  | 12 |

geeft voor noemer TEST als output (mits geconfigureerd):

| key  | A | B | C |
|---|---|---|---|
| abc-001  | 1 | A | 12 |
| abc-002  |   | B | 14 |
| abc-003  | 1 | C | 17 |
| abc-004  | 2 | C | 12 |
| abc-005  | 2 |   | 12 |

Dit is een voorbeeld van 1 noemer. maar je kan er dus meerdere hebben. Dus meerdere tabellen kan je zo genereren.

## configureren

Hier een voorbeeld van een docker-compose file hoe je deze tool kan draaien

```yaml
version: '2'

services:
  grafana:
    image: grafana/grafana:6.5.0
    container_name: grafana
    ports:
      - 8182:3000
    networks:
      default:
        aliases:
          - grafana      
  jsongrafana:
    image: opvolger/json.grafana.datasources:latest
    container_name: jsongrafanadatasources
    ports:
      - 8181:80
    networks:
      default:
        aliases:
          - jsongrafana
    volumes:
        - $PWD/data:/home/data
    restart: always
```

installeer plugin:

```bash
grafana-cli plugins install grafana-simple-json-datasource
```

Aanmaken DataSource in grafana

zet deze naar de "json.grafana.datasources" http://jsongrafana:80/simpeljson

## Voorbeeld (uit meerdere bronnen)

We willen uit 3 bronnen data halen en in 1 overzicht hebben, we zullen dus eerst het overzicht moeten maken, dit kan met:

### Aanmaken table/info

Post de volgende info in [storedatakeyvalue/set_info](http://localhost:8181/storedatakeyvalue/set_info), dit kan via [swagger](http://localhost:8181/swagger)

```json
{
  "name": "test",
  "info": {
    "description": "Overzicht voor test",
    "type": "key_value"
  },
  "table": [
    {
        "jsonvalue":  "key",
        "type":  "string",
        "text":  "Machinename"
    },
    {
        "jsonvalue":  "bron1_bool",
        "type":  "bool",
        "text":  "Bron1 bool"
    },
    {
        "jsonvalue":  "bron1_time",
        "type":  "time",
        "text":  "Bron1 tijd"
    },
    {
        "jsonvalue":  "bron2_bool",
        "type":  "bool",
        "text":  "Bron2 Bool"
    },
    {
        "jsonvalue":  "bron3_string",
        "type":  "string",
        "text":  "Bron3 string"
    }
  ]
}
```

Dit zal in de data-folder een dit test aanmaken met 2 files erin, table.json en info.json

### Data sturen

Post de volgende info in [storedatakeyvalue/send_data](http://localhost:8181/storedatakeyvalue/send_data), dit kan via [swagger](http://localhost:8181/swagger)

4x dus

```json
{
  "subject": "bron1_bool",
  "name": "test",
  "json_data": { "machine1": true , "machine2": true , "machine3": false , "machine5": true } 
}
```

```json
{
  "subject": "bron1_time",
  "name": "test",
  "json_data": { "machine1": "2020-10-27T21:24:31.78Z" , "machine3": "2020-10-29T23:24:32.78Z",  "machine4": "2020-10-30T22:24:34.78Z", "machine5": "2020-09-01T21:24:36.78Z" } 
}
```

```json
{
  "subject": "bron2_bool",
  "name": "test",
  "json_data": { "machine1": false , "machine2": false , "machine3": false , "machine4": true, "machine5": true } 
}
```

```json
{
  "subject": "bron3_string",
  "name": "test",
  "json_data": { "machine1": "heel goed" , "machine2": "erg fout" , "machine3": "welkom" , "machine4": "iets anders", "machine5": "iets" }
}
```

### Dashboard maken

Je kan nu in Grafana een dashboard maken kies table ipv timeserie en natuurlijk de SimpleJson Als bron. Kies test en zie je dashboard.

## TODO

Als je info wegschrijft via API worden in de info.json en table.json enums als getallen weggeschreven ipv enum-strings
