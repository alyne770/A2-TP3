using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.LivroDigital.Models
{
    public class Autor
    {
        public int AutorId { get; set; }

        [Required(ErrorMessage = "O nome do autor é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome do autor deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A Data de Nascimento é obrigatória.")]
        [DataType(DataType.Date, ErrorMessage = "Data de nascimento inválida.")]
        [DisplayName("Data Nascimento")]
        public DateTime DataNascimento { get; set; }

        [StringLength(50, ErrorMessage = "A nacionalidade deve ter no máximo 50 caracteres.")]
        public string Nacionalidade { get; set; }

        [NotMapped]
        [JsonIgnore]
        public virtual ICollection<Livro> Livros { get; set; } = new List<Livro>();
    }
}
