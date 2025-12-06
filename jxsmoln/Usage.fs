// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage command =
  cp "\foConvert between JSON and JXSMOLN\f0"
  cp ""
  cp "\fojxsmoln \fg-j \fcfile.json\f0 [\fg-o \fcfile.xml\f0]"
  cp "   Convert JSON to JXSMOLN"
  cp ""
  cp "\fojxsmoln \fg-x \fcfile.xml\f0 [\fg-o \fcfile.json\f0] [\fg-indent \fo0\f0|\fg-indent \fcspaces\f0]]"
  cp "   Convert JXSMOLN to JSON."
  cp "   \fg-indent \fo0\f0       Save not-indented JSON"
  cp "   \fg-indent \fcspaces\f0  Save JSON indented by the specified number of \fcspaces\f0 (default \fg-indent \fc2\f0)"
  cp ""
  cp "\fg-v               \f0Verbose mode"



