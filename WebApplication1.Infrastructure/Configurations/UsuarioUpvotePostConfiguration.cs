using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Post;
using WebApplication1.Domain.Usuario;
using WebApplication1.Domain.UsuarioUpvotePost;

namespace WebApplication1.Infrastructure.Configurations;

public class UsuarioUpvotePostConfiguration : IEntityTypeConfiguration<UsuarioUpvotePost>
{
    public void Configure(EntityTypeBuilder<UsuarioUpvotePost> builder)
    {
        builder.ToTable("UsuarioUpvotePost");
        builder.HasKey(x => new { x.IdUsuario, x.IdPost }).HasName("PK_UsuarioUpvotePost");
        builder.Property(x => x.IdUsuario).HasColumnType("integer");
        builder.Property(x => x.Tipo).HasConversion<int>().HasColumnType("integer")
            .IsRequired().HasDefaultValue(TipoVoto.UpVote);
        builder.Property(x => x.IdPost).HasColumnType("bigint");
        builder.HasIndex(x => x.IdPost).HasDatabaseName("IX_UsuarioUpvotePost_IdPost");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_UsuarioUpvotePost_Usuario");
        builder.HasOne<Post>().WithMany().HasForeignKey(x => x.IdPost)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UsuarioUpvotePost_Post");
    }
}
