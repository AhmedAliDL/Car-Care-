using App.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
    }
}
