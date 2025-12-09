// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  cp "\foTool for applying XSLT style sheets\f0."
  cp "\foxsltrx \fg-s \fcstylesheet.xslt \f0{\fg-f \fcdoc.xml\f0} [\fg-o \fcoutfile\f0] {\fg-O \fcinTail\fo:\fcoutTail\f0} [\fg-doc\f0]"
  cp "   Apply an XSLT transform to an XML document. Supports EXSLT and other extensions."
  cp "\fg-s \fcstylesheet.xslt\f0     The stylesheet"
  cp "\fg-f \fcdoc.xml\f0             The document(s) to transform. Repeatable"
  cp "\fg-o \fcoutfile\f0             The output file. By default derived from the preceding document (\fg-f\f0)"
  cp "\fg-doc\f0\fx                   Enable the XSLT \fodocument()\f0 function. Without this it is disabled."
  cp "\fx\fx\fx                       Even when given, only filesystem based documents are allowed."
  cp "\fg-O \fcinTail\fo:\fcoutTail\f0      Add a rule to derive the output name from an input (\fg-f\f0) name."
  cp "\fx\fx\fx                       If the input ends with \fcinTail\f0 then replace that with \fcoutTail\f0 and remove the path."
  cp "\fx\fx\fx                       If no rule matches, the default rule that replaces the input extension with \fo.out.xml\f0 applies."
  cp "\fx\fx\fx                       Example: '\fg-O \fc.xml\fo:\fc.out.xml\f0'"
  cp "\fg-v               \f0Verbose mode"



