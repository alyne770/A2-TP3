using API.LivroDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace API.LivroDigital.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public AppDbContext()
        {
            
        }

        public DbSet<Livro> Livros { get; set; }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Editora> Editoras { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração para a relação muitos-para-muitos entre Livro e Autor
            modelBuilder.Entity<Livro>()
                .HasMany(l => l.Autores)
                .WithMany(a => a.Livros)
                .UsingEntity<Dictionary<string, object>>(
                    "LivroAutor", // Nome da tabela de junção
                    j => j.HasOne<Autor>().WithMany().HasForeignKey("AutorId"), // Configuração da chave estrangeira para Autor
                    j => j.HasOne<Livro>().WithMany().HasForeignKey("LivroId")  // Configuração da chave estrangeira para Livro
                );

            // Configuração para a relação muitos-para-muitos entre Livro e Categoria
            modelBuilder.Entity<Livro>()
                .HasMany(l => l.Categorias)
                .WithMany(c => c.Livros)
                .UsingEntity<Dictionary<string, object>>(
                    "LivroCategoria", // Nome da tabela de junção
                    j => j.HasOne<Categoria>().WithMany().HasForeignKey("CategoriaId"), // Configuração da chave estrangeira para Categoria
                    j => j.HasOne<Livro>().WithMany().HasForeignKey("LivroId")          // Configuração da chave estrangeira para Livro
                );

            base.OnModelCreating(modelBuilder);
        }

    }
}
