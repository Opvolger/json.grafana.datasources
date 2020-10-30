# JSON.Grafana.datasources

Deze tool is ontstaan omdat ik op mijn werk in grafana een dashboard wilde hebben uit verschillende bronnen welke elke nacht geupdaten moet worden.
We hadden 3 waarheden en deze tool maakte een overzicht of al deze waarheden nog overeenkwamen.

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
  jsongrafana:
    image: opvolger/json.grafana.datasources:latest
    container_name: jsongrafanadatasources
    ports:
      - 8080:80
    networks:
      default:
        aliases:
          - jsongrafana
    volumes:
        - $PWD/data:/home/data
    restart: always
```
