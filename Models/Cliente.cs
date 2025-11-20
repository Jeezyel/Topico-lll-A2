using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
        [StringLength(100)]
        public string NomeEmpresa { get; set; }

        [Required(ErrorMessage = "O CNPJ é obrigatório.")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "O CNPJ deve ter 14 dígitos.")]
        public string CNPJ { get; set; }

        [StringLength(100)]
        public string? NomeContato { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public ICollection<EnderecoCliente>? Enderecos { get; set; }
    }
}
