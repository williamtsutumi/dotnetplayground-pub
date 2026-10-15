using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplication1.Domain.Comentario;
using WebApplication1.Domain.Usuario;
using WebApplication1.Domain.UsuarioUpvoteComentario;

namespace WebApplication1.Infrastructure.Configurations;

public class UsuarioUpvoteComentarioConfiguration : IEntityTypeConfiguration<UsuarioUpvoteComentario>
{
    public void Configure(EntityTypeBuilder<UsuarioUpvoteComentario> builder)
    {
        builder.ToTable("UsuarioUpvoteComentario");
        builder.HasKey(x => new { x.IdUsuario, x.IdComentario }).HasName("PK_UsuarioUpvoteComentario");
        builder.Property(x => x.IdUsuario).HasColumnType("integer");
        builder.Property(x => x.Tipo).HasConversion<int>().HasColumnType("integer")
            .IsRequired().HasDefaultValue(TipoVotoComentario.UpVote);
        builder.Property(x => x.IdComentario).HasColumnType("integer");
        builder.HasIndex(x => x.IdComentario).HasDatabaseName("IX_UsuarioUpvoteComentario_IdComentario");
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_UsuarioUpvoteComentario_Usuario");
        builder.HasOne<Comentario>().WithMany().HasForeignKey(x => x.IdComentario)
            .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_UsuarioUpvoteComentario_Comentario");
    }
}
