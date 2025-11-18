using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace A2.Models
{
    public class Usuario
    {
        [Key] // Marca a propriedade como a chave primária
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Define o tamanho máximo do campo Nome
        public string Nome { get; set; }

        [Required]
        [EmailAddress] // Valida o formato do email
        [StringLength(100)] // Define o tamanho máximo do campo Email
        public string Email { get; set; }

        [Required]
        public string SenhaHash { get; set; } // O hash da senha armazenada

        //[ForeignKey("Role")] // Relacionamento de chave estrangeira com a tabela Role
        //public int RoleID { get; set; }

        // Navegação para a entidade Role (relacionamento de 1:N)
       // public Role Role { get; set; }
    }
}
