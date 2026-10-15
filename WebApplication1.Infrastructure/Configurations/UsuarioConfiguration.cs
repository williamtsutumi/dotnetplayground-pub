using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.NomeUsuario).HasColumnType("character varying(50)").IsRequired();
        builder.Property(x => x.Email).HasColumnType("character varying(255)").IsRequired();
        builder.Property(x => x.IdiomaPreferido).HasColumnType("character varying(100)")
            .HasMaxLength(100).IsRequired().HasDefaultValue("EN");
        builder.Property(x => x.CriadoEm).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasAlternateKey(x => x.NomeUsuario).HasName("UQ_Usuario_NomeUsuario");
        builder.HasAlternateKey(x => x.Email).HasName("UQ_Usuario_Email");
        builder.Property(x => x.Senha).HasColumnType("character varying(255)").IsRequired(false);
        builder.Property(x => x.DataNascimento).HasColumnType("date").IsRequired(false);
        builder.Property(x => x.Descricao).HasColumnType("character varying(1000)").IsRequired(false);
        // Nome is the legacy alias; the Reddit table stores NomeUsuario.
        builder.Ignore(x => x.Nome);
    }
}
