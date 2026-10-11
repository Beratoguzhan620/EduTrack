using Core.Domain;

namespace EduTrack.Domain.Entities;

public class Role : Entity<int>
{
    public string Name { get; set; } = string.Empty;
    public virtual List<UserRole> UserRoles { get; set; } = default!;
}
