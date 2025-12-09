// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  cp "\foTool for applying XSLT style sheets\f0."
  cp "\foxsltrx \fg-s \fcstylesheet.xslt \fg-f \fcdoc.xml \f0[\fg-o \fcoutfile\f0] [\fg-doc\f0]"
  cp "   Apply an XSLT transform to an XML document. Supports EXSLT and other extensions."
  cp "\fg-s \fcstylesheet.xslt\f0     The stylesheet"
  cp "\fg-f \fcdoc.xml\f0             The document to transform"
  cp "\fg-o \fcoutfile\f0             The output file. By default derived from the document (\fg-f\f0)"
  cp "\fg-doc\f0\fx                   Enable the XSLT \fodocument()\f0 function. Without this it is disabled."
  cp "\fx\fx\fx                       Even when given, only filesystem based documents are allowed."
  cp "\fg-v               \f0Verbose mode"



