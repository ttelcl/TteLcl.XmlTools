namespace TteLcl.XsltExtensionFunctions

open System
open System.IO
open System.Xml
open System.Xml.Xsl
open System.Xml.XPath

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

  member public this.``file-stem-if``( fileName:string, suffixToRemove:string ) : string =
    let shortname = Path.GetFileName(fileName)
    if shortname.EndsWith(suffixToRemove, StringComparison.OrdinalIgnoreCase) then
      shortname.Substring(0, shortname.Length - suffixToRemove.Length)
    else
      shortname

  member public this.``split-folder``( fullname: string ) =
    let host = new XmlDocument()
    host.LoadXml("<name-parts/>")
    let root = host.DocumentElement
    let folder = Path.GetDirectoryName(fullname)
    if folder |> String.IsNullOrEmpty |> not then
      let e = host.CreateElement("folder")
      e.InnerText <- folder
      root.AppendChild(e) |> ignore
    let name = Path.GetFileName(fullname)
    if name |> String.IsNullOrEmpty |> not then
      let e = host.CreateElement("name")
      e.InnerText <- name
      root.AppendChild(e) |> ignore
    //host.CreateNavigator().Select("/name-parts/*") // return a node set containing just the <folder> and <name> children
    root.CreateNavigator() // return a single node with children (<name-parts>)
  
  member public this.``cwd``( ) : string =
    Environment.CurrentDirectory

  member public this.``path-combine``( paths: XPathNodeIterator ) : string =
    let pathArray =
      paths
      |> Utilities.unfoldNodes
      |> Seq.map (fun node -> node.Value)
      |> Seq.toArray
    Path.Combine(pathArray)

  member public this.``path-combine``( path1:string, path2:string ) : string =
    Path.Combine(path1, path2)

  member public this.``path-combine``( path1:string, path2:string, path3:string ) : string =
    Path.Combine(path1, path2, path3)

  member public this.``path-combine``( path1:string, path2:string, path3:string, path4:string ) : string =
    Path.Combine(path1, path2, path3, path4)

  member public this.``files-in-folder``( folder:string, mask:string, suffix:string ) : XPathNodeIterator =
    let host = new XmlDocument()
    host.LoadXml("<files/>")
    let root = host.DocumentElement
    let di = new DirectoryInfo(folder)
    let files =
      if mask |> String.IsNullOrEmpty then
        di.GetFiles()
      else
        di.GetFiles(mask)
    let files =
      if suffix |> String.IsNullOrEmpty then
        files
      else
        files
        |> Array.filter (fun fi -> fi.Name.EndsWith(suffix, StringComparison.InvariantCulture))
    for fi in files do
      let e = host.CreateElement("file")
      if suffix |> String.IsNullOrEmpty |> not then
        let tag = fi.Name.Substring(0, fi.Name.Length - suffix.Length)
        e.SetAttribute("tag", tag)
      e.SetAttribute("name", fi.Name)
      // e.SetAttribute("stem", fi.Name |> Path.GetFileNameWithoutExtension)
      // e.SetAttribute("ext", fi.Extension)
      e.SetAttribute("path", folder)
      e.InnerText <- Path.Combine(folder, fi.Name)
      root.AppendChild(e) |> ignore
    host.CreateNavigator().Select("/files/*")

  member public this.``files-in-folder``( folder:string, mask:string ) : XPathNodeIterator =
    this.``files-in-folder``( folder, mask, null)

  member public this.``files-in-folder``( folder:string ) : XPathNodeIterator =
    this.``files-in-folder``( folder, null, null)



