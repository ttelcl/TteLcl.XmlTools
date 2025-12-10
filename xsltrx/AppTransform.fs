module AppTransform

open System
open System.IO
open System.Xml
open System.Xml.XPath
open System.Xml.Xsl

open Mvp.Xml.Common.Xsl

open Newtonsoft.Json

open TteLcl.JxSmoln

open ColorPrint
open CommonTools

type private OutputDerivation = {
  InputTail: string
  OutputTail: string
}

type private InputOutput = {
  Input: string
  Output: string
}

type private TransformStage = {
  TransformFile: string
  DiagnosticAbort: bool // if set: emit a diagnostic file after transformation and abort
}

type private Transformation = {
  XsltFile: string
  LoadedTransform: MvpXslTransform
  DiagnosticAbort: bool // if set: emit a diagnostic file after transformation and abort
}

type private JsonFormat =
  | NotJson
  | Indented
  | Flat
  // | MultiIndented
  // | MultiFlat

type private Options = {
  Stylesheets: TransformStage list
  EnableDocumentFunction: bool
  InOutPairs: InputOutput list
  OutputDerivations: OutputDerivation list
  JsonMode: JsonFormat
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

let private tryParseDerivation (derivation: string) =
  let parts = derivation.Split(':')
  if parts.Length <> 2 then
    cp $"\frInvalid argument to '-O'. \f0Expecting a string with exactly one '\fo:\f0' but got '\fy{derivation}\f0'"
    None
  else
    {
      InputTail = parts[0]
      OutputTail = parts[1]
    } |> Some

let private tryDeriveOutput derivations input =
  let tryDerive (input: string) derivation =
    if input.EndsWith(derivation.InputTail, StringComparison.OrdinalIgnoreCase) then
      let head = input.Substring(0, input.Length - derivation.InputTail.Length)
      Some(head + derivation.OutputTail)
    else
      None
  derivations
  |> Seq.choose (tryDerive input)
  |> Seq.tryHead

let private diagnosticAbort (filename:string) (xr: XmlReader) =
  cp $"\foWriting diagnostic dump \fr{filename}\f0."
  let xws = new XmlWriterSettings()
  xws.CloseOutput <- true
  xws.Indent <- true
  use xw = XmlWriter.Create(filename, xws)
  xw.WriteNode(xr, true)
  failwith "Aborting after writing diagnostic file"

let cacheReader (xrdr:XmlReader) =
  new XPathDocument(xrdr)

let private emitJson mode filename xrdr =
  //// The XmlReader produced by MvpXslTransform.Transform and the one taken by
  //// JxConversion.ReadJsonFromXml disagree on their expectations for behaviour.
  //// As a hack, materialize the XML document first
  //let doc = xrdr |> cacheReader
  //let xrdr = doc.CreateNavigator().ReadSubtree()
  let isMulti =
    match mode with
    | JsonFormat.NotJson ->
      failwith "Internal error: expecting a JSON mode, not 'NotJson'"
    | JsonFormat.Flat -> false
    | JsonFormat.Indented -> false
  let isIndented =
    match mode with
    | JsonFormat.NotJson ->
      failwith "Internal error: expecting a JSON mode, not 'NotJson'"
    | JsonFormat.Flat -> false
    | JsonFormat.Indented -> true
  let formatting =
    // Explicit namespace to avoid any confusion between Newtonsoft.Json.Formatting and System.Xml.Formatting
    if isIndented then Newtonsoft.Json.Formatting.Indented else Newtonsoft.Json.Formatting.None
  if isMulti then
    failwith "Multi-JSON conversion NYI"
  else
    let node = xrdr |> JxConversion.ReadJsonFromXml
    do
      let json = JsonConvert.SerializeObject(node, formatting)
      use w = filename |> startFile
      w.WriteLine(json) // TODO: should this be suppressed for Flat format?
    filename |> finishFile

let private runTransform o =
  // see also https://github.com/devlooped/Mvp.Xml/blob/main/src/Mvp.Xml/Exslt/Xsl/MvpXslTransform.cs
  
  // To Do: extract stylesheet from document if o.StyleSheet is missing

  // Create a transform loader function with a memoized resolver and settings if applicable
  let transformloader: MvpXslTransform -> string -> unit =
    if o.EnableDocumentFunction then
      let resolver = XmlResolver.FileSystemResolver
      let settings = new XsltSettings(o.EnableDocumentFunction, false)
      fun trx xsltFile -> trx.Load(xsltFile, settings, resolver)
    else
      fun trx xsltFile -> trx.Load(xsltFile)
  let createTransform xsltFile =
    let trx = new MvpXslTransform()
    cp $"Loading stylesheet \fb{xsltFile}\f0."
    transformloader trx xsltFile
    trx
  let jsonOutput = o.JsonMode <> JsonFormat.NotJson
  let stylesheets = o.Stylesheets
  let pipeline =
    stylesheets
    |> List.map
      (fun ss -> {
        XsltFile = ss.TransformFile
        LoadedTransform = ss.TransformFile |> createTransform
        DiagnosticAbort = ss.DiagnosticAbort})
  // validate output method of intermediate stages and return the final output method
  let rec getPipelineOutputMethod transformList =
    match transformList with
    | trx :: [] ->
      let om = trx.LoadedTransform.OutputSettings.OutputMethod
      if jsonOutput then
        if om <> XmlOutputMethod.Xml then
          cp $"\frError: When using JSON output, the final stylesheet must have output method 'XML', but got '\fb{om}\fr' in \f0'\fc{trx.XsltFile}\f0'."
          None
        else
          om |> Some
      else
        om |> Some
    | trx :: rest ->
      let outputMethod = trx.LoadedTransform.OutputSettings.OutputMethod
      if outputMethod <> XmlOutputMethod.Xml then
        cp $"\frError: Intermediate stylesheets must have output method 'XML', but got '\fb{outputMethod}\fr' in \f0'\fc{trx.XsltFile}\f0'."
        None
      else
        rest |> getPipelineOutputMethod
    | [] ->
      cp "\frNo transformations in the pipeline\f0."
      None
  let lastOutputMethod = pipeline |> getPipelineOutputMethod
  match lastOutputMethod with
  | None ->
    // an error was printed already
    1
  | Some(lastOutputMethod) ->
    let defaultExtension =
      match o.JsonMode with
      | JsonFormat.NotJson ->
        match lastOutputMethod with
        | XmlOutputMethod.Text -> ".txt"
        | XmlOutputMethod.Html -> ".html"
        | _ -> ".out.xml" // avoid overwriting input, which most likely already has extension ".xml"
      | JsonFormat.Indented -> ".json"
      | JsonFormat.Flat -> ".json"
    let resolveOutput pair =
      if pair.Output |> String.IsNullOrEmpty then
        let input = pair.Input |> Path.GetFileName
        let output =
          if o.OutputDerivations |> List.isEmpty then
            // silently use the default
            Path.ChangeExtension(input, defaultExtension)
          else
            let attempt = input |> tryDeriveOutput o.OutputDerivations
            match attempt with
            | Some(output) -> output
            | None ->
              // noisily use the default
              cp $"\frWarning\f0: no '\fg-O\f0' rule matched '\fy{input}\f0'. Using the default output naming rule."
              Path.ChangeExtension(input, defaultExtension)
        {pair with Output = output}
      else
        pair
    let pairs = o.InOutPairs |> List.map resolveOutput
    let transformIntermediate trx (xin:XmlInput) =
      cp $"  Applying intermediate transform \fc{trx.XsltFile}\f0."
      let arglist = new XsltArgumentList()
      let xout = trx.LoadedTransform.Transform(xin, arglist, false, 256 (* that's the default *))
      if trx.DiagnosticAbort then
        let diagname = Path.ChangeExtension(Path.GetFileName(trx.XsltFile), ".diag.xml")
        xout |> diagnosticAbort diagname
      xout
    let transformLast trx outname (xin:XmlInput) =
      cp $"  Applying final transform \fc{trx.XsltFile}\f0 (mode \fb{trx.LoadedTransform.OutputSettings.OutputMethod}\f0)."
      if trx.DiagnosticAbort then
        cp "  (\frIgnoring \fg-diag\fr for final transform stage\f0)"
      let arglist = new XsltArgumentList()
      do
        use tw = outname |> startFile
        let xout = new XmlOutput(tw)
        trx.LoadedTransform.Transform(xin, arglist, xout) |> ignore
      outname |> finishFile
    let transformJson outname (xrdr:XmlReader) =
      cp $"  Converting to JSON (format '\fg{o.JsonMode}\f0')"
      if o.TraceJson then
        JxConversion.ReaderTracer <- (fun rdr message line caller -> traceJson rdr message line caller)
      xrdr |> emitJson o.JsonMode outname
    let transformJsonLast trx outname (xin:XmlInput) =
      let xout = xin |> transformIntermediate trx
      xout |> transformJson outname
    let rec transformPipeline outname xin pipeline =
      match pipeline with
      | trx :: [] ->
        if jsonOutput then
          xin |> transformJsonLast trx outname
        else
          xin |> transformLast trx outname
      | trx :: remainder ->
        let xout = xin |> transformIntermediate trx
        let xin2 = new XmlInput(xout)
        remainder |> transformPipeline outname xin2
      | [] ->
        failwith "Not expecting an empty pipeline here"
    for pair in pairs do
      let input = pair.Input
      let output = pair.Output
      cp $"Transforming \fg{input}\f0 to \fo{output}\f0."
      let doc = new XPathDocument(input)
      let xin = new XmlInput(doc)
      pipeline |> transformPipeline output xin
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
      let pair = {
        Input = file
        Output = null
      }
      rest |> parseMore {o with InOutPairs = pair :: o.InOutPairs}
    | "-s" :: stylesheet :: rest ->
      let stage = {
        TransformFile = stylesheet
        DiagnosticAbort = false
      }
      rest |> parseMore {o with Stylesheets = stage :: o.Stylesheets}
    | "-o" :: outfile :: rest ->
      match o.InOutPairs with
      | head :: others ->
        let pair = {head with Output = outfile}
        rest |> parseMore {o with InOutPairs = pair :: others}
      | [] ->
        cp $"\fr'fg-o \f0{outfile}\fr' applies to the preceding '\fg-f\fr', but there were none of those yet\f0."
        None
    | "-doc" :: rest ->
      // Voodoo inferred from https://stackoverflow.com/a/77903294/271323
      AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);
      rest |> parseMore {o with EnableDocumentFunction = true}
    | "-O" :: derivation :: rest ->
      match derivation |> tryParseDerivation with
      | None -> None
      | Some(d) ->
        rest |> parseMore {o with OutputDerivations = d :: o.OutputDerivations}
    | "-json" :: rest ->
      rest |> parseMore {o with JsonMode = JsonFormat.Indented}
    | "-json-flat" :: rest ->
      rest |> parseMore {o with JsonMode = JsonFormat.Flat}
    | "-mjson" :: rest ->
      cp "\frNot yet implemented: \fy-mjson\f0."
      None
    | "-jsonl" :: rest ->
      cp "\frNot yet implemented: \fy-jsonl\f0."
      None
    | "-diag" :: rest ->
      match o.Stylesheets with
      | head :: others ->
        let stage = {head with DiagnosticAbort = true}
        rest |> parseMore {o with Stylesheets = stage :: others}
      | [] ->
        cp $"\fr'fg-diag\fr' applies to the preceding '\fg-s\fr', but there were none of those yet\f0."
        None
    | "-trace" :: rest ->
      rest |> parseMore {o with TraceJson = true}
    | [] ->
      if o.Stylesheets |> List.isEmpty then
        cp "\foNo stylesheet file (\fg-s\fo) given\f0."
        None
      elif o.InOutPairs |> List.isEmpty then
        cp "\foNo input files (\fg-f\fo) given\f0."
        None
      else
        {o with OutputDerivations = o.OutputDerivations |> List.rev; Stylesheets = o.Stylesheets |> List.rev} |> Some
    | x :: _ ->
      cp $"\frUnrecognized argument \f0'\fy{x}\f0'"
      None
  let oo = args |> parseMore {
    Stylesheets = []
    InOutPairs = []
    EnableDocumentFunction = false
    OutputDerivations = []
    JsonMode = JsonFormat.NotJson
    TraceJson = false
  }
  match oo with
  | Some(o) ->
    o |> runTransform
  | None ->
    cp ""
    Usage.usage "scc"
    1


