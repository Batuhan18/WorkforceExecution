namespace WorkforceExecution.Domain.Entities;

public class SubTypeOfWork : BaseEntity
{
    public string Code { get; set; } = null!; // STOW-01
    public int TypeOfWorkId { get; set; }
    public TypeOfWork TypeOfWork { get; set; } = null!;
    public ICollection<SubSubTypeOfWork> SubSubTypes { get; set; } = new List<SubSubTypeOfWork>();
}
