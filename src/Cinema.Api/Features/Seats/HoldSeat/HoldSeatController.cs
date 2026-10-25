using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Api.Features.Seats.HoldSeat;

/// <summary>Translates HTTP into a command and the result into a status code. It holds no business rules.</summary>
[ApiController]
[Route("api/v1/sessions/{sessionId}/seats")]
public sealed class HoldSeatController(ISender sender) : ControllerBase
{
    [HttpPost("hold")]
    public async Task<IActionResult> Hold(
        string sessionId,
        HoldSeatRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new HoldSeatCommand(sessionId, request.SeatId, request.CustomerId), cancellationToken);

        return Ok(new HoldSeatResponse(sessionId, request.SeatId, request.CustomerId));
    }
}
