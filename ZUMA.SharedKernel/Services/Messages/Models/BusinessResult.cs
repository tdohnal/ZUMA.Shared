public class BusinessResult<TSuccess, TFailure>
{
    public TSuccess? SuccessData { get; init; }
    public TFailure? FailureData { get; init; }
    public bool IsSuccess => SuccessData != null;

    public static BusinessResult<TSuccess, TFailure> Success(TSuccess data) => new() { SuccessData = data };
    public static BusinessResult<TSuccess, TFailure> Failure(TFailure data) => new() { FailureData = data };
}