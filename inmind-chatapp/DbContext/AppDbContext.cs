using inmind_chatapp_client.Models;
using Microsoft.EntityFrameworkCore;

namespace inmind_chatapp_client.DbContext;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Message> Messages { get; set; }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}