using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace DT.Common.IO
{
    public class FileUtils
    {
        public static bool GetIdleDir(string path, int backOffDelta = 0)
        {
            var dirIdle = false;
            foreach (var file in Directory.GetFiles(path))
            {
                // ReSharper disable once AssignmentInConditionalExpression
                if (dirIdle = GetIdleFile(file))
                {
                }
            }
            return dirIdle;
        }

        public static bool GetIdleFile(string path, int backOffDelta = 0)
        {
            var fileIdle = false;
            const int maximumAttemptsAllowed = 30000;
            var attemptsMade = 0;

            while (!fileIdle && attemptsMade <= maximumAttemptsAllowed)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        fileIdle = true;
                    }
                }
                catch(Exception exception)
                {
                    Debug.WriteLine(exception.Message);
                    attemptsMade++;
                    Thread.Sleep(100 * backOffDelta + 1);
                }
            }

            return fileIdle;
        }

        public static bool CanWriteToFile(string filename)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, filename);
            permissionSet.AddPermission(writePermission);

            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                return true;
            }
            return false;
        }

        public static bool IsReadable(string path)
        {

            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (Exception)
            { //Posible UnauthorizedAccessException
                return false;
            }

            bool isAllow = false;
            string userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadExtendedAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadPermissions)) && rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    else if ((rule.FileSystemRights.HasFlag(FileSystemRights.Read) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAndExecute) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadExtendedAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.ReadPermissions)) && rule.AccessControlType == AccessControlType.Allow)
                        isAllow = true;
                }
            }

            return isAllow;
        }

        public static bool IsWriteable(string path)
        {
            AuthorizationRuleCollection rules;
            WindowsIdentity identity;
            try
            {
                rules = Directory.GetAccessControl(path).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                identity = WindowsIdentity.GetCurrent();
            }
            catch (Exception)
            { //Posible UnauthorizedAccessException
                return false;
            }

            bool isAllow = false;
            string userSID = identity.User.Value;

            foreach (FileSystemAccessRule rule in rules)
            {
                if (rule.IdentityReference.ToString() == userSID || identity.Groups.Contains(rule.IdentityReference))
                {
                    if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteExtendedAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) && rule.AccessControlType == AccessControlType.Deny)
                        return false;
                    else if ((rule.FileSystemRights.HasFlag(FileSystemRights.Write) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.WriteExtendedAttributes) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateDirectories) ||
                        rule.FileSystemRights.HasFlag(FileSystemRights.CreateFiles)) && rule.AccessControlType == AccessControlType.Allow)
                        isAllow = true;
                }
            }

            return IsReadable(path) && isAllow;
        }
    }
}
