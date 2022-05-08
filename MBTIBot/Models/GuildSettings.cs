using System.ComponentModel.DataAnnotations;

namespace MBTIBot.Models;

public class GuildSettings
{
    [Key] public ulong GuildId { get; set; }
    public ulong? IntroductionChannel { get; set; }
}