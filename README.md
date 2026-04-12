## Loggy

API that logs events (bank account), Aspire .Net 10

## Run

```sh
docker-compose up --build
```

Regenerate docker-compose
```sh
aspire publish src/Loggy.AppHost -o .
```

Endpoints
localhost:8080
- POST /events      | see EventDto
- GET /events       | ?type?from?to?message?limit
- GET /events/{guid}

Example
```sh
curl -i -X POST http://localhost:8080/events ^
  -H "Content-Type: application/json" ^
  -d "{\"type\":\"Transaction\",\"amount\":-999.0,\"message\":\"DLC battlebass premium\"}"
```
```sh
curl "http://localhost:8080/events?type=Transaction&limit=10"
```