using System;
using System.ComponentModel.DataAnnotations;

namespace A2.DTOs
{
    public class VeiculoCreateUpdateDto
    {
        [Required(ErrorMessage = "A placa é obrigatória.")]
        [StringLength(15, ErrorMessage = "A placa não pode exceder 15 caracteres.")]
        public string Placa { get; set; }

        [Required]
        [StringLength(50)]
        public string Marca { get; set; }

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int AnoFabricacao { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CapacidadeCarga { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CapacidadeVolume { get; set; }

        public int Status { get; set; }

        // As datas são anuláveis para indicar que são opcionais
        public DateTime? DataUltimaManutencao { get; set; }

        public DateTime? DataProximaManutencao { get; set; }
    }
}
