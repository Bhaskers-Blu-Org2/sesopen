//-------------------------------------------------------------------------------------------------------
// Copyright (C) Microsoft Corporation and contributors. All rights reserved.
// See LICENSE.txt file in the project root for full license information.
//-------------------------------------------------------------------------------------------------------

namespace WebSec.Common
{
    using System.Diagnostics;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Object factory class that supports the system-wide resolution of objects for Dependency Injection.
    /// </summary>
    [DebuggerStepThrough]
    public static class ObjectResolver
    {
        /// <summary>
        /// The container.
        /// </summary>
        private static readonly IUnityContainer Container = new UnityContainer();

        /// <summary>
        /// Registers an instance of Type T with the object factory.
        /// </summary>
        /// <typeparam name="T">The Type of the object to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public static void RegisterInstance<T>(T instance)
        {
            Container.RegisterInstance(instance);
        }

        /// <summary>
        /// Registers an instance of Type T with the object factory.
        /// </summary>
        /// <typeparam name="T">The Type of the object to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <param name="name">name of the instance</param>
        public static void RegisterInstance<T>(T instance, string name)
        {
            Container.RegisterInstance(typeof(T), name, instance);
        }

        /// <summary>
        /// Registers an instance of Type T with the object factory.
        /// </summary>
        /// <typeparam name="T">The Type of the object to register.</typeparam>
        /// <param name="instance">The instance to register.</param>
        public static void RegisterSingletonInstance<T>(T instance)
        {
            Container.RegisterInstance(instance, new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// RegistersType mapping with the object factory.
        /// </summary>
        /// <typeparam name="TFrom">The type that will be requested.</typeparam>
        /// <typeparam name="TTo">The type that will be returned.</typeparam>
        /// <param name="injectionMembers">Optional parameters used to construct the object.</param>
        public static void RegisterType<TFrom, TTo>(params InjectionMember[] injectionMembers) where TTo : TFrom
        {
            Container.RegisterType<TFrom, TTo>(injectionMembers);
        }

        /// <summary>
        /// RegistersType mapping with the object factory.
        /// </summary>
        /// <typeparam name="TFrom">The type that will be requested.</typeparam>
        /// <typeparam name="TTo">The type that will be returned.</typeparam>
        public static void RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            Container.RegisterType<TFrom, TTo>();
        }

        /// <summary>
        /// Returns an instance of T from the object factory.
        /// </summary>
        /// <typeparam name="T">The Type of object to return.</typeparam>
        /// <returns>The requested object.</returns>
        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        /// <summary>
        /// Returns an instance of T from the object factory.
        /// </summary>
        /// <typeparam name="T">
        /// The Type of object to return.
        /// </typeparam>
        /// <param name="parameterName">
        /// Name of the parameter.
        /// </param>
        /// <param name="parameterValue">
        /// The parameter value.
        /// </param>
        /// <returns>
        /// The requested object.
        /// </returns>
        public static T Resolve<T>(string parameterName, object parameterValue)
        {
            return Container.Resolve<T>(new ParameterOverride(parameterName, parameterValue));
        }

        /// <summary>
        /// Returns an instance of T from the object factory.
        /// </summary>
        /// <typeparam name="T">The Type of object to return.</typeparam>
        /// <param name="name">name of the instance</param>
        /// <returns>The requested object.</returns>
        public static T Resolve<T>(string name)
        {
            return Container.Resolve<T>(name);
        }
    }
}