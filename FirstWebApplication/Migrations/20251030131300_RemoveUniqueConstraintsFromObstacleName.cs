using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintsFromObstacleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Obstacles_ObstacleName",
                table: "Obstacles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Obstacles_ObstacleName",
                table: "Obstacles",
                column: "ObstacleName",
                unique: true);
        }
    }
}
