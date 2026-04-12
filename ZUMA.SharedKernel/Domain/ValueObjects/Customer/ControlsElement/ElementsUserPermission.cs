namespace ZUMA.SharedKernel.Domain.ValueObjects.Customer.ControlsElement;

public class ElementsUserPermission
{
    public long UserId { get; set; }
    public Guid PublicUserId { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
