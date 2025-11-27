using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A2.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "A placa deve ter 10 caracteres.")]
        public string Placa { get; set; }

        [Required]
        [StringLength(50)]
        public string Marca { get; set; }

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; }

        [Required]
        public int AnoFabricacao { get; set; }

        [Required]
        public decimal CapacidadeCarga { get; set; }

        [Required]
        public decimal CapacidadeVolume { get; set; }

        public DateTime DataUltimaManutencao { get; set; }

        // para validar data atual > DataProximaManutencao
        public DateTime DataProximaManutencao { get; set; }

        public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

        public ICollection<Rota>? Rotas { get; set; }

        public ICollection<Manutencao>? Manutencoes { get; set; }
    }

    public enum StatusVeiculo
    {
        Disponivel = 0,
        EmRota = 1,
        EmManutencao = 2,
        Inativo = 3
    }
}
