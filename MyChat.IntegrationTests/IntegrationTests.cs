﻿namespace MyChat.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;

    using Andriy.MyChat.Server;
    using Andriy.MyChat.Server.DAL;

    using Moq;

    using NUnit.Framework;

    using ChatClient = Client.Core.ChatClient;

    [TestFixture]
    public class IntegrationTests
    {
        private int ServerPort { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            this.ServerPort = int.Parse(ConfigurationManager.AppSettings[Consts.DefaultServerListeningPortProperty]);
        }

        [Test]
        public void OneServerOneClientTest()
        {
            const string Login = "testUser1";
            const string Pass = "testPass1";
            var serverDataContext = Mock.Of<IDataContext>(ctx => ctx.LoginExists(Login) &&
                ctx.ValidateLoginPass(Login, Pass));

            var server = new ChatServer(serverDataContext, this.ServerPort);

            var chatClient = new ChatClient();

            chatClient.Init("localhost", this.ServerPort, Login, Pass);
            chatClient.ValidateItselfAndServer();

            chatClient.SetUpSecureChannel();

            var logonResult = chatClient.LogOn();
            Assert.AreEqual(0, logonResult);

            chatClient.StartListener();

            var joinRoomResult = chatClient.performJoinRoom("r1", "testRoomPass");
            Assert.True(joinRoomResult);

            var messageReceivedBack = false;
            var syncRef = new object();
            chatClient.MessageProcessor.addProcessor(
                "r1",
                (source, dest, msg) =>
                    {
                        if (msg == "testMsg")
                        {
                            lock (syncRef)
                            {
                                messageReceivedBack = true;
                            }
                        }
                    });

            chatClient.queueChatMsg(3, "r1", "testMsg");

            Func<bool> messageReceivedBackRead = () =>
                {
                    lock (syncRef)
                    {
                        return messageReceivedBack;
                    }
                };

            var waiter = Task.Run(
                () =>
                    {
                        while (!messageReceivedBackRead())
                        {
                            Thread.Sleep(100);
                        }
                    });
            Task.WaitAll(new[] { waiter }, 5000);
            Assert.True(messageReceivedBackRead());

            chatClient.StopListener();
            server.Stop();
        }

        [Test]
        public void OneServerSeveralClientsTest()
        {
            const string LoginPrefix = "testUser";
            const string PassPrefix = "testPass";
            var serverDataContext =
                Mock.Of<IDataContext>(
                    ctx =>
                    ctx.LoginExists(It.Is<string>(l => l.StartsWith(LoginPrefix)))
                    && ctx.ValidateLoginPass(It.IsAny<string>(), It.IsAny<string>()));

            var server = new ChatServer(serverDataContext, this.ServerPort);

            var clientList = new List<ChatClient>(10);
            for (int i = 0; i < 10; i++)
            {
                clientList.Add(this.SetUpChatClient(LoginPrefix + i, PassPrefix + i));
            }

            int j = 0;
            foreach (var chatClient in clientList)
            {
                var messageReceivedBack = false;
                var syncRef = new object();
                chatClient.MessageProcessor.addProcessor(
                    "r1",
                    (source, dest, msg) =>
                        {
                            if (source == LoginPrefix + j && msg == "testMsg" + j)
                            {
                                lock (syncRef)
                                {
                                    messageReceivedBack = true;
                                }
                            }
                        });

                chatClient.queueChatMsg(3, "r1", "testMsg" + j);

                Func<bool> messageReceivedBackRead = () =>
                    {
                        lock (syncRef)
                        {
                            return messageReceivedBack;
                        }
                    };

                var waiter = Task.Run(
                    () =>
                        {
                            while (!messageReceivedBackRead())
                            {
                                Thread.Sleep(50);
                            }
                        });
                Task.WaitAll(new[] { waiter }, 5000);
                Assert.True(messageReceivedBackRead());
                j++;
            }

            foreach (var chatClient in clientList)
            {
                chatClient.StopListener();
            }

            server.Stop();
        }

        [Test]
        public void OneServerOneClientProtocolErrorTest()
        {
            const string Login = "testUser1";
            const string Pass = "testPass1";
            var serverDataContext = Mock.Of<IDataContext>(ctx => ctx.LoginExists(Login) &&
                ctx.ValidateLoginPass(Login, Pass));

            var server = new ChatServer(serverDataContext, this.ServerPort);

            var chatClient = new ChatClient();

            chatClient.Init("localhost", this.ServerPort, Login, Pass);
            chatClient.ValidateItselfAndServer();

            chatClient.SetUpSecureChannel();

            var logonResult = chatClient.LogOnInvalidMessage1();
            Assert.AreEqual(0, logonResult);

            chatClient.StartListener();

            var joinRoomResult = chatClient.performJoinRoom("r1", "testRoomPass");
            Assert.True(joinRoomResult);

            var messageReceivedBack = false;
            var syncRef = new object();
            chatClient.MessageProcessor.addProcessor(
                "r1",
                (source, dest, msg) =>
                {
                    if (msg == "testMsg")
                    {
                        lock (syncRef)
                        {
                            messageReceivedBack = true;
                        }
                    }
                });

            chatClient.queueChatMsg(3, "r1", "testMsg");

            Func<bool> messageReceivedBackRead = () =>
            {
                lock (syncRef)
                {
                    return messageReceivedBack;
                }
            };

            var waiter = Task.Run(
                () =>
                {
                    while (!messageReceivedBackRead())
                    {
                        Thread.Sleep(100);
                    }
                });
            Task.WaitAll(new[] { waiter }, 5000);
            Assert.True(messageReceivedBackRead());

            chatClient.StopListener();
            server.Stop();
        }

        [Test]
        public void OneServerOneClientTooBigMessageTest()
        {
            const string Login = "testUser1";
            const string Pass = "testPass1";
            var serverDataContext = Mock.Of<IDataContext>(ctx => ctx.LoginExists(Login) &&
                ctx.ValidateLoginPass(Login, Pass));

            var server = new ChatServer(serverDataContext, this.ServerPort);

            var chatClient = new ChatClient();

            chatClient.Init("localhost", this.ServerPort, Login, Pass);
            chatClient.ValidateItselfAndServer();

            chatClient.SetUpSecureChannel();

            var logonResult = chatClient.LogOnInvalidMessage1();
            Assert.AreEqual(0, logonResult);

            chatClient.StartListener();

            var joinRoomResult = chatClient.performJoinRoom("r1", "testRoomPass");
            Assert.True(joinRoomResult);

            var messageReceivedBack = false;
            var syncRef = new object();
            chatClient.MessageProcessor.addProcessor(
                "r1",
                (source, dest, msg) =>
                {
                    if (msg == "testMsg")
                    {
                        lock (syncRef)
                        {
                            messageReceivedBack = true;
                        }
                    }
                });

            chatClient.queueChatMsg(3, "r1", "testMsg");

            Func<bool> messageReceivedBackRead = () =>
            {
                lock (syncRef)
                {
                    return messageReceivedBack;
                }
            };

            var waiter = Task.Run(
                () =>
                {
                    while (!messageReceivedBackRead())
                    {
                        Thread.Sleep(100);
                    }
                });
            Task.WaitAll(new[] { waiter }, 5000);
            Assert.True(messageReceivedBackRead());

            chatClient.StopListener();
            server.Stop();
        }

        [Test]
        public void OneServerOneClientTooBigMessage2Test()
        {
            const string Login = "testUser1";
            const string Pass = "testPass1";
            var serverDataContext = Mock.Of<IDataContext>(ctx => ctx.LoginExists(Login) &&
                ctx.ValidateLoginPass(Login, Pass));

            var server = new ChatServer(serverDataContext, this.ServerPort);

            var chatClient = new ChatClient();

            chatClient.Init("localhost", this.ServerPort, Login, Pass);
            chatClient.ValidateItselfAndServer();

            chatClient.SetUpSecureChannel();

            var logonResult = chatClient.LogOnInvalidMessage1();
            Assert.AreEqual(0, logonResult);

            chatClient.StartListener();
            
            chatClient.StopListener();
            server.Stop();
        }

        private ChatClient SetUpChatClient(string login, string pass)
        {
            var chatClient = new ChatClient();

            chatClient.Init("localhost", this.ServerPort, login, pass);
            chatClient.ValidateItselfAndServer();
            chatClient.SetUpSecureChannel();

            var logonResult = chatClient.LogOn();
            Assert.AreEqual(0, logonResult);

            chatClient.StartListener();

            var joinRoomResult = chatClient.performJoinRoom("r1", "testRoomPass");
            Assert.True(joinRoomResult);
            
            return chatClient;
        }
    }
}
