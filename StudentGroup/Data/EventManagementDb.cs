using Microsoft.EntityFrameworkCore;
using StudentGroup.Entities;

namespace StudentGroup.Data
{
    public class EventManagementDb: DbContext
    {
        public EventManagementDb(DbContextOptions<EventManagementDb> options) : base(options)
        {
        }
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Organizer> Organizers => Set<Organizer>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventManagementDb).Assembly);
        }
    }
}
