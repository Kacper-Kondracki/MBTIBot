using MBTIBot.Models;
using Microsoft.EntityFrameworkCore;

namespace MBTIBot;

public sealed class BotContext : DbContext
{
    public BotContext(DbContextOptions<BotContext> options) : base(options)
    {
        Database.Migrate();
    }

    public DbSet<GuildSettings> GuildSettings { get; set; } = null!;
    public DbSet<RemindedUsers> RemindedUsers { get; set; } = null!;
    public DbSet<TrackedMessage> TrackedMessages { get; set; } = null!;
}