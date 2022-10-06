module Tests

open FfmpegFSharp
open Xunit

[<Fact>]
let ``My test`` () =
    let res =
        MediaFileInfo.readJsonAsync
            MediaFileInfoItems.VersionsAndBasicMedia
            @"z:\recordings\Jezevec Chrujda, Vecernicek-CT _D-art HD2022-09-04 18-45.mkv"
        |> Async.RunSynchronously

    res

[<Fact>]
let Test2 () =
    let res =
        MediaFileInfo.readJsonAsync
            MediaFileInfoItems.VersionsAndBasicMedia
            @"z:\recordings\Jezevec Chrujda, Vecernicek-CT _D-art HD2022-09-04 18-45.mkv"
        |> Async.RunSynchronously

    match res with
    | Ok a ->
        let d = MediaFileInfo.prepareInfo a
        0
    | _ -> 0

let mapAsync f op =
    async {
        let! x = op
        let value = f x
        return value
    }

[<Fact>]
let TestGlobal () =
    let files =
        System.IO.Directory.GetFiles(@"z:\recordings\", "*.mka")

    let p =
        files
        |> Seq.map (
            MediaFileInfo.readJsonAsync MediaFileInfoItems.VersionsAndBasicMedia
            >> mapAsync (Result.map (MediaFileInfo.prepareInfo))
        )

    let res =
        Async.Parallel(p, 5) |> Async.RunSynchronously

    ()
