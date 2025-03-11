using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDeliverable.Utils.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IDeliverable.Utils.Core.Tests
{
    [TestClass]
    public class HandlersTest
    {
        [TestMethod]
        [Description("Registration using handler implementation instance.")]
        public void HandlersTest01()
        {
            var handler = new TestHandler<TestMessage>();

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(handler)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handler.HandledMessage);
            Assert.AreEqual(value, handler.HandledMessage.Value);
        }

		[TestMethod]
        [Description("Registration using handler implementation factory.")]
        public void HandlersTest02()
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(implementationFactory: serviceProvider => new TestHandler<TestMessage>())
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var handler = (TestHandler<TestMessage>)serviceProvider.GetRequiredService<IHandler<TestMessage>>();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handler.HandledMessage);
            Assert.AreEqual(value, handler.HandledMessage.Value);
        }

        [TestMethod]
        [Description("Registration using handler implementation type.")]
        public void HandlersTest03()
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestHandler<TestMessage>>()
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var handler = (TestHandler<TestMessage>)serviceProvider.GetRequiredService<IHandler<TestMessage>>();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handler.HandledMessage);
            Assert.AreEqual(value, handler.HandledMessage.Value);
        }

        [TestMethod]
        [Description("Registration using sync delegate handler.")]
        public void HandlersTest04()
        {
            TestMessage handledMessage = null;

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>((message, cancellationToken) => handledMessage = message)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handledMessage);
            Assert.AreEqual(value, handledMessage.Value);
        }

        [TestMethod]
        [Description("Registration using async delegate handler.")]
        public void HandlersTest05()
        {
            TestMessage handledMessage = null;
            
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(message =>
                    {
                        handledMessage = message;
                        return Task.CompletedTask;
                    })
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handledMessage);
            Assert.AreEqual(value, handledMessage.Value);
        }

        [TestMethod]
        [Description("Registration of multiple handlers of same message type.")]
        public void HandlersTest06()
        {
            TestMessage handledMessageByOne = null;
            TestMessage handledMessageByOther = null;

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(message => handledMessageByOne = message)
                    .AddHandler<TestMessage>(message => handledMessageByOther = message)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);

            Assert.IsNotNull(handledMessageByOne);
            Assert.IsNotNull(handledMessageByOther);
            Assert.AreEqual(value, handledMessageByOne.Value);
            Assert.AreEqual(value, handledMessageByOther.Value);
        }

        [TestMethod]
        [Description("Resolution of multiple handlers of same message type.")]
        public void HandlersTest07()
        {
            TestMessage handledMessageByOne = null;
            TestMessage handledMessageByOther = null;

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(message => handledMessageByOne = message)
                    .AddHandler<TestMessage>(message => handledMessageByOther = message)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            // Ensure there's two IHandler<TestMessage> registered.
            var handlerList = serviceProvider.GetRequiredService<IEnumerable<IHandler<TestMessage>>>();
            Assert.AreEqual(2, handlerList.Count());
        }

        [TestMethod]
        [Description("Exception in first handler stops execution and propagates to sender.")]
        public void HandlersTest08()
        {
            TestMessage handledMessageByOne = null;
            TestMessage handledMessageByOther = null;

            const string exceptionMessage = "TestException";
            
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(handler: message => throw new Exception(exceptionMessage))
                    .AddHandler<TestMessage>(message => handledMessageByOther = message)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();

            try
            {
                sender.Send(value);
                Assert.Fail("No exception was thrown by Send() method.");
            }
            catch (AssertFailedException)
            {
            }
            catch (Exception ex)
            {
                Assert.AreEqual(exceptionMessage, ex.Message);
            }

            Assert.IsNull(handledMessageByOne);
            Assert.IsNull(handledMessageByOther);
        }

        [TestMethod]
        [Description("Exception in first handler does not stop execution when ignoreExceptions argument is set to true.")]
        public void HandlersTest09()
        {
            TestMessage handledMessageByOne = null;
            TestMessage handledMessageByOther = null;

            const string exceptionMessage = "TestException";

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(handler: message => throw new Exception(exceptionMessage))
                    .AddHandler<TestMessage>(message => handledMessageByOther = message)
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();

            sender.Send(value, ignoreExceptions: true);

            Assert.IsNull(handledMessageByOne);
            Assert.IsNotNull(handledMessageByOther);
            Assert.AreEqual(value, handledMessageByOther.Value);
        }

        [TestMethod]
        [Description("Sender works even if no handlers are registered.")]
        public void HandlersTest10()
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();
            sender.Send(value);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [Description("Cancellation exceptions are always thrown.")]
        public void HandlersTest11(bool ignoreExceptions)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestMessage>(handler: s => throw new OperationCanceledException())
                    .AddSingleton<TestSender>()
                    .BuildServiceProvider();

            var value = Guid.NewGuid().ToString();
            var sender = serviceProvider.GetRequiredService<TestSender>();

            Assert.ThrowsException<OperationCanceledException>(() => sender.Send(value, ignoreExceptions));
        }

		[TestMethod]
        [Description("Registration using handler implementation instance is idempotent.")]
        public void HandlersTest12()
        {
			var handler = new TestHandler<TestMessage>();

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(handler)
                    .AddHandler(handler)
                    .BuildServiceProvider();

            var handlers = serviceProvider.GetRequiredService<IEnumerable<IHandler<TestMessage>>>();

            Assert.AreEqual(expected: 1, handlers.Count());
        }

		[TestMethod]
        [Description("Registration using handler implementation factory is idempotent.")]
        public void HandlersTest13()
        {
            static TestHandler<TestMessage> handlerFactory(IServiceProvider serviceProvider) => new();

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(handlerFactory)
                    .AddHandler(handlerFactory)
                    .BuildServiceProvider();

            var handlers = serviceProvider.GetRequiredService<IEnumerable<IHandler<TestMessage>>>();

            Assert.AreEqual(expected: 1, handlers.Count());
        }

        [TestMethod]
        [Description("Registration using handler implementation type is idempotent.")]
        public void HandlersTest14()
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestHandler<TestMessage>>()
					.AddHandler<TestHandler<TestMessage>>()
                    .BuildServiceProvider();

            var handlers = serviceProvider.GetRequiredService<IEnumerable<IHandler<TestMessage>>>();

            Assert.AreEqual(expected: 1, handlers.Count());
        }

		[TestMethod]
        [Description("Same handler implementation instance is used to handle multiple message types (instance registration).")]
        public void HandlersTest15()
        {
			var handler = new TestDualHandler();

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(handler)
                    .BuildServiceProvider();

            var handler1 = serviceProvider.GetRequiredService<IHandler<Message1>>();
			var handler2 = serviceProvider.GetRequiredService<IHandler<Message2>>();

            Assert.AreSame<object>(handler1, handler2);
        }

		[TestMethod]
        [Description("Same handler implementation instance is used to handle multiple message types (type registration).")]
        public void HandlersTest16()
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddHandler<TestDualHandler>()
                    .BuildServiceProvider();

            var handler1 = serviceProvider.GetRequiredService<IHandler<Message1>>();
			var handler2 = serviceProvider.GetRequiredService<IHandler<Message2>>();

            Assert.AreSame<object>(handler1, handler2);
        }

		[TestMethod]
        [Description("Handler implementation factory is called once per handled message type.")]
        public void HandlersTest17()
        {
			var numInvocations = 0;

			TestDualHandler handlerFactory(IServiceProvider serviceProvider)
			{
				numInvocations++;
				return new();
			}

            var serviceProvider =
                new ServiceCollection()
                    .AddHandler(handlerFactory)
                    .BuildServiceProvider();

            var handler1 = serviceProvider.GetRequiredService<IHandler<Message1>>();
			var handler2 = serviceProvider.GetRequiredService<IHandler<Message2>>();

			Assert.AreEqual(expected: 2, numInvocations);
            Assert.AreNotSame<object>(handler1, handler2);
        }

        public class TestMessage
        {
            public TestMessage(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        public class TestHandler<TMessage> : IHandler<TMessage>
        {
            public TMessage HandledMessage { get; private set; }

            public Task HandleAsync(TMessage message, CancellationToken cancellationToken)
            {
                HandledMessage = message;
                return Task.CompletedTask;
            }
        }

        public class TestSender
        {
            public TestSender(IEnumerable<IHandler<TestMessage>> handlers)
            {
                mHandlers = handlers;
            }

            private readonly IEnumerable<IHandler<TestMessage>> mHandlers;

            public void Send(string value, bool ignoreExceptions = false)
            {
                try
                {
                    mHandlers.HandleAsync(new TestMessage(value), ignoreExceptions).GetAwaiter().GetResult();
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerException;
                }
            }
        }

		public class TestDualHandler : IHandler<Message1>, IHandler<Message2>
        {
            public Message1 HandledMessage1 { get; private set; }
			public Message2 HandledMessage2 { get; private set; }

            public Task HandleAsync(Message1 message, CancellationToken cancellationToken)
            {
                HandledMessage1 = message;
                return Task.CompletedTask;
            }

            public Task HandleAsync(Message2 message, CancellationToken cancellationToken)
            {
                HandledMessage2 = message;
                return Task.CompletedTask;
            }
        }

		public class Message1 { }
		public class Message2 { }
    }
}
