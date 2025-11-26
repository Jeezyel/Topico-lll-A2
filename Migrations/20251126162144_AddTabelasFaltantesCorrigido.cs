using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelasFaltantesCorrigido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Rotas_RotaId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_RotaId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "RotaId",
                table: "Pedidos");

            migrationBuilder.CreateTable(
                name: "AlertasClimaticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RotaId = table.Column<int>(type: "int", nullable: false),
                    TipoAlerta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Severidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasClimaticos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertasClimaticos_Rotas_RotaId",
                        column: x => x.RotaId,
                        principalTable: "Rotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidenciasRota",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RotaId = table.Column<int>(type: "int", nullable: false),
                    MotoristaId = table.Column<int>(type: "int", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidenciasRota", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidenciasRota_Motoristas_MotoristaId",
                        column: x => x.MotoristaId,
                        principalTable: "Motoristas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidenciasRota_Rotas_RotaId",
                        column: x => x.RotaId,
                        principalTable: "Rotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JanelasHorarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnderecoClienteId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JanelasHorarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JanelasHorarias_EnderecosClientes_EnderecoClienteId",
                        column: x => x.EnderecoClienteId,
                        principalTable: "EnderecosClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsIntegracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsIntegracao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PontosDeParada",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RotaId = table.Column<int>(type: "int", nullable: false),
                    EnderecoClienteId = table.Column<int>(type: "int", nullable: false),
                    OrdemParada = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PontosDeParada", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PontosDeParada_EnderecosClientes_EnderecoClienteId",
                        column: x => x.EnderecoClienteId,
                        principalTable: "EnderecosClientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PontosDeParada_Rotas_RotaId",
                        column: x => x.RotaId,
                        principalTable: "Rotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RotaPedidos",
                columns: table => new
                {
                    RotaId = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    OrdemEntrega = table.Column<int>(type: "int", nullable: false),
                    StatusEntrega = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotaPedidos", x => new { x.RotaId, x.PedidoId });
                    table.ForeignKey(
                        name: "FK_RotaPedidos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RotaPedidos_Rotas_RotaId",
                        column: x => x.RotaId,
                        principalTable: "Rotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasClimaticos_RotaId",
                table: "AlertasClimaticos",
                column: "RotaId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidenciasRota_MotoristaId",
                table: "IncidenciasRota",
                column: "MotoristaId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidenciasRota_RotaId",
                table: "IncidenciasRota",
                column: "RotaId");

            migrationBuilder.CreateIndex(
                name: "IX_JanelasHorarias_EnderecoClienteId",
                table: "JanelasHorarias",
                column: "EnderecoClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PontosDeParada_EnderecoClienteId",
                table: "PontosDeParada",
                column: "EnderecoClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PontosDeParada_RotaId",
                table: "PontosDeParada",
                column: "RotaId");

            migrationBuilder.CreateIndex(
                name: "IX_RotaPedidos_PedidoId",
                table: "RotaPedidos",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasClimaticos");

            migrationBuilder.DropTable(
                name: "ConfiguracoesSistema");

            migrationBuilder.DropTable(
                name: "IncidenciasRota");

            migrationBuilder.DropTable(
                name: "JanelasHorarias");

            migrationBuilder.DropTable(
                name: "LogsIntegracao");

            migrationBuilder.DropTable(
                name: "PontosDeParada");

            migrationBuilder.DropTable(
                name: "RotaPedidos");

            migrationBuilder.AddColumn<int>(
                name: "RotaId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_RotaId",
                table: "Pedidos",
                column: "RotaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Rotas_RotaId",
                table: "Pedidos",
                column: "RotaId",
                principalTable: "Rotas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
