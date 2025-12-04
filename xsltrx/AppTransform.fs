module AppTransform

open System
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Xsl

open Mvp.Xml.Exslt
open Mvp.Xml.Common.Xsl

open ColorPrint
open CommonTools

type private Options = {
  Stylesheet: string
  Input: string
  Output: string
}

let private runTransform o =
  // see also https://github.com/devlooped/Mvp.Xml/blob/main/src/Mvp.Xml/Exslt/Xsl/MvpXslTransform.cs
  let doc = new XPathDocument(o.Input)
  let xsltFile =
    // To Do: extract stylesheet from document if o.StyleSheet is missing
    o.Stylesheet
  let transform =
    let trx = new MvpXslTransform()
    trx.Load(xsltFile)
    trx
  do
    use tw = o.Output |> startFile
    let arglist = new XsltArgumentList()
    let input = new XmlInput(doc)
    let output = new XmlOutput(tw)
    transform.Transform(input, arglist, output) |> ignore
  o.Output |> finishFile
  0

let run args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "--help" :: _
    | "-h" :: _ ->
      None
    | "-f" :: file :: rest ->
      rest |> parseMore {o with Input = file}
    | "-s" :: stylesheet :: rest ->
      rest |> parseMore {o with Stylesheet = stylesheet}
    | "-o" :: outfile :: rest ->
      rest |> parseMore {o with Output = outfile}
    | [] ->
      if o.Stylesheet |> String.IsNullOrEmpty then
        cp "\foNo stylesheet file (\fg-s\fo) given\f0."
        None
      elif o.Input |> String.IsNullOrEmpty then
        cp "\foNo input file (\fg-f\fo) given\f0."
        None
      elif o.Output |> String.IsNullOrEmpty then
        // For now, require an output file
        cp "\foNo output file (\fg-o\fo) given\f0."
        None
      else
        o |> Some
    | x :: _ ->
      cp $"\frUnrecognized argument \f0'\fy{x}\f0'"
      None
  let oo = args |> parseMore {
    Stylesheet = null
    Input = null
    Output = null
  }
  match oo with
  | Some(o) ->
    o |> runTransform
  | None ->
    cp ""
    Usage.usage "scc"
    1


