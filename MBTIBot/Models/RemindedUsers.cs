using System.ComponentModel.DataAnnotations;

namespace MBTIBot.Models;

public class RemindedUsers
{
    [Key] public ulong UserId { get; set; }
}