using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace A2.Models
{
    public class JanelaHorario
    {
        [Key] // Chave primária
        public int Id { get; set; }

        [Required(ErrorMessage = "O ID do Endereço do Cliente é obrigatório.")]
        public int EnderecoClienteId { get; set; } // FK

        [Required]
        [StringLength(15, ErrorMessage = "O dia da semana deve ter no máximo 15 caracteres.")]
        public string DiaSemana { get; set; }

        [Required(ErrorMessage = "A hora de início é obrigatória.")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "A hora de fim é obrigatória.")]
        public TimeSpan HoraFim { get; set; }

        [ForeignKey(nameof(EnderecoClienteId))] // FK para EnderecoCliente
        [JsonIgnore]
        public EnderecoCliente? EnderecoCliente { get; set; }
    }
}
