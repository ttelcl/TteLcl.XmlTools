# TteLcl.XsltExtensionFunctions

This library contains objects that implement extension functions for use with
System.Xml.Xsl.XslCompiledTransform.

The implementation is in F# rather than C# to use the more flexible identifier naming rules
of F# (as an example: unlike C#, F# can declare a method named 'get-segment()')

## SegmentFunctions

XSL extension functions in the namespace `urn:ttelcl:xsltextensionfunctions:segment`:

### get-segment
`get-segment(string text, string separator, int index)`

Split `text` into one or more segments using the `separator`. Return the segment at
`index` (1-based indexing). If `index` is negative count from the end instead of the start.

#### Examples
```
get-segment('path\to\a\file.txt', '\', 1) -> 'path'
get-segment('path\to\a\file.txt', '\', -1) -> 'file.txt'
```

### segment-head
`segment-head(string text, string separator, int count)`

Split `text` into one or more segments using the `separator`.
If `count` is positive then recombine the first `count` segments and return it.
If `count` is negative then recombine all segments except the final `-count`.

#### Examples
```
segment-head('path\to\a\file.txt', '\', 2) -> 'path\to'
segment-head('path\to\a\file.txt', '\', -1) -> 'path\to\a'
```
