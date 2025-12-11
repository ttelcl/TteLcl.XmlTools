// (c) 2025  ttelcl / ttelcl
module Usage

open CommonTools
open ColorPrint

let usage command =
  cp "\foConvert between JSON and JXSMOLN\f0"
  cp ""
  cp "\fojxsmoln \fg-j \fcfile.json\f0 [\fg-o \fcfile.xml\f0] [\fg-mjson\f0]"
  cp "   Convert JSON to JXSMOLN"
  cp "   \fg-mjson\f0\fx          Read input as *.mjson (even if the extension is not *.mjson or *.jsonl)"
  cp ""
  cp "\fojxsmoln \fg-x \fcfile.xml\f0 [\fg-o \fcfile.json\f0] [\fg-indent \fo0\f0|\fg-indent \fcspaces\f0]] [\fg-trace\f0]"
  cp "   Convert JXSMOLN to JSON."
  cp "   \fg-indent \fo0\f0       Save not-indented JSON"
  cp "   \fg-indent \fcspaces\f0  Save JSON indented by the specified number of \fcspaces\f0 (default \fg-indent \fc2\f0)"
  cp "   \fg-trace\f0\fx          Debug the converter, tracing state changes. Very spammy."
  cp ""
  cp "\fg-v               \f0Verbose mode"
  cp ""
  cp "You can provide multiple \fg-j\f0 and \fg-x\f0 options, each defining a conversion job."
  cp "Any \fg-o\f0 options affect the preceding \fg-j\f0 or \fg-x\f0 option."



