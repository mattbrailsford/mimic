using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Newtonsoft.Json.Linq;

namespace Mimic.Handlebars.ObjectConverters
{
    public class JValueObjectConverter : IObjectConverter
    {
        public bool TryConvert(Engine engine, object value, out JsValue result)
        {
            var jValue = value as JValue;
            if (jValue != null)
            {
                result = JsValue.FromObject(engine, jValue.Value);
                return true;
            }

            result = default(JsValue);
            return false;
        }
    }
}
