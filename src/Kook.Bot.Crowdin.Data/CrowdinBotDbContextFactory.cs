using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kook.Bot.Crowdin.Data;

public class CrowdinBotDbContextFactory : IDesignTimeDbContextFactory<CrowdinBotDbContext>
{
    public CrowdinBotDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<CrowdinBotDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite("Data Source=.\\db\\CrowdinBot.db",
            b => b.MigrationsAssembly("Kook.Bot.Crowdin.Migrations"));

        return new CrowdinBotDbContext(optionsBuilder.Options);
    }
}
