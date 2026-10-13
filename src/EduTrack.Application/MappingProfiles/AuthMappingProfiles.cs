using AutoMapper;
using EduTrack.Application.DTOs.Auth;
using EduTrack.Domain.Entities;


namespace EduTrack.Application.MappingProfiles;

public class AuthMappingProfiles : Profile
{
	public AuthMappingProfiles()
	{
		CreateMap<User, LoginResponse>()
			 .ForMember(c => c.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()))
			 .ForMember(c => c.IsEmailVerified, opt => opt.MapFrom(src => src.EmailConfirmed));


		CreateMap<RegisterRequest, User>();
		CreateMap<User, RegisterResponse>()
			 .ForMember(c => c.UserId, opt => opt.MapFrom(src => src.Id))
			 .ForMember(c => c.VerificationEmailSent, opt => opt.Ignore())
			 .ForMember(c => c.VerificationCode, opt => opt.Ignore());
    }
}
