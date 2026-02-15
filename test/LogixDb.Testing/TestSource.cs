using System.Reflection;
using Bogus;
using L5Sharp.Core;

namespace LogixDb.Testing;

/// <summary>
/// Provides factory methods for creating test L5X content for unit testing purposes.
/// Supports both custom configuration and auto-generated fake data using the Bogus library.
/// </summary>
public static class TestSource
{
    private const string LocalExampleFile = @"C:\Users\tnunn\Documents\L5X\Example.L5X";

    /// <summary>
    /// Creates a new L5X instance with custom configuration applied.
    /// Initializes a base L5X with default test settings and allows the caller to customize it.
    /// </summary>
    /// <param name="config">An action that configures the L5X instance with custom test data.</param>
    /// <returns>A configured L5X instance ready for testing.</returns>
    public static L5X Custom(Action<L5X> config)
    {
        var content = L5X.New("Test", "1756-L83E", 33.1);
        config.Invoke(content);
        return content;
    }

    /// <summary>
    /// Creates a new L5X instance populated with auto-generated fake data using the Bogus library.
    /// Generates test data types and optionally applies additional custom configuration.
    /// </summary>
    /// <param name="config">An optional action that applies additional configuration to the L5X instance.</param>
    /// <returns>An L5X instance populated with fake test data.</returns>
    public static L5X Fake(Action<L5X>? config = null)
    {
        var file = L5X.New("Test", "1756-L83E", 33.1);

        file.DataTypes.AddRange(CreateFakeDataTypes(10));
        //todo implement more fake component 

        // Apply optional config.
        config?.Invoke(file);
        return file;
    }

    /// <summary>
    /// Loads an L5X instance from the specified file path.
    /// Reads and parses the L5X XML content from the provided file path into a usable instance.
    /// </summary>
    /// <param name="filePath">The path of the file containing the L5X content to be loaded.</param>
    /// <returns>An L5X instance loaded with content from the specified file.</returns>
    public static L5X Load(string filePath)
    {
        return L5X.Load(filePath);
    }

    /// <summary>
    /// Loads a predefined L5X instance from a specified local file path.
    /// This method is intended for creating a test L5X instance sourced from a consistent local file.
    /// </summary>
    /// <returns>An L5X instance loaded from the predefined local file path.</returns>
    public static L5X LocalTest()
    {
        const string resourceName = "LogixDb.Testing.Test.L5X";
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        var xml = reader.ReadToEnd();
        return L5X.Parse(xml);
    }

    /// <summary>
    /// Loads an L5X instance from a predefined example file path.
    /// Reads and parses the L5X XML content from the example file into a usable instance.
    /// </summary>
    /// <returns>An L5X instance loaded with content from the predefined example file.</returns>
    public static L5X LocalExample()
    {
        return File.Exists(LocalExampleFile) ? L5X.Load(LocalExampleFile) : LocalTest();
    }

    /// <summary>
    /// Generates a collection of fake DataType instances using the Bogus library.
    /// Each data type is populated with random names, families, classes, and descriptions.
    /// </summary>
    /// <param name="count">The number of fake DataType instances to generate.</param>
    /// <returns>An enumerable collection of fake DataType instances.</returns>
    private static List<DataType> CreateFakeDataTypes(int count)
    {
        var faker = new Faker<DataType>()
            .CustomInstantiator(f => new DataType
            {
                Name = f.Random.AlphaNumeric(10),
                Family = f.PickRandom<DataTypeFamily>(),
                Class = f.PickRandom<DataTypeClass>(),
                Description = f.Lorem.Sentence()
            });

        return faker.Generate(count);
    }
}