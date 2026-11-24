Implements the `CK.tOrganization` table on top of CK.DB.Workspace: an organization is a workspace, either created with
it or an existing workspace plugged as an organization (and unplugged, the workspace remaining).

`OrganizationTable` creates, plugs and unplugs the organizations (`OrganizationTable.CreateOrganizationAsync`,
`OrganizationTable.PlugOrganizationAsync`, `OrganizationTable.UnplugOrganizationAsync`); the `CK.vOrganization` view
lists them.

See the [README](https://github.com/signature-opensource/CK-DB-Organization/blob/stable/README.md).
