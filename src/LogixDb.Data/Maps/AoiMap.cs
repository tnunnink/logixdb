using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "aoi" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="AddOnInstruction"/> class.
/// </summary>
public class AoiMap : TableMap<AddOnInstruction>
{
    /// <inheritdoc />
    public override string TableName => "aoi";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<AddOnInstruction>> Columns =>
    [
        ColumnMap<AddOnInstruction>.For(a => a.Name, "aoi_name"),
        ColumnMap<AddOnInstruction>.For(a => a.Revision?.ToString(), "revision"),
        ColumnMap<AddOnInstruction>.For(a => a.RevisionExtension, "revision_extension"),
        ColumnMap<AddOnInstruction>.For(a => a.RevisionNote, "revision_note"),
        ColumnMap<AddOnInstruction>.For(a => a.Vendor, "vendor"),
        ColumnMap<AddOnInstruction>.For(a => a.Description, "description"),
        ColumnMap<AddOnInstruction>.For(a => a.ExecutePreScan, "execute_pre_scan"),
        ColumnMap<AddOnInstruction>.For(a => a.ExecutePostScan, "execute_post_scan"),
        ColumnMap<AddOnInstruction>.For(a => a.ExecuteEnableInFalse, "execute_enable_in_false"),
        ColumnMap<AddOnInstruction>.For(a => a.CreatedDate, "created_date"),
        ColumnMap<AddOnInstruction>.For(a => a.CreatedBy, "created_by"),
        ColumnMap<AddOnInstruction>.For(a => a.EditedDate, "edited_date"),
        ColumnMap<AddOnInstruction>.For(a => a.EditedBy, "edited_by"),
        ColumnMap<AddOnInstruction>.For(a => a.SoftwareRevision?.ToString(), "software_revision"),
        ColumnMap<AddOnInstruction>.For(a => a.AdditionalHelpText, "help_text"),
        ColumnMap<AddOnInstruction>.For(a => a.IsEncrypted, "is_encrypted"),
        //todo SQL Server can't import the default date 1/1/0001. It has a minimum range. Need to find a general solution for dates.
        /*ColumnMap<AddOnInstruction>.For(a => a.SignatureID, "signature_id"),
        ColumnMap<AddOnInstruction>.For(a => a.SignatureTimestamp, "signature_timestamp"),*/
        ColumnMap<AddOnInstruction>.For(a => a.Class?.Name, "component_class")
    ];
}