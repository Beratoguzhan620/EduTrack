using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Instructor = "Instructor";

    public const int AdminId = 1;
    public const int InstructorId = 2;

    public static readonly List<Role> BaseRoles =
        [
            new Role(){Id = AdminId, Name = Admin},
            new Role(){Id = InstructorId, Name = Instructor},
        ];
}

