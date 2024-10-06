using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exoplanets",
                columns: table => new
                {
                    name = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    parallax = table.Column<double>(type: "REAL", nullable: false),
                    x = table.Column<double>(type: "REAL", nullable: false),
                    y = table.Column<double>(type: "REAL", nullable: false),
                    z = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exoplanets", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "Stars",
                columns: table => new
                {
                    GAIA_id = table.Column<string>(type: "TEXT", maxLength: 19, nullable: false),
                    parallax = table.Column<double>(type: "REAL", nullable: false),
                    x = table.Column<double>(type: "REAL", nullable: false),
                    y = table.Column<double>(type: "REAL", nullable: false),
                    z = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stars", x => x.GAIA_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Exoplanets");

            migrationBuilder.DropTable(
                name: "Stars");
        }
    }
}
