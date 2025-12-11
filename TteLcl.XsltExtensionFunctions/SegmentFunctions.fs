namespace TteLcl.XsltExtensionFunctions

open System
open System.Xml
open System.Xml.Xsl

/// Container class for XSLT extension functions, suitable for use as extension object.
type public SegmentFunctions () =
  
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
  
  static member public NamespaceUri
    with get() =
      "urn:ttelcl:xsltextensionfunctions:segment"

  static member public AddExtensionObject(arglist: XsltArgumentList): unit =
    // Remove existing mapping if it exists
    arglist.RemoveExtensionObject(SegmentFunctions.NamespaceUri) |> ignore
    arglist.AddExtensionObject(SegmentFunctions.NamespaceUri, new SegmentFunctions())

//
