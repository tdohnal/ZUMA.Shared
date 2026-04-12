namespace ZUMA.SharedKernel.MessagingContracts;

public class SuccessResponseBase
{
}

public class FailedResponseBase
{
    public string ErrorMessage { get; set; } = string.Empty;

}
