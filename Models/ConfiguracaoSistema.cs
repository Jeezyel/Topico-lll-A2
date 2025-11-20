using System.ComponentModel.DataAnnotations;

namespace A2.Models
{
    public class ConfiguracaoSistema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ApiNome { get; set; }

        [Required]
        [StringLength(200)]
        public string Endpoint { get; set; }

        [Required]
        [StringLength(100)]
        public string Chave { get; set; }

        [Required]
        [StringLength(100)]
        public string Valor { get; set; }
    }
}
