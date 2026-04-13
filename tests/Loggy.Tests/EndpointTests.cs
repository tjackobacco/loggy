using Loggy.Core.Models.Events;
using System.Net;
using System.Net.Http.Json;
using System.Text;

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

        #region Bad Request tests
        [Theory]
        [InlineData("{")]                                            // truncated
        [InlineData("{\"accountId\": \"acc-1\",}")]                  // trailing comma
        [InlineData("not json at all")]                              // garbage
        [InlineData("")]                                             // empty body
        [InlineData("{\"accountId\": 12345, \"type\": 1, \"amount\": 0}")] // wrong type for accountId
        public async Task Post_MalformedJson_Returns400(string body)
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/events", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertErrorShape(response);
        }


        public static TheoryData<EventDto, string> InvalidDtos() => new()
        {
          { new() { AccountId = "",      Type = EventType.Transaction,  Amount =  10m }, "empty sender" },
          { new() { AccountId = "   ",   Type = EventType.Transaction,  Amount =  10m }, "whitespacey sender" },
          { new() { AccountId = "acc-1", Type = EventType.Unknown,      Amount =  10m }, "unknown type" },
          { new() { AccountId = "acc-1", Type = (EventType)256,          Amount =  10m }, "out-of-range type" },
          { new() { AccountId = "acc-1", Type = EventType.Transaction,  Amount =   0m }, "zero transaction" },
          { new() { AccountId = new string('q', 5000), Type = EventType.Transaction, Amount = 10m }, "big big accountId" },
          { new() { AccountId = "acc-1", Type = EventType.Transaction,  Amount = 10m, Message = new string('m', 5000) }, "long long message" },
        };

        // // todo; Refactor validation annotation above events dto
        //[Theory]
        //[MemberData(nameof(InvalidDtos))]
        //public async Task Post_InvalidDto_Returns400(EventDto dto, string _label)
        //{
        //    using var factory = new LoggyApiFactory();
        //    var client = factory.CreateClient();

        //    var response = await client.PostAsJsonAsync("/events", dto);

        //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        //    await AssertErrorShape(response);
        //}


        [Fact]
        public async Task GetById_NonexistentGuid_Returns404WithShape()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync($"/events/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await AssertErrorShape(response);
        }

        [Fact]
        public async Task GetById_MalformedGuid_Returns404()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var response = await client.GetAsync("/events/not-a-guid");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_WrongContentType_Returns415Or400()
        {
            using var factory = new LoggyApiFactory();
            var client = factory.CreateClient();

            var content = new StringContent("accountId=acc-1", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync("/events", content);

            Assert.True(
                response.StatusCode == HttpStatusCode.UnsupportedMediaType ||
                response.StatusCode == HttpStatusCode.BadRequest,
                $"expected 415 or 400, got {(int)response.StatusCode}");
        }

        static async Task AssertErrorShape(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<ErrorShape>();
            Assert.NotNull(body);
            Assert.False(string.IsNullOrWhiteSpace(body!.Message));
            Assert.Equal((int)response.StatusCode, body.StatusCode);
        }

        sealed record ErrorShape(string Message, int StatusCode);
        #endregion
    }
}
