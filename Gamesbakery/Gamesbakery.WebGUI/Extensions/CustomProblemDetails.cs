using Microsoft.AspNetCore.Mvc;

public class CustomProblemDetails
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string TraceId { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }

    public CustomProblemDetails(ActionContext context)
    {
        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1";
        Title = "One or more validation errors occurred.";
        Status = StatusCodes.Status400BadRequest;
        TraceId = context.HttpContext.TraceIdentifier;
        Errors = new Dictionary<string, string[]>();

        foreach (var keyModelStatePair in context.ModelState)
        {
            var key = keyModelStatePair.Key;
            var errors = keyModelStatePair.Value.Errors;
            if (errors != null && errors.Count > 0)
            {
                if (errors.Count == 1)
                {
                    Errors.Add(key, new[] { errors[0].ErrorMessage });
                }
                else
                {
                    var errorMessages = new string[errors.Count];
                    for (var i = 0; i < errors.Count; i++)
                    {
                        errorMessages[i] = errors[i].ErrorMessage;
                    }
                    Errors.Add(key, errorMessages);
                }
            }
        }
    }
}