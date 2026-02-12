using L5Sharp.Core;
using LogixDb.Core.Abstractions;
using LogixDb.Core.Common;

namespace LogixDb.Core.Maps;

public class ControllerMap : TableMap<Controller>
{
    // <inheritdoc />
    protected override string TableName => "controller";

    // <inheritdoc />
    public override IReadOnlyList<ColumnMap<Controller>> Columns =>
    [
        ColumnMap<Controller>.For(t => t.Name, "name"),
    ];
}