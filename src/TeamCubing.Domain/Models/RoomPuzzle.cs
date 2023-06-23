using System.Runtime.Serialization;

namespace TeamCubing.Domain.Models;

public enum RoomPuzzle
{
    [EnumMember(Value = "3x3")]
    ThreeByThreeCube,
}
