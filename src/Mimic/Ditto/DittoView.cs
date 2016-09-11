using System;
using Mimic;
using Mimic.Web.ViewModels;
using Newtonsoft.Json.Linq;
using RazorEngine.Templating;

namespace Our.Umbraco.Ditto
{
    public class DittoView<TModel> : TemplateBase
    {
        private DittoViewModel _model;

        // We have to make Model dynamic because the RazorEngine sets the Model property explicitly
        // and we need a way of allowing the setter to be a different type to the getter.
        // In regulard MVC this is handled by the fact the view data is set via a SetViewData method
        // but the standalone RazorEngine sets the Model property directly, hence the workaround.
        public dynamic Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (value is TModel)
                {
                    _model = new DittoViewModel(value);
                }
                else if (value is JObject)
                {
                    // We won't actually instatiate the TModel type as we are just using
                    // it as a means to located the correct ViewModel json file. As MimicViewModel
                    // is a dynamic object, it can handle all property requests so no need to 
                    // instantiate the class explicitly.

                    // Get the mode type name
                    var modelTypeName = typeof(TModel).Name;

                    // Look for a template in Handlebars service with same name as model
                    if(!MimicContext.Current.Services.MimicService.HasModelTemplate(modelTypeName))
                        throw new ApplicationException("No ViewModel file found with the name " + modelTypeName + ".json");

                    var jsonModel = MimicContext.Current.Services.MimicService.GenerateModel(modelTypeName, MimicContext.Current.CurrentPage);

                    _model = new DittoViewModel(new MimicViewModel(jsonModel));
                }
                else
                {
                    _model = value;
                }
            }
        }
    }

    public class DittoViewModel
    {
        public dynamic View { get; set; }

        public DittoViewModel(dynamic viewModel)
        {
            View = viewModel;
        }
    }
}