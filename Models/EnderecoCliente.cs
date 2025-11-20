using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class EnderecoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [StringLength(8)]
        public string CEP { get; set; }

        [Required]
        [StringLength(100)]
        public string Logradouro { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; }

        [StringLength(50)]
        public string? Complemento { get; set; }

        [Required]
        [StringLength(50)]
        public string Bairro { get; set; }

        [Required]
        [StringLength(50)]
        public string Cidade { get; set; }

        [Required]
        [StringLength(2)]
        public string UF { get; set; }

        // Campos para Latitude/Longitude (Nominatim)
        [Column(TypeName = "decimal(10,8)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal Longitude { get; set; }
    }
}
