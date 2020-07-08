﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// A repository class for maintaining instances of DomainUser (which inherits from IdentityUser).
    /// NOTE: This class is one of the "Domain Identity" classes (DomainUserManager, 
    /// DomainRoleManager, DomainUser, DomainRole, IdentityApplication, and IdentityOrganization),
    /// which supports centralized management of user security across different applications and
    /// organizations. This is not a multi-tenant architecture, but an architecture that allows
    /// user management across applications and allows user management to be delegated 
    /// to organization admins.
    /// </summary>
    /// <typeparam name="TUser">DomainUser or subclass</typeparam>
    public class DomainUserManager<TUser> : AspNetUserManager<TUser> 
        where TUser: DomainUser, new() {
        public DomainUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, 
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
            IServiceProvider services, ILogger<UserManager<TUser>> logger) 
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
        }

        public virtual async Task<IEnumerable<TRole>> GetRolesAsync<TRole>(TUser user, string applicationName)
            where TRole : DomainRole {

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetRolesAsync(TUser user, string applicationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            var qry = store.Context.Set<TRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetUserRoles ur
    on r.Id = ur.RoleId
  inner join AspNetUsers u
    on u.Id = ur.UserId
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where a.Name = {applicationName}
    and a.Id = {user.Id}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TRole>> GetRolesAsync<TRole>(TUser user, string[] applicationNames)
            where TRole : DomainRole {

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetRolesAsync(TUser user, string applicationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            var qry = store.Context.Set<TRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetUserRoles ur
    on r.Id = ur.RoleId
  inner join AspNetUsers u
    on u.Id = ur.UserId
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where a.Name In({string.Join(',',applicationNames.Select(a=>$"'{a}'"))})
    and a.Id = {user.Id}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TUser>> GetUsersForOrganizationAsync(int organizationId, int pageNumber = 1, int pageSize = 100) {
            if(!SupportsQueryableUsers)
                throw new Exception("Cannot use DomainUserManager.GetUsersForOrganizationAsync(int organizationId, int pageNumber, int pageSize) without Queryable Users.");

            var (skip, take) = ((pageNumber - 1) * pageSize, pageSize);

            var qry = Users
                .AsNoTracking()
                .Where(u => u.OrganizationId == organizationId)
                .Skip(skip)
                .Take(take);

            return await qry.ToListAsync();
                
        }

        public virtual async Task<IEnumerable<TUser>> GetUsersForOrganizationAsync(string organizationName) {


            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForOrganizationAsync(string organizationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  inner join AspNetOrganizations o
    on o.Id = u.OrganizationId
  where o.Name = {organizationName}
            ").AsNoTracking();

            return await qry.ToListAsync();

        }

        public virtual async Task<IEnumerable<TUser>> GetUsersForOrganizationAsync(string organizationName, int pageNumber, int pageSize) {

            var (skip, take) = ((pageNumber - 1) * pageSize, pageSize);

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForOrganizationAsync(string organizationName, int pageNumber, int pageSize) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            if(!store.Context.Database.ProviderName.Contains("SqlServer") && !store.Context.Database.ProviderName.Contains("Oracle"))
                throw new Exception("Cannot use DomainUserManager.GetUsersForOrganizationAsync(string organizationName, int pageNumber, int pageSize) without SqlServer or Oracle providers");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  inner join AspNetOrganizations o
    on o.Id = u.OrganizationId
  where o.Name = {organizationName}
  offset {skip} rows
  fetch next {take} rows only
            ").AsNoTracking();

            return await qry.ToListAsync();

        }

        public virtual async Task<IEnumerable<TUser>> GetUsersForApplicationAsync(string applicationName) {

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(string applicationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  where exists (
    select 0
      from AspNetRoles r
      inner join AspNetUserRoles ur
        on r.Id = ur.RoleId
      inner join AspNetApplications a
        on r.ApplicationId = a.Id
      where a.Name = {applicationName}
        and ur.UserId = u
  )").AsNoTracking();

            return await qry.ToListAsync();

        }



        public virtual async Task<IEnumerable<TUser>> GetUsersForApplicationAsync(string applicationName, int pageNumber, int pageSize) {

            var (skip, take) = ((pageNumber - 1) * pageSize, pageSize);

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(string applicationName, int pageNumber, int pageSize) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            if (!store.Context.Database.ProviderName.Contains("SqlServer") && !store.Context.Database.ProviderName.Contains("Oracle"))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(string applicationName, int pageNumber, int pageSize) without SqlServer or Oracle providers");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  where exists (
    select 0
      from AspNetRoles r
      inner join AspNetUserRoles ur
        on r.Id = ur.RoleId
      inner join AspNetApplications a
        on r.ApplicationId = a.Id
      where a.Name = {applicationName}
        and ur.UserId = u
  )
  offset {skip} rows
  fetch next {take} rows only").AsNoTracking();

            return await qry.ToListAsync();

        }


        public virtual async Task<IEnumerable<TUser>> GetUsersForApplicationAsync(int applicationId) {

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(int applicationId) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  where exists (
    select 0
      from AspNetRoles r
      inner join AspNetUserRoles ur
        on r.Id = ur.RoleId
      where r.ApplicationId = {applicationId}
        and ur.UserId = u
  )").AsNoTracking();

            return await qry.ToListAsync();

        }



        public virtual async Task<IEnumerable<TUser>> GetUsersForApplicationAsync(int applicationId, int pageNumber, int pageSize) {

            var (skip, take) = ((pageNumber - 1) * pageSize, pageSize);

            if (!(Store is UserStore<TUser> store))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(int applicationId, int pageNumber, int pageSize) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser> where TUser : DomainUser.");

            if (!store.Context.Database.ProviderName.Contains("SqlServer") && !store.Context.Database.ProviderName.Contains("Oracle"))
                throw new Exception("Cannot use DomainUserManager.GetUsersForApplicationAsync(int applicationId, int pageNumber, int pageSize) without SqlServer or Oracle providers");

            var qry = store.Context.Set<TUser>()
                .FromSqlInterpolated($@"
select u.* 
  from AspNetUsers u
  where exists (
    select 0
      from AspNetRoles r
      inner join AspNetUserRoles ur
        on r.Id = ur.RoleId
      where r.ApplicationId = {applicationId}
        and ur.UserId = u
  )
  offset {skip} rows
  fetch next {take} rows only").AsNoTracking();

            return await qry.ToListAsync();

        }


    }
}