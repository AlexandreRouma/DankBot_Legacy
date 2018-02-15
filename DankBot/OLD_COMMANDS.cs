using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DankBot
{
    class OLD_COMMANDS
    {
        //static async Task MessageReceived(SocketMessage message)
        //{

        //    if (message.Author.Id == client.CurrentUser.Id)
        //    {
        //        return;
        //    }
        //    if (message.Content.ToUpper().Contains("TRAPS AREN'T GAY"))
        //    {
        //        await message.Channel.SendMessageAsync($"STFU {message.Author.Mention}");
        //        return;
        //    }

        //    if (message.Content.StartsWith(ConfigUtils.Configuration.Prefix))
        //    {
        //        string msg = message.Content.Substring(ConfigUtils.Configuration.Prefix.Length);
        //        string[] arg = msg.Split(' ');
        //        string cmd = arg[0];

        //        // Alt handling
        //        if (cmd.ToUpper() == "G")
        //        {
        //            cmd = "GOOGLE";
        //            msg = Regex.Replace(msg, "G", "GOOGLE", RegexOptions.IgnoreCase);
        //        }
        //        else if (cmd.ToUpper() == "YT")
        //        {
        //            cmd = "YOUTUBE";
        //            msg = Regex.Replace(msg, "YT", "YOUTUBE", RegexOptions.IgnoreCase);
        //        }
        //        else if (cmd.ToUpper() == "SE")
        //        {
        //            cmd = "SOUNDEFFECT";
        //            msg = Regex.Replace(msg, "SE", "SOUNDEFFECT", RegexOptions.IgnoreCase);
        //        }
        //        else if (cmd.ToUpper() == "B64E")
        //        {
        //            cmd = "B64ENCODE";
        //            msg = Regex.Replace(msg, "B64E", "B64ENCODE", RegexOptions.IgnoreCase);
        //        }
        //        else if (cmd.ToUpper() == "B64D")
        //        {
        //            cmd = "B64DECODE";
        //            msg = Regex.Replace(msg, "B64D", "B64DECODE", RegexOptions.IgnoreCase);
        //        }
        //        // --------

        //        var type = message.Channel.EnterTypingState();

        //        switch (cmd.ToUpper())
        //        {
        //            case "":
        //                await message.Channel.SendMessageAsync("U wot m8 ?!");
        //                break;
        //            case "CALC":
        //                await message.Channel.SendMessageAsync("I'm not ur dank calculator asshole !");
        //                break;
        //            case "D0G3":
        //                await message.Channel.SendMessageAsync("░░░░░░░█▐▓▓░████▄▄▄█▀▄▓▓▓▌█\n░░░░░▄█▌▀▄▓▓▄▄▄▄▀▀▀▄▓▓▓▓▓▌█\n░░░▄█▀▀▄▓█▓▓▓▓▓▓▓▓▓▓▓▓▀░▓▌█\n░░█▀▄▓▓▓███▓▓▓███▓▓▓▄░░▄▓▐█▌\n░█▌▓▓▓▀▀▓▓▓▓███▓▓▓▓▓▓▓▄▀▓▓▐█\n▐█▐██▐░▄▓▓▓▓▓▀▄░▀▓▓▓▓▓▓▓▓▓▌█▌\n█▌███▓▓▓▓▓▓▓▓▐░░▄▓▓███▓▓▓▄▀▐█\n█▐█▓▀░░▀▓▓▓▓▓▓▓▓▓██████▓▓▓▓▐█\n▌▓▄▌▀░▀░▐▀█▄▓▓██████████▓▓▓▌█▌\n▌▓▓▓▄▄▀▀▓▓▓▀▓▓▓▓▓▓▓▓█▓█▓█▓▓▌█▌\n█▐▓▓▓▓▓▓▄▄▄▓▓▓▓▓▓█▓█▓█▓█▓▓▓▐█");
        //                break;
        //            case "GUYMASTURBATINGONWHAMEN":
        //                await message.Channel.SendMessageAsync(":point_up:️             :man:\n     :bug::zzz::necktie: :bug:\n                    :fuelpump:️     :boot:\n                :zap:️ 8==:punch: =D:sweat_drops:\n             :trumpet:   :eggplant:                      :sweat_drops:\n            :boot:      :boot:                       :ok_woman::skin-tone-1:");
        //                break;
        //            case "THINKING":
        //                await message.Channel.SendMessageAsync("⠰⡿⠿⠛⠛⠻⠿⣷\n      ⣀⣄⡀⠀⠀⠀⠀⢀⣀⣀⣤⣄⣀⡀\n     ⢸⣿⣿⣷⠀⠀⠀⠀⠛⠛⣿⣿⣿⡛⠿⠷\n     ⠘⠿⠿⠋⠀⠀⠀⠀⠀⠀⣿⣿⣿⠇\n               ⠈⠉⠁\n \n    ⣿⣷⣄⠀⢶⣶⣷⣶⣶⣤⣀\n    ⣿⣿⣿⠀⠀⠀⠀⠀⠈⠙⠻⠗\n   ⣰⣿⣿⣿⠀⠀⠀⠀⢀⣀⣠⣤⣴⣶⡄\n ⣠⣾⣿⣿⣿⣥⣶⣶⣿⣿⣿⣿⣿⠿⠿⠛⠃\n⢰⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄\n⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡁\n⠈⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠁\n  ⠛⢿⣿⣿⣿⣿⣿⣿⡿⠟\n     ⠉⠉⠉");
        //                break;
        //            case "PENIS":
        //                await message.Channel.SendMessageAsync("8================================-");
        //                break;
        //            case "SUICIDE":
        //                await message.Channel.SendFileAsync("resources/images/nooseman.png");
        //                break;
        //            case "YTBUDDY":
        //                await message.Channel.SendFileAsync("resources/images/ytbuddy_top.png");
        //                await message.Channel.SendFileAsync("resources/images/ytbuddy_middle.png");
        //                await message.Channel.SendFileAsync("resources/images/ytbuddy_bottom.png");
        //                break;
        //            case "PI":
        //                await message.Channel.SendMessageAsync("`PI = 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798609437027705392171762931767523846748184676694051320005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235420199561121290219608640344181598136297747713099605187072113499999983729780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083814206171776691473035982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989`");
        //                break;
        //            case "XD":
        //                await message.Channel.SendMessageAsync($"​:joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy:\n:joy::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::joy:\n:joy::cool::100::cool::cool::cool::100::cool::100::100::100::cool::cool::cool::joy:\n:joy::cool::100::100::cool::100::100::cool::100::cool::100::100::cool::cool::joy:\n:joy::cool::cool::100::cool::100::cool::cool::100::cool::cool::100::100::cool::joy:\n:joy::cool::cool::100::100::100::cool::cool::100::cool::cool::cool::100::cool::joy:\n:joy::cool::cool::cool::100::cool::cool::cool::100::cool::cool::cool::100::cool::joy:\n:joy::cool::cool::100::100::100::cool::cool::100::cool::cool::cool::100::cool::joy:\n:joy::cool::cool::100::cool::100::cool::cool::100::cool::cool::100::100::cool::joy:\n:joy::cool::100::100::cool::100::100::cool::100::cool::100::100::cool::cool::joy:\n:joy::cool::100::cool::cool::cool::100::cool::100::100::100::cool::cool::cool::joy:\n:joy::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::cool::joy:\n:joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy::joy:\n");
        //                break;
        //            case "WHY":
        //                await message.Channel.SendMessageAsync($"`{because[new Random().Next(0, because.Count())]}`");
        //                break;
        //            default:
        //                await message.Channel.SendMessageAsync($":no_entry: `The command '{cmd}' is as legit as an OpticGaming player on this server :(`");
        //                break;
        //        }
        //        type.Dispose();
        //    }
        //}
    }
}
