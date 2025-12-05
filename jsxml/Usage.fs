// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage command =
  cp "\foConvert between JSON and JSXML\f0"
  cp ""
  cp "\fojsxml \fg-j \fcfile.json\f0 [\fg-o \fcfile.jsxml\f0]"
  cp "   Convert JSON to JSXML"
  cp "\fojsxml \fg-x \fcfile.jsxml\f0 [\fg-o \fcfile.json\f0] [\fg-indent \fo0\f0|\fg-indent \fcspaces\f0]]"
  cp "   Convert JSXML to JSON. Default is \fg-indent \fc2\f0."
  cp "\fg-v               \f0Verbose mode"



