using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeamCubing.API.Models;

namespace TeamCubing.API.Helpers;

public static class ModelErrorsHelper
{
    public static void PutModelStateErrorsToResponseModel<T>(
        ModelStateDictionary modelState,
        T model) where T : ResponseModelBase
    {
        foreach (
            var modelError in modelState.Values.SelectMany(
                modelStateEntry => modelStateEntry.Errors))
        {
            model.Errors.Add(modelError.ErrorMessage);
        }
    }
}
