using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentGroup.Entities;

namespace StudentGroup.Configurations
{
    public class OrganizerConfiguration: IEntityTypeConfiguration<Organizer>
    {
        public void Configure(EntityTypeBuilder<Organizer> builder)
        {
            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(o => o.Phone)
                .HasMaxLength(20);
            builder.Property(o => o.LogoUrl)
                .HasMaxLength(200);
        }
    }
}
