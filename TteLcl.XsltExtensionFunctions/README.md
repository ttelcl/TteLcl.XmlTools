# TteLcl.XsltExtensionFunctions

This library contains objects that implement extension functions for use with
System.Xml.Xsl.XslCompiledTransform.

The implementation is in F# rather than C# to use the more flexible identifier naming rules
of F# (as an example: unlike C#, F# can declare a method named 'get-segment()')

## SegmentFunctions

XSL extension functions in the namespace `urn:ttelcl:xslt-extension-functions:segment`:

### segment-get
`segment-get(string text, string separator, int index)`

_Alias:_ `get-segment(string text, string separator, int index)`

Split `text` into one or more segments using the `separator`. Return the segment at
`index` (1-based indexing). If `index` is negative count from the end instead of the start.

#### Examples
```
segment-get('path\to\a\file.txt', '\', 1) -> 'path'
segment-get('path\to\a\file.txt', '\', -1) -> 'file.txt'
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

## FilesystemFunctions

XSL extension functions in the namespace `urn:ttelcl:xslt-extension-functions:filesystem`:

### file-name
`file-name(string fullname)`

Returns the file name part of the argument (with any folder information removed)

### full-name
`full-name(string fileOrFolder)`

Returns the full path of the given file or folder

### file-extension
`file-extension(string fileName)`

Returns the (last) file extension of `fileName` (including the leading '.')


### parent-folder
`parent-folder(string fileOrFolder)`

Returns the folder part of the file name or the parent folder part of the folder name.

### file-stem (1 argument version)
`file-extension(string fileName)`

Returns `fileName` without its folder part and without its last extension

### file-stem (2 arguments version)
`file-extension(string fileName, string suffixToRemove)`

First removes the folder name from `fileName`. If the file name ends with
`suffixToRemove`, it returns the file name with that suffix removed. Otherwise
this function aborts. See also `file-stem-if` below.

### file-stem-if
`file-extension(string fileName, string suffixToRemove)`

First removes the folder name from `fileName`. If the file name ends with
`suffixToRemove`, it returns the file name with that suffix removed. Otherwise
this function returns that short file name without any further removals.
See also `file-stem (2 arguments version)` above.

### cwd
`cwd()`

Returns the full path of the current working directory

### path-combine (and variants)
`path-combine(nodeset paths)`

Combines the paths in the given node set (combining each relative path with
its preceding absolute path and starting over at each absolute path).

`path-combine(string path1, string path2)`

`path-combine(string path1, string path2, string path3)`

`path-combine(string path1, string path2, string path3, string path4)`

Combines the explicitly given paths into one.

### split-folder
`split-folder(string filename)`

Split the filename into folder and name parts and return a `<name-parts>`
element with `<folder>` and `<name>` children. If there is no folder
(the name is just a file name without path), the `<folder>` child is omitted.
If there is no name (the name ends with a path separator), the `<name>` child
is omitted.

