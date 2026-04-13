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
- POST /events      | details further down
- GET /events       | -//-
- GET /events/{guid}

Example
```bash
  curl -i -X POST http://localhost:8080/events -H "Content-Type: application/json" -d "{\"accountId\":\"1234-myaccount\",\"type\":3,\"amount\":-9999.99,\"message\":\"Battlepass DLC premium\"}"

  curl "http://localhost:8080/events?accountId=acc-1&type=3&limit=10"
  
  curl "http://localhost:8080/events/<whatever_guid_was_created_from_post>
```

POST /events `accountId`, `type`, `amount` required, `message` optional. `type`: 1 Reservation, 2 ReservationReleased, 3 Transaction

GET /events `accountId`, `type`, `from`, `to`, `message`, `limit`