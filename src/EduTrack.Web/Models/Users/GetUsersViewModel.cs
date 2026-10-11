using Core.Results;
using EduTrack.Application.DTOs.User;

namespace EduTrack.Web.Models.Users;

public class GetUsersViewModel
{
    public GetAllUsersQuery Query { get; set; } = new();
    public Paged<UserDto> Response { get; set; } = new();
}
