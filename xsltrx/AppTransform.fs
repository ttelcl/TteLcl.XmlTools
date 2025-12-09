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

type private OutputDerivation = {
  InputTail: string
  OutputTail: string
}

type private InputOutput = {
  Input: string
  Output: string
}

type private Transformation = {
  XsltFile: string
  LoadedTransform: MvpXslTransform
}

type private Options = {
  Stylesheets: string list
  EnableDocumentFunction: bool
  InOutPairs: InputOutput list
  OutputDerivations: OutputDerivation list
}

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
  let stylesheets = o.Stylesheets
  let pipeline =
    stylesheets
    |> List.map (fun ss -> {XsltFile = ss; LoadedTransform = ss |> createTransform})
  // validate output method of intermediate stages and return the final output method
  let rec getPipelineOutputMethod transformList =
    match transformList with
    | trx :: [] ->
      trx.LoadedTransform.OutputSettings.OutputMethod |> Some
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
  | Some(lastOutputMethod) ->
    let defaultExtension =
      match lastOutputMethod with
      | XmlOutputMethod.Text -> ".txt"
      | XmlOutputMethod.Html -> ".html"
      | _ -> ".out.xml" // avoid overwriting input, which most likely already has extension ".xml"
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
      new XmlInput(xout)
    let transformLast trx outname (xin:XmlInput) =
      cp $"  Applying final transform \fc{trx.XsltFile}\f0 (mode \fb{trx.LoadedTransform.OutputSettings.OutputMethod}\f0)."
      let arglist = new XsltArgumentList()
      // PLACEHOLDER!
      do
        use tw = outname |> startFile
        let xout = new XmlOutput(tw)
        trx.LoadedTransform.Transform(xin, arglist, xout) |> ignore
      outname |> finishFile
    let rec transformPipeline outname xin pipeline =
      match pipeline with
      | trx :: [] ->
        xin |> transformLast trx outname
      | trx :: remainder ->
        let xin2 = xin |> transformIntermediate trx
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
      //do
      //  use tw = output |> startFile
      //  let arglist = new XsltArgumentList()
      //  let input = new XmlInput(doc)
      //  let output = new XmlOutput(tw)
      //  transform.Transform(input, arglist, output) |> ignore
      //output |> finishFile
    0
  | None ->
    // an error was printed already
    1

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
      rest |> parseMore {o with Stylesheets = stylesheet :: o.Stylesheets}
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
  }
  match oo with
  | Some(o) ->
    o |> runTransform
  | None ->
    cp ""
    Usage.usage "scc"
    1


