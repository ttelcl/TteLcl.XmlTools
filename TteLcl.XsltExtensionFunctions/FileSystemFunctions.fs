namespace TteLcl.XsltExtensionFunctions

open System
open System.IO
open System.Xml
open System.Xml.Xsl

type public FileSystemFunctions () =
  
  /// The XML namespace URI used to register XSL extension functions implemented in this class
  static member public NamespaceUri
    with get() =
      "urn:ttelcl:xslt-extension-functions:filesystem"

  /// <summary>Create an instance of this class and register it as an extension object
  /// in <paramref name="arglist"/>, using the namespace <see cref="NamespaceUri"/>
  /// </summary>
  static member public AddExtensionObject(arglist: XsltArgumentList): unit =
    // Remove existing mapping if it exists
    arglist.RemoveExtensionObject(FileSystemFunctions.NamespaceUri) |> ignore
    arglist.AddExtensionObject(FileSystemFunctions.NamespaceUri, new FileSystemFunctions())

  member public this.``file-name``( fullname:string ) : string =
    Path.GetFileName(fullname)

  member public this.``full-name``( fileOrFolder:string ) : string =
    Path.GetFullPath(fileOrFolder)

  member public this.``file-extension``( fileName:string ) : string =
    Path.GetExtension(fileName)

  member public this.``parent-folder``( fileOrFolder:string ) : string =
    let dir = Path.GetDirectoryName(fileOrFolder)
    if dir = null then "" else dir

  member public this.``file-stem``( fileName:string ) : string =
    Path.GetFileNameWithoutExtension(fileName)

  member public this.``file-stem``( fileName:string, suffixToRemove:string ) : string =
    let shortname = Path.GetFileName(fileName)
    if shortname.EndsWith(suffixToRemove, StringComparison.OrdinalIgnoreCase) then
      shortname.Substring(0, shortname.Length - suffixToRemove.Length)
    else
      new ArgumentException($"Expecting file name '{shortname}' to end with '{suffixToRemove}'") |> raise

