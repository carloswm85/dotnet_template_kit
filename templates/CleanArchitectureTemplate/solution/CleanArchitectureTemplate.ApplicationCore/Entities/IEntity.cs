namespace CleanArchitectureTemplate.ApplicationCore.Entities;

public interface IEntity<TKey>
{
    TKey Id { get; }
}
