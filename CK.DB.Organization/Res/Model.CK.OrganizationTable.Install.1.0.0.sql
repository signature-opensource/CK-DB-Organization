create table CK.tOrganization
(
    OrganizationId int not null
        constraint PK_tOrganization primary key clustered
        constraint FK_tOrganization_OrganizationId foreign key references CK.tWorkspace( WorkspaceId )
);

insert into CK.tOrganization values( 0 );
