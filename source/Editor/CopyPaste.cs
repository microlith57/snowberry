using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;

namespace Snowberry.Editor;

public static class CopyPaste{

    public static string Clipboard{
        get => TextInput.GetClipboardText();
        set => TextInput.SetClipboardText(value);
    }

    public static string CopyEntities(IEnumerable<Entity> entities){
        StringBuilder clipboard = new StringBuilder("{");
        foreach(Entity entity in entities){
            BinaryPacker.Element elem = new(){
                Attributes = new() {
                    ["_fromLayer"] = entity.IsTrigger ? "triggers" : "entities",
                    ["_id"] = entity.EntityID,
                    ["_name"] = entity.Name,
                    ["_type"] = entity.IsTrigger ? "trigger" : "entity",
                    ["width"] = entity.Width,
                    ["height"] = entity.Height,
                    ["x"] = entity.X,
                    ["y"] = entity.Y
                }
            };

            if(entity.Nodes.Count > 0){
                // here we do a little crime and throw a list in an Element
                // this is ok because MarshallToTable understands it, but you generally shouldn't do this!
                //  - L
                elem.Attributes["nodes"] = entity.Nodes.Select(node =>
                    new BinaryPacker.Element{
                        Attributes = new(){
                            ["x"] = node.X,
                            ["y"] = node.Y
                        }
                    }).ToList();
            }

            entity.SaveAttrs(elem);
            clipboard.Append(MarshallToTable(elem)).Append(",\n");
        }
        clipboard.Append("}");
        return clipboard.ToString();
    }

    // for Copy, straightforwardly printing entity data
    private static string MarshallToTable(BinaryPacker.Element e){
        if(e.Attributes == null)
            return "{}";

        StringBuilder sb = new("{\n");
        foreach(var attr in e.Attributes){
            sb.Append("\t").Append(attr.Key).Append(" = ");
            if(attr.Value is string s)
                sb.Append("\"").Append(s.Escape()).Append("\"");
            else if(attr.Value.GetType().IsEnum)
                sb.Append("\"").Append(attr.Value).Append("\"");
            else if(attr.Value is List<BinaryPacker.Element> es)
                if(es.Count > 0)
                    sb.Append("{").Append(string.Join(", ", es.Select(MarshallToTable))).Append("}");
                else
                    sb.Append("{}");
            else
                sb.Append(attr.Value);
            sb.Append(",\n");
        }

        sb.Append("}");

        return sb.ToString();
    }

    public static List<(EntityData data, bool trigger)> PasteEntities(string table) =>
        new TableParser(table).Parse();

    private class TableParser{
        private string rest;

        public TableParser(string rest) => this.rest = rest;

        // for Paste, parsing from a lua-style table
        public List<(EntityData data, bool trigger)> Parse(){
            rest = rest.Trim();
            if (!(rest.StartsWith("{") && rest.EndsWith("}")))
                return new();
            // remove the starting and ending braces
            rest = rest.Substring(1, rest.Length - 2);

            List<(EntityData, bool)> data = new();
            // comma separated list of entities, possibly ending in another comma
            while(rest.Length > 0){
                if(startsWith(","))
                    rest = rest.Substring(1); // take off the comma
                if(!startsWith("{")){
                    if(rest.Length == 0)
                        break; // took off the trailing comma and now we're done
                    throw new ArgumentException($"Failed to paste, invalid character: {rest.First()}"); // encountered something evil
                }

                data.Add(parseEntity());
            }

            return data;
        }

        private (EntityData data, bool trigger) parseEntity(){
            expect("{");
            EntityData data = new(){
                Values = new(),
                Nodes = new Vector2[0]
            };
            bool isTrigger = false;

            // keep parsing key/values until we reach the end of a table
            while(!startsWith("}")){
                var name = parseId();
                expect("=");
                // parse nodes differently
                if(name == "nodes")
                    data.Nodes = parseNodes();
                else{
                    // check special names
                    var v = parseValue();
                    if(name == "_type")
                        isTrigger = v switch{
                            "trigger" => true,
                            _ => false
                        };
                    else if (name == "_name")
                        data.Name = v;
                    else if (name == "width")
                        data.Width = Convert.ToInt32(v);
                    else if (name == "height")
                        data.Height = Convert.ToInt32(v);
                    else if (name is "_id" or "_fromLayer"){
                        // no-op
                    }else if (name == "x")
                        data.Position.X = Convert.ToSingle(v);
                    else if (name == "y")
                        data.Position.Y = Convert.ToSingle(v);
                    else
                        // and just throw ordinary values in the table
                        data.Values[name] = v;
                }
                removeIfPresent(",");
            }

            // then get rid of the closing brace
            rest = rest.Substring(1);
            return (data, isTrigger);
        }

        private string parseId(){
            rest = rest.TrimStart();
            if(rest.Length == 0)
                throw new ArgumentException("Failed to paste, expected an identifier but got nothing");
            if(!isIdStart(rest.First()))
                throw new ArgumentException($"Failed to paste, expected an identifier but got \"{rest.First()}\"");
            string id = rest.TakeWhile(isIdChar).IntoString();
            rest = rest.Substring(id.Length);
            return id;
        }

        // EntityData is fine with everything being strings, so we'll handle them like that
        private string parseValue(){
            // either true, false, a number, a string, or nil
            if(startsWith("\"")){
                expect("\"");
                StringBuilder v = new();
                bool escaping = false;
                while(escaping || rest.First() != '"'){
                    char next = rest.First();
                    if(escaping){
                        escaping = false;
                        if(next == '\\')
                            v.Append("\\");
                        else if(next == 'n')
                            v.Append("\n");
                        else if(next == 't')
                            v.Append("\t");
                        else if(next == '"')
                            v.Append('"');
                    }else if(next == '\\')
                        escaping = true;
                    else
                        v.Append(next);

                    rest = rest.Substring(1);
                }
                rest = rest.Substring(1);
                return v.ToString();
            }

            if(startsWith("-") || char.IsDigit(rest.First())){
                var v = rest.First() + rest.Skip(1).TakeWhile(c => char.IsDigit(c) || c is '.').IntoString();
                rest = rest.Substring(v.Length);
                if(isIdChar(rest.First()))
                    throw new ArgumentException("Failed to paste, found a letter at the end of a numeric value");
                return v;
            }

            string kw;
            if(startsWithIgnoreCase(kw = "true") || startsWithIgnoreCase(kw = "false") || startsWithIgnoreCase(kw = "nil")){
                rest = rest.Substring(kw.Length);
                if(isIdChar(rest.First()))
                    throw new ArgumentException("Failed to paste, found a letter at the end of a keyword value");
                return kw;
            }

            throw new ArgumentException($"Failed to paste, expected a value but got something beginning with {rest.First()}");
        }

        private Vector2[] parseNodes(){
            // comma separated list of tiny tables
            List<Vector2> values = new();
            expect("{");
            while(!startsWith("}")){
                int x = 0, y = 0;
                expect("{");
                while(!startsWith("}")){
                    var id = parseId();
                    expect("=");
                    var v = parseValue();
                    if(id == "x")
                        x = Convert.ToInt32(v);
                    else if(id == "y")
                        y = Convert.ToInt32(v);
                    removeIfPresent(",");
                }
                expect("}");
                values.Add(new(x, y));
                removeIfPresent(",");
            }
            expect("}");

            return values.ToArray();
        }

        private void expect(string c){
            if(!startsWith(c))
                throw new ArgumentException($"Failed to paste, expected \"{c}\" but got \"{rest.Take(c.Length).IntoString()}\"");
            rest = rest.Substring(c.Length);
        }

        private void removeIfPresent(string c){
            if(startsWith(c))
                rest = rest.Substring(c.Length);
        }

        private bool startsWith(string c){
            rest = rest.TrimStart();
            return rest.StartsWith(c);
        }

        private bool startsWithIgnoreCase(string c){
            rest = rest.TrimStart();
            return rest.StartsWith(c, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool isIdStart(char c) => char.IsLetter(c) || c is '_';
        private static bool isIdChar(char c) => isIdStart(c) || char.IsDigit(c);
    }
}

internal static class ParseExt{
    public static string IntoString(this IEnumerable<char> chars) => new(chars.ToArray());

    public static string Escape(this string s){
        StringBuilder escaped = new(s.Length);
        foreach(char c in s)
            escaped.Append(c switch{
                '"' => "\\\"",
                '\n' => "\\n",
                '\t' => "\\t",
                '\\' => "\\\\",
                _ => c
            });

        return escaped.ToString();
    }
}