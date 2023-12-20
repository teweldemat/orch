using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orch.content.Migrations
{
    public partial class content_ref : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "content_file");

            migrationBuilder.RenameColumn(
                name: "RefCount",
                table: "content_file",
                newName: "ref_count");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "content_file",
                newName: "mime_type");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "content_file",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "content_file",
                newName: "file_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_content_file",
                table: "content_file",
                column: "file_id");

            migrationBuilder.CreateTable(
                name: "content_reference",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ref_count = table.Column<int>(type: "integer", nullable: false),
                    ref_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_reference", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_reference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_content_file",
                table: "content_file");

            migrationBuilder.RenameTable(
                name: "content_file",
                newName: "Files");

            migrationBuilder.RenameColumn(
                name: "ref_count",
                table: "Files",
                newName: "RefCount");

            migrationBuilder.RenameColumn(
                name: "mime_type",
                table: "Files",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "Files",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "file_id",
                table: "Files",
                newName: "FileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "FileId");
        }
    }
}
