using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2.Migrations
{
    /// <inheritdoc />
    public partial class AddJanelaHorarioNavigationToEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnderecoClienteId1",
                table: "JanelasHorarias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JanelasHorarias_EnderecoClienteId1",
                table: "JanelasHorarias",
                column: "EnderecoClienteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_JanelasHorarias_EnderecosClientes_EnderecoClienteId1",
                table: "JanelasHorarias",
                column: "EnderecoClienteId1",
                principalTable: "EnderecosClientes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JanelasHorarias_EnderecosClientes_EnderecoClienteId1",
                table: "JanelasHorarias");

            migrationBuilder.DropIndex(
                name: "IX_JanelasHorarias_EnderecoClienteId1",
                table: "JanelasHorarias");

            migrationBuilder.DropColumn(
                name: "EnderecoClienteId1",
                table: "JanelasHorarias");
        }
    }
}
