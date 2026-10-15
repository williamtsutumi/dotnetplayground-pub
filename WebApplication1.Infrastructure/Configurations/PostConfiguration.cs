using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Comunidade;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Post");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("bigint").UseIdentityByDefaultColumn();
        builder.Property(x => x.Titulo).HasColumnType("character varying(300)").IsRequired();
        builder.Property(x => x.Conteudo).HasColumnType("text").IsRequired();
        builder.Property(x => x.ConteudoMarkdown).HasColumnType("boolean").IsRequired();
        builder.Property(x => x.CriadoEm).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.EditadoEm).HasColumnType("timestamp with time zone");

        builder
            .HasOne(x => x.Comunidade)
            .WithMany().HasForeignKey(x => x.ComunidadeId)
            .IsRequired(false).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Post_Comunidade");

        builder
            .HasOne<Usuario>()
            .WithMany().HasForeignKey(x => x.IdAutorUsuario)
            .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_Post_Autor");
    }
}
