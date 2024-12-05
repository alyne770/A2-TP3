using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.LivroDigital.Models
{
    public class Livro
    {
        public int LivroId { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "O ano de publicação é obrigatório.")]
        [Range(1500, 2100, ErrorMessage = "O ano de publicação deve ser entre 1500 e o ano atual.")]
        public int AnoPublicacao { get; set; }

        [Required(ErrorMessage = "O ISBN é obrigatório.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "O ISBN deve ter entre 10 e 13 caracteres.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "A editora é obrigatória.")]
        public int EditoraId { get; set; }

        //[NotMapped]
        [JsonIgnore]
        public virtual Editora? Editora { get; set; }

        //[NotMapped]
        public virtual ICollection<Autor> Autores { get; set; }

        //[NotMapped]
        public virtual ICollection<Categoria> Categorias { get; set; }
      
    }
}
