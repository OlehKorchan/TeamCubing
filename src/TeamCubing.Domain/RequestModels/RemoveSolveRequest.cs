using Microsoft.AspNetCore.Mvc;

namespace TeamCubing.Domain.RequestModels;

public class RemoveSolveRequest
{
    [FromQuery]
    public string RoomName { get; set; }

    [FromQuery]
    public int SolveNumber { get; set; }
}
