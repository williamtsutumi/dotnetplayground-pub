using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.AppGlobalSettings;

namespace WebApplication1.Infrastructure.Configurations;

public sealed class AppGlobalSettingConfiguration : IEntityTypeConfiguration<AppGlobalSetting>
{
    public void Configure(EntityTypeBuilder<AppGlobalSetting> builder)
    {
        builder.ToTable("AppGlobalSettings");
        builder.HasKey(setting => setting.Key);
        builder.Property(setting => setting.Key).HasColumnType("character varying(200)").HasMaxLength(200);
        builder.Property(setting => setting.Value).HasColumnType("text").IsRequired();
    }
}
