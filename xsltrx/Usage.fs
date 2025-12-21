// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  cp "\foTool for applying XSLT style sheets\f0."
  cp "\foxsltrx \f0{\fg-s \fcstylesheet.xslt \f0[\fg-diag\f0]} {\fg-zip \fcfile.zip\f0} {[\fg-f \fcdoc.xml\f0|\fg-e \fczipentry\f0] [\fg-o \fcoutfile\f0]} {\fg-O \fcinTail\fo:\fcoutTail\f0} [\fg-doc\f0]"
  cp "   Apply an XSLT transform to an XML document. Supports EXSLT and other extensions. Supports XML documents inside ZIP files."
  cp "\fg-s \fcstylesheet.xslt\f0     Add a transform pipeline step using the given stylesheet. There must be at least one."
  cp "\fx\fx\fx                       All except the last must have output method 'XML'."
  cp "\fg-diag\f0\fx                  Debug aid: Abort after the preceding transform stage (\fg-s\f0) and dump its output xml to a *.diag.xml file"
  cp "\fg-zip \fcfile.zip\f0          The zip file to use for subsequent \fg-e\f0 arguments (ignored if there are no \fg-e\f0 arguments)"
  cp "\fg-f \fcdoc.xml\f0             The document(s) to transform. Repeatable"
  cp "\fg-e \fcpath/zipentry.xml\f0   The path to the XML file entry inside the preceding ZIP file (\fg-zip\f0). Repeatable"
  cp "\fg-o \fcoutfile\f0             The output file for the preceding document (\fg-f\f0 or \fg-e\f0). By default derived from that preceding document"
  cp "\fg-doc\f0\fx                   Enable the XSLT \fodocument()\f0 function. Without this it is disabled."
  cp "\fx\fx\fx                       Even when given, only filesystem based documents are allowed."
  cp "\fg-O \fcinTail\fo:\fcoutTail\f0      Add a rule to derive the output name from an input (\fg-f\f0) name."
  cp "\fx\fx\fx                       If the input ends with \fcinTail\f0 then replace that with \fcoutTail\f0 and remove the path."
  cp "\fx\fx\fx                       If no rule matches, the default rule that replaces the input extension with \fo.out.xml\f0 applies."
  cp "\fx\fx\fx                       Example: '\fg-O \fc.xml\fo:\fc.out.xml\f0'"
  cp "\fg-json\f0\fx                  Convert the final output to indented JSON. For this to work the final stylesheet must"
  cp "\fx\fx\fx                       produce XML in the JxSmoln style."
  cp "\fg-json-flat\f0\fx             Like \fg-json\f0, but writing flat JSON instead of indented JSON."
  cp "\fg-mjson\f0\fx                 Like \fg-json\f0, but writing multi-json (\fo*.mjson\f0: multiple indented JSON items separated by newlines)."
  cp "\fg-jsonl\f0\fx                 Like \fg-json\f0, but writing line-json (\fo*.jsonl\f0: multiple flat JSON items, each on a line)."
  cp "\fg-v               \f0Verbose mode"



