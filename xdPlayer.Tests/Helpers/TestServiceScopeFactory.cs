using Microsoft.Extensions.DependencyInjection;
using Moq;
using xdPlayer.Domain.Interfaces;

namespace xdPlayer.Tests.Helpers;

public static class TestServiceScopeFactory
{
    public static IServiceScopeFactory Create(IUnitOfWork uow)
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IUnitOfWork)))
            .Returns(uow);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        return scopeFactory.Object;
    }
}