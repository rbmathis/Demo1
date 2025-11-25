using Microsoft.Extensions.Logging;
using Moq;

namespace Demo1.UnitTests.Infrastructure;

internal static class LoggerExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, Times times)
    {
        logger.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
