using AutoMapper;
using TeamCubing.Domain.Models;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.Domain.MappingProfiles;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<Solve, SolveResponse>();
        CreateMap<Room, RoomResponse>();
    }
}
