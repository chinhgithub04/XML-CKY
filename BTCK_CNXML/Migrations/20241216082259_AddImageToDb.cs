using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTCK_CNXML.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Image_Hoas_HoaId",
                table: "Image");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Image",
                table: "Image");

            migrationBuilder.RenameTable(
                name: "Image",
                newName: "Images");

            migrationBuilder.RenameIndex(
                name: "IX_Image_HoaId",
                table: "Images",
                newName: "IX_Images_HoaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Hoas_HoaId",
                table: "Images",
                column: "HoaId",
                principalTable: "Hoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Hoas_HoaId",
                table: "Images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "Image");

            migrationBuilder.RenameIndex(
                name: "IX_Images_HoaId",
                table: "Image",
                newName: "IX_Image_HoaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Image",
                table: "Image",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Image_Hoas_HoaId",
                table: "Image",
                column: "HoaId",
                principalTable: "Hoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
