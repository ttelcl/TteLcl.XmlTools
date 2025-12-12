namespace TteLcl.XsltExtensionFunctions

open System
open System.Xml
open System.Xml.Xsl

/// Container class for XSLT extension functions, suitable for use as extension object.
type public SegmentFunctions () =
  
  /// The XML namespace URI used to register XSL extension functions implemented in this class
  static member public NamespaceUri
    with get() =
      "urn:ttelcl:xslt-extension-functions:segment"

  /// <summary>Create an instance of this class and register it as an extension object
  /// in <paramref name="arglist"/>, using the namespace <see cref="NamespaceUri"/>
  /// </summary>
  static member public AddExtensionObject(arglist: XsltArgumentList): unit =
    // Remove existing mapping if it exists
    arglist.RemoveExtensionObject(SegmentFunctions.NamespaceUri) |> ignore
    arglist.AddExtensionObject(SegmentFunctions.NamespaceUri, new SegmentFunctions())
  
  /// <summary>
  /// Split <paramref name="text"/> using the given <paramref name="separator"/> into one or
  /// more segments and return the segment index by <paramref name="index"/>. The index is
  /// 1-based (as per XPath convention) and can be negative to count from the end.
  /// Returns an empty string for out of range indexes
  /// </summary>  
  member public this.``get-segment``( text:string, separator:string, index:int ) : string =
    if index = 0 then
      new ArgumentException("Expecting an index other than 0") |> raise
    let parts = text.Split(separator)
    if index > parts.Length || index < -parts.Length then
      ""
    elif index > 0 then
      parts[index - 1]
    else
      parts[parts.Length + index]
  
  member public this.``segment-get``( text:string, separator:string, index:int ) : string =
    this.``get-segment``(text, separator, index)

  /// <summary>
  /// Split <paramref name="text"/> using the given <paramref name="separator"/> into one or
  /// more segments. Recombine the first <paramref name="count"/> of those segments and return it.
  /// If <paramref name="count"/> is negative then recombine all except the final 
  /// <paramref name="count"/> segments.
  /// </summary>  
  member public this.``segment-head``( text:string, separator:string, count:int ) : string =
    if count = 0 then
      new ArgumentException("Expecting a count other than 0") |> raise
    let parts = text.Split(separator)
    if count > 0 then
      if count >= parts.Length then
        text
      else
        String.Join(separator, parts |> Array.take count)
    else
      let count = count + parts.Length
      if count <= 0 then
        ""
      else
        String.Join(separator, parts |> Array.take count)

//
