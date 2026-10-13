using Core.Persistence.Repository;
using EduTrack.Domain.Entities;

namespace EduTrack.Application.Repositories;

public interface IAssignmentRepository : IRepository<Assignment, Guid>
{
}

public interface IAssignmentSubmissionRepository : IRepository<AssignmentSubmission, Guid>
{
}
