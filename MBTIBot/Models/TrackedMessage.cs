using System.ComponentModel.DataAnnotations;

namespace MBTIBot.Models;

public class TrackedMessage
{
    [Key] public ulong MessageId { get; set; }
    public ulong Guild { get; set; }
    public ulong Author { get; set; }
}