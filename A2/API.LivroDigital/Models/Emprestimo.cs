using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.LivroDigital.Models
{
    public class Emprestimo
    {
        public int EmprestimoId { get; set; }

        [Required(ErrorMessage = "O Livro é obrigatório.")]
        public int LivroId { get; set; }

        //[NotMapped]
        [JsonIgnore]
        public virtual Livro? Livro { get; set; }

        
        public int UsuarioId { get; set; }

        //[NotMapped]
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "A data de empréstimo é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataEmprestimo { get; set; }

        [DataType(DataType.Date)]
        [CustomValidation(typeof(Emprestimo), "ValidarDataDevolucao")]
        public DateTime? DataDevolucao { get; set; }

        public static ValidationResult ValidarDataDevolucao(DateTime? dataDevolucao, ValidationContext context)
        {
            var instance = context.ObjectInstance as Emprestimo;
            if (dataDevolucao.HasValue && instance != null && dataDevolucao < instance.DataEmprestimo)
            {
                return new ValidationResult("A data de devolução não pode ser anterior à data de empréstimo.");
            }
            return ValidationResult.Success;
        }

    }
}
