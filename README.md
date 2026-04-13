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
```bash
  curl -i -X POST http://localhost:8080/events -H "Content-Type: application/json" -d "{\"accountId\":\"acc-1\",\"type\":3,\"amount\":-250.50,\"message\":\"Grocery store\"}"

  curl "http://localhost:8080/events?accountId=acc-1&type=3&limit=10"
```