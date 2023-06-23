namespace TeamCubing.Domain.ResponseModels;

public class BaseResponse
{
    public bool IsSuccess { get; set; }

    public string ErrorMessage { get; set; }
}
