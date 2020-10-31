/*
 * This file is part of OpenCollar.Extensions.SqlClient.
 *
 * OpenCollar.Extensions.SqlClient is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Extensions is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Extensions.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using System;
using System.IO;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenCollar.Extensions.Environment;
using OpenCollar.Extensions.SqlClient;

using TestRig.DatabaseConnections;

namespace TestRig
{
    internal class Program
    {
        /// <summary>
        ///     Gets the object from which to read configuration.
        /// </summary>
        /// <value>
        ///     The object from which to read configuration.
        /// </value>

        public static IConfigurationRoot Configuration { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this process is in the Azure cloud.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if this process is in the Azure cloud; otherwise, <see langword="false" />.
        /// </value>
        private static bool IsInAzure => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(@"WEBSITE_HOME_STAMPNAME"));

        public static async Task<string> TestDbExecuteAsync(ConnectionProxy connection)
        {
            var result = await connection.QueryProcedure("[configuration].[AddOrUpdateConfigurationValue]")
                    .WithParameter("@key", "key-1")
                    .WithParameter("@value", "VALUE-1")
                    .Read(r =>
                    {
                        if(r.Read())
                        {
                            return r.GetValueOrFail<string>(0);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }).ExecuteQueryAsync<string>().ConfigureAwait(false);

            return result;
        }

        /// <summary>
        ///     Configures the configuration services.
        /// </summary>
        /// <param name="configBuilder">
        ///     The configuration builder.
        /// </param>
        /// <exception cref="OpenCollar.Extensions.Configuration.ConfigurationException">
        /// </exception>
        private static void ConfigureConfiguration(IConfigurationBuilder configBuilder)
        {
            var isInAzure = IsInAzure;

            string rootDirectory;
            if(isInAzure)
            {
                rootDirectory = System.IO.Path.Combine(System.Environment.GetEnvironmentVariable(@"HOME"), @"site", @"wwwroot");
            }
            else
            {
                rootDirectory = System.Environment.CurrentDirectory;
            }

            // Order is important and appsettings should come after host
            configBuilder.SetBasePath(rootDirectory)
                .AddJsonFile(@"appsettings.json", false, true)
                .AddEnvironmentVariables();

            // Only use the developer settings if the debugger is attached and the file exists.
            if(!isInAzure && System.Diagnostics.Debugger.IsAttached)
            {
                // We don't get development app settings for free on Function Apps, but we can emulate it easily enough.
                var devConfigPath = System.IO.Path.Combine(rootDirectory, @"appsettings.Development.json");
                if(File.Exists(devConfigPath))
                {
                    configBuilder.AddJsonFile(devConfigPath, true, true);
                }
            }
        }

        /// <summary>
        ///     Configures the logging service.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration service.
        /// </param>
        /// <param name="builder">
        ///     The builder to configure.
        /// </param>
        /// <exception cref="Wtw.Crb.CarrierPortal.Services.Configuration.ConfigurationException">
        /// </exception>
        private static void ConfigureLogging(IConfiguration configuration, ILoggingBuilder builder)
        {
            // ReSharper disable PossibleNullReferenceException
            builder.AddConfiguration(configuration.GetSection(@"Logging"));
            if(!IsInAzure && System.Diagnostics.Debugger.IsAttached)
            {
                // Add debugger logging only when a debugger is attached.
                builder.AddConsole();

                // builder.AddDebug(); <--- This seems to break the app when run from func.exe on the desktop, at least
                // it does for me (JDE).
                builder.SetMinimumLevel(LogLevel.Trace);
            }
        }

        private static void ConfigureServices(ServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddLogging(logging => ConfigureLogging(configuration, logging));
            serviceCollection.AddApplicationService<SampleEnvironmentMetadataProvider>();
            serviceCollection.AddConnectionFactory<DefaultConnectionFactory>();
        }

        private static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();

            ConfigureConfiguration(configBuilder);

            // ReSharper disable once AssignNullToNotNullAttribute
            Configuration = configBuilder.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Configuration);

            ConfigureServices(serviceCollection, Configuration);

            var services = serviceCollection.BuildServiceProvider();

            var application = services.GetService<IApplicationService>();

            var factory = services.GetService<DefaultConnectionFactory>();

            var connection = factory.GetConnection("jevans@open-collar.org.uk");

            var value = TestDbExecuteAsync(connection).Result;
        }
    }
}