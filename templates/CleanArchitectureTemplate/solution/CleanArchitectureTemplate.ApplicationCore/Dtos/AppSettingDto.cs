namespace CleanArchitectureTemplate.ApplicationCore.Dtos;

public class AppSettingDto
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public required string Value { get; set; }
    public string? ModifiedByUserId { get; set; }
    public DateTimeOffset? ModificationDate { get; set; }
}
