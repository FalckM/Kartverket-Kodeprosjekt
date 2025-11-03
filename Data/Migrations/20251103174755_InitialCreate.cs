using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NRLWebApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Etternavn",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fornavn",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrganisasjonID",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Organisasjoner",
                columns: table => new
                {
                    OrganisasjonID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Navn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisasjoner", x => x.OrganisasjonID);
                });

            migrationBuilder.CreateTable(
                name: "Statuser",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Navn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuser", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "Hindre",
                columns: table => new
                {
                    HinderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Navn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Hoyde = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Beskrivelse = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Lokasjon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tidsstempel = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hindre", x => x.HinderID);
                    table.ForeignKey(
                        name: "FK_Hindre_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hindre_Statuser_StatusID",
                        column: x => x.StatusID,
                        principalTable: "Statuser",
                        principalColumn: "StatusID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Behandlinger",
                columns: table => new
                {
                    BehandlingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kommentar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tidsstempel = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinderID = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Behandlinger", x => x.BehandlingID);
                    table.ForeignKey(
                        name: "FK_Behandlinger_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Behandlinger_Hindre_HinderID",
                        column: x => x.HinderID,
                        principalTable: "Hindre",
                        principalColumn: "HinderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganisasjonID",
                table: "AspNetUsers",
                column: "OrganisasjonID");

            migrationBuilder.CreateIndex(
                name: "IX_Behandlinger_ApplicationUserId",
                table: "Behandlinger",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Behandlinger_HinderID",
                table: "Behandlinger",
                column: "HinderID");

            migrationBuilder.CreateIndex(
                name: "IX_Hindre_ApplicationUserId",
                table: "Hindre",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Hindre_StatusID",
                table: "Hindre",
                column: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organisasjoner_OrganisasjonID",
                table: "AspNetUsers",
                column: "OrganisasjonID",
                principalTable: "Organisasjoner",
                principalColumn: "OrganisasjonID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organisasjoner_OrganisasjonID",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Behandlinger");

            migrationBuilder.DropTable(
                name: "Organisasjoner");

            migrationBuilder.DropTable(
                name: "Hindre");

            migrationBuilder.DropTable(
                name: "Statuser");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganisasjonID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Etternavn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Fornavn",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganisasjonID",
                table: "AspNetUsers");
        }
    }
}
