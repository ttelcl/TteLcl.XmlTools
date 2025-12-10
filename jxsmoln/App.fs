module App

open System
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Xsl

open Newtonsoft.Json

open TteLcl.JxSmoln

open ColorPrint
open CommonTools

type private ConversionJob =
  | JsonToXml of JsonIn: string * XmlOut: string
  | XmlToJson of XmlIn: string * JsonOut: string

type private Options = {
  Jobs: ConversionJob list
  JsonIndent: int
  TraceJson: bool
}

// Debug utility for JsonConversion
let traceJson (reader: XmlReader) (message: string) (line: int) (caller: string) =
  cpx $"\fb{line,4}\fo:\fw{caller,-20} \fg{reader.NodeType,-12}\f0 "
  match reader.NodeType with
  | XmlNodeType.Element ->
    if reader.IsEmptyElement then
      cpx $"\fr<\fc{reader.Name}\fr/>\f0"
    else
      cpx $"\fo<\fc{reader.Name}\fo>\f0"
  | XmlNodeType.EndElement -> cpx $"\fo</\fc{reader.Name}\fo>\f0"
  | _ -> cpx $"'\fy{reader.Name}\f0'"
  if message |> String.IsNullOrEmpty |> not then
    cpx $"\t\f0(\fy{message}\f0)"
  cp ""

let private convertJsonToXml o jsonIn xmlOut =
  cp "\frNot Yet Implemented!\f0 (json to xml)"
  1

let private convertXmlToJson o xmlIn jsonOut =
  if o.TraceJson then
    JxConversion.ReaderTracer <- (fun rdr message line caller -> traceJson rdr message line caller)
  let formatting =
    match o.JsonIndent with
    | 0 -> Formatting.None
    | 2 -> Formatting.Indented
    | _ ->
      failwith "Json indents other than 0 (none) or 2 are not yet supported."
  let jsonOut =
    if jsonOut |> String.IsNullOrEmpty then
      Path.ChangeExtension(xmlIn, ".json")
    else
      jsonOut
  let node =
    use xr = XmlReader.Create(xmlIn)
    xr |> JxConversion.ReadJsonFromXml
  do
    let json = JsonConvert.SerializeObject(node, formatting)
    use w = jsonOut |> startFile
    w.WriteLine(json)
  jsonOut |> finishFile
  0

let private runConversion o conversion =
  match conversion with
  | JsonToXml (jsonIn, xmlOut) ->
    convertJsonToXml o jsonIn xmlOut
  | XmlToJson (xmlIn, jsonOut) ->
    convertXmlToJson o xmlIn jsonOut

let private runConversions o =
  let allOk = o.Jobs |> List.forall (fun job -> (job |> runConversion o) = 0)
  if allOk then 0 else 1

let run args =
  let rec parseMore o args =
    match args with
    | "-v" :: rest ->
      verbose <- true
      rest |> parseMore o
    | "--help" :: _
    | "-h" :: _ ->
      None
    | "-j" :: jsonFile :: rest ->
      let job = ConversionJob.JsonToXml(jsonFile, null)
      rest |> parseMore {o with Jobs = job :: o.Jobs}
    | "-x" :: xmlFile :: rest ->
      let job = ConversionJob.XmlToJson(xmlFile, null)
      rest |> parseMore {o with Jobs = job :: o.Jobs}
    | "-trace" :: rest ->
      rest |> parseMore {o with TraceJson = true}
    | "-o" :: outfile :: rest ->
      match o.Jobs with
      | JsonToXml(json, _) :: jobs ->
        let job = JsonToXml(json, outfile)
        rest |> parseMore {o with Jobs = job :: jobs}
      | XmlToJson(xml, _) :: jobs ->
        let job = XmlToJson(xml, outfile)
        rest |> parseMore {o with Jobs = job :: jobs}
      | [] ->
        cp "\frExpecting a \fg-j\fr or \fg-x\fr before the first \fg-o\f0."
        None
    | "-indent" :: spaces :: rest ->
      let ok, spaceCount = spaces |> Int32.TryParse
      if ok && spaceCount >= 0 then
        rest |> parseMore {o with JsonIndent = spaceCount}
      else
        cp $"\fr'\fg-indent\fr': Failed to parse '\fc{spaces}\fr' as a non-negative integer\f0."
        None
    | [] ->
      if o.Jobs |> List.isEmpty then
        cp "\foNo conversion jobs (\fg-j\fo and/or \fg-x\fo) given\f0."
        None
      else
        {o with Jobs = o.Jobs |> List.rev} |> Some
    | x :: _ ->
      cp $"\frUnrecognized argument \f0'\fy{x}\f0'"
      None
  let oo = args |> parseMore {
    Jobs = []
    JsonIndent = 2
    TraceJson = false
  }
  match oo with
  | Some(o) ->
    o |> runConversions
  | None ->
    cp ""
    Usage.usage "convert"
    1



