using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeamCubing.Domain.ResponseModels;

namespace TeamCubing.BLL.Helpers;

public static class ModelErrorsHelper
{
    public static void PutModelStateErrorsToResponseModel<T>(
        ModelStateDictionary modelState,
        T model) where T : BaseResponse
    {
        var sb = new StringBuilder();
        foreach (
            var modelError in modelState.Values.SelectMany(
                modelStateEntry => modelStateEntry.Errors))
        {
            sb.Append(modelError.ErrorMessage + "\n");
        }

        model.ErrorMessage = sb.ToString();
    }

    public static void MapToSingleError<T>(IEnumerable<IdentityError> errors, T model)
        where T : BaseResponse
    {
        var sb = new StringBuilder();
        foreach (var error in errors)
        {
            sb.Append(error.Description + "\n");
        }

        model.ErrorMessage = sb.ToString();
    }
}
