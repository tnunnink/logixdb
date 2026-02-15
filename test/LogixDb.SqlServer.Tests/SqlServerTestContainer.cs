using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;
using Testcontainers.MsSql;

namespace LogixDb.SqlServer.Tests;

/// <summary>
/// A utility class responsible for setting up and managing a SQL Server test environment
/// using Testcontainers. This class provides methods for initializing and cleaning up
/// a containerized instance of SQL Server, enabling integration testing against an
/// isolated database environment.
/// </summary>
[SetUpFixture]
public static class SqlServerTestContainer
{
    /// <summary>
    /// Represents a test container instance for managing a Microsoft SQL Server database lifecycle
    /// within the SqlServerTestFixture. This container facilitates integration testing by providing
    /// an isolated and disposable SQL Server environment.
    /// </summary>
    private static MsSqlContainer _container;

    /// <summary>
    /// Provides an instance of <see cref="ILogixDb"/> for interacting with the LogixDb database
    /// within the context of integration tests using the SqlServerTestContainer.
    /// </summary>
    /// <remarks>
    /// This property is initialized during the test lifecycle setup and represents a configured
    /// database connection to a SQL Server instance running in a container. It allows for executing
    /// database operations such as migrations, snapshots, and data interactions during tests.
    /// </remarks>
    public static ILogixDb Database { get; private set; }

    /// <summary>
    /// Performs a one-time setup for initializing the SQL Server test environment. This includes
    /// creating and starting a SQL Server container using Testcontainers, configuring the
    /// connection details, and initializing a database instance to be used in integration tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("LogixDb!Test123")
            .Build();

        await _container.StartAsync();

        var connection = new SqlConnectionInfo(
            SqlProvider.SqlServer,
            _container.Hostname,
            Port: _container.GetMappedPublicPort(),
            User: "sa",
            Password: "LogixDb!Test123"
        );

        Database = new SqlServerDb(connection);
    }

    /// <summary>
    /// Performs a one-time teardown for cleaning up the SQL Server test environment. This includes
    /// disposing of the database instance and stopping and disposing of the SQL Server container.
    /// This method ensures that all resources used in the integration tests are properly released.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}