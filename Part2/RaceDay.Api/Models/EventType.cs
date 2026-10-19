using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

// Road Running, Trail Running, Walking or Cycling
public class EventType
{
    public int EventTypeId { get; set; }

    [MaxLength(50)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public List<Event> Events { get; set; } = new();
}
