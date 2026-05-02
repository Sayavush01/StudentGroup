using Microsoft.EntityFrameworkCore;

namespace StudentGroup.Configurations
{
    public class EventConfiguration: IEntityTypeConfiguration<Event>
    {
      public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Event> builder)
        {
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(150);
            builder.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.BannerImageUrl);
            builder.HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }   

    }
}
