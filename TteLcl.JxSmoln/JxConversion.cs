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
    TraceReader(reader);
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
          while(reader.Read())
          {
            reader.MoveToContent();
            TraceReader(reader);
            if(reader.NodeType == XmlNodeType.EndElement)
            {
              reader.Read();
              TraceReader(reader);
              break; // we're done
            }
            if(reader.NodeType == XmlNodeType.Element)
            {
              var item = ReadJsonFromXml(reader);
              yield return item;
            }
            else
            {
              throw new InvalidOperationException(
                $"Unexpected content in <j:multi> element. Expecting elements, but found a '{reader.NodeType}'");
            }
          }
        }
      }
      else
      {
        // Not multi-json, just a plain single node.
        // Do not consider this an error, just return that node
        var node = ReadJsonFromXml(reader);
        yield return node;
      }
    }
  }

  /// <summary>
  /// Read the next JSON token from its XML representation. If necessary
  /// this method first moves the reader to the next content node.
  /// This method does not support multi-json (&lt;j:mjson&gt;).
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static JToken ReadJsonFromXml(XmlReader reader)
  {
    var kind = reader.MoveToContent();
    TraceReader(reader);
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
        TraceReader(reader);
        return new JValue(true);
      case "false":
        reader.Skip();
        TraceReader(reader);
        return new JValue(false);
      case "null":
        reader.Skip();
        TraceReader(reader);
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
  /// Given an XML reader positioned on a {j:str} node, return the string content
  /// and move the reader beyond the end of the node.
  /// </summary>
  /// <param name="reader"></param>
  /// <returns></returns>
  private static JValue ReadStringNode(XmlReader reader)
  {
    var text = reader.ReadElementContentAsString("str", JxSmolnNamespace);
    TraceReader(reader);
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
      reader.Read();
      TraceReader(reader);
      return list;
    }
    while(reader.Read())
    {
      reader.MoveToContent();
      TraceReader(reader);
      if(reader.NodeType == XmlNodeType.EndElement)
      {
        //reader.Read();
        //TraceReader(reader);
        return list;
      }
      if(reader.NodeType == XmlNodeType.Element)
      {
        var item = ReadJsonFromXml(reader);
        list.Add(item);
      }
      else
      {
        throw new InvalidOperationException(
          $"Unexpected content in <j:list> element. Expecting elements, but found a '{reader.NodeType}'");
      }
    }
    throw new InvalidOperationException(
      "Unexpected EOF while reading a <j:list>.");
  }

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
      reader.Read();
      TraceReader(reader);
      return ob;
    }
    while(reader.Read())
    {
      //TraceReader(reader);
      reader.MoveToContent();
      TraceReader(reader);
      if(reader.NodeType == XmlNodeType.EndElement)
      {
        //reader.Read();
        //TraceReader(reader);
        return ob;
      }
      if(!reader.IsStartElement("prop", JxSmolnNamespace))
      {
        throw new InvalidOperationException(
          $"Expecting all child elements of <j:ob> to be <j:prop>, but encountered a '{reader.NodeType}' named '{reader.LocalName}' instead.");
      }
      ReadProperty(reader, ob);
    }
    throw new InvalidOperationException(
      "Unexpected EOF while reading a <j:ob>.");
  }

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
    reader.Read();
    TraceReader(reader);
    var value = ReadJsonFromXml(reader);
    reader.MoveToContent();
    TraceReader(reader);
    if(reader.NodeType != XmlNodeType.EndElement)
    {
      throw new InvalidOperationException(
        $"Expecting </j:prop> after <j:prop>'s content");
    }
    //reader.Read();
    //TraceReader(reader);
    ob.Add(name, value);
  }

  private static void TraceReader(
    XmlReader reader,
    [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string? caller = null)
  {
    if(ReaderTracer!=null)
    {
      ReaderTracer(reader, lineNumber, caller ?? "???");
    }
  }
  
  /// <summary>
  /// A delegate that, if set, is spammed with callbacks whenever the XML source advances
  /// </summary>
  public static Action<XmlReader, int, string>? ReaderTracer { get; set; }
}

