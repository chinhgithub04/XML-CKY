using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTCK_CNXML.Migrations
{
    /// <inheritdoc />
    public partial class chinhSuaDatHoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "DatHoas");

            migrationBuilder.RenameColumn(
                name: "CheckOut",
                table: "DatHoas",
                newName: "OrderDate");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DatHoas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DatHoas");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "DatHoas",
                newName: "CheckOut");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "DatHoas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
