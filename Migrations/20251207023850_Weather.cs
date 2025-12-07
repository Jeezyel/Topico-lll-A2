using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2.Migrations
{
    /// <inheritdoc />
    public partial class Weather : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "WeatherForces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Temperatura = table.Column<double>(type: "float", nullable: false),
                    SensacaoTermica = table.Column<double>(type: "float", nullable: false),
                    Icone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoAlerta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severidade = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForces", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherForces");

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
    }
}
