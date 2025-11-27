using System;
using Microsoft.EntityFrameworkCore.Migrations;
using A2.Models;

#nullable disable

namespace A2.Migrations
{
    public partial class BotaDado : Migration // O nome da classe pode mudar dependendo do nome que vc deu no passo 3, mantenha o que foi gerado
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. ROLES
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nome" },
                values: new object[,] {
                    { 1, "Administrador" },
                    { 2, "Motorista" },
                    { 3, "Cliente" }
                });

            // 2. CONFIGURAÇÕES DO SISTEMA
            migrationBuilder.InsertData(
                table: "ConfiguracoesSistema",
                columns: new[] { "Id", "ApiNome", "Chave", "Endpoint", "Valor" },
                values: new object[] { 1, "OpenWeatherMap", "SUA_CHAVE_NOS_USER_SECRETS", "https://api.openweathermap.org/data/2.5/", "Configuração Padrão" });

            // 3. USUÁRIOS (Hash de '123456')
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Email", "Nome", "RoleId", "SenhaHash" },
                values: new object[,] {
                    { 1, "admin@logifleet.com", "Admin Sistema", 1, "$2a$11$ZsW8.M5x/cW5.u.s.a.1.e.u.x.u.x.u.x.u.x.u.x.u.x.u.x.u" },
                    { 2, "carlos.mot@logifleet.com", "Carlos Motorista", 2, "$2a$11$ZsW8.M5x/cW5.u.s.a.1.e.u.x.u.x.u.x.u.x.u.x.u.x.u.x.u" },
                    { 3, "ana.mot@logifleet.com", "Ana Motorista", 2, "$2a$11$ZsW8.M5x/cW5.u.s.a.1.e.u.x.u.x.u.x.u.x.u.x.u.x.u.x.u" },
                    { 4, "roberto.cli@techstore.com", "Roberto Cliente", 3, "$2a$11$ZsW8.M5x/cW5.u.s.a.1.e.u.x.u.x.u.x.u.x.u.x.u.x.u.x.u" }
                });

            // 4. MOTORISTAS
            migrationBuilder.InsertData(
                table: "Motoristas",
                columns: new[] { "Id", "CNH", "CPF", "Email", "Nome", "Telefone", "UsuarioId" },
                values: new object[,] {
                    { 1, "11111111111", "12345678901", "carlos.mot@logifleet.com", "Carlos Oliveira", "11999998888", 2 },
                    { 2, "22222222222", "98765432109", "ana.mot@logifleet.com", "Ana Souza", "11977776666", 3 }
                });

            // 5. VEÍCULOS
            migrationBuilder.InsertData(
                table: "Veiculos",
                columns: new[] { "Id", "AnoFabricacao", "CapacidadeCarga", "CapacidadeVolume", "DataProximaManutencao", "DataUltimaManutencao", "Marca", "Modelo", "Placa", "Status" },
                values: new object[,] {
                    { 1, 2022, 8000.00m, 45.00m, DateTime.Now.AddMonths(3), DateTime.Now.AddMonths(-3), "Volvo", "VM 270", "LOG-1001", (int)StatusVeiculo.Disponivel },
                    { 2, 2023, 1500.00m, 12.00m, DateTime.Now.AddDays(-1), DateTime.Now.AddMonths(-6), "Mercedes", "Sprinter", "LOG-2002", (int)StatusVeiculo.EmManutencao },
                    { 3, 2021, 800.00m, 4.00m, DateTime.Now.AddMonths(1), DateTime.Now.AddMonths(-2), "Fiat", "Fiorino", "LOG-3003", (int)StatusVeiculo.EmRota }
                });

            // 6. MANUTENÇÕES
            migrationBuilder.InsertData(
                table: "Manutencoes",
                columns: new[] { "Id", "Custo", "DataFim", "DataInicio", "Descricao", "OficinaOuResponsavel", "Tipo", "VeiculoId" },
                values: new object[,] {
                    { 1, 450.00m, DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(-3).AddDays(-1), "Troca de óleo e filtros", "Oficina Central", 0, 1 },
                    { 2, 0.00m, null, DateTime.Now.AddDays(-2), "Motor falhando, aguardando peças", "Mecânica Rápida", 1, 2 }
                });

            // 7. CLIENTES
            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "CNPJ", "Email", "NomeContato", "NomeEmpresa", "Telefone" },
                values: new object[,] {
                    { 1, "12345678000199", "compras@techstore.com.br", "Roberto Lima", "TechStore Eletrônicos", "1130304040" },
                    { 2, "98765432000155", "gerencia@supermercado.com", "Fernanda Costa", "Supermercado Preço Bom", "1132325050" }
                });

            // 8. ENDEREÇOS
            migrationBuilder.InsertData(
                table: "EnderecosClientes",
                columns: new[] { "Id", "Bairro", "CEP", "Cidade", "ClienteId", "Complemento", "Latitude", "Logradouro", "Longitude", "Numero", "UF" },
                values: new object[,] {
                    { 1, "Bela Vista", "01310100", "São Paulo", 1, "Conjunto 101", -23.561496m, "Avenida Paulista", -46.655967m, "1000", "SP" },
                    { 2, "Centro", "01001000", "São Paulo", 2, "Loja Térrea", -23.5489m, "Praça da Sé", -46.6388m, "10", "SP" },
                    { 3, "Vila Madalena", "05416000", "São Paulo", 2, null, -23.5568m, "Rua Fradique Coutinho", -46.6866m, "500", "SP" }
                });

            // 9. JANELAS DE HORÁRIO
            migrationBuilder.InsertData(
                table: "JanelasHorarias",
                columns: new[] { "Id", "DiaSemana", "EnderecoClienteId", "HoraFim", "HoraInicio" },
                values: new object[,] {
                { 1, "Segunda a Sexta", 1, new TimeSpan(18, 0, 0), new TimeSpan(9, 0, 0) },
                { 2, "Sábado", 2, new TimeSpan(12, 0, 0), new TimeSpan(6, 0, 0) }
            });

            // 10. PEDIDOS
            migrationBuilder.InsertData(
                table: "Pedidos",
                columns: new[] { "Id", "ClienteId", "DataCriacao", "DataLimiteEntrega", "EnderecoEntregaId", "PesoTotalKg", "Status", "VolumeTotalM3" },
                values: new object[,] {
                    { 1, 1, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(5), 1, 1500.00m, (int)StatusPedido.Pendente, 10.00m },
                    { 2, 2, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(2), 2, 500.00m, (int)StatusPedido.Pendente, 3.00m },
                    { 3, 2, DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-1), 3, 50.00m, (int)StatusPedido.Entregue, 0.50m }
                });

            // 11. ITENS DE PEDIDO
            migrationBuilder.InsertData(
                table: "ItensPedido",
                columns: new[] { "Id", "CodigoProduto", "Descricao", "PedidoId", "PesoUnitarioKg", "Quantidade", "VolumeUnitarioM3" },
                values: new object[,] {
                    { 1, "TV55", "Smart TV 55 polegadas", 1, 25.00m, 60, 0.15m },
                    { 2, "ARROZ5KG", "Fardo de Arroz 5kg", 2, 5.00m, 100, 0.03m },
                    { 3, "SABONETE", "Caixa de Sabonetes", 3, 1.00m, 50, 0.01m }
                });

            // 12. ROTAS
            migrationBuilder.InsertData(
                table: "Rotas",
                columns: new[] { "Id", "DataRota", "MotoristaId", "Status", "VeiculoId" },
                values: new object[,] {
                    { 1, DateTime.Now.AddDays(1), 1, (int)StatusRota.Planejada, 1 },
                    { 2, DateTime.Now.AddDays(-1), 2, (int)StatusRota.Concluida, 3 }
                });

            // 13. ROTA PEDIDOS
            migrationBuilder.InsertData(
                table: "RotaPedidos",
                columns: new[] { "PedidoId", "RotaId", "OrdemEntrega", "StatusEntrega" },
                values: new object[,] {
                    { 1, 1, 2, "Pendente" },
                    { 2, 1, 1, "Pendente" },
                    { 3, 2, 1, "Entregue" }
                });

            // 14. PONTOS DE PARADA
            migrationBuilder.InsertData(
               table: "PontosDeParada",
               columns: new[] { "Id", "EnderecoClienteId", "Latitude", "Longitude", "OrdemParada", "RotaId" },
               values: new object[,] {
                    { 1, 2, -23.5489m, -46.6388m, 1, 1 },
                    { 2, 1, -23.561496m, -46.655967m, 2, 1 }
               });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ordem inversa para deletar
            migrationBuilder.DeleteData(table: "PontosDeParada", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "PontosDeParada", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "RotaPedidos", keyColumns: new[] { "PedidoId", "RotaId" }, keyValues: new object[] { 1, 1 });
            migrationBuilder.DeleteData(table: "RotaPedidos", keyColumns: new[] { "PedidoId", "RotaId" }, keyValues: new object[] { 2, 1 });
            migrationBuilder.DeleteData(table: "RotaPedidos", keyColumns: new[] { "PedidoId", "RotaId" }, keyValues: new object[] { 3, 2 });
            migrationBuilder.DeleteData(table: "Rotas", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Rotas", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "ItensPedido", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "ItensPedido", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "ItensPedido", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Pedidos", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Pedidos", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Pedidos", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "JanelasHorarias", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "JanelasHorarias", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "EnderecosClientes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "EnderecosClientes", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "EnderecosClientes", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Clientes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Clientes", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Manutencoes", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Manutencoes", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Veiculos", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Veiculos", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Veiculos", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Motoristas", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Motoristas", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Usuarios", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Usuarios", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Usuarios", keyColumn: "Id", keyValue: 3);
            migrationBuilder.DeleteData(table: "Usuarios", keyColumn: "Id", keyValue: 4);
            migrationBuilder.DeleteData(table: "ConfiguracoesSistema", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: 1);
            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: 2);
            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: 3);
        }
    }
}