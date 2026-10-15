using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfil");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("int").UseIdentityByDefaultColumn();

        builder
            .HasOne(x => x.Usuario)
            .WithMany().HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Perfil_Usuario");

        builder.Property(x => x.Tipo).HasColumnType("int").IsRequired(true);
    }
}
