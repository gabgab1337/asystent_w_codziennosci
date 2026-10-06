using AssistantDatabase.Model;

namespace Assistant.Api.Contracts
{
    /// <summary>
    /// A protege (ASD person) as seen by their caregiver. Password hashes are never included.
    /// </summary>
    public class ProtegeDto
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public UserType Type { get; set; }

        public bool IsActive { get; set; }

        public DateTime JoiningDate { get; set; }

        /// <summary>True when this protege is the caregiver's current <c>SelectedASD</c>.</summary>
        public bool IsSelected { get; set; }

        public static ProtegeDto FromEntity(UserDM user, int? selectedProtegeId)
        {
            return new ProtegeDto
            {
                Id = user.Id,
                Username = user.Username,
                Type = user.Type,
                IsActive = user.IsActive,
                JoiningDate = user.JoiningDate,
                IsSelected = selectedProtegeId.HasValue && selectedProtegeId.Value == user.Id
            };
        }
    }
}
