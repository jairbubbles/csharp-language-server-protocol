using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.JsonRpc.Server;
using Xunit;
using Xunit.Abstractions;
using Arg = NSubstitute.Arg;

namespace JsonRpc.Tests
{
    public class MediatorTestsNotificationHandlerOfT : AutoTestBase
    {
        [Method("$/cancelRequest")]
        public interface ICancelRequestHandler : IJsonRpcNotificationHandler<CancelParams>
        {
        }

        public class CancelParams : IRequest
        {
            public object Id { get; set; }
        }

        public MediatorTestsNotificationHandlerOfT(ITestOutputHelper testOutputHelper) : base(testOutputHelper) => Container = JsonRpcTestContainer.Create(testOutputHelper);

        [Fact]
        public async Task ExecutesHandler()
        {
            var cancelRequestHandler = Substitute.For<ICancelRequestHandler>();
            var mediator = Substitute.For<IMediator>();

            var collection = new HandlerCollection(Substitute.For<IResolverContext>(), new HandlerTypeDescriptorProvider(new [] { typeof(HandlerTypeDescriptorProvider).Assembly, typeof(HandlerResolverTests).Assembly })) { cancelRequestHandler };
            AutoSubstitute.Provide<IHandlersManager>(collection);
            var router = AutoSubstitute.Resolve<RequestRouter>();

            var @params = new CancelParams { Id = Guid.NewGuid() };
            var notification = new Notification("$/cancelRequest", JObject.Parse(JsonConvert.SerializeObject(@params)));

            await router.RouteNotification(router.GetDescriptors(notification), notification, CancellationToken.None);

            await cancelRequestHandler.Received(1).Handle(Arg.Any<CancelParams>(), Arg.Any<CancellationToken>());
        }
    }
}
