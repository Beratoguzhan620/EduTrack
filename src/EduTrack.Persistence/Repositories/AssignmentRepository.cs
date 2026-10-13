using Core.Persistence.Repository;
using EduTrack.Application.Repositories;
using EduTrack.Domain.Entities;

namespace EduTrack.Persistence.Repositories;

public class AssignmentRepository(AppDbContext context)
    : BaseRepository<Assignment, Guid, AppDbContext>(context), IAssignmentRepository
{
}

public class AssignmentSubmissionRepository(AppDbContext context)
    : BaseRepository<AssignmentSubmission, Guid, AppDbContext>(context), IAssignmentSubmissionRepository
{
}
