using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjEyMzQ1LCJ1c2VybmFtZSI6ImpvaG5fZG9lIiwiZW1haWwiOiJqb2huLmRvZUBleGFtcGxlLmNvbSIsImlhdCI6MTYxODQ2NzQ3NCwiZXhwIjoxNjE4NDcwMjc0LCJpc3MiOiJteUFwcCJ9.bvqPBUyPKV7Xe8RyWV7vHw5J5h5DJom8nH5djAeZxJQ

namespace FlightAPI.Controllers
{
    public class Flight
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string FlightNo { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public int Seats { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly ILogger<FlightsController> _logger;

        private readonly List<Flight> _flights = new List<Flight>
        {
            new Flight { Id = 1, Date = "2022-05-01", FlightNo = "XY123", Departure = "Istanbul", Arrival = "Ankara", Seats = 50 },
            new Flight { Id = 2, Date = "2022-05-01", FlightNo = "XY124", Departure = "Ankara", Arrival = "Istanbul", Seats = 30 },
            new Flight { Id = 3, Date = "2022-05-02", FlightNo = "XY125", Departure = "Istanbul", Arrival = "Izmir", Seats = 20 },
            new Flight { Id = 4, Date = "2022-05-02", FlightNo = "XY126", Departure = "Izmir", Arrival = "Istanbul", Seats = 40 },
            new Flight { Id = 5, Date = "2022-05-03", FlightNo = "XY127", Departure = "Istanbul", Arrival = "Antalya", Seats = 35 },
            new Flight { Id = 6, Date = "2022-05-03", FlightNo = "XY128", Departure = "Antalya", Arrival = "Istanbul", Seats = 25 },
            new Flight { Id = 7, Date = "2022-05-04", FlightNo = "XY129", Departure = "Istanbul", Arrival = "Bursa", Seats = 45 },
            new Flight { Id = 8, Date = "2022-05-04", FlightNo = "XY130", Departure = "Bursa", Arrival = "Istanbul", Seats = 10 }
        };

        public FlightsController(ILogger<FlightsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string date, string departure, string arrival, int num_passengers = 1)
        {
            var flights = _flights.Where(f => f.Date == date && f.Departure == departure && f.Arrival == arrival);

            var results = new List<FlightResult>();

            foreach (var flight in flights)
            {
                if (flight.Seats >= num_passengers)
                {
                    results.Add(new FlightResult
                    {
                        Date = flight.Date,
                        Departure = flight.Departure,
                        Arrival = flight.Arrival,
                        Seats = flight.Seats,
                        FlightNo = flight.FlightNo
                    });
                }
            }

            if (results.Count == 0)
            {
                return NotFound();
            }

            return Ok(results);



        }

        //  [Authorize]
        [HttpPost("buy")]
        public IActionResult BuyTicket(BuyTicketRequest request)
        {
            var user = HttpContext.User.Identity.Name;

            var flight = _flights.FirstOrDefault(f => f.Date == request.Date
                                                && f.Departure == request.From
                                                && f.Arrival == request.To
                                                && f.Seats >= 1);

            if (flight == null)
            {
                return BadRequest("No flights available.");
            }

            flight.Seats--;

            return Ok("Ticket bought successfully.");
        }

    }

    public class BuyTicketRequest
    {
        public string Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string PassengerName { get; set; }
    }

    public class FlightResult
    {
        public string Date { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public int Seats { get; set; }
        public string FlightNo { get; set; }

        public override string ToString()
        {
            return $"Flight {FlightNo} on {Date} from {Departure} to {Arrival} with {Seats} seats available";
        }
    }

    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();

            if (header != null && header.StartsWith("Bearer "))
            {
                var token = header.Substring("Bearer ".Length).Trim();

                await _next.Invoke(context);
            }
            else
            {
                context.Response.StatusCode = 401;
                return;
            }
        }
    }

}
