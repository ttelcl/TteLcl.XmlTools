module AppTransform

open System
open System.IO

open ColorPrint
open CommonTools

type private Options = {
  Stylesheet: string
  Input: string
  Output: string
}

let private runTransform o =
  cp "\frNot Implemented\f0."
  1

let run args =
  runTransform ()

