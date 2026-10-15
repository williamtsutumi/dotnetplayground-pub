using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Comunidade;
using WebApplication1.Domain.ComunidadeUsuario;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class ComunidadeUsuarioConfiguration : IEntityTypeConfiguration<ComunidadeUsuario>
{
    public void Configure(EntityTypeBuilder<ComunidadeUsuario> builder)
    {
        builder.ToTable("ComunidadeUsuario");
        builder.HasKey(x => new { x.IdComunidade, x.IdUsuario }).HasName("PK_ComunidadeUsuario");
        builder.Property(x => x.Papel).HasColumnType("smallint").HasDefaultValue((byte)0);
        builder.Property(x => x.EntrouEm).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasOne<Comunidade>().WithMany().HasForeignKey(x => x.IdComunidade)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ComunidadeUsuario_Comunidade");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_ComunidadeUsuario_Usuario");
    }
}
