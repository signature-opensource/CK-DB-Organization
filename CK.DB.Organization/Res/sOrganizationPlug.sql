-- SetupConfig: {}
create procedure CK.sOrganizationPlug
(
    @ActorId int,
    @WorkspaceId int
)
as
begin
    if CK.fAclGrantLevel( @ActorId, 1 ) < 112 throw 50000, 'Security.MustBeSafeAdminOnSystemAcl', 1;

    --[beginsp]

    if not exists( select 1 from CK.tOrganization where OrganizationId = @WorkspaceId )
    begin
        --<PrePlug revert />

        -- Inserting the Organization.
        insert into CK.tOrganization( OrganizationId ) values( @WorkspaceId );

        --<PostPlug />
    end
    --[endsp]
end
