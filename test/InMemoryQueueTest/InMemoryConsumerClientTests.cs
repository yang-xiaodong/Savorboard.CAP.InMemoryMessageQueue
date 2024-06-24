using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Savorboard.CAP.InMemoryMessageQueue;
using Xunit;

namespace InMemoryQueueTest
{
    public class InMemoryConsumerClientTests
    {
        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

        private readonly InMemoryConsumerClient _sut;

        private readonly InMemoryQueue _queue;

        private readonly string _groupId;
        private readonly byte _groupConcurrent;

        public InMemoryConsumerClientTests()
        {
            _queue = _fixture.Freeze<InMemoryQueue>();

            _groupId = _fixture.Create<string>();
            _groupConcurrent = _fixture.Create<byte>();

            _sut = new InMemoryConsumerClient(_fixture.Create<ILogger>(), _queue, _groupId, _groupConcurrent);

            _sut.Subscribe(_fixture.CreateMany<string>());
        }

        //[Fact]
        //public void Dispose_Removes_Only_Subscriptions_For_The_Certain_GroupId()
        //{
        //    var otherTopicsRegisteredInTheQueue = _fixture.CreateMany<string>();
        //    foreach (var topic in otherTopicsRegisteredInTheQueue)
        //    {
        //        _queue.Subscribe(_fixture.Create<string>(), message => { }, topic);
        //    }

        //    _sut.Dispose();

        //    _queue.GroupTopics.Should().NotContainKey(_groupId);
        //    _queue.GroupTopics.Should().NotBeEmpty();
        //}
    }
}