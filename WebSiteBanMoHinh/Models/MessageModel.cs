using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebSiteBanMoHinh.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Message text is required")]
        public string Text { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public AppUserModel Sender { get; set; }
        [ForeignKey(nameof(ReceiverId))]
        public AppUserModel Receiver { get; set; }
        public bool IsRead { get; set; }
    }
}
