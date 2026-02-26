using L5Sharp.Core;
using LogixDb.Data.Abstractions;

namespace LogixDb.Data.Maps;

/// <summary>
/// Represents a mapping configuration for the "controller" table within the database.
/// This class defines the schema of the table, including the table name and the columns
/// that map to the properties of the <see cref="Controller"/> class.
/// </summary>
public class ControllerMap : TableMap<Controller>
{
    /// <inheritdoc />
    public override string TableName => "controller";

    /// <inheritdoc />
    public override IReadOnlyList<ColumnMap<Controller>> Columns =>
    [
        ColumnMap<Controller>.For(t => t.Name, "controller_name"),
        ColumnMap<Controller>.For(t => t.ProcessorType, "processor"),
        ColumnMap<Controller>.For(t => t.Revision?.ToString(), "revision"),
        ColumnMap<Controller>.For(t => t.Description, "description"),
        ColumnMap<Controller>.For(t => t.ProjectCreationDate, "project_creation_date"),
        ColumnMap<Controller>.For(t => t.LastModifiedDate, "last_modified_date"),
        ColumnMap<Controller>.For(t => t.CommPath, "comm_path"),
        ColumnMap<Controller>.For(t => t.SFCExecutionControl?.Name, "sfc_execution_control"),
        ColumnMap<Controller>.For(t => t.SFCRestartPosition?.Name, "sfc_restart_position"),
        ColumnMap<Controller>.For(t => t.SFCLastScan?.Name, "sfc_last_scan"),
        ColumnMap<Controller>.For(t => t.ProjectSN, "project_sn"),
        ColumnMap<Controller>.For(t => t.MatchProjectToController, "match_project_to_controller"),
        ColumnMap<Controller>.For(t => t.InhibitAutomaticFirmwareUpdate, "inhibit_firmware_updates"),
        ColumnMap<Controller>.For(t => t.CanUseRPIFromProducer, "allow_rfi_from_producer"),
        ColumnMap<Controller>.For(t => t.PassThroughConfiguration?.Name, "pass_through"),
        ColumnMap<Controller>.For(t => t.DownloadProjectDocumentationAndExtendedProperties, "download_documentation"),
        ColumnMap<Controller>.For(t => t.DownloadProjectCustomProperties, "download_properties"),
        //todo once this is added to L5Sharp
        //ColumnMap<Controller>.For(t => t.EthernetIPMode?.Name, "ethernet_ip_mode")
    ];
}