using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ParkyWeb_XTest
{
    public static class AuthorizationTest
    {
        /// <summary>
        /// Check to see if a method allows anonymous access -
        /// 1. A method is anonymous if it is decorated with the AllowAnonymousAttribute attribute.
        /// 2. Or, a method is anonymous if neither the method nor controller are decorated with the AuthorizeAttribute attribute.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="methodName"></param>
        /// <param name="methodTypes">Optional</param>
        /// <returns>true is method is anonymous</returns>
        public static bool IsAnonymous(Controller controller, string methodName, Type[] methodTypes)
        {
            return GetMethodAttribute<AllowAnonymousAttribute>(controller, methodName, methodTypes) != null ||
                (GetControllerAttribute<AuthorizeAttribute>(controller) == null &&
                    GetMethodAttribute<AuthorizeAttribute>(controller, methodName, methodTypes) == null);

        }

        /// <summary>
        /// Check to see if a method requires authorization -
        /// 1. A method is authorized if it is decorated with the Authorize attribute.
        /// 2. Or, a method is authorized if the controller is decorated with the AuthorizeAttribute attribute, and
        /// the method is not decorated with the AllowAnonymousAttribute attribute.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="methodName"></param>
        /// <param name="methodTypes">Optional</param>
        /// <returns></returns>
        public static bool IsAuthorized(Controller controller, string methodName, Type[] methodTypes)
        {
            return GetMethodAttribute<AuthorizeAttribute>(controller, methodName, methodTypes) != null ||
                (GetControllerAttribute<AuthorizeAttribute>(controller) != null &&
                    GetMethodAttribute<AllowAnonymousAttribute>(controller, methodName, methodTypes) == null);
        }

        /// <summary>
        /// Check to see if a method requires authorization for the roles and users specified
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="methodName"></param>
        /// <param name="methodTypes">Optional</param>
        /// <param name="roles"></param>
        /// <param name="users"></param>
        /// <returns></returns>
        public static bool IsAuthorized(Controller controller, string methodName, Type[] methodTypes, string[] roles, string[] users)
        {
            if (roles == null && users == null)
                return IsAuthorized(controller, methodName, methodTypes);

            if (!IsAuthorized(controller, methodName, methodTypes))
                return false;

            AuthorizeAttribute controllerAttribute = GetControllerAttribute<AuthorizeAttribute>(controller);
            AuthorizeAttribute methodAttribute = GetMethodAttribute<AuthorizeAttribute>(controller, methodName, methodTypes);

            // Check to see if all roles are authorized
            if (roles != null)
            {
                foreach (string role in roles)
                {
                    string lowerRole = role.ToLower();

                    bool roleIsAuthorized =
                        (controllerAttribute != null ?
                            controllerAttribute.Roles.ToLower().Split(',').Any(r => r == lowerRole) : false) ||
                        (methodAttribute != null ?
                            methodAttribute.Roles.ToLower().Split(',').Any(r => r == lowerRole) : false);

                    if (!roleIsAuthorized)
                        return false;
                }
            }

            return true;
        }

        private static T GetControllerAttribute<T>(Controller controller) where T : Attribute
        {
            Type type = controller.GetType();
            object[] attributes = type.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Count() == 0 ? null : (T)attributes[0];
            return attribute;
        }

        private static T GetMethodAttribute<T>(Controller controller, string methodName, Type[] methodTypes) where T : Attribute
        {
            Type type = controller.GetType();
            if (methodTypes == null)
            {
                methodTypes = new Type[0];
            }
            MethodInfo method = type.GetMethod(methodName, methodTypes);
            object[] attributes = method.GetCustomAttributes(typeof(T), true);
            T attribute = attributes.Count() == 0 ? null : (T)attributes[0];
            return attribute;
        }
    }
}

