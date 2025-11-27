using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace A2.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CNPJ", "Email", "NomeContato", "NomeEmpresa", "Telefone" },
                values: new object[] { 1, "00000000000000", "contato@alfa.com", "Maria Silva", "Distribuidora Alfa", "1133445566" });

            migrationBuilder.InsertData(
                table: "ConfiguracoesSistema",
                columns: new[] { "Id", "ApiNome", "Chave", "Endpoint", "Valor" },
                values: new object[] { 1, "WeatherAPI", "WEATHER_API_KEY", "https://api.clima.com/v1", "xyz123abc" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Administrador" },
                    { 2, "Motorista" },
                    { 3, "Cliente" }
                });

            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "AnoFabricacao", "CapacidadeCarga", "CapacidadeVolume", "DataProximaManutencao", "DataUltimaManutencao", "Marca", "Modelo", "Placa", "Status" },
                values: new object[] { 1, 2020, 4500.00m, 25.00m, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ford", "Cargo 816", "ABC1234", 1 });

            migrationBuilder.InsertData(
                table: "EnderecosClientes",
                columns: new[] { "Id", "Bairro", "CEP", "Cidade", "ClienteId", "Complemento", "Latitude", "Logradouro", "Longitude", "Numero", "UF" },
                values: new object[] { 1, "Centro", "01001000", "São Paulo", 1, "Galpão A", -23.550520m, "Rua Principal", -46.633308m, "100", "SP" });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "Nome", "RoleId", "SenhaHash" },
                values: new object[,]
                {
                    { 1, "admin@empresa.com", "Admin Sistema", 1, "hashed_admin_password" },
                    { 2, "joao@empresa.com", "João Motorista", 2, "hashed_joao_password" }
                });

            migrationBuilder.InsertData(
                table: "Motoristas",
                columns: new[] { "Id", "CNH", "CPF", "Email", "Nome", "Telefone", "UsuarioId" },
                values: new object[] { 1, "12345678901", "11122233344", "joao@empresa.com", "João Motorista", "999887766", 2 });

            migrationBuilder.InsertData(
                table: "Pedidos",
                columns: new[] { "Id", "ClienteId", "DataCriacao", "DataLimiteEntrega", "EnderecoEntregaId", "PesoTotalKg", "Status", "VolumeTotalM3" },
                values: new object[] { 1, 1, new DateTime(2025, 11, 26, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 500.00m, 0, 5.00m });

            migrationBuilder.InsertData(
                table: "ItensPedido",
                columns: new[] { "Id", "CodigoProduto", "Descricao", "PedidoId", "PesoUnitarioKg", "Quantidade", "VolumeUnitarioM3" },
                values: new object[,]
                {
                    { 1, "PROD001", "Caixa de Eletrônicos", 1, 40.00m, 10, 0.40m },
                    { 2, "PROD005", "Pallet de Roupas", 1, 20.00m, 5, 0.20m }
                });

            migrationBuilder.InsertData(
                table: "Rotas",
                columns: new[] { "Id", "DataRota", "MotoristaId", "Status", "VeiculoId" },
                values: new object[] { 1, new DateTime(2025, 11, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, 1 });

            migrationBuilder.InsertData(
                table: "PontosDeParada",
                columns: new[] { "Id", "EnderecoClienteId", "Latitude", "Longitude", "OrdemParada", "RotaId" },
                values: new object[] { 1, 1, -23.550520m, -46.633308m, 1, 1 });

            migrationBuilder.InsertData(
                table: "RotaPedidos",
                columns: new[] { "PedidoId", "RotaId", "OrdemEntrega", "StatusEntrega" },
                values: new object[] { 1, 1, 1, "Aguardando" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfiguracoesSistema",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ItensPedido",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ItensPedido",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PontosDeParada",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RotaPedidos",
                keyColumns: new[] { "PedidoId", "RotaId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Pedidos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rotas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EnderecosClientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Motoristas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Veiculos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clientes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
