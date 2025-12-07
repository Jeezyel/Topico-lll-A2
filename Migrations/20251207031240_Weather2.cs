using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace A2.Migrations
{
    /// <inheritdoc />
    public partial class Weather2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "latitude",
                table: "WeatherForces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "longitude",
                table: "WeatherForces",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "WeatherForces");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "WeatherForces");
        }
    }
}
