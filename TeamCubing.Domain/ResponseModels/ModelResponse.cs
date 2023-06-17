namespace TeamCubing.Domain.ResponseModels;

public class ModelResponse<T> : BaseResponse
{
    public T Model { get; set; }
}
