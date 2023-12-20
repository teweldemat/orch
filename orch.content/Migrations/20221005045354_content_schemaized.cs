using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace orch.content.Migrations
{
    public partial class content_schemaized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.RenameTable(
                name: "content_reference",
                newName: "content_reference",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "content_file",
                newName: "content_file",
                newSchema: "content");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "content_reference",
                schema: "content",
                newName: "content_reference");

            migrationBuilder.RenameTable(
                name: "content_file",
                schema: "content",
                newName: "content_file");
        }
    }
}
