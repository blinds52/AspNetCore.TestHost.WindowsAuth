﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth
{
    /// <summary>
    ///     A base fixture for providing a <see cref="TestServer" /> for integration tests that has Windows Auth capabilities.
    ///     Created a derived implementation with overridden members to configure and customize behavior.
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    public abstract class WindowsAuthServerFixture<TStartup> : IDisposable
        where TStartup : class
    {
        protected WindowsAuthServerFixture()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Server = CreateServer();
        }

        /// <summary>
        ///     The <see cref="TestServer" /> instance that requests can be made against.
        /// </summary>
        public TestServer Server { get; }

        /// <summary>
        ///     The name of the application - e.g. "IntelliTect.Coalesce.Web".
        ///     Used the calulate <see cref="ContentRoot" /> assuming web and test projects are in the same location.
        ///     Should match the name of the directory that holds the web project.
        /// </summary>
        protected abstract string ApplicationName { get; }

        /// <summary>
        ///     The path to the content root of the web project, relative to the executing assembly of the test project.
        ///     Defaults to $"../../../../{ApplicationName}".
        /// </summary>
        protected virtual string ContentRoot => $"../../../../{ApplicationName}";

        /// <summary>
        ///     Name of the environment to be used by the web host.
        /// </summary>
        protected virtual string EnvironmentName => "Test";

        /// <summary>
        ///     Base address to be used by the server.
        /// </summary>
        protected virtual Uri BaseAddress => new Uri("http://localhost/");

        /// <summary>
        ///     Whether or not the standard <see cref="Microsoft.AspNetCore.Authentication.AuthenticationMiddleware" /> should be added to the request pipeline
        ///     (by calling
        ///     <see
        ///         cref="Microsoft.AspNetCore.Builder.AuthAppBuilderExtensions.UseAuthentication(Microsoft.AspNetCore.Builder.IApplicationBuilder)" />
        ///     ).
        ///     Defaults to false. Override to true if this middleware is not already added by your Startup class.
        /// </summary>
        protected virtual bool AddAuthenticationMiddleware => false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Server?.Dispose();
        }

        /// <summary>
        ///     Get a web host builder that will be used to create the <see cref="TestServer" />
        /// </summary>
        protected virtual IWebHostBuilder GetWebHostBuilder()
        {
            return new WebHostBuilder()
                .UseStartup<TStartup>()
                .UseEnvironment(EnvironmentName)
                .UseContentRoot(Path.GetFullPath(ContentRoot));
        }

        /// <summary>
        ///     Instantiate a <see cref="TestServer" /> with the needed services for Windows Auth.
        ///     Call extension methods in <see cref="WindowsAuthTestServerExtensions" /> to get
        ///     <see cref="System.Net.Http.HttpClient" /> instances that will behave as specific Windows users.
        /// </summary>
        /// <returns></returns>
        protected virtual TestServer CreateServer()
        {
            IWebHostBuilder builder = GetWebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddWindowsAuthenticationForTesting();

                    if (AddAuthenticationMiddleware)
                        services.AddTransient<IStartupFilter, AddAuthenticationStartupFilter>();
                });

            return new TestServer(builder)
            {
                BaseAddress = BaseAddress
            };
        }
    }
}