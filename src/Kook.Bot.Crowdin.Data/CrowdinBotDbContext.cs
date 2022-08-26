using Kook.Bot.Crowdin.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Kook.Bot.Crowdin.Data;

public class CrowdinBotDbContext : DbContext
{
    public CrowdinBotDbContext(DbContextOptions<CrowdinBotDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<TermEntity> Terms { get; set; }
}