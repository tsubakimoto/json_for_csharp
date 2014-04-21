using System.ComponentModel.DataAnnotations;

namespace itunes_aspx.Models
{
    public class SearchModel
    {
        [Required]
        [Display(Name = "歌手名")]
        public string Artist { get; set; }
    }
}