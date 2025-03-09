using inmind_chatapp_client.Models;
using Microsoft.EntityFrameworkCore;

namespace inmind_chatapp_client.DbContext;

public interface IAppDbContext
{
    public DbSet<Message> Messages { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}