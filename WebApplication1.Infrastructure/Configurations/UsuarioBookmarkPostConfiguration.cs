using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;
using WebApplication1.Domain.UsuarioBookmarkPost;

namespace WebApplication1.Infrastructure.Configurations;

public class UsuarioBookmarkPostConfiguration : IEntityTypeConfiguration<UsuarioBookmarkPost>
{
    public void Configure(EntityTypeBuilder<UsuarioBookmarkPost> builder)
    {
        builder.ToTable("UsuarioBookmarkPost");
        builder.HasKey(x => new { x.IdUsuario, x.IdPost }).HasName("PK_UsuarioBookmarkPost");
        builder.Property(x => x.IdUsuario).HasColumnType("integer");
        builder.Property(x => x.IdPost).HasColumnType("bigint");
        builder.HasIndex(x => x.IdPost).HasDatabaseName("IX_UsuarioBookmarkPost_IdPost");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_UsuarioBookmarkPost_Usuario");
        builder.HasOne<Post>().WithMany().HasForeignKey(x => x.IdPost)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UsuarioBookmarkPost_Post");
    }
}
