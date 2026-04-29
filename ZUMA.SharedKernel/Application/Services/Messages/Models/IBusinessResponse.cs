public interface IBusinessResponse { }
public record BusinessSuccess(string Message = "OK") : IBusinessResponse;
public record BusinessFailure(string Error, int StatusCode = 400) : IBusinessResponse;