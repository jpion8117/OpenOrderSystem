using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Twilio.Rest.Video.V1.Room.Participant;

namespace OpenOrderSystem.Data.DataModels
{
    [PrimaryKey(nameof(OrganizationId), nameof(UsertId))]
    public class OrganizationUser
    {
        public string OrganizationId { get; set; } = string.Empty;
        public Organization? Organization { get; set; }
        public string UsertId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
    }
}
