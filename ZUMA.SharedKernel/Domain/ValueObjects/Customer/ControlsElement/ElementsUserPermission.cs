namespace ZUMA.SharedKernel.Domain.ValueObjects.Customer.ControlsElement;

public class ElementsUserPermission
{
    public Guid PublicUserId { get; set; }
    public bool CanView { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
