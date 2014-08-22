using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.UserSecurity
{
    /// <summary>
    /// These below defined classes are corresponding to the User security tables.
    /// </summary>
    
    [CollectionDataContract(Name = "sites", Namespace = "http://www.iringtools.org/library", ItemName = "site")]
    public class Sites : List<Site>
    {

    }

    [DataContract(Name = "site", Namespace = "http://www.iringtools.org/library")]
    public class Site
    {
        [DataMember(Name = "siteId", Order = 0)]
        public int SiteId { get; set; }

        [DataMember(Name = "siteName", Order = 1, EmitDefaultValue = false)]
        public string SiteName { get; set; }

        [DataMember(Name = "siteDesc", Order = 2, EmitDefaultValue = false)]
        public string SiteDesc { get; set; }

        [DataMember(Name = "active", Order = 3, EmitDefaultValue = false)]
        public Byte Active { get; set; }

    }

    [CollectionDataContract(Name = "groups", Namespace = "http://www.iringtools.org/library", ItemName = "group")]
    public class Groups : List<Group>
    {

    }

    [DataContract(Name = "group", Namespace = "http://www.iringtools.org/library")]
    public class Group
    {
        [DataMember(Name = "groupId", Order = 0)]
        public int GroupId { get; set; }

        [DataMember(Name = "siteId", Order = 1, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "groupName", Order = 2, EmitDefaultValue = false)]
        public string GroupName { get; set; }

        [DataMember(Name = "groupDesc", Order = 3, EmitDefaultValue = false)]
        public string GroupDesc { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }

    }

    [CollectionDataContract(Name = "users", Namespace = "http://www.iringtools.org/library", ItemName = "user")]
    public class Users : List<User>
    {

    }

    [DataContract(Name = "user", Namespace = "http://www.iringtools.org/library")]
    public class User
    {
        [DataMember(Name = "userId", Order = 0)]
        public int UserId { get; set; }

        [DataMember(Name = "userName", Order = 1, EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(Name = "siteId", Order = 2, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "userFirstName", Order = 3, EmitDefaultValue = false)]
        public string UserFirstName { get; set; }

        [DataMember(Name = "userLastName", Order = 4, EmitDefaultValue = false)]
        public string UserLastName { get; set; }
        
        [DataMember(Name = "userFullName", Order = 5, EmitDefaultValue = false)]
        public string UserFullName { get; set; }
        
        [DataMember(Name = "userEmail", Order = 6, EmitDefaultValue = false)]
        public string UserEmail { get; set; }

        [DataMember(Name = "userPhone", Order = 7, EmitDefaultValue = false)]
        public string UserPhone { get; set; }

        [DataMember(Name = "userDesc", Order = 8, EmitDefaultValue = false)]
        public string UserDesc { get; set; }

        [DataMember(Name = "active", Order = 9, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }

    [CollectionDataContract(Name = "roles", Namespace = "http://www.iringtools.org/library", ItemName = "role")]
    public class Roles : List<Role>
    {

    }

    [DataContract(Name = "role", Namespace = "http://www.iringtools.org/library")]
    public class Role
    {
        [DataMember(Name = "roleId", Order = 0)]
        public int RoleId { get; set; }

        [DataMember(Name = "siteId", Order = 1, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "roleName", Order = 2, EmitDefaultValue = false)]
        public string RoleName { get; set; }

        [DataMember(Name = "roleDesc", Order = 3, EmitDefaultValue = false)]
        public string RoleDesc { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }

    [CollectionDataContract(Name = "permissions", Namespace = "http://www.iringtools.org/library", ItemName = "permission")]
    public class Permissions : List<Permission>
    {

    }

    [DataContract(Name = "permission", Namespace = "http://www.iringtools.org/library")]
    public class Permission
    {
        [DataMember(Name = "permissionId", Order = 0)]
        public int PermissionId { get; set; }

        [DataMember(Name = "siteId", Order = 1, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "permissionName", Order = 2, EmitDefaultValue = false)]
        public string PermissionName { get; set; }

        [DataMember(Name = "permissionDesc", Order = 3, EmitDefaultValue = false)]
        public string PermissionDesc { get; set; }

        [DataMember(Name = "active", Order = 4, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }

    [CollectionDataContract(Name = "userGroups", Namespace = "http://www.iringtools.org/library", ItemName = "userGroup")]
    public class UserGroups : List<UserGroup>
    {

    }

    [DataContract(Name = "userGroup", Namespace = "http://www.iringtools.org/library")]
    public class UserGroup
    {
        [DataMember(Name = "userGroupId", Order = 0)]
        public int UserGroupId { get; set; }

        [DataMember(Name = "groupId", Order = 1, EmitDefaultValue = false)]
        public int GroupId { get; set; }

        [DataMember(Name = "userId", Order = 2, EmitDefaultValue = false)]
        public int UserId { get; set; }

        [DataMember(Name = "siteId", Order = 3, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        [DataMember(Name = "userGroupsDesc", Order = 4, EmitDefaultValue = false)]
        public string UserGroupsDesc { get; set; }

        [DataMember(Name = "active", Order = 5, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }

    [CollectionDataContract(Name = "groupRoles", Namespace = "http://www.iringtools.org/library", ItemName = "groupRole")]
    public class GroupRoles : List<GroupRole>
    {

    }

    [DataContract(Name = "groupRole", Namespace = "http://www.iringtools.org/library")]
    public class GroupRole
    {
        //[DataMember(Name = "groupRoleId", Order = 0)]
        //public int GroupRoleId { get; set; }

        [DataMember(Name = "groupId", Order = 0, EmitDefaultValue = false)]
        public int GroupId { get; set; }

        [DataMember(Name = "roleId", Order = 1, EmitDefaultValue = false)]
        public int RoleId { get; set; }

        [DataMember(Name = "siteId", Order = 2, EmitDefaultValue = false)]
        public int SiteId { get; set; }

        //[DataMember(Name = "groupRolesDesc", Order = 4, EmitDefaultValue = false)]
        //public string GroupRolesDesc { get; set; }

        [DataMember(Name = "active", Order = 3, EmitDefaultValue = false)]
        public Byte Active { get; set; }
    }
}
