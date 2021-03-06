﻿using System;
using System.Threading;
using System.Runtime.Serialization;
using FluentAssertions;
using MessageBus.Core.API;
using NUnit.Framework;

namespace Core.IntegrationTest
{
    [TestFixture]
    public class BusDurableTests
    {
        private const string QueueName = "DurableTestQueue";

        [TestFixtureSetUp]
        public void Initialize()
        {
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IRouteManager routeManager = bus.CreateRouteManager())
                {
                    routeManager.CreateQueue(QueueName, true, false, CreateQueueSettings.Default);
                    
                    routeManager.QueueBindMessage<ImportiantData>(QueueName);
                }
            }
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IRouteManager routeManager = bus.CreateRouteManager())
                {
                    routeManager.DeleteQueue(QueueName);
                }
            }
        }

        [Test]
        public void Bus_DurableBus_SubscribesAfterPublishing_MessagesShouldBeReceived()
        {
            ImportiantData data = new ImportiantData { Info = "Valuable information" };
            
            // Dispatch message
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (IPublisher publisher = bus.CreatePublisher())
                {
                    publisher.Send(data);
                }
            }

            ImportiantData expected = null;
            ManualResetEvent ev = new ManualResetEvent(false);

            // Second bus subscribes to message after it was dispatched and should receive it
            using (var bus = new MessageBus.Core.RabbitMQBus())
            {
                using (ISubscriber subscriber = bus.CreateSubscriber(c => c.UseDurableQueue(QueueName)))
                {
                    subscriber.Subscribe<ImportiantData>(p =>
                        {
                            expected = p;

                            ev.Set();
                        });

                    subscriber.Open();

                    bool wait = ev.WaitOne(TimeSpan.FromSeconds(5));

                    wait.Should().BeTrue();
                }
            }

            expected.Should().NotBeNull();

            expected.ShouldBeEquivalentTo(data);
        }
    }

    [DataContract]
    public class ImportiantData
    {
        [DataMember]
        public string Info { get; set; }
    }
}
