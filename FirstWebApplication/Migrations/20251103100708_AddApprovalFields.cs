using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalComments",
                table: "Obstacles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Obstacles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Obstacles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Obstacles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "Obstacles",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                table: "Obstacles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Obstacles",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalComments",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                table: "Obstacles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Obstacles");
        }
    }
}
