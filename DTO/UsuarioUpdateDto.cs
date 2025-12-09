namespace A2.DTO
{
    public class UsuarioUpdateDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string? SenhaHash { get; set; } // Senha é nula se não for alterada
    }
}
