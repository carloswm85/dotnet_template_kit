using TestContactEntity = CleanArchitectureTemplate.ApplicationCore.Entities.TestContact;

namespace CleanArchitectureTemplate.Web.Models.ViewModels.TestContact;

public class ContacListViewModel
{
    public IList<TestContactEntity> TestContacts { get; set; } = [];
}
