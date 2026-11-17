using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRegistrationDateAndAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "RegistrationDate",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AppSetting",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PreviousValue",
                table: "AppSetting",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AppSetting",
                columns: new[] { "Name", "Category", "Description", "IsPrivate", "ModificationDate", "ModifiedByUserId", "PreviousValue", "Type", "Value" },
                values: new object[] { "AnnouncementMessage", "Application", "Text shown in a banner at the top of every page. Leave empty to hide the banner.", false, null, null, null, "String", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSetting",
                keyColumn: "Name",
                keyValue: "AnnouncementMessage");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AppSetting",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "PreviousValue",
                table: "AppSetting",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
