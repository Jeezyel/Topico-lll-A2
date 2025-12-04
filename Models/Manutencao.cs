using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace A2.Models
{
    public class Manutencao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [ForeignKey("VeiculoId")]
        [JsonIgnore]
        public Veiculo? Veiculo { get; set; }

        [Required]
        public TipoManutencao Tipo { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        // DataFim é anulável (?). Se for null, significa que o veículo AINDA está na oficina.
        public DateTime? DataFim { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
        public string Descricao { get; set; }

        public decimal Custo { get; set; }

        [StringLength(100)]
        public string? OficinaOuResponsavel { get; set; }
    }

    public enum TipoManutencao
    {
        Preventiva = 0, // Agendada
        Corretiva = 1   // Quebrou, consertou
    }
}
