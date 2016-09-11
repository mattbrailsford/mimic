using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Mimic.Web.ViewModels
{
    public class MimicViewModel : DynamicObject
    {
        private JObject _node;

        public MimicViewModel(JObject node)
        {
            _node = node;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prop = _node[binder.Name.ToLower()];
            if (prop != null)
            {
                return TryJTokenToDotNetObject(prop, out result);
            }

            result = null;
            return false;
        }

        protected bool TryJTokenToDotNetObject(JToken token, out object result)
        {
            var success = false;
            switch (token.Type)
            {
                case JTokenType.String:
                case JTokenType.Null:
                case JTokenType.Uri:
                    result = token.ToObject<string>();
                    success = true;
                    break;
                case JTokenType.Integer:
                    result = token.ToObject<long>();
                    success = true;
                    break;
                case JTokenType.Float:
                    result = token.ToObject<double>();
                    success = true;
                    break;
                case JTokenType.Boolean:
                    result = token.ToObject<bool>();
                    success = true;
                    break;
                case JTokenType.Date:
                    result = token.ToObject<DateTime>();
                    success = true;
                    break;
                case JTokenType.Object:
                    result = new MimicViewModel((JObject)token);
                    success = true;
                    break;
                case JTokenType.Array:
                    result = ((JArray)token).Select(JTokenToDotNetObject).ToList();
                    success = true;
                    break;
                case JTokenType.Guid:
                    result = token.ToObject<Guid>();
                    success = true;
                    break;
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    result = token.ToObject<byte[]>();
                    success = true;
                    break;
                case JTokenType.TimeSpan:
                    result = token.ToObject<TimeSpan>();
                    success = true;
                    break;
                case JTokenType.Property:
                    var tokenProp = token as JProperty;
                    result = new KeyValuePair<string, object>(tokenProp?.Name, JTokenToDotNetObject(tokenProp?.Value));
                    success = true;
                    break;
                case JTokenType.Comment:
                    result = ((JValue)token).Value.ToString();
                    success = true;
                    break;
                default:
                    result = null;
                    break;
            }

            return success;
        }

        protected object JTokenToDotNetObject(JToken token)
        {
            object val;
            TryJTokenToDotNetObject(token, out val);
            return val;
        }
    }
}
