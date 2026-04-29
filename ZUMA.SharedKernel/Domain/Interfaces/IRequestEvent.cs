namespace ZUMA.SharedKernel.Domain.Interfaces;

public interface IRequestEvent
{
    public Guid? PublicId { get; set; }
    public HttpMethod Method { get; set; }
}
