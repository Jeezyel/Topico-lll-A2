using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class Motorista
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do motorista é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos.")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "A CNH é obrigatória.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "A CNH deve ter 11 dígitos.")]
        public string CNH { get; set; } // Carteira Nacional de Habilitação

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        // um motorista tiver um UsuarioId, ele é considerado um 'Usuario' com a Role 'Motorista'.
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // Um motorista pode ser responsável por várias rotas.
        public ICollection<Rota>? Rotas { get; set; }

        // Um motorista pode registrar várias incidências em rotas.
        //public ICollection<IncidenciaRota>? Incidencias { get; set; }
    }
}