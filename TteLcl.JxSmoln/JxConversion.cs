/*
 * (c) 2025  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TteLcl.JxSmoln;

/// <summary>
/// Implements conversions between JSON and jxsmoln XML.
/// Use <see cref="ReadJsonFromXml(XmlReader)"/> to read single-node JSON
/// from XML, or <see cref="ReadMultiJsonFromXml(XmlReader)"/> to read
/// multi-node JSON.
/// </summary>
public static class JxConversion
{

  /// <summary>
  /// The jxsmoln XML namespace
  /// </summary>
  public const string JxSmolnNamespace =
    "https://github.com/ttelcl/TteLcl.XmlTools/blob/main/jxsmoln/README.md";

  /// <summary>
  /// Read multi-json content from a jxsmoln multi-json model. If the content
  /// is just a single node, return it without flagging an error.
  /// </summary>
  /// <param name="reader">
  /// The <see cref="XmlReader"/> to read from, pointing to jxsmoln content
  /// </param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static IEnumerable<JToken> ReadMultiJsonFromXml(XmlReader reader)
  {
    var kind = reader.MoveToContent();
    TraceReader(reader, "looking for multi-json node");
    if(kind != XmlNodeType.None) // else return nothing
    {
      if(kind != XmlNodeType.Element)
      {
        throw new InvalidOperationException(
          $"Expecting an XML element but found '{kind}'");
      }
      var ns = reader.NamespaceURI;
      var ln = reader.LocalName;
      if(ns != JxSmolnNamespace)
      {
        throw new InvalidOperationException(
          $"Incorrect namespace in node. Expecting the jxsmoln namespace, but got '{reader.NamespaceURI}'");
      }
      if(ln == "multi")
      {
        if(!reader.IsEmptyElement) // else return nothing
        {
          reader.Read();
          reader.MoveToContent();
          while(reader.IsStartElement())
          {
            TraceReader(reader, "start of multi-json item");
            var item = ReadJsonFromXml(reader);
            reader.MoveToContent();
            yield return item;
          }
          TraceReader(reader, "leaving multi-json content");
          if(reader.NodeType != XmlNodeType.EndElement)
          {
            throw new InvalidOperationException(
              $"Unexpected node type inside <j:multi>: '{reader.NodeType}'");
          }
          if(reader.LocalName != "multi")
          {
            throw new InvalidOperationException(
              $"Unexpected end-of-element node inside <j:multi>: </{reader.Name}>");
          }
          reader.Skip();
          TraceReader(reader, $"completed <j:multi>, and moved beyond");
        }
      }
      else
      {
        // Not multi-json, just a plain single node.
        // Do not consider this an error, just return that node
        var node = ReadJsonFromXml(reader);
        reader.MoveToContent();
        TraceReader(reader, "completed the single item as if in a multi-json list");
        yield return node;
      }
    }
  }

  /// <summary>
  /// Read the next JSON token from its XML representation. If necessary
  /// this method first moves the reader to the next content node.
  /// Upon return the reader points beyond the end of the element it initially
  /// pointed at.
  /// This method does not support multi-json (&lt;j:mjson&gt;).
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static JToken ReadJsonFromXml(XmlReader reader)
  {
    var kind = reader.MoveToContent();
    TraceReader(reader, "dispatcher");
    if(kind == XmlNodeType.None)
    {
      throw new InvalidOperationException(
        "Unexpected end of XML content");
    }
    if(kind != XmlNodeType.Element)
    {
      throw new InvalidOperationException(
        $"Expecting an XML element but found '{kind}'");
    }
    var ns = reader.NamespaceURI;
    var ln = reader.LocalName;
    if(ns != JxSmolnNamespace)
    {
      throw new InvalidOperationException(
        $"Incorrect namespace in node. Expecting the jxsmoln namespace, but got '{reader.NamespaceURI}'");
    }
    if(ln == "multi")
    {
      throw new InvalidOperationException(
        $"Encountered a 'multi' element at an invalid position. Multi-json is not supported here.");
    }
    if(ln == "prop")
    {
      throw new InvalidOperationException(
        "Encountered a 'prop' element outside an object");
    }
    switch(ln)
    {
      case "str":
        return ReadStringNode(reader);
      case "num":
        return ReadNumberNode(reader);
      case "true":
        reader.Skip();
        TraceReader(reader, "completed <j:true>");
        return new JValue(true);
      case "false":
        reader.Skip();
        TraceReader(reader, "completed <j:false>");
        return new JValue(false);
      case "null":
        reader.Skip();
        TraceReader(reader, "completed <j:null>");
        return JValue.CreateNull();
      case "list":
        return ReadListNode(reader);
      case "ob":
        return ReadObjectNode(reader);
      default:
        throw new InvalidOperationException(
          $"Unrecognized jxsmoln element '{ln}'");
    }
  }

  /// <summary>
  /// Convert JSON content to XML
  /// </summary>
  /// <param name="reader">
  /// The <see cref="JsonReader"/> pointing to the JSON to write, still in its
  /// '<see cref="JsonToken.None"/>' state
  /// </param>
  /// <param name="writer"></param>
  /// <param name="multiJson"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public static void WriteJsonToXmlDocument(JsonReader reader, XmlWriter writer, bool multiJson)
  {
    if(multiJson)
    {
      throw new NotImplementedException(
        "multi-json support is not yet implemented");
    }
    writer.WriteStartDocument();
    if(reader.TokenType != JsonToken.None)
    {
      throw new InvalidOperationException(
        "Expecting a new JsonReader (in state 'None')");
    }
    if(!reader.Read())
    {
      throw new InvalidOperationException(
        "Not expecting an empty JsonReader");
    }
    var result = WriteJsonItem(reader, writer);
    if(result)
    {
      throw new InvalidOperationException(
        $"Expecting EOF but found {reader.TokenType}");
    }
    writer.WriteEndDocument();
  }

  private static void ReadNotEnd(JsonReader reader)
  {
    if(!reader.Read())
    {
      throw new InvalidOperationException(
        "Unexpected EOF while reading JSON");
    }
  }

  /// <summary>
  /// Write the JSON item that <paramref name="reader"/> currently points at
  /// and advance the reader to the next item
  /// to <paramref name="writer"/>
  /// </summary>
  /// <param name="reader"></param>
  /// <param name="writer"></param>
  private static bool WriteJsonItem(JsonReader reader, XmlWriter writer)
  {
    switch(reader.TokenType)
    {
      case JsonToken.StartObject:
        return WriteJsonObject(reader, writer);
      case JsonToken.StartArray:
        return WriteJsonArray(reader, writer);
      case JsonToken.Integer:
        var i = (long)reader.Value!;
        writer.WriteStartElement("j", "num", JxSmolnNamespace);
        writer.WriteValue(i);
        writer.WriteEndElement();
        return reader.Read();
      case JsonToken.Float:
        var f = (double)reader.Value!;
        writer.WriteStartElement("j", "num", JxSmolnNamespace);
        writer.WriteValue(f);
        writer.WriteEndElement();
        return reader.Read();
      case JsonToken.String:
        var s = (string)reader.Value!;
        writer.WriteStartElement("j", "str", JxSmolnNamespace);
        writer.WriteValue(s);
        writer.WriteEndElement();
        return reader.Read();
      case JsonToken.Boolean:
        var b = (bool)reader.Value!;
        if(b)
        {
          writer.WriteStartElement("j", "true", JxSmolnNamespace);
        }
        else
        {
          writer.WriteStartElement("j", "false", JxSmolnNamespace);
        }
        writer.WriteEndElement();
        return reader.Read();
      case JsonToken.Null:
        writer.WriteStartElement("j", "null", JxSmolnNamespace);
        writer.WriteEndElement();
        return reader.Read();
      default:
        throw new NotSupportedException(
          $"Unexpected JSON token type : {reader.TokenType}");
    }
  }

  private static bool WriteJsonObject(JsonReader reader, XmlWriter writer)
  {
    writer.WriteStartElement("j", "ob", JxSmolnNamespace);
    ReadNotEnd(reader);
    while(reader.TokenType != JsonToken.EndObject)
    {
      if(reader.TokenType != JsonToken.PropertyName)
      {
        throw new InvalidOperationException(
          "Expecting a property name");
      }
      var propertyName = (string)reader.Value! 
        ?? throw new InvalidOperationException("Expecting property names to be strings");
      writer.WriteStartElement("j", "prop", JxSmolnNamespace);
      writer.WriteAttributeString("key", propertyName);
      ReadNotEnd(reader);
      WriteJsonItem(reader, writer);
      writer.WriteEndElement();
    }
    writer.WriteEndElement();
    return reader.Read();
  }

  private static bool WriteJsonArray(JsonReader reader, XmlWriter writer)
  {
    writer.WriteStartElement("j", "list", JxSmolnNamespace);
    ReadNotEnd(reader);
    while(reader.TokenType != JsonToken.EndArray)
    {
      WriteJsonItem(reader, writer);
    }
    writer.WriteEndElement();
    return reader.Read();
  }

  /// <summary>
  /// Given an XML reader positioned on a {j:str} node, return the string content
  /// and move the reader beyond the end of the node.
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  private static JValue ReadStringNode(XmlReader reader)
  {
    var text = reader.ReadElementContentAsString("str", JxSmolnNamespace);
    TraceReader(reader, $"string value == '{text}'");
    return JValue.CreateString(text);
  }

  /// <summary>
  /// Given an XML reader positioned on a {j:str} node, return the number content
  /// and move the reader beyond the end of the node.
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  private static JValue ReadNumberNode(XmlReader reader)
  {
    var text = reader.ReadElementContentAsString("num", JxSmolnNamespace);
    TraceReader(reader);
    if(Int64.TryParse(text, CultureInfo.InvariantCulture, out var result))
    {
      return new JValue(result);
    }
    else if(Double.TryParse(text, CultureInfo.InvariantCulture, out var number))
    {
      return new JValue(number);
    }
    else
    {
      throw new InvalidOperationException(
        $"Failed to parse '{text}' as an integer or a real.");
    }
  }

  /// <summary>
  /// Given an XML reader positioned on a {j:list} node, read the entire list
  /// and move the reader beyond the end of the list
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private static JArray ReadListNode(XmlReader reader)
  {
    if(!reader.IsStartElement("list", JxSmolnNamespace))
    {
      throw new InvalidOperationException(
        "Internal error, expecting to be on a <j:list> node");
    }
    var list = new JArray();
    if(reader.IsEmptyElement)
    {
      reader.Skip();
      TraceReader(reader, "completed empty list and moved beyond");
      return list;
    }
    reader.Read(); // jump into the list
    reader.MoveToContent();
    while(reader.IsStartElement())
    {
      TraceReader(reader, "start of list item");
      var item = ReadJsonFromXml(reader);
      reader.MoveToContent();
      list.Add(item);
    }
    TraceReader(reader, "leaving list content");
    if(reader.NodeType != XmlNodeType.EndElement)
    {
      throw new InvalidOperationException(
        $"Unexpected node type inside <j:list>: '{reader.NodeType}'");
    }
    if(reader.LocalName != "list")
    {
      throw new InvalidOperationException(
        $"Unexpected end-of-element node inside <j:list>: </{reader.Name}>");
    }
    reader.Skip();
    TraceReader(reader, $"completed <j:list> with {list.Count} items, and moved beyond");
    return list;
  }

  /// <summary>
  /// Given an XML reader positioned on a {j:ob} node, read the entire object
  /// and move the reader beyond the end of the object
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private static JObject ReadObjectNode(XmlReader reader)
  {
    if(!reader.IsStartElement("ob", JxSmolnNamespace))
    {
      throw new InvalidOperationException(
        "Internal error, expecting to be on a '<j:ob>' node");
    }
    var ob = new JObject();
    if(reader.IsEmptyElement)
    {
      reader.Skip();
      TraceReader(reader, "completed empty object and moved beyond");
      return ob;
    }
    reader.Read();
    while(reader.IsStartElement("prop", JxSmolnNamespace))
    {
      TraceReader(reader, "start of property");
      ReadProperty(reader, ob);
    }
    TraceReader(reader, "leaving object content");
    if(reader.NodeType != XmlNodeType.EndElement)
    {
      throw new InvalidOperationException(
        $"Unexpected node inside <j:ob>: type '{reader.NodeType}' ({reader.Name})");
    }
    if(reader.LocalName != "ob")
    {
      throw new InvalidOperationException(
        $"Unexpected end-of-element node inside <j:ob>: </{reader.Name}>");
    }
    reader.Skip();
    TraceReader(reader, $"completed <j:ob> with {ob.Count} properties, and moved beyond");
    return ob;
  }

  /// <summary>
  /// Given an XML reader positioned on a {j:prop} node, read the entire property
  /// and move the reader beyond the end of the property
  /// </summary>
  /// <param name="reader"></param>
  /// <param name="ob"></param>
  /// <exception cref="InvalidOperationException"></exception>
  private static void ReadProperty(XmlReader reader, JObject ob)
  {
    if(!reader.IsStartElement("prop", JxSmolnNamespace))
    {
      throw new InvalidOperationException(
        $"Expecting a <j:prop>");
    }
    var name = reader.GetAttribute("key");
    if(String.IsNullOrEmpty(name))
    {
      throw new InvalidOperationException(
        $"Missing or empty 'key' attribute on <j:prop> element");
    }
    if(reader.IsEmptyElement)
    {
      throw new InvalidOperationException(
        $"<j:prop> elements must not be empty");
    }
    if(ob.ContainsKey(name))
    {
      throw new InvalidOperationException(
        $"Duplicate property name '{name}'");
    }
    TraceReader(reader, $"key = '{name}'");
    reader.Read();
    reader.MoveToContent();
    TraceReader(reader, $"moved to start of content for '{name}'");
    var value = ReadJsonFromXml(reader);
    reader.MoveToContent();
    TraceReader(reader, $"finished reading property '{name}'");
    if(reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "prop")
    {
      throw new InvalidOperationException(
        $"Expecting </j:prop> after <j:prop key='{name}'>'s content");
    }
    reader.ReadEndElement();
    reader.MoveToContent();
    TraceReader(reader, $"Completed <j:prop> '{name}', and moved beyond");
    ob.Add(name, value);
  }

  private static void TraceReader(
    XmlReader reader,
    string? message = null,
    [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string? caller = null)
  {
    if(ReaderTracer!=null)
    {
      ReaderTracer(reader, message, lineNumber, caller ?? "???");
    }
  }
  
  /// <summary>
  /// A delegate that, if set, is spammed with callbacks whenever the XML source advances
  /// </summary>
  public static Action<XmlReader, string?, int, string>? ReaderTracer { get; set; }
}

