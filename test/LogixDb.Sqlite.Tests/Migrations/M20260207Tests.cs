namespace LogixDb.Sqlite.Tests.Migrations;

[TestFixture]
public class M20260207Tests : SqliteMigrationTestBase
{
    [Test]
    public void MigrateUp_ToM003_CreatesTagTableWithExpectedColumns()
    {
        MigrateUp(toVersion: 202602070830);

        using var connection = OpenConnection();

        using (Assert.EnterMultipleScope())
        {
            AssertTableExists(connection, "tag");

            AssertColumnDefinition(connection, "tag", "tag_id", "integer");
            AssertColumnDefinition(connection, "tag", "snapshot_id", "integer");
            AssertColumnDefinition(connection, "tag", "base_name", "text");
            AssertColumnDefinition(connection, "tag", "tag_name", "text");
            AssertColumnDefinition(connection, "tag", "scope_type", "text");
            AssertColumnDefinition(connection, "tag", "container_name", "text");
            AssertColumnDefinition(connection, "tag", "tag_depth", "integer");
            AssertColumnDefinition(connection, "tag", "data_type", "text");
            AssertColumnDefinition(connection, "tag", "value", "text");
            AssertColumnDefinition(connection, "tag", "description", "text");
            AssertColumnDefinition(connection, "tag", "dimensions", "text");
            AssertColumnDefinition(connection, "tag", "external_access", "text");
            AssertColumnDefinition(connection, "tag", "opcua_access", "text");
            AssertColumnDefinition(connection, "tag", "radix", "text");
            AssertColumnDefinition(connection, "tag", "constant", "integer");
            AssertColumnDefinition(connection, "tag", "tag_type", "text");
            AssertColumnDefinition(connection, "tag", "tag_usage", "text");
            AssertColumnDefinition(connection, "tag", "alias", "text");
            AssertColumnDefinition(connection, "tag", "component_class", "text");
            AssertColumnDefinition(connection, "tag", "value_hash", "text");
            AssertColumnDefinition(connection, "tag", "record_hash", "text");

            AssertPrimaryKey(connection, "tag", "tag_id");
            AssertForeignKey(connection, "tag", "snapshot_id", "snapshot", "snapshot_id");
            AssertUniqueIndex(connection, "tag", "snapshot_id", "container_name", "tag_name");
        }
    }
}