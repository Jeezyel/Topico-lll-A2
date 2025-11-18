using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace A2.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da Role é obrigatório.")]
        [StringLength(50)]
        public string Nome { get; set; } // "Administrador", "Gerente", "Motorista"

        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
