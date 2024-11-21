using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Organization;

[SqlTable( "tOrganization", Package = typeof( Package ), ResourcePath = "Res" )]
[Versions( "1.0.0" )]
[SqlObjectItem( "vOrganization" )]
public abstract class OrganizationTable : SqlTable
{
    void StObjConstruct( CK.DB.Workspace.WorkspaceTable workspaceTable )
    {
    }

    /// <summary>
    /// Creates an organization.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="organizationName">The organization name.</param>
    /// <returns>The organization identifier.</returns>
    [SqlProcedure( "sOrganizationCreate" )]
    public abstract Task<int> CreateOrganizationAsync( ISqlCallContext ctx, int actorId, string organizationName );

    /// <summary>
    /// Plug an organization to an existing workspace.
    /// <para>
    /// This is (by default) possible only for global Administrators (members of the Administrator group
    /// which has the special reserved identifer 2).
    /// </para>
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="workspaceId">The workspace to decorate.</param>
    /// <returns></returns>
    [SqlProcedure( "sOrganizationPlug" )]
    public abstract Task PlugOrganizationAsync( ISqlCallContext ctx, int actorId, int workspaceId );

    /// <summary>
    /// Unplug the Organization.
    /// <para>
    /// This is possible only for plateform administrators (i.e. the <paramref name="actorId"/> must have at least Safe Administrator level (112)
    /// on the SystemAcl (1).
    /// </para>
    /// The Worksapce will not be destroy.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="organizationId">The organization identifier.</param>
    [SqlProcedure( "sOrganizationUnplug" )]
    public abstract Task UnplugOrganizationAsync( ISqlCallContext ctx, int actorId, int organizationId );
}
