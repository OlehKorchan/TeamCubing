using AutoMapper;
using TeamCubing.API.Models;
using TeamCubing.BLL.Models;
using TeamCubing.DAL.Models;

namespace TeamCubing.API.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<Session, UserSessionDto>().ReverseMap();
        CreateMap<Solve, SolveDto>().ReverseMap();
        CreateMap<UserSessionDto, SessionResponseModel>();
        CreateMap<SolveRequestModel, SolveDto>();
        CreateMap<SolveDto, SolveResponseModel>();
        CreateMap<RoomLoginModel, RoomLoginDto>();
        CreateMap<Room, RoomDto>()
            .ForMember(
                r => r.ConnectedUserNames,
                options => options.MapFrom(
                    rm => rm.Users.Select(u => u.UserName)
                        .ToList()))
            .ForMember(
                r => r.WasOnceConnectedUsers,
                options => options.MapFrom(
                    rm => rm.WasOnceConnectedUsers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList()));
        CreateMap<RoomSolveResult, RoomSolveResultDto>()
            .ForMember(
                r => r.UserName,
                options => options.MapFrom(
                    d => d.User.UserName));
        CreateMap<RoomSolve, RoomSolveDto>()
            .ForMember(
                r => r.Results,
                options => options.MapFrom(
                    d => d.Results));
        ;
    }
}
