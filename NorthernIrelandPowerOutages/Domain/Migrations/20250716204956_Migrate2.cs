using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Migrate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressApplicationUser");

            migrationBuilder.CreateTable(
                name: "FavouriteAddressPreferences",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    AddressId = table.Column<int>(type: "integer", nullable: false),
                    EmailAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SmsAlertsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteAddressPreferences", x => new { x.ApplicationUserId, x.AddressId });
                    table.ForeignKey(
                        name: "FK_FavouriteAddressPreferences_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavouriteAddressPreferences_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteAddressPreferences_AddressId",
                table: "FavouriteAddressPreferences",
                column: "AddressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavouriteAddressPreferences");

            migrationBuilder.CreateTable(
                name: "AddressApplicationUser",
                columns: table => new
                {
                    FavoriteAddressesId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressApplicationUser", x => new { x.FavoriteAddressesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AddressApplicationUser_Addresses_FavoriteAddressesId",
                        column: x => x.FavoriteAddressesId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddressApplicationUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressApplicationUser_UsersId",
                table: "AddressApplicationUser",
                column: "UsersId");
        }
    }
}
