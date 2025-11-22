using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Gamesbakery.WebGUI.Extensions
{
    public class IgnoreAntiforgeryTokenConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            // Для каждого селектора отключаем link generation
            foreach (var selector in model.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel.SuppressLinkGeneration = true;
                }

                // Добавляем атрибут для игнорирования антимошеннических токенов в коллекцию метаданных конечной точки
                selector.EndpointMetadata.Add(new IgnoreAntiforgeryTokenAttribute());
            }
        }
    }
}
