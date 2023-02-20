using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace UZVR
{
    /// <summary>
    /// CSharp rebuild of ataulien/Zenlib
    /// </summary>
    public class ZenFile
    {
        public ZenHeader header = new();

        public class ZenHeader
        {
            public int version;
            public string type; // ASCII|BINARY|BIN_SAFE
            public bool isSaveGame;
            public string date;
            public string user;

            public override string ToString()
            {
                var ret = GetType().GetFields()
                    .Select(info => (info.Name, Value: info.GetValue(this)))
                    .Aggregate(
                        new StringBuilder(),
                        (sb, pair) => sb.AppendLine($"  {pair.Name}: {pair.Value}"),
                        sb => sb.ToString());

                return GetType().Name + "\n" + ret;
            }
        }


        public override string ToString()
        {
            var ret = GetType().GetFields()
                .Select(info => (info.Name, Value: info.GetValue(this)))
                .Aggregate(
                    new StringBuilder(),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());

            return GetType().Name + "\n" + ret;
        }
    }
}