using System.Runtime.Serialization;
using Demo1.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Octokit;

namespace Demo1.UnitTests.Services;

public class GitHubPipelineServiceTests
{
    [Fact]
    public async Task GetRecentRunsAsync_WhenGitHubApiThrows_ReturnsEmptyList()
    {
        var gitHubClientMock = new Mock<IGitHubClient>();
        var issuesClientMock = new Mock<IIssuesClient>();

        gitHubClientMock.SetupGet(client => client.Issue).Returns(issuesClientMock.Object);
        issuesClientMock
            .Setup(client => client.GetAllForRepository(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RepositoryIssueRequest>(),
                It.IsAny<ApiOptions>()))
            .ThrowsAsync(CreateApiException());

        var service = new GitHubPipelineService(
            gitHubClientMock.Object,
            Mock.Of<ILogger<GitHubPipelineService>>(),
            "owner",
            "repo");

        var result = await service.GetRecentRunsAsync();

        Assert.Empty(result);
    }

    private static ApiException CreateApiException()
    {
        foreach (var constructor in typeof(ApiException).GetConstructors()
                     .OrderBy(info => info.GetParameters().Length))
        {
            try
            {
                var parameters = constructor.GetParameters();
                var args = parameters
                    .Select(parameter => parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null)
                    .ToArray();
                return (ApiException)constructor.Invoke(args);
            }
            catch
            {
                // Try the next constructor signature.
            }
        }

#pragma warning disable SYSLIB0050
        return (ApiException)FormatterServices.GetUninitializedObject(typeof(ApiException));
#pragma warning restore SYSLIB0050
    }
}
