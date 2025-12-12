module TteLcl.XsltExtensionFunctions.Utilities

open System
open System.IO
open System.Xml
open System.Xml.Xsl
open System.Xml.XPath

/// Unfold the content of a node-set (represented by an XPathNodeIterator) into a sequence of
/// nodes (represented by XPathNavigator)
let public unfoldNodes (nodes: XPathNodeIterator): XPathNavigator seq =
  seq {
    while nodes.MoveNext() do
      yield nodes.Current
  }
