namespace TeamCubing.Domain.Models;

public interface ICosmosModel
{
    public string PartitionKey { get; set; }
}
