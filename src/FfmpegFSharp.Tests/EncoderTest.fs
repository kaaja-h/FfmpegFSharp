﻿module FfmpegFSharp.Tests.EncoderTest

open FfmpegFSharp
open FfmpegFSharp.Converting
open Xunit

open FsUnit



[<Fact>]
let encodingParamsTest () =
    let u =
        EncodingParameters.prepareSessionParameters
            @"z:\recordings\Bob a Bobek - kralici z klobouku, Vecernicek-CT _D-art HD2022-09-29 18-45.mkv"
            "sss.mkv"
            Defaults.defaultConfiguration

    let res =
        u
        |> Result.map (EncodingParameters.setOverwriteTarget true)
        |> Result.bind (
            (Encoder.encode (fun _ -> ()))
            >> Async.RunSynchronously
        )

    match res with
    | Ok o -> o |> should equal 0
    | Error e -> e |> should equal null

    ()

let formatTest () =
    let f =
        EncodingParameters.getFormats Defaults.defaultConfiguration

    match f with
    | Ok o -> o |> should not' (equal List.empty<FormatType>)
    | Error e -> e |> should equal null
