using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace GLGen;

public class GLXML
{
    private XmlDocument Document { get; init; }
    
    public static GLXML from(String filename)
    {
        var document = new XmlDocument();
        document.Load(new FileStream(filename, FileMode.Open));
        return new GLXML { Document = document };
    }
    
    // TODO Implement Parsing Commands by Version
    
    /* The OpenGL XML Schema is like so
     * <registry> - the root block
     * <types> - typedef mappings, useful for mapping <command> blocks types to good documentation or your defined types.
     *           They map to the <ptype> block on each command definition.
     * <kinds> - Fake data types identified by name in the OpenGL functions themselves, added as a "kind" attribute to the
     *           <param> tag themselves
     * <enums> - many top level blocks representing function integer types (as values mapped to names)
     * <commands> - single top level block nests <command> blocks representing functions with params/types
     * <feature> - many top level blocks that contain <require>/<remove> blocks to add <command> and <enum>
     *             to the api and version. Use the "name" attribute to get the OpenGL version or the "number"
     *             to get the version number. Use the "api" attribute to determine what gl* api the feature is for e.g gl, wgl...
     * <extensions> - single top level blocks that nests <extension> blocks similar to <feature>
     * <extension> - many nested blocks under <extension> that are identical to <feature> blocks. The attribute "supported"
     *               seems to let you know if its supported for the chosen `api`
     * <remove> - blocks in <feature> or <extension> with a "profile" attribute usually to identify
     *           `compatibility` profile <command>/<enum> to remove. The profile you are using is the ones
     *            whose remove tags you want to run on the previous versions feature set
     * <require> - blocks in <feature> or <extension> that identify <command> and <enum> to add in the <feature>/<extension>
     *             which can include a profile as well
     *
     * NOTE: Pointer types have a `*` in front of the <name> block which is funny
     * 
     * I have no idea what the other tags really do, but I do not need them either 
     */

    public record struct GLEnum
    {
        public string Name { get; init; }
        public string Groups { get; init; }
        public string MainGroup { get; init; }
        public string Value { get; init; }
        
        public string Comment { get; init; }

        public int NumericValue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Value)) return 0;
                var result = 0;
                if (Value.StartsWith("0x"))
                {
                    int.TryParse(Value[2..], NumberStyles.AllowHexSpecifier, null, out result);
                }
                else if (Value.StartsWith("-"))
                {
                    int.TryParse(Value, NumberStyles.AllowHexSpecifier, null, out result);
                }
                else
                {
                    result = Convert.ToInt32(Value, 16);
                }
                return result;
            } 
            
        }
    }

    private Dictionary<string, HashSet<GLEnum>>? _Enums { get; set; } = null;

    public Dictionary<string, HashSet<GLEnum>> Enums
    {
        get
        {
            var root = Document.DocumentElement;
            if (root is null) return _Enums ??= new Dictionary<string, HashSet<GLEnum>>();
            if (_Enums != null) return _Enums;
            _Enums = new Dictionary<string, HashSet<GLEnum>>();
            foreach (var element in root.GetElementsByTagName("enums").OfType<XmlElement>())
            {
                var group = element.GetAttribute("group");
                if (!element.GetElementsByTagName("enum").OfType<XmlElement>().Any()) continue;
                if (!_Enums.ContainsKey(group)) _Enums.Add(group, []);
                foreach (var enumElement in element.GetElementsByTagName("enum").OfType<XmlElement>())
                {
                    _Enums[group].Add(new GLEnum()
                    {
                        Name = enumElement.GetAttribute("name"),
                        Value = enumElement.GetAttribute("value"),
                        Groups = enumElement.GetAttribute("group"),
                        MainGroup = group,
                        Comment = enumElement.GetAttribute("comment")
                    });
                }

            }
            return _Enums;
        }
    }
}