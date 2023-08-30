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

    // for Copy, straightforwardly printing entity data
    public static string MarshallToTable(BinaryPacker.Element e){
        if(e.Attributes == null)
            return "{}";

        StringBuilder sb = new("{\n");
        foreach(var attr in e.Attributes){
            sb.Append("\t").Append(attr.Key).Append(" = ");
            if(attr.Value is string s)
                sb.Append("\"").Append(s).Append("\"");
            else if(attr.Value.GetType().IsEnum)
                sb.Append("\"").Append(attr.Value).Append("\"");
            else if(attr.Value is List<BinaryPacker.Element> es)
                if(es.Count > 0)
                    sb.Append("{").Append(es.Select(MarshallToTable).Aggregate((l, r) => l + ", " + r)).Append("}");
                else
                    sb.Append("{}");
            else
                sb.Append(attr.Value);
            sb.Append(",\n");
        }

        sb.Append("}");

        return sb.ToString();
    }

    public static List<(EntityData data, bool trigger)> MarshallFromTable(string table) =>
        new TableParser(table).Parse();

    private class TableParser{
        private string rest;

        public TableParser(string rest) => this.rest = rest;

        // for Paste, parsing from a lua-style table
        public List<(EntityData data, bool trigger)> Parse(){
            if (!(rest.StartsWith("{") && rest.EndsWith("}")))
                return new();
            // remove all forms of whitespace
            // TODO: this is wrong,
            rest = rest.Where(c => !char.IsWhiteSpace(c)).IntoString();
            // remove the starting and ending braces, as well as possible trailing comma
            rest = rest.Substring(1, rest.Length - 2);

            List<(EntityData, bool)> data = new();
            // comma separated list of entities, possibly ending in another comma
            while(rest.Length > 0){
                if(rest.StartsWith(","))
                    rest = rest.Substring(1); // take off the comma
                if(!rest.StartsWith("{")){
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
                Values = new()
            };
            bool isTrigger = false;

            // keep parsing key/values until we reach the end of a table
            while(!rest.StartsWith("}")){
                var name = parseId();
                expect("=");
                // parse nodes differently
                if(name == "nodes")
                    data.Nodes = parseNodes();
                else {
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
                    else if (name is "_id" or "_fromLayer" or "x" or "y"){
                        // no-op
                    }else
                        // and just throw ordinary values in the table
                        data.Values[name] = v;

                }
                if(rest.StartsWith(","))
                    expect(",");
            }

            // then get rid of the closing brace
            rest = rest.Substring(1);
            return (data, isTrigger);
        }

        private string parseId(){
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
            if(rest.StartsWith("\"")){
                expect("\"");
                var v = rest.TakeWhile(c => c != '"').IntoString();
                rest = rest.Substring(v.Length + 1);
                return v;
            }

            if(rest.StartsWith("-") || char.IsDigit(rest.First())){
                var v = rest.First() + rest.Skip(1).TakeWhile(char.IsDigit).IntoString();
                rest = rest.Substring(v.Length);
                if(char.IsLetter(rest.First()))
                    throw new ArgumentException("Failed to paste, found a letter at the end of a numeric value");
                return v;
            }

            string kw;
            if(rest.StartsWith(kw = "true") || rest.StartsWith(kw = "false") || rest.StartsWith(kw = "nil")){
                rest = rest.Substring(kw.Length);
                if(char.IsLetter(rest.First()))
                    throw new ArgumentException("Failed to paste, found a letter at the end of a keyword value");
                return kw;
            }

            throw new ArgumentException($"Failed to paste, expected a value but got something beginning with {rest.First()}");
        }

        private Vector2[] parseNodes(){
            // comma separated list of tiny tables
            List<Vector2> values = new();
            expect("{");
            while(!rest.StartsWith("}")) {
                int x = 0, y = 0;
                expect("{");
                while(!rest.StartsWith("}")){
                    var id = parseId();
                    expect("=");
                    var v = parseValue();
                    if(id == "x")
                        x = Convert.ToInt32(v);
                    else if(id == "y")
                        y = Convert.ToInt32(v);
                }
                expect("}");
                values.Add(new(x, y));
                if(rest.StartsWith(","))
                    expect(",");
            }

            return values.ToArray();
        }

        private void expect(string c){
            if(!rest.StartsWith(c))
                throw new ArgumentException($"Failed to paste, expected \"{c}\" but got \"{rest.Take(c.Length).IntoString()}\"");
            rest = rest.Substring(c.Length);
        }

        private static bool isIdStart(char c) => char.IsLetter(c) || c is '_';
        private static bool isIdChar(char c) => isIdStart(c) || char.IsDigit(c);
    }
}

internal static class ParseExt {
    public static string IntoString(this IEnumerable<char> chars) => new(chars.ToArray());
}