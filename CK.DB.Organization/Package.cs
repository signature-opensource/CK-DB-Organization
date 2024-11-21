using CK.Core;

namespace CK.DB.Organization;

[SqlPackage( Schema = "CK", ResourcePath = "Res" )]
[Versions( "1.0.0" )]
public abstract class Package : SqlPackage
{
    void StObjConstruct( CK.DB.Workspace.Package workspacePackage )
    {
    }

    [InjectObject]
    public OrganizationTable OrganizationTable { get; private set; }
}
