using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Diagnostics;

namespace HLWebRole.Models
{
    public class CustomRoleProvider : RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Plucking our own role business logic into ASP.NET MVC RoleProvider framework.
        /// </summary>
        public override string[] GetRolesForUser(string username)
        {
            try
            {
                using (var WS = new HLServiceReference.HLServiceClient())
                {
                    /* We use a Users Email as username. */
                    var roles = WS.GetRolesForUser(username);

                    if (roles != null)
                    {
                        return roles;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CustomRoleProvider.GetRolesForUser(): " + ex.ToString());
                return null;
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}