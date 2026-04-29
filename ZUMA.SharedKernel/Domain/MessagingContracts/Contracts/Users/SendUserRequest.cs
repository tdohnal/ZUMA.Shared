using ZUMA.SharedKernel.Domain.Interfaces;

namespace ZUMA.SharedKernel.Domain.MessagingContracts.Contracts.Users;


#region Get User By ID
public record SendGetUserByIdRequest : IRequestEvent
{
    public Guid PublicId { get; set; }
}
public record SendGetUserByIdSuccess : ISuccessResponse
{
    public UserMessageModel User { get; set; }
}
#endregion

#region Get All Users
public record SendGetUsersRequest : IRequestEvent
{

}
public record SendGetUsersSuccess : ISuccessResponse
{
    public List<UserMessageModel> User { get; set; }
}

public class UserMessageModel
{
    public Guid PublicId { get; set; }
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public DateTime? Deleted { get; set; }
}

#endregion

#region Create User
public record SendCreateUserRequest : IRequestEvent
{
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}
public record SendCreateUserSuccess : ISuccessResponse
{
    public UserMessageModel User { get; set; }
}
#endregion

#region Update User
public record SendUpdateUserRequest : IRequestEvent
{
    public Guid PublicId { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
}
public record SendUpdateUserSuccess : ISuccessResponse
{
    public UserMessageModel User { get; set; }
}
#endregion

#region Delete User
public record SendDeleteUserRequest : IRequestEvent
{
    public Guid PublicId { get; set; }
}
public record SendDeleteUserSuccess : ISuccessResponse
{
}
#endregion

#region Common Failed Response
public record SendUserFailed : IFailedResponse
{
    public string ErrorMessage { get; set; }
    public string ErrorCode = "INTERNAL_ERROR";

}
#endregion

