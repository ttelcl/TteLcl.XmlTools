# jxsmoln - converter between JSON and an XML model of JSON

`jxsmoln` is a tool to convert between JSON and "jxsmoln", a model of JSON in XML.
"jxsmoln" is a mashup of the words JSON and XML; I arrived at that name after I realized
that all the "good names" for that purpose were taken already :)

JXSMOLN is XML using a specific schema to model JSON data. While the purpose is similar
to IBM's "JSONx", the model is slightly different.

* The namespace is `https://github.com/ttelcl/TteLcl.XmlTools/blob/main/jxsmoln/README.md`
* If using a namespace prefix, the suggested one is "j". Of course you are free
  to use whatever prefix you want, or just use a default namespace
* Unlike "JSONx", object properties are made explicit
* JSON keywords all have their own element: `<j:null/>`, `<j:true/>`, and  `<j:false\>`

These are the elements in a JXSMOLN:

| element | description |
| --- | --- |
| `<j:ob> ... </j:ob>` | A JSON object `{ ... }` |
| `<j:prop key="NAME">(value)</j:prop>` | A property in a JSON object `"NAME": (value)`|
| `<j:list> ... </j:list>` | A JSON list (array) `[ ... ]` |
| `<j:null/>` | A `null` literal |
| `<j:true/>` | A `true` literal |
| `<j:false/>` | A `false` literal |
| `<j:str>TEXT</j:str>` | A JSON string `"TEXT"` |
| `<j:num>NUMBER</j:num>` | A JSON numeric value (integer or real) |
| `<j:mjson>...</j:mjson>` | Only valid as root element, indicating the content is <BR/>multi-JSON rather than normal JSON |

Example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<j:ob xmlns:j="https://github.com/ttelcl/TteLcl.XmlTools/blob/main/jxsmoln/README.md">
  <j:prop key="hello">
    <j:str>world</j:str>
  </j:prop>
  <j:prop key="is-it-real">
    <j:true/>
  </j:prop>
</j:ob>
```

This corresponds to the JSON document:
```json
{
  "hello": "world",
  "is-it-real": true
}
```
