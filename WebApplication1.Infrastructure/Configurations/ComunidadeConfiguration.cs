using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Comunidade;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class ComunidadeConfiguration : IEntityTypeConfiguration<Comunidade>
{
    public void Configure(EntityTypeBuilder<Comunidade> builder)
    {
        builder.ToTable("Comunidade");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Nome).HasColumnType("character varying(50)").IsRequired();
        builder.Property(x => x.Descricao).HasColumnType("character varying(500)");
        builder.Property(x => x.CriadoEm).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasAlternateKey(x => x.Nome).HasName("UQ_Comunidade_Nome");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdCriadorUsuario)
            .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_Comunidade_Criador");
    }
}
