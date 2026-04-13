using Loggy.Core.Models.Events;
using System.Net;
using System.Net.Http.Json;

namespace Loggy.Tests
{
    public class EventsEndpointTests
    {
        [Fact]
        public async Task Post_Valid_Returns201WithLocation()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var dto = new EventDto { AccountId = "acc-1", Type = EventType.Transaction, Amount = -50m, Message = "yyyyyy" };

            var response = await client.PostAsJsonAsync("/events", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.StartsWith("/events/", response.Headers.Location!.ToString());
        }

        [Fact]
        public async Task Get_EmptyDatabase_ReturnsEmptyArray()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var events = await client.GetFromJsonAsync<List<Event>>("/events");

            Assert.NotNull(events);
            Assert.Empty(events!);
        }

        [Fact]
        public async Task Post_ThenGetById_ReturnsSameEvent()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var dto = new EventDto { AccountId = "acc-1", Type = EventType.Reservation, Amount = 100m, Message = "xxxxx" };
            var postResponse = await client.PostAsJsonAsync("/events", dto);
            var created = await postResponse.Content.ReadFromJsonAsync<Event>();

            var fetched = await client.GetFromJsonAsync<Event>($"/events/{created!.Id}");

            Assert.NotNull(fetched);
            Assert.Equal(created.Id, fetched!.Id);
            Assert.Equal(dto.Type, fetched.Type);
            Assert.Equal(dto.Amount, fetched.Amount);
            Assert.Equal(dto.Message, fetched.Message);
        }

        [Fact]
        public async Task GetById_Missing_Returns404()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync($"/events/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_FiltersByType()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            await client.PostAsJsonAsync("/events", new EventDto { AccountId = "acc-1", Type = EventType.Transaction, Amount = -10m });
            await client.PostAsJsonAsync("/events", new EventDto { AccountId = "acc-1", Type = EventType.Reservation, Amount = 50m });
            await client.PostAsJsonAsync("/events", new EventDto { AccountId = "acc-1", Type = EventType.Transaction, Amount = -20m });

            var transactions = await client.GetFromJsonAsync<List<Event>>($"/events?type={(int)EventType.Transaction}");

            Assert.NotNull(transactions);
            Assert.Equal(2, transactions!.Count);
            Assert.All(transactions, e => Assert.Equal(EventType.Transaction, e.Type));
        }

        [Fact]
        public async Task Get_FiltersByMessage()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            await client.PostAsJsonAsync("/events", new EventDto
            {
                AccountId = "acc-1",
                Type = EventType.Transaction,
                Amount = -10m,
                Message = "One sandwich"
            });
            await client.PostAsJsonAsync("/events", new EventDto
            {
                AccountId = "acc-1",
                Type = EventType.Transaction,
                Amount = -20m,
                Message = "Hospital bills"
            });

            var results = await client.GetFromJsonAsync<List<Event>>("/events?message=Hospital");

            Assert.NotNull(results);
            Assert.Single(results!);
            Assert.Contains("Hospital bills", results![0].Message);
        }
    }
}
