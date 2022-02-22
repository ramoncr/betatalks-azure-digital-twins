# Queries

## Basics
```sql
SELECT * FROM digitaltwins

SELECT * FROM digitaltwins WHERE $dtId = 'BETABIT-ROTTERDAM'

SELECT $dtId, name, surfaceArea, address.city, geolocation.alt FROM digitaltwins WHERE name = 'Betabit Rotterdam'

SELECT * FROM digitaltwins WHERE IS_OF_MODEL('dtmi:ramon:floor;1')

SELECT * FROM digitaltwins WHERE STARTSWITH($metadata.$model, 'dtmi:ramon:floor')

SELECT * FROM digitaltwins WHERE IS_DEFINED(tags.broken)

SELECT  -> TOP, COUNT
WHERE -> CONTAINS, IS_DEFINEd, IS_NULL, IS_NUMBER, IS_OBJECT, IS_PRIMITIVE, IS_STRING

```

## Joins

```sql

-- Mandatory $dtId filter
SELECT building, floor FROM digitalTwins building JOIN floor RELATED building.consistsOf WHERE building.$dtId = 'BETABIT-ROTTERDAM'

```

## Relationships

```sql

SELECT * FROM relationships

SELECT * FROM relationships WHERE IS_DEFINED(contractId)

```
# Match
```sql
SELECT space FROM digitaltwins MATCH (building)-[*1..2]->(space)-[]->(sensor) WHERE building.$dtId = 'BETABIT-ROTTERDAM' AND sensor.latestValue > 20 AND IS_OF_MODEL(sensor, 'dtmi:ramon:sensor:temperature;1')
```

# Azure Data Explorer
```kql
.create table DeviceMessagesV2 (deviceId: string, temperature: real)

.create table DeviceMessagesV2 ingestion json mapping 'DeviceMessagesMappingV2' '[{"column":"temperature","path":"$.Temperature","datatype":"real"},{"column":"deviceId","path":"$.iothub-connection-device-id","datatype":"string"}]'

evaluate azure_digital_twins_query_request("https://we-demo-01.api.weu.digitaltwins.azure.net", "SELECT $dtId as id FROM DIGITALTWINS") 

evaluate azure_digital_twins_query_request("https://we-demo-01.api.weu.digitaltwins.azure.net", "SELECT $dtId as id, name FROM DIGITALTWINS")

DeviceMessagesV2
| summarize arg_max(temperature, *) by deviceId
| project id=deviceId, temperature
| join (
    evaluate azure_digital_twins_query_request("https://we-demo-01.api.weu.digitaltwins.azure.net", "SELECT $dtId as id, serialNo, type FROM DIGITALTWINS WHERE IS_OF_MODEL('dtmi:ramon:sensor:temperature;1')")
    | extend id = tostring(id)
) on id, $left.id == $right.id
| project id, serialNo, temperature, type

```

