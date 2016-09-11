namespace Mimic.Handlebars.Helpers
{
    [HandlebarsHelper("range")]
    public class RangeHelper : HandlebarsHelper
    {
        public override string GetJs()
        {
            return @"function(from, to, options){ 
                var data = options.data ? Handlebars.createFrame(options.data) : {};
                var out = """";
                for (var i = from; i <= to; i++) {
                    data.first = i == from;
                    data.last = i == to;
                    data.index = i - from;
                    out += options.fn(i, { data : data });
                }
                return out;
            }";
        }
    }
}
