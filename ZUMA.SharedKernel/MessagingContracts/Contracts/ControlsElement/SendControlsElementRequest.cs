using ZUMA.SharedKernel.Domain.ValueObjects.Customer.ControlsElement;
using ZUMA.SharedKernel.Enums;
using ZUMA.SharedKernel.MessagingContracts.Base;

namespace ZUMA.SharedKernel.MessagingContracts.Contracts.ControlsElement;


#region Get ControlsElement By ID
public record SendGetControlsElementByIdRequest : IRequestEvent
{
    public Guid PublicId { get; set; }
}
public record SendGetControlsElementByIdSuccess : ISuccessResponse
{
    public required ControlsElementMessageModel ControlsElement { get; set; }
}
#endregion

#region Get All ControlsElements
public record SendGetControlsElementsRequest : IRequestEvent
{

}
public record SendGetControlsElementsSuccess : ISuccessResponse
{
    public required List<ControlsElementMessageModel> ControlsElement { get; set; }
}

public class ControlsElementMessageModel
{
    #region Base

    public Guid PublicId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Deleted { get; set; }

    #endregion

    public required string Title { get; set; }

    public required Guid OwnerUserPublicId { get; set; }

    public required ListType ListType { get; set; }

    public List<ControlsElementsItemModel> Items { get; set; } = new();
    public ElementsPermission ElementsPermission { get; set; } = new();
}

public class ControlsElementsItemModel
{
    public Guid PublicId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Deleted { get; set; }
    public Guid ControlElementPublicId { get; set; }
    public required string Content { get; set; }
    public string? Metadata { get; set; }
}

#endregion

#region Create ControlsElement
public record SendCreateControlsElementRequest : IRequestEvent
{
    public required string Title { get; set; }

    public required Guid OwnerUserPublicId { get; set; }

    public required ListType ListType { get; set; }

    public List<ControlsElementsItemModel> Items { get; set; } = new();
    public ElementsPermission ElementsPermission { get; set; } = new();
}
public record SendCreateControlsElementSuccess : ISuccessResponse
{
    public ControlsElementMessageModel ControlsElement { get; set; }
}
#endregion

#region Update ControlsElement
public record SendUpdateControlsElementRequest : IRequestEvent
{
    #region Base

    public Guid PublicId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Deleted { get; set; }

    #endregion

    public required string Title { get; set; }

    public required Guid OwnerUserPublicId { get; set; }

    public required ListType ListType { get; set; }

    public List<ControlsElementsItemModel> Items { get; set; } = new();
    public ElementsPermission ElementsPermission { get; set; } = new();
}
public record SendUpdateControlsElementSuccess : ISuccessResponse
{
    public ControlsElementMessageModel ControlsElement { get; set; }
}
#endregion

#region Delete ControlsElement
public record SendDeleteControlsElementRequest : IRequestEvent
{
    public Guid PublicId { get; set; }
}
public record SendDeleteControlsElementSuccess : ISuccessResponse
{
}
#endregion

#region Common Failed Response
public record SendControlsElementFailed : IFailedResponse
{
    public string ErrorMessage { get; set; }
    public string ErrorCode = "INTERNAL_ERROR";

}
#endregion

