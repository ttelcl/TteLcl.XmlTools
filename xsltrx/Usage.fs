// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage focus =
  cp "\foxsltrx \fg-s \fcstylesheet.xslt \fg-f \fcdoc.xml \f0[\fg-o outfile\f0]"
  cp "   Apply an XSLT transform to an XML document. Supports EXSLT and other extensions."
  cp "\fg-v               \f0Verbose mode"



