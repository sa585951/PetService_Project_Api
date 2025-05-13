using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetService_Project_Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tChatMessage_fSession_id",
                table: "tChatMessage",
                column: "fSession_id");

            migrationBuilder.CreateIndex(
                name: "IX_tChatSession_fEmployee_id",
                table: "tChatSession",
                column: "fEmployee_id");

            migrationBuilder.CreateIndex(
                name: "IX_tChatSession_fMember_id",
                table: "tChatSession",
                column: "fMember_id");

            migrationBuilder.CreateIndex(
                name: "IX_tEmployee_Photo_fEmployeeId",
                table: "tEmployee_Photo",
                column: "fEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_tEmployee_Service_fDistrictId",
                table: "tEmployee_Service",
                column: "fDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_tEmployee_Service_fEmployeeId",
                table: "tEmployee_Service",
                column: "fEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_tHotel_Items_fHotel_id",
                table: "tHotel_Items",
                column: "fHotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_tHotel_Reviews_fHotel_id",
                table: "tHotel_Reviews",
                column: "fHotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_tHotel_Reviews_fMember_id",
                table: "tHotel_Reviews",
                column: "fMember_id");

            migrationBuilder.CreateIndex(
                name: "IX_tHotel_Reviews_fOrder_id",
                table: "tHotel_Reviews",
                column: "fOrder_id");

            migrationBuilder.CreateIndex(
                name: "IX_tHotel_Reviews_fRoomtype_id",
                table: "tHotel_Reviews",
                column: "fRoomtype_id");

            migrationBuilder.CreateIndex(
                name: "IX_tMemberSource_FSourceId",
                table: "tMemberSource",
                column: "FSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_tNewsFiles_fNews_Id",
                table: "tNewsFiles",
                column: "fNews_Id");

            migrationBuilder.CreateIndex(
                name: "IX_tOrder_Hotel_Detail_fHotelId",
                table: "tOrder_Hotel_Detail",
                column: "fHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_tOrder_Hotel_Detail_fOrderId",
                table: "tOrder_Hotel_Detail",
                column: "fOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tOrder_Hotel_Detail_fRoomDetailId",
                table: "tOrder_Hotel_Detail",
                column: "fRoomDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_tOrder_Walk_Detail_fEmployeeServiceId",
                table: "tOrder_Walk_Detail",
                column: "fEmployeeServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_tOrder_Walk_Detail_fOrderId",
                table: "tOrder_Walk_Detail",
                column: "fOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tPetList_fMemberId",
                table: "tPetList",
                column: "fMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_tQty_Status_fHotel_id",
                table: "tQty_Status",
                column: "fHotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_tRooms_Detail_fHotel_id",
                table: "tRooms_Detail",
                column: "fHotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_tRooms_Detail_fRoomtype_id",
                table: "tRooms_Detail",
                column: "fRoomtype_id");

            migrationBuilder.CreateIndex(
                name: "IX_tWalk_Reviews_fMember_id",
                table: "tWalk_Reviews",
                column: "fMember_id");

            migrationBuilder.CreateIndex(
                name: "IX_tWalk_Reviews_fOrder_id",
                table: "tWalk_Reviews",
                column: "fOrder_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
