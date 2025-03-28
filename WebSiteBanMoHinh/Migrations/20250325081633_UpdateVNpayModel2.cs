﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSiteBanMoHinh.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVNpayModel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "VnpayInfos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "VnpayInfos");
        }
    }
}
