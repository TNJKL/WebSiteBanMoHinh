using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSiteBanMoHinh.Models
{
    public class AppUserModel : IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { get; set; }

        public string Token { get; set; }

        [InverseProperty(nameof(MessageModel.Sender))]
        public virtual ICollection<MessageModel> SentMessages { get; set; }

        [InverseProperty(nameof(MessageModel.Receiver))]
        public virtual ICollection<MessageModel> ReceivedMessages { get; set; }


    }
}
