using EduTrack.Application.DTOs.User;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Queries;

public static class UserQueryExtensions
{
    /// <summary>Role filter value meaning "users without any role" (i.e. students).</summary>
    public const int StudentsOnly = 0;

    /// <summary>
    /// Applies the optional filters of <see cref="GetAllUsersQuery"/>. Each filter is only added
    /// to the SQL when it has a value, instead of one large predicate with "OR is null" branches.
    /// </summary>
    public static IQueryable<User> ApplyFilters(this IQueryable<User> query, GetAllUsersQuery filter)
    {
        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search) ||
                (u.FirstName + " " + u.LastName).Contains(search) ||
                u.Email.Contains(search));
        }

        if (filter.RoleId == StudentsOnly)
        {
            query = query.Where(u => !u.UserRoles.Any());
        }
        else if (filter.RoleId is int roleId)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
        }

        if (filter.IsActive is bool isActive)
        {
            query = query.Where(u => u.IsActive == isActive);
        }

        return query;
    }
}
