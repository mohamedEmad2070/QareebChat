using Microsoft.EntityFrameworkCore;

namespace QareebChat.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): DbContext(options)
{
   
}
