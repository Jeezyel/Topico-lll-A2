using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace A2.Models
{
    public class PontoDeParada
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O ID da Rota é obrigatório.")]
        public int RotaId { get; set; }

        [Required(ErrorMessage = "O ID do Endereço do Cliente é obrigatório.")]
        public int EnderecoClienteId { get; set; } // FK

        [Required(ErrorMessage = "A ordem do ponto é obrigatória.")]
        public int OrdemParada { get; set; }

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        // Navegação
        [ForeignKey(nameof(RotaId))]
        [JsonIgnore]
        public Rota? Rota { get; set; }

        [ForeignKey(nameof(EnderecoClienteId))]
        [JsonIgnore]
        public EnderecoCliente? EnderecoCliente { get; set; }
    }
}
