-- SetupConfig: { "Requires": [] }
create procedure CK.sOrganizationUnplug
(
    @ActorId int, -- not null
    @OrganizationId int -- not null
)
as
begin
    if @OrganizationId <= 0 throw 50000, 'Organization.InvalidOrganizationId', 1;
    if CK.fAclGrantLevel( @ActorId, 1 ) < 112 throw 50000, 'Security.MustBeSafeAdminOnSystemAcl', 1;

    -- Only if the organization exists, try to unplug it...
    if exists( select 1 from CK.tOrganization where OrganizationId = @OrganizationId )
    begin
        --[beginsp]
    
        --<PreUnplug revert />
    
        -- Delete organization
        delete from CK.tOrganization where OrganizationId = @OrganizationId;
    
        --<PostUnplug />
    
        --[endsp]
    end
end
