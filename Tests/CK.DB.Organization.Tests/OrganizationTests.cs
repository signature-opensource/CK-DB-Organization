using CK.Core;
using CK.DB.Actor;
using CK.DB.Acl;
using CK.DB.Workspace;
using CK.SqlServer;
using static CK.Testing.MonitorTestHelper;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using CK.Testing;

namespace CK.DB.Organization.Tests;

[TestFixture]
public class OrganizationTests
{
    [Test]
    public async Task Can_create_Organization_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

        var organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );
        organizationId.ShouldBeGreaterThan( 0 );
    }

    [Test]
    public async Task Not_plateform_administrator_user_cannot_create_organization_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var userTable = services.GetRequiredService<UserTable>();
        var aclTable = services.GetRequiredService<AclTable>();
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

        var userId = await userTable.CreateUserAsync( ctx, 1, NewGuid() );
        await aclTable.AclGrantSetAsync( ctx, 1, 1, userId, "Not plateform administrator", 8 );

        await Util.Invokable( () => organizationTable.CreateOrganizationAsync( ctx, userId, NewGuid() ) )
                               .ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task Plateform_administrator_user_can_create_organization_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var userTable = services.GetRequiredService<UserTable>();
        var aclTable = services.GetRequiredService<AclTable>();
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

        var userId = await userTable.CreateUserAsync( ctx, 1, NewGuid() );
        await aclTable.AclGrantSetAsync( ctx, 1, 1, userId, "Plateform administrator", 112 );

        var organizationId = await organizationTable.CreateOrganizationAsync( ctx, userId, NewGuid() );
        organizationId.ShouldBeGreaterThan( 0 );
    }

    [Test]
    public async Task plug_organization_create_an_organization_with_same_workspace_id_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();
        var organiationTable = services.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );

        organiationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", workspace.WorkspaceId ).ShouldBeNull();

        await organiationTable.PlugOrganizationAsync( ctx, 1, workspace.WorkspaceId );

        organiationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", workspace.WorkspaceId ).ShouldBe( 1 );
    }

    [Test]
    public async Task random_user_cannot_plug_an_organization_Async()
    {
        var services = SharedEngine.AutomaticServices;

        var userTable = services.GetRequiredService<UserTable>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );
        int userId = await userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

        await Util.Invokable( () => organizationTable.PlugOrganizationAsync( ctx, userId, workspace.WorkspaceId ) ).ShouldThrowAsync<Exception>();
    }

    [Test]
    public async Task unplug_organization_destroy_organization_but_let_workspace_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        int organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );

        organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", organizationId ).ShouldBe( 1 );
        organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", organizationId ).ShouldBe( 1 );

        await organizationTable.UnplugOrganizationAsync( ctx, 1, organizationId );
        organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", organizationId ).ShouldBe( 1 );
        organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", organizationId ).ShouldBeNull();
    }

    [Test]
    public async Task plug_organization_is_idempotent_Async()
    {
        var service = SharedEngine.AutomaticServices;
        var workspaceTable = service.GetRequiredService<WorkspaceTable>();
        var organizationTable = service.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );
        OrganizationExists( workspaceTable, workspace.WorkspaceId ).ShouldBeFalse();

        for( int i = 0; i < 10; i++ )
        {
            await organizationTable.PlugOrganizationAsync( ctx, 1, workspace.WorkspaceId );
            OrganizationExists( workspaceTable, workspace.WorkspaceId ).ShouldBeTrue();
        }
    }

    [Test]
    public async Task unplug_organization_is_idempotent_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        var organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );
        OrganizationExists( organizationTable, organizationId ).ShouldBeTrue();

        for( int i = 0; i < 10; i++ )
        {
            await organizationTable.UnplugOrganizationAsync( ctx, 1, organizationId );
            OrganizationExists( organizationTable, organizationId ).ShouldBeFalse();
        }
    }

    [Test]
    public async Task cannot_unplug_organizationId_0_Async()
    {
        var services = SharedEngine.AutomaticServices;
        var organizationTable = services.GetRequiredService<OrganizationTable>();

        using SqlStandardCallContext ctx = new( TestHelper.Monitor );

        await Util.Invokable( () => organizationTable.UnplugOrganizationAsync( ctx, 1, 0 ) )
            .ShouldThrowAsync<Exception>();
    }

    static string NewGuid() => Guid.NewGuid().ToString();

    static bool OrganizationExists( SqlPackage pkg, int organizationId )
        => pkg.Database.ExecuteScalar<int>(
            @"select isnull( (select 1 from CK.tOrganization where OrganizationId = @0), 0 );",
            organizationId ) > 0;
}
