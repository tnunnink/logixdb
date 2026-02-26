using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "module" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Module"/> class.
/// </summary>
public class ModuleMap : TableMap<Module>
{
    /// <inheritdoc />
    public override string TableName => "module";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Module>> Columns =>
    [
        ColumnMap<Module>.For(m => m.Name, "module_name"),
        ColumnMap<Module>.For(m => m.CatalogNumber, "catalog_number"),
        ColumnMap<Module>.For(m => m.Revision?.ToString(), "revision"),
        ColumnMap<Module>.For(m => m.Description, "description"),
        ColumnMap<Module>.For(m => m.Vendor?.Id ?? 0, "vendor_id"),
        ColumnMap<Module>.For(m => m.ProductType?.Id ?? 0, "product_id"),
        ColumnMap<Module>.For(m => m.ProductCode, "product_code"),
        ColumnMap<Module>.For(m => m.ParentModule, "parent_name"),
        ColumnMap<Module>.For(m => m.ParentModPortId, "parent_port"),
        ColumnMap<Module>.For(m => m.Keying?.Name, "electronic_keying"),
        ColumnMap<Module>.For(m => m.Inhibited, "inhibited"),
        ColumnMap<Module>.For(m => m.MajorFault, "major_fault"),
        ColumnMap<Module>.For(m => m.SafetyEnabled, "safety_enabled"),
        ColumnMap<Module>.For(m => m.IP?.ToString(), "ip_address"),
        ColumnMap<Module>.For(m => m.Slot, "slot_number")
    ];
}