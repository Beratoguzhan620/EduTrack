using Core.Results;
using EduTrack.Application.DTOs.StreamPost;

namespace EduTrack.Application.Services.Abstract;

public interface IStreamPostService
{
    Task<Result<List<StreamPostDto>>> GetStreamPosts(Guid courseId);
    Task<Result> CreateStreamPost(CreateStreamPostRequest createDto);
}
