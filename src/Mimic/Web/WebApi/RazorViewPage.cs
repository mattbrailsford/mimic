using System;
using RazorEngine.Templating;

namespace Mimic.Web.WebApi
{
    public class RazorViewPage : TemplateBase
    {
        public HtmlHelper Html { get; set; }

        public RazorViewPage()
        {
            Html = new HtmlHelper(this);
        }
    }

    public class HtmlHelper
    {
        private TemplateBase _currentView;

        public HtmlHelper(TemplateBase currentView)
        {
            _currentView = currentView;
        }

        public virtual TemplateWriter Partial(string name, object model = null)
        {
            return _currentView.Include(name, model);

            //this._currentView.TemplateService.e
            //var instance = _currentView.TemplateService.CreateTemplate()

            //if (instance == null)
            //    throw new ArgumentException("No template could be resolved with name '" + name + "'");

            //var result = new TemplateWriter(tw =>
            //{
            //    var t = instance.Run(new ExecuteContext());
            //    tw.Write(t);
            //});

            //return result;
        }
    }
}
