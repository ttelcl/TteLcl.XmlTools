# jsxml

Tool to convert between JSON and JSXML.

JSXML is XML using a specific schema to model JSON data. It uses the following
elements:

| element | description |
| --- | --- |
| `<ob> ... </ob>` | A JSON object `{ ... }` |
| `<prop key="NAME">(value)</prop>` | A property in a JSON object `"NAME": (value)`|
| `<ar> ... </ar>` | A JSON array `[ ... ]` |
| `<null/>` | A `null` literal |
| `<true/>` | A `true` literal |
| `<false/>` | A `false` literal |
| `<str>TEXT</str>` | A JSON string `"TEXT"` |
| `<num>NUMBER</num>` | A JSON numeric value (integer or real) |

~~In this initial version there is no dedicated namespace~~.

Example:
```
<?xml version="1.0" encoding="utf-8"?>
<ob xmlns="https://github.com/ttelcl/TteLcl.XmlTools">
  <prop key="hello">
    <str>world</str>
  </prop>
  <prop key="is-it-real">
    <true/>
  </prop>
</ob>
```
