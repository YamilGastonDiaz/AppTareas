using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTareas.Migrations
{
    /// <inheritdoc />
    public partial class Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS(SELECT Id FROM AspNetRoles WHERE Id = '0d16444c-1519-4b25-86b9-cb9dec734ef4')
                BEGIN
                    INSERT AspNetRoles (Id, Name, NormalizedName)
                    VALUES ('0d16444c-1519-4b25-86b9-cb9dec734ef4', 'admin', 'ADMIN')
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Id = '0d16444c-1519-4b25-86b9-cb9dec734ef4';");
        }
    }
}
