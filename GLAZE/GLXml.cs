using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GLGen;

public class GLXml
{
    private const string UnknownValue = "???";
    private XmlElement? RootElement { get; set; }
    private XmlDocument Document { get; } = new();
    
    internal static GLXml From(string filename)
    {
        var that = new GLXml();
        that.Document.Load(filename);
        that.RootElement = that.Document.DocumentElement;
        return that;
    }

    private string SafeGetAttribute(XmlElement? element, string attribute)
    {
        if (element == null || !element.HasAttribute(attribute)) return UnknownValue;
        var value = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(value) ? UnknownValue : value;
    }
    
    private string SafeGetInnerText(XmlElement? element)
    {
        if (element == null) return UnknownValue;
        var value = element.InnerText;
        return string.IsNullOrWhiteSpace(value) ? UnknownValue : value;
    }

    private IEnumerable<XmlElement> SafeGetElements(XmlElement? element, string xpath)
    {   
        if (element == null) return [];
        var nodes = element.SelectNodes(xpath);
        return nodes == null ? [] : nodes.OfType<XmlElement>();
    }
    
    private IEnumerable<XmlElement> SafeGetElements(string xpath)
    {   
       return SafeGetElements(RootElement, xpath);
    }
    
    private XmlElement? SafeGetElement(XmlElement? element, string xpath)
    {
        var node = element?.SelectSingleNode(xpath);
        return node as XmlElement;
    }

    private string SafeGetElementAttribute(XmlElement? element, string name, string attribute)
    {
        return SafeGetAttribute(SafeGetElement(element, name), attribute);
    }
    
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
                if (string.IsNullOrWhiteSpace(Value)) return 0;
                
                var result = 0;
                if (Value.StartsWith("0x"))
                {
                    int.TryParse(Value[2..], NumberStyles.AllowHexSpecifier, null, out result);
                }
                else if (Value.StartsWith('-'))
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
    
    private Dictionary<string, GLEnum>? _glenums { get; set; } = null;

    public Dictionary<string, GLEnum> GLEnums
    {
        get
        {
            if (_glenums is not null) return _glenums;
            _glenums = new Dictionary<string, GLEnum>();
            
            foreach (var enumerations in SafeGetElements("enums"))
            {
                var group = SafeGetAttribute(enumerations, "group");

                foreach (var element in SafeGetElements(enumerations, "enum"))
                {
                    var name = SafeGetAttribute(element, "name");
                        
                    _glenums[name] = new GLEnum()
                    {
                        Name = name,
                        Value = SafeGetAttribute(element, "value"),
                        Groups = SafeGetAttribute(element, "group"),
                        MainGroup = group,
                        Comment = SafeGetAttribute(element, "comment")
                    };
                }

            }
            return _glenums;
        }
    }

    public record struct GLFunction
    {
        public record struct GLParameter
        {
            public string Kind { get; init; }
            public string Name { get; init; }
            public string Comment { get; init; }
            public string Param { get; init; }
            public string Type { get; init; }
            public string Len { get; init; }
            public string Group { get; init; }
        }
        
        public string Name { get; init; }
        public string Alias { get; init; }
        public string ReturnType { get; init; }
        public GLParameter[] Parameters { get; init; }
        public string Kind { get; init; }
        public string Comment { get; init; }
    }
    
    private Dictionary<string, GLFunction>? _glfunctions { get; set; } = null;

    public Dictionary<string, GLFunction> GLFunctions
    {
        get
        {
            if (_glfunctions != null) return _glfunctions;
            _glfunctions = new Dictionary<string, GLFunction>();
            foreach (var command in SafeGetElements("commands/command"))
            {
                var functionName = SafeGetInnerText(SafeGetElement(command, "proto/name"));
                Console.WriteLine(functionName);
                var returnTypeOne = SafeGetInnerText(SafeGetElement(command, "proto/ptype"));
                var returnTypeTwo = SafeGetInnerText(SafeGetElement(command, "proto")).Split(" ")[0];
                _glfunctions[functionName] = new GLFunction()
                {
                    Name = functionName,
                    Parameters = SafeGetElements(command, "param")
                        .Select(parameter => new GLFunction.GLParameter()
                        {
                            Name = SafeGetInnerText(SafeGetElement(parameter, "name")),
                            Type = SafeGetInnerText(SafeGetElement(parameter, "ptype")),
                            Comment = SafeGetInnerText(SafeGetElement(parameter, "comment")),
                            Len = SafeGetAttribute(parameter, "len"),
                            Param = ExtractParameters(parameter.InnerXml),
                            Kind = SafeGetAttribute(parameter, "kind"),
                            Group = SafeGetAttribute(parameter, "group"),
                        }).ToArray(),
                    Alias = SafeGetAttribute(SafeGetElement(command, "alias"), "name"),
                    ReturnType = returnTypeOne == UnknownValue ? returnTypeTwo : returnTypeOne,
                    Kind = SafeGetAttribute(SafeGetElement(command, "proto"), "kind"),
                    Comment = SafeGetAttribute(command, "comment")
                };

            }
            return _glfunctions;

            string ExtractParameters(string innerXml)
            {
                var p = Regex.Replace(innerXml, @"(.*?)<ptype>(.*?)</ptype>(.*?)<name>(.*?)</name>", "$1 $2 $3$4");
                return Regex.Replace(p, @"\s{2,}", " ").Trim();
            }
        }
    }

    public record struct GLVersion
    {
        public string Major { get; init; }
        public string Minor { get; init; }
        public string Name { get; init; }
    }

    public record struct GLFeature()
    {
        public HashSet<string> Enums { get; set; } = [];
        public HashSet<string> Commands { get; set; } = [];
    }
    
    private Dictionary<GLVersion, HashSet<GLFeature>>? _glfeatures { get; set; } = null;

    public Dictionary<GLVersion, HashSet<GLFeature>> GLFeatures
    {
        get
        {   
            if (_glfeatures is not null) return _glfeatures;
            _glfeatures = new Dictionary<GLVersion, HashSet<GLFeature>>();
            
            // We keep track of every removed feature from the core profile
            // And we remove those from the version sets going from the back to first
            // OpenGl version if they exist
            var removedEnums = new HashSet<string>();
            var removedCommands = new HashSet<string>();
            
            foreach (var feature in SafeGetElements("feature[@api='gl']").Reverse())
            {
                var glVersion = new GLVersion()
                {
                    Major = feature.GetAttribute("number").Split('.')[0],
                    Minor = feature.GetAttribute("number").Split('.')[1],
                    Name = feature.GetAttribute("name")
                };
                
                var versionFeatures = new HashSet<GLFeature>();

                foreach (var required in SafeGetElements(feature, "require"))
                {
                    var glFeature = new GLFeature();
                    glFeature.Enums.UnionWith(SafeGetElements(required, "enum").Select(element => SafeGetAttribute(element, "name")));
                    glFeature.Commands.UnionWith(SafeGetElements(required, "command").Select(element => SafeGetAttribute(element, "name")));
                    versionFeatures.Add(glFeature);
                }
                
                foreach (var removed in SafeGetElements(feature, "remove[@profile='core']"))
                {
                    removedEnums.UnionWith(SafeGetElements(removed, "enum").Select(element => SafeGetAttribute(element, "name")));
                    removedCommands.UnionWith(SafeGetElements(removed, "command").Select(element => SafeGetAttribute(element, "name")));
                }
                
                foreach (var vFeature in versionFeatures)
                {
                    vFeature.Commands.ExceptWith(removedCommands);
                    vFeature.Enums.ExceptWith(removedEnums);
                }
                _glfeatures.Add(glVersion, versionFeatures);
               
            }
            return _glfeatures;
        }
    }
    
    public record struct GLExtension()
    {
        public HashSet<string> Enums { get; set; } = [];
        public HashSet<string> Commands { get; set; } = [];
    }
    
    private Dictionary<string, HashSet<GLExtension>>? _glextensions { get; set; } = null;

    public Dictionary<string, HashSet<GLExtension>> GLExtensions
    {
        get
        {   
            if (_glextensions is not null) return _glextensions;
            _glextensions = new Dictionary<string, HashSet<GLExtension>>();

            foreach (var extension in SafeGetElements("extensions/extension[@supported='gl']"))
            {
                var name = SafeGetAttribute(extension, "name");
                var versionFeatures = new HashSet<GLExtension>();

                foreach (var required in SafeGetElements(extension, "require"))
                {
                    var glExtension = new GLExtension();
                    glExtension.Enums.UnionWith(SafeGetElements(required, "enum").Select(element => SafeGetAttribute(element, "name")));
                    glExtension.Commands.UnionWith(SafeGetElements(required, "command").Select(element => SafeGetAttribute(element, "name")));
                    versionFeatures.Add(glExtension);
                }
                
                _glextensions.Add(name, versionFeatures);
               
            }
            return _glextensions;
        }
    }
}