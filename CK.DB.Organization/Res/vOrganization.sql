create view CK.vOrganization
as
    select o.OrganizationId
          ,OrganizationName = g.GroupName
          ,w.AdminGroupId
          ,w.AclId
    from CK.tOrganization o
        inner join CK.tWorkspace w on w.WorkspaceId = o.OrganizationId
        inner join CK.tZone z on z.ZoneId = o.OrganizationId
        inner join CK.vGroup g on g.GroupId = z.ZoneId;
