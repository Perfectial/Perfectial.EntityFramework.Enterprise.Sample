namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Infrastructure.Persistence.Base;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    public class UserRepository<TDbContext> : Repository<TDbContext, User, string>, IUserRepository where TDbContext : DbContext, IDbContext
    {
        /*private readonly IDbSet<UserLogin> userLogins;
        private readonly IDbSet<UserClaim> userClaims;
        private readonly IDbSet<UserRole> userRoles;
        private readonly IDbSet<Role> roles;*/

        public UserRepository(IAmbientDbContextLocator ambientDbContextLocator) : base(ambientDbContextLocator)
        {
            /*this.userLogins = this.DbContext.Set<UserLogin>();
            this.userClaims = this.DbContext.Set<UserClaim>();
            this.userRoles = this.DbContext.Set<UserRole>();
            this.roles = this.DbContext.Set<Role>();*/
        }

        public virtual async Task<User> FindByIdAsync(string userId)
        {
            var user = await this.GetAsync(userId);
            await this.LoadAggregateAsync(user);

            return user;
        }

        public virtual async Task<User> FindByNameAsync(string userName)
        {
            var user = await this.FirstOrDefaultAsync(u => u.UserName.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
            await this.LoadAggregateAsync(user);

            return user;
        }

        public virtual async Task<User> FindByEmailAsync(string email)
        {
            var user = await this.FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));
            await this.LoadAggregateAsync(user);

            return user;
        }

        public virtual async Task<IList<UserLinkedLogin>> GetLoginsAsync(User user)
        {
            await this.LoadLogins(user);
            var userLinkedLogins = user.Logins.Select(userLogin => new UserLinkedLogin(userLogin.LoginProvider, userLogin.ProviderKey)).ToList();

            return userLinkedLogins;
        }

        public virtual Task AddLoginAsync(User user, UserLinkedLogin login)
        {
            UserLogin userLogin = new UserLogin { UserId = user.Id, ProviderKey = login.ProviderKey, LoginProvider = login.LoginProvider };
            this.DbContext.Set<UserLogin>().Add(userLogin);

            return Task.FromResult(0);
        }

        public virtual async Task RemoveLoginAsync(User user, UserLinkedLogin login)
        {
            string loginProvider = login.LoginProvider;
            string providerKey = login.ProviderKey;

            UserLogin userLogin;
            if (this.GetIsCollectionLoaded(user, u => u.Logins))
            {
                userLogin = user.Logins.SingleOrDefault(ul =>
                {
                    if (ul.LoginProvider == loginProvider)
                    {
                        return ul.ProviderKey == providerKey;
                    }

                    return false;
                });
            }
            else
            {
                userLogin = await this.DbContext.Set<UserLogin>().SingleOrDefaultAsync(ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey && ul.UserId.Equals(user.Id));
            }

            if (userLogin != null)
            {
                this.DbContext.Set<UserLogin>().Remove(userLogin);
            }
        }

        public virtual async Task<User> FindByLoginAsync(UserLinkedLogin login)
        {
            string loginProvider = login.LoginProvider;
            string providerKey = login.ProviderKey;

            UserLogin userLogin = await this.DbContext.Set<UserLogin>().FirstOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
            User user;

            if (userLogin != null)
            {
                user = await this.FirstOrDefaultAsync(u => u.Id.Equals(userLogin.UserId));
                await this.LoadAggregateAsync(user);
            }
            else
            {
                user = default(User);
            }

            return user;
        }

        public virtual async Task<IList<Claim>> GetClaimsAsync(User user)
        {
            await this.LoadClaims(user);
            var claims = user.Claims.Select(userClaim => new Claim(userClaim.ClaimType, userClaim.ClaimValue)).ToList();

            return claims;
        }

        public virtual Task AddClaimAsync(User user, Claim claim)
        {
            var userClaim = new UserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value };
            this.DbContext.Set<UserClaim>().Add(userClaim);

            return Task.FromResult(0);
        }

        public virtual async Task RemoveClaimAsync(User user, Claim claim)
        {
            string claimValue = claim.Value;
            string claimType = claim.Type;

            IEnumerable<UserClaim> claims;
            if (this.AreClaimsLoaded(user))
            {
                claims = user.Claims.Where(userClaim =>
                {
                    if (userClaim.ClaimValue == claimValue)
                    {
                        return userClaim.ClaimType == claimType;
                    }

                    return false;
                }).ToList();
            }
            else
            {
                claims = await this.DbContext.Set<UserClaim>().Where(userClaim => userClaim.ClaimValue == claimValue && userClaim.ClaimType == claimType && userClaim.UserId.Equals(user.Id))
                .ToListAsync();
            }

            foreach (var userClaim in claims)
            {
                this.DbContext.Set<UserClaim>().Remove(userClaim);
            }
        }

        public virtual async Task<IList<string>> GetRolesAsync(User user)
        {
            IQueryable<string> query = this.DbContext.Set<UserRole>().Where(userRole => userRole.UserId.Equals(user.Id)).Join(this.DbContext.Set<Role>(), userRole => userRole.RoleId, role => role.Id, (userRole, role) => role.Name);

            return await query.ToListAsync() as IList<string>;
        }

        public virtual async Task AddToRoleAsync(User user, string roleName)
        {

            Role roleEntity = await this.DbContext.Set<Role>().SingleOrDefaultAsync(role => role.Name.ToUpper() == roleName.ToUpper());
            UserRole userRole = new UserRole { UserId = user.Id, RoleId = roleEntity.Id };

            this.DbContext.Set<UserRole>().Add(userRole);
        }

        public virtual async Task RemoveFromRoleAsync(User user, string roleName)
        {
            Role roleEntity = await this.DbContext.Set<Role>().SingleOrDefaultAsync(role => role.Name.ToUpper() == roleName.ToUpper());
            if ((object)roleEntity != null)
            {
                UserRole userRole = await this.DbContext.Set<UserRole>().FirstOrDefaultAsync(role => roleEntity.Id.Equals(role.RoleId) && role.UserId.Equals(user.Id));
                if (userRole != null)
                {
                    this.DbContext.Set<UserRole>().Remove(userRole);
                }
            }
        }

        public virtual async Task<bool> IsInRoleAsync(User user, string roleName)
        {
            bool isInRole = false;

            Role role = await this.DbContext.Set<Role>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper());
            if ((object)role != null)
            {
                isInRole = await this.DbContext.Set<UserRole>().AnyAsync(ur => ur.RoleId.Equals(role.Id) && ur.UserId.Equals(user.Id));
            }

            return isInRole;
        }

        public virtual Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public virtual Task SetEmailConfirmedAsync(User user, bool emailConfirmed)
        {
            user.EmailConfirmed = emailConfirmed;

            return Task.FromResult(0);
        }

        public virtual Task SetEmailAsync(User user, string email)
        {
            user.Email = email;

            return Task.FromResult(0);
        }

        public virtual Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        public virtual Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            return Task.FromResult(user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : new DateTimeOffset());
        }

        public virtual Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEndDate)
        {
            user.LockoutEndDateUtc = lockoutEndDate == DateTimeOffset.MinValue ? new DateTime?() : lockoutEndDate.UtcDateTime;

            return Task.FromResult(0);
        }

        public virtual Task<int> IncrementAccessFailedCountAsync(User user)
        {
            ++user.AccessFailedCount;

            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;

            return Task.FromResult(0);
        }

        public virtual Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public virtual Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public virtual Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.LockoutEnabled = enabled;

            return Task.FromResult(0);
        }

        public virtual Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;

            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public virtual Task SetPhoneNumberAsync(User user, string phoneNumber)
        {
            user.PhoneNumber = phoneNumber;

            return Task.FromResult(0);
        }

        public virtual Task<string> GetPhoneNumberAsync(User user)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public virtual Task<bool> GetPhoneNumberConfirmedAsync(User user)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public virtual Task SetPhoneNumberConfirmedAsync(User user, bool phoneNumberConfirmed)
        {
            user.PhoneNumberConfirmed = phoneNumberConfirmed;

            return Task.FromResult(0);
        }

        public virtual Task SetSecurityStampAsync(User user, string stamp)
        {
            user.SecurityStamp = stamp;

            return Task.FromResult(0);
        }

        public virtual Task<string> GetSecurityStampAsync(User user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public virtual Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;

            return Task.FromResult(0);
        }

        public virtual Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        /*private async Task SaveChanges()
        {
            if (this.AutoSaveChanges)
            {
                int num = await TaskExtensions.WithCurrentCulture<int>(this.Context.SaveChangesAsync());
            }
        }*/
        private bool AreClaimsLoaded(User user)
        {
            return this.DbContext.Entry(user).Collection(u => u.Claims).IsLoaded;
        }

        private async Task EnsureCollectionIsLoaded<TEntity, TElement>(
            User user,
            IDbSet<TEntity> dbSet,
            Expression<Func<User, ICollection<TElement>>> collectionExpression,
            Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
            where TElement : class
        {
            this.Attach(user);

            if (!this.GetIsCollectionLoaded(user, collectionExpression))
            {
                await this.LoadCollection(dbSet, predicate);
                this.SetIsCollectionLoaded(user, collectionExpression, true);
            }
        }
        private async Task LoadCollection<TEntity>(IDbSet<TEntity> dbSet, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            await dbSet.Where(predicate).LoadAsync();
        }

        private bool GetIsCollectionLoaded<TElement>(User user, Expression<Func<User, ICollection<TElement>>> collectionExpression) where TElement : class
        {
            return this.DbContext.Entry(user).Collection(collectionExpression).IsLoaded;
        }

        private void SetIsCollectionLoaded<TElement>(User user, Expression<Func<User, ICollection<TElement>>> collectionExpression, bool isCollectionLoaded) where TElement : class
        {
            this.DbContext.Entry(user).Collection(collectionExpression).IsLoaded = isCollectionLoaded;
        }

        private async Task LoadRoles(User user)
        {
            await this.EnsureCollectionIsLoaded(user, this.DbContext.Set<UserRole>(), u => u.UserRoles, userRole => userRole.UserId.Equals(user.Id));
        }

        private async Task LoadClaims(User user)
        {
            await this.EnsureCollectionIsLoaded(user, this.DbContext.Set<UserClaim>(), u => u.Claims, userClaim => userClaim.UserId.Equals(user.Id));
        }

        private async Task LoadLogins(User user)
        {
            await this.EnsureCollectionIsLoaded(user, this.DbContext.Set<UserLogin>(), u => u.Logins, userLogin => userLogin.UserId.Equals(user.Id));
        }

        private async Task LoadAggregateAsync(User user)
        {
            if (user != null)
            {
                await this.LoadRoles(user);
                await this.LoadClaims(user);
                await this.LoadLogins(user);
            }
        }

        /*private static class FindByIdFilterParser
        {
            private static readonly Expression<Func<User, bool>> predicate = u => u.Id.Equals(default(string));
            private static readonly MethodInfo equalsMethodInfo = ((MethodCallExpression)predicate.Body).Method;
            private static readonly MemberInfo userIdMemberInfo = ((MemberExpression)((MethodCallExpression)predicate.Body).Object)?.Member;

            internal static bool TryMatchAndGetId(Expression<Func<User, bool>> filter, out string id)
            {
                id = default(string);

                if (filter.Body.NodeType != ExpressionType.Call)
                {
                    return false;
                }

                MethodCallExpression methodCallExpression = (MethodCallExpression)filter.Body;
                if (methodCallExpression.Method != equalsMethodInfo
                    || methodCallExpression.Object == null
                    || (methodCallExpression.Object.NodeType != ExpressionType.MemberAccess
                    || ((MemberExpression)methodCallExpression.Object).Member != userIdMemberInfo) || methodCallExpression.Arguments.Count != 1)
                {
                    return false;
                }

                MemberExpression memberExpression;
                if (methodCallExpression.Arguments[0].NodeType == ExpressionType.Convert)
                {
                    UnaryExpression unaryExpression = (UnaryExpression)methodCallExpression.Arguments[0];
                    if (unaryExpression.Operand.NodeType != ExpressionType.MemberAccess)
                    {
                        return false;
                    }

                    memberExpression = (MemberExpression)unaryExpression.Operand;
                }
                else
                {
                    if (methodCallExpression.Arguments[0].NodeType != ExpressionType.MemberAccess)
                    {
                        return false;
                    }

                    memberExpression = (MemberExpression)methodCallExpression.Arguments[0];
                }

                if (memberExpression.Member.MemberType != MemberTypes.Field || memberExpression.Expression.NodeType != ExpressionType.Constant)
                {
                    return false;
                }

                FieldInfo fieldInfo = (FieldInfo)memberExpression.Member;
                object entity = ((ConstantExpression)memberExpression.Expression).Value;
                id = (string)fieldInfo.GetValue(entity);

                return true;
            }
        }*/
    }
}
