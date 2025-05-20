using System.Text.Json.Serialization;

namespace PetService_Project_Api.DTO.PetDTO
{
    public class PetRequestDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("memberId")]
        public int MemberId { get; set; }

        [JsonPropertyName("petName")]
        public string? PetName { get; set; }

        [JsonPropertyName("petDelete")]
        public int? PetDelete { get; set; }

        [JsonPropertyName("petWeight")]
        public int? PetWeight { get; set; }

        [JsonPropertyName("petBirthday")]
        public DateTime? PetBirthday { get; set; }

        [JsonPropertyName("petDe")]
        public int? PetDe { get; set; }

        [JsonPropertyName("petImagePath")]
        public string? PetImagePath { get; set; }

    }
}
