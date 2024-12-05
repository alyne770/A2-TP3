using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.LivroDigital.Models
{
    public class Editora
    {
        public int EditoraId { get; set; }

        [Required(ErrorMessage = "O nome da editora é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome da editora deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
        public string Cidade { get; set; }

        [JsonIgnore]
        public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
    }
}
