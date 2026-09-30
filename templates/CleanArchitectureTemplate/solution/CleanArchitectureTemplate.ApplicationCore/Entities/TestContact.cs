using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureTemplate.ApplicationCore.Entities;

public class TestContact
{
    public int TestContactId { get; set; }

    public string? OwnerID { get; set; } // user ID from AspNetUser table.

    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }

    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    public TestContactStatus Status { get; set; }
}

public enum TestContactStatus
{
    Submitted,
    Approved,
    Rejected,
}
