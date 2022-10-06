module FfmpegFSharp.Tests.JsonConvertorsTest

open System.Text.Json
open FfmpegFSharp.JsonConvertors
open Xunit

open FsUnit

type OptionTestType =
    { intValue: int option
      stringValue: string option }


let optionSerializationData: obj [] list =
    [ [| { intValue = Some 1
           stringValue = Some "ss" }
         "{\"intValue\":1,\"stringValue\":\"ss\"}" |]
      [| { intValue = None
           stringValue = Some "ss" }
         "{\"intValue\":null,\"stringValue\":\"ss\"}" |]
      [| { intValue = Some 1
           stringValue = None }
         "{\"intValue\":1,\"stringValue\":null}" |] ]


[<Theory>]
[<MemberData(nameof optionSerializationData)>]
let ``OptionSerializationTest`` (object: OptionTestType) (result: string) =
    let options = JsonSerializerOptions()
    options.Converters.Add(OptionConverter())
    options.WriteIndented <- false

    let res =
        JsonSerializer.Serialize<_>(object, options)

    res |> should equal result


[<Theory>]
[<MemberData(nameof optionSerializationData)>]
let ``OptionDeserializationTest`` (object: OptionTestType) (json: string) =
    let options = JsonSerializerOptions()
    options.Converters.Add(OptionConverter())
    options.WriteIndented <- false

    let res =
        JsonSerializer.Deserialize<OptionTestType>(json, options)

    res |> should equal object

type ListSerializationTestType =
    { intList: int list
      stringList: string list }

let listSerializationData: obj [] list =
    [ [| { intList = []; stringList = [] }
         "{\"intList\":[],\"stringList\":[]}" |]
      [| { intList = [ 1 ]; stringList = [] }
         "{\"intList\":[1],\"stringList\":[]}" |]
      [| { intList = [ 1; 2 ]; stringList = [] }
         "{\"intList\":[1,2],\"stringList\":[]}" |]
      [| { intList = []; stringList = [ "a" ] }
         "{\"intList\":[],\"stringList\":[\"a\"]}" |]
      [| { intList = []
           stringList = [ "a"; "b" ] }
         "{\"intList\":[],\"stringList\":[\"a\",\"b\"]}" |] ]

[<Theory>]
[<MemberData(nameof listSerializationData)>]
let ``ListSerializationTest`` (object: ListSerializationTestType) (result: string) =
    let options = JsonSerializerOptions()
    options.Converters.Add(ListConverter())
    options.WriteIndented <- false

    let res =
        JsonSerializer.Serialize<_>(object, options)

    res |> should equal result
