using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QareebChat.Entities;
using System.Reflection;

namespace QareebChat.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
    IdentityDbContext<ApplicationUser>(options)
{


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
   
}
