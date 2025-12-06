// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage command =
  cp "\foConvert between JSON and JSXML\f0"
  cp ""
  cp "\fojsxml \fg-j \fcfile.json\f0 [\fg-o \fcfile.jsxml\f0]"
  cp "   Convert JSON to JSXML"
  cp ""
  cp "\fojsxml \fg-x \fcfile.jsxml\f0 [\fg-o \fcfile.json\f0] [\fg-indent \fo0\f0|\fg-indent \fcspaces\f0]]"
  cp "   Convert JSXML to JSON."
  cp "   \fg-indent \fo0\f0       Save not-indented JSON"
  cp "   \fg-indent \fcspaces\f0  Save JSON indented by the specified number of \fcspaces\f0. Default is \fg-indent \fc2\f0"
  cp ""
  cp "\fg-v               \f0Verbose mode"



