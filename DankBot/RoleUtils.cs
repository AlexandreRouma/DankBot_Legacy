using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class RoleUtils
    {
        public static bool IsUserBotAdmin(ulong id)
        {
            foreach (SocketRole role in Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(id).Roles)
            {
                if (ConfigUtils.Configuration.AdminRoles.Contains(role.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool DoesUserHaveRole(ulong userId, ulong roleId)
        {
            foreach (SocketRole role in Program.client.GetGuild(ConfigUtils.Configuration.ServerID).GetUser(userId).Roles)
            {
                if (role.Id == roleId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
