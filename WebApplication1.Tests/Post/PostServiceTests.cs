using Moq;
using WebApplication1.Application.Repositories;
using WebApplication1.Application.Services.Post;
using WebApplication1.Domain.UsuarioUpvotePost;

namespace WebApplication1.Tests.Post;

public class PostServiceTests
{
    [Theory]
    [InlineData(26)]
    [InlineData(1234)]
    [InlineData(0)]
    public void HappyPathDeletePost(int id)
    {
        var voteRepository = new Mock<IVoteRepository>();
        var postRepository = new Mock<IPostRepository>();

        voteRepository.Setup(r => r.BuscarPorPostId(id)).Returns((UsuarioUpvotePost?)null);

        var service = new PostService(voteRepository.Object, postRepository.Object);
        var result = service.Delete(id);

        Assert.True(result.Sucesso);

        postRepository.Verify(
            r => r.Delete(id),
            Times.Once()
        );
        postRepository.Verify(
            r => r.SaveChanges(),
            Times.Once()
        );
    }

    [Theory]
    [InlineData(26)]
    [InlineData(1234)]
    [InlineData(0)]
    public void TentarExcluirPostComVotoDeveFalhar(int id)
    {
        var voteRepository = new Mock<IVoteRepository>();
        var postRepository = new Mock<IPostRepository>();

        voteRepository.Setup(r => r.BuscarPorPostId(id)).Returns(
            new UsuarioUpvotePost()
            {
                IdPost = id
            }
        );

        var service = new PostService(voteRepository.Object, postRepository.Object);
        var result = service.Delete(id);

        Assert.False(result.Sucesso);
        postRepository.Verify(
            r => r.Delete(id),
            Times.Never()
        );
    }
}
