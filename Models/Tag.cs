using System.ComponentModel.DataAnnotations;

namespace DevLog.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
