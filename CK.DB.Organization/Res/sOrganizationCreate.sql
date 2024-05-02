create procedure CK.sOrganizationCreate
(
    @ActorId int,
    @OrganizationName nvarchar( 128 ),
    @OrganizationIdResult int output
)
as
begin
    -- No need to check Actor grant level because, check is in sWorkspacePlug + it's a transaction

    --[beginsp]
    
    --<PreCreate revert />

    exec CK.sWorkspaceCreate @ActorId, @OrganizationName, @OrganizationIdResult output;

    insert into CK.tOrganization values( @OrganizationIdResult );

    --<PostCreate />

    --[endsp]
end
