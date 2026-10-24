using Mediator;

namespace Cinema.Api.Features.Seats.HoldSeat;

public sealed class HoldSeatHandler : ICommandHandler<HoldSeatCommand, HoldSeatResult>
{
    // Red phase: rejects on purpose so the scenario fails on its assertion.
    public ValueTask<HoldSeatResult> Handle(HoldSeatCommand command, CancellationToken cancellationToken) =>
        ValueTask.FromResult(HoldSeatResult.Rejected());
}
