#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Queryable;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Services
{
    public class UserService<TUser, TKey> : IUserService<TUser>
       where TUser : IdentityUserExtended<TKey>
       where TKey : IEquatable<TKey>
    {
        private readonly IQueryableProvider<TUser> _queryableProvider;
        private readonly UserManager<TUser> _userManager;

        public UserService(UserManager<TUser> userManager, IQueryableProvider<TUser> queryableProvider)
        {
            _userManager = userManager;
            _queryableProvider = queryableProvider;
        }

        public IQueryable<TUser> Users => _userManager.Users;

        public Task<Operation<IEnumerable<TUser>>> GetAsync()
        {
            var all = _queryableProvider.SafeAll ?? Users;
            return Task.FromResult(new Operation<IEnumerable<TUser>>(all));
        }

        public async Task<Operation<TUser>> CreateAsync(CreateUserModel model)
        {
            var user = (TUser)FormatterServices.GetUninitializedObject(typeof(TUser));
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
            user.EmailConfirmed = model.EmailConfirmed;

            var result = await _userManager.CreateAsync(user, model.Password);
            return result.ToOperation(user);
        }

        public async Task<Operation> UpdateAsync(TUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            return result.ToOperation();
        }

        public async Task<Operation> DeleteAsync(string id)
        {
            var operation = await FindByIdAsync(id);
            if (!operation.Succeeded)
                return operation;

            var deleted = await _userManager.DeleteAsync(operation.Data);
            return deleted.ToOperation();
        }

        #region Find

        public async Task<Operation<TUser>> FindByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user == null
                ? new Operation<TUser>(new Error(ErrorEvents.NotFound, ErrorStrings.UserNotFound, HttpStatusCode.NotFound))
                : new Operation<TUser>(user);
        }

        public async Task<Operation<TUser>> FindByEmailAsync(string email)
        {
            return new Operation<TUser>(await _userManager.FindByEmailAsync(email));
        }

        public async Task<Operation<TUser>> FindByNameAsync(string username)
        {
            return new Operation<TUser>(await _userManager.FindByNameAsync(username));
        }

        public async Task<Operation<TUser>> FindByPhoneNumberAsync(string phoneNumber)
        {
            return new Operation<TUser>(await _userManager.FindByPhoneNumberAsync(phoneNumber));
        }

        public async Task<Operation<TUser>> FindByLoginAsync(string loginProvider, string providerKey)
        {
            return new Operation<TUser>(await _userManager.FindByLoginAsync(loginProvider, providerKey));
        }

        public async Task<Operation<TUser>> FindByAsync(Expression<Func<TUser, bool>> predicate)
        {
            return new Operation<TUser>(await _queryableProvider.SafeQueryable.FirstOrDefaultAsync(predicate));
        }

        #endregion

        #region Role Assignment

        public async Task<Operation<IList<string>>> GetRolesAsync(TUser user)
        {
            return new Operation<IList<string>>(await _userManager.GetRolesAsync(user));
        }

        public async Task<Operation> AddToRoleAsync(TUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result.ToOperation();
        }

        public async Task<Operation> RemoveFromRoleAsync(TUser user, string role)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            return result.ToOperation();
        }

        #endregion

        #region Claims Assignment

        public async Task<Operation<IList<Claim>>> GetClaimsAsync(TUser user)
        {
            return new Operation<IList<Claim>>(await _userManager.GetClaimsAsync(user));
        }

        public async Task<Operation> AddClaimAsync(TUser user, Claim claim)
        {
            var result = await _userManager.AddClaimAsync(user, claim);

            return result.ToOperation();
        }

        public async Task<Operation> RemoveClaimAsync(TUser user, Claim claim)
        {
            var result = await _userManager.RemoveClaimAsync(user, claim);

            return result.ToOperation();
        }

        public async Task<Operation> AddClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            var result = await _userManager.AddClaimsAsync(user, claims);

            return result.ToOperation();
        }

        public async Task<Operation> RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims)
        {
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            return result.ToOperation();
        }

        #endregion

        #region Token Generation

        public async Task<Operation<string>> GenerateChangePhoneNumberTokenAsync(TUser user, string phoneNumber)
        {
            return new Operation<string>(await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber));
        }

        public async Task<Operation<string>> GenerateChangeEmailTokenAsync(TUser user, string newEmail)
        {
            return new Operation<string>(await _userManager.GenerateChangeEmailTokenAsync(user, newEmail));
        }

        public async Task<Operation<string>> GenerateEmailConfirmationTokenAsync(TUser user)
        {
            return new Operation<string>(await _userManager.GenerateEmailConfirmationTokenAsync(user));
        }

        public async Task<Operation<string>> GeneratePasswordResetTokenAsync(TUser user)
        {
            return new Operation<string>(await _userManager.GeneratePasswordResetTokenAsync(user));
        }

        public async Task<Operation<IEnumerable<string>>> GenerateNewTwoFactorRecoveryCodesAsync(TUser user, int number)
        {
            return new Operation<IEnumerable<string>>(
                await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, number));
        }

        public async Task<Operation> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token)
        {
            var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);
            return result.ToOperation();
        }

        public async Task<Operation> ChangeEmailAsync(TUser user, string newEmail, string token)
        {
            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            return result.ToOperation();
        }

        public async Task<Operation> ChangePasswordAsync(TUser user, string token, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, token, newPassword);
            return result.ToOperation();
        }

        public async Task<Operation> ConfirmEmailAsync(TUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.ToOperation();
        }

        public async Task<Operation> ResetPasswordAsync(TUser user, string token, string newPassword)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.ToOperation();
        }

        #endregion
    }
}
