﻿using System;

namespace MessageBus.Core.API
{
    /// <summary>
    /// Message bus interface provides a way to publishers messages and subscriber for messages.
    /// All publishers and subscribers will be associated with same bus runtime.
    /// </summary>
    /// <example>
    /// IBus bus = new SomeBus()
    /// 
    /// using (bus)
    /// {
    ///     using (IPublisher publisher = bus.CreatePublisher())
    ///     {
    ///         publisher.Send(new MyData { Id = 5, Name = "hello" } );
    ///     }
    /// 
    ///     using (ISubscriber subscriber = bus.CreateSubscriber())
    ///     {
    ///         subscriber.Subscribe(d => Console.WriteLine(d.Address));
    ///     } // Once disposed no data will be consumed any more
    /// }
    /// </example>
    public interface IBus : IDisposable
    {
        /// <summary>
        /// Bus client name which uniquely identifies publishers and subscribers created within this instance
        /// </summary>
        string BusId { get; }

        /// <summary>
        /// Creates publisher session. It is recommended to open new session every time there is a need to send messages.
        /// </summary>
        /// <returns>Publisher instance</returns>
        IPublisher CreatePublisher(Action<IPublisherConfigurator> configure = null);

        /// <summary>
        /// Creates an receiver that will receive messages from the bus on demand
        /// </summary>
        /// <returns></returns>
        IReceiver CreateReceiver(Action<ISubscriberConfigurator> configure = null);

        /// <summary>
        /// Creates subscriber. Subscriber implementation should provide ordered message delivery, i.e. preserve message dispatching order.
        /// </summary>
        /// <remarks>
        /// To logically separate processing of different message types, separate subscriber instances should be created.
        /// </remarks>
        /// <returns>
        /// Subscriber instance.
        /// </returns>
        /// <exception cref="NoIncomingConnectionAcceptedException">No incoming connection were accepted.</exception>
        ISubscriber CreateSubscriber(Action<ISubscriberConfigurator> configure = null);
        
        /// <summary>
        /// Register subscription instance.
        /// </summary>
        /// <remarks>
        /// Subscription instance type should be annotated by SubscriptionAttribute.
        /// </remarks>
        /// <remarks>
        /// Only public methods annotated by MessageSubscriptionAttribute will be subscribed to the messages. These methods should have single parameter of message construct type or BusMessage type.
        /// </remarks>
        /// <see cref="MessageSubscriptionAttribute"/>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="configure"></param>
        /// <returns>Disposable subscriber object. It must be disposed to deactivate the subscription.</returns>
        ISubscription RegisterSubscription<T>(T instance, Action<ISubscriberConfigurator> configure = null);

        /// <summary>
        /// Create route manager instance
        /// </summary>
        /// <returns></returns>
        IRouteManager CreateRouteManager();
    }
}
