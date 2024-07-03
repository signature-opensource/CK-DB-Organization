using CK.Core;
using CK.DB.Actor;
using CK.DB.Acl;
using CK.DB.Workspace;
using CK.SqlServer;
using static CK.Testing.DBSetupTestHelper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CK.DB.Organization.Tests
{
    [TestFixture]
    public class OrganizationTests
    {
        [Test]
        public async Task Can_create_Organization_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );
            organizationId.Should().BeGreaterThan( 0 );
        }

        [Test]
        public async Task Not_plateform_administrator_user_cannot_create_organization_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var userTable = services.GetRequiredService<UserTable>();
            var aclTable = services.GetRequiredService<AclTable>();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var userId = await userTable.CreateUserAsync( ctx, 1, NewGuid() );
            await aclTable.AclGrantSetAsync( ctx, 1, 1, userId, "Not plateform administrator", 8 );

            await organizationTable.Invoking( t => t.CreateOrganizationAsync( ctx, userId, NewGuid() ) )
                                   .Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task Plateform_administrator_user_can_create_organization_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var userTable = services.GetRequiredService<UserTable>();
            var aclTable = services.GetRequiredService<AclTable>();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var userId = await userTable.CreateUserAsync( ctx, 1, NewGuid() );
            await aclTable.AclGrantSetAsync( ctx, 1, 1, userId, "Plateform administrator", 112 );

            var organizationId = await organizationTable.CreateOrganizationAsync( ctx, userId, NewGuid() );
            organizationId.Should().BeGreaterThan( 0  );
        }

        [Test]
        public async Task plug_organization_create_an_organization_with_same_workspace_id_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var workspaceTable = services.GetRequiredService<WorkspaceTable>();
            var organiationTable = services.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() ); 

            organiationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", workspace.WorkspaceId ).Should().BeNull();

            await organiationTable.PlugOrganizationAsync( ctx, 1, workspace.WorkspaceId );

            organiationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", workspace.WorkspaceId ).Should().Be( 1 );
        }

        [Test]
        public async Task random_user_cannot_plug_an_organization_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();

            var userTable = services.GetRequiredService<UserTable>();
            var workspaceTable = services.GetRequiredService<WorkspaceTable>();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );
            int userId = await userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

            await organizationTable.Invoking( t => t.PlugOrganizationAsync( ctx, userId, workspace.WorkspaceId ) ).Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task unplug_organization_destroy_organization_but_let_workspace_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            int organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );

            organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", organizationId ).Should().Be( 1 );
            organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", organizationId ).Should().Be( 1 );

            await organizationTable.UnplugOrganizationAsync( ctx, 1, organizationId );
            organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", organizationId ).Should().Be( 1 );
            organizationTable.Database.ExecuteScalar<int?>( "select 1 from CK.tOrganization where OrganizationId = @0", organizationId ).Should().BeNull();
        }

        [Test]
        public async Task plug_organization_is_idempotent_Async()
        {
            using var service = TestHelper.CreateAutomaticServices();
            var workspaceTable = service.GetRequiredService<WorkspaceTable>();
            var organizationTable = service.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );
            OrganizationExists( workspaceTable, workspace.WorkspaceId ).Should().BeFalse();

            for( int i = 0; i < 10; i++ )
            {
                await organizationTable.PlugOrganizationAsync( ctx, 1, workspace.WorkspaceId );
                OrganizationExists( workspaceTable, workspace.WorkspaceId ).Should().BeTrue();
            }
        }

        [Test]
        public async Task unplug_organization_is_idempotent_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            var organizationId = await organizationTable.CreateOrganizationAsync( ctx, 1, NewGuid() );
            OrganizationExists( organizationTable, organizationId ).Should().BeTrue();

            for( int i = 0; i < 10; i++ )
            {
                await organizationTable.UnplugOrganizationAsync( ctx, 1, organizationId );
                OrganizationExists( organizationTable, organizationId ).Should().BeFalse();
            }
        }

        [Test]
        public async Task cannot_unplug_organizationId_0_Async()
        {
            using var services = TestHelper.CreateAutomaticServices();
            var organizationTable = services.GetRequiredService<OrganizationTable>();

            using SqlStandardCallContext ctx = new( TestHelper.Monitor );

            await organizationTable.Invoking( t => t.UnplugOrganizationAsync( ctx, 1, 0 ) )
                .Should().ThrowAsync<Exception>();
        }

        static string NewGuid() => Guid.NewGuid().ToString();

        static bool OrganizationExists( SqlPackage pkg, int organizationId )
            => pkg.Database.ExecuteScalar<int>(
                @"select isnull( (select 1 from CK.tOrganization where OrganizationId = @0), 0 );",
                organizationId ) > 0;
    }
}
