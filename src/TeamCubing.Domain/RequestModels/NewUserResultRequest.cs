using TeamCubing.Domain.Models;

namespace TeamCubing.Domain.RequestModels;

public class NewUserResultRequest
{
    public string RoomId { get; set; }

    public int SolveNumber { get; set; }

    public int TimeInMilliseconds { get; set; }

    public Penalty Penalty { get; set; }
}
