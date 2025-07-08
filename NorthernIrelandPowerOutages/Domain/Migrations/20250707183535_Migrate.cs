using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Migrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Hazards");

            migrationBuilder.CreateTable(
                name: "HazardImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    HazardId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HazardImage_Hazards_HazardId",
                        column: x => x.HazardId,
                        principalTable: "Hazards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HazardImage_HazardId",
                table: "HazardImage",
                column: "HazardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HazardImage");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Hazards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }
    }
}
