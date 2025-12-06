module App

open System
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Xsl

open ColorPrint
open CommonTools

type private ConversionJob =
  | JsonToXml of JsonIn: string * XmlOut: string
  | XmlToJson of XmlIn: string * JsonOut: string

type private Options = {
  Jobs: ConversionJob list
  JsonIndent: int
}

let private convertJsonToXml o jsonIn xmlOut =
  cp "\frNot Yet Implemented!\f0 (json to xml)"
  1

let private convertXmlToJson o xmlIn jsonOut =
  cp "\frNot Yet Implemented!\f0 (xml to json)"
  1

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
  }
  match oo with
  | Some(o) ->
    o |> runConversions
  | None ->
    cp ""
    Usage.usage "convert"
    1



