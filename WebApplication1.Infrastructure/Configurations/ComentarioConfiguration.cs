using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Comentario;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;

namespace WebApplication1.Infrastructure.Configurations;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("integer").UseIdentityByDefaultColumn();
        builder.Property(x => x.IdPost).HasColumnType("bigint");
        builder.Property(x => x.IdComentario).HasColumnType("integer");
        builder.Property(x => x.CriadoEm).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.Texto).HasColumnType("character varying(3000)")
            .HasMaxLength(3000).IsRequired();

        builder.HasOne<Post>().WithMany().HasForeignKey(x => x.IdPost)
            .IsRequired(false).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Comentario_Post");

        builder.HasOne<Comentario>().WithMany().HasForeignKey(x => x.IdComentario)
            .IsRequired(false).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_Comentario_Comentario");

        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdAutorUsuario)
            .OnDelete(DeleteBehavior.SetNull).HasConstraintName("FK_Comentario_Autor");
    }
}
