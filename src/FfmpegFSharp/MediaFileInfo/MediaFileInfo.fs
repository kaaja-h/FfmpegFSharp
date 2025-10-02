module FfmpegFSharp.MediaFileInfo

open System.Diagnostics
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.Core
open FfmpegFSharp
open FfmpegFSharp.JsonConvertors


let private jsonFactories () : JsonConverterFactory list =
    [ MapConverter()
      OptionConverter()
      ListConverter()
      SimpleTypeConverterFactory<_, StreamValueConverter>()
      SimpleTypeConverterFactory<_, ChapterTypeValueConverter>() ]


let private prepareJsonOptions () =
    let res = JsonSerializerOptions()
    let factories = jsonFactories ()

    factories |> List.fold (fun _ c -> res.Converters.Add(c)) ()

    res

let private jsonOptions = lazy (prepareJsonOptions ())


let prepareInfo (stringJsonResult: string) =
    let data =
        JsonSerializer.Deserialize<MediaFileInfoType>(stringJsonResult, jsonOptions.Force())

    data

let prepareInfoResult (stringJsonResult: string) =
    try
        Ok(prepareInfo stringJsonResult)
    with e ->
        Error $"Error parsing text\n%s{stringJsonResult}\n%s{e.ToString()}"



let private checkFilename (filename: string) =
    if (filename.StartsWith("http://") || filename.StartsWith("https://")) then
        Ok(filename)
    else
        let fi = FileInfo filename

        if not fi.Exists then
            Error $"file %s{filename} does not exists"
        else
            Ok(fi.FullName)


let private commandLineFlags =
    lazy
        Map[MediaFileInfoItems.Format, "-show_format"
            MediaFileInfoItems.Streams, "-show_streams"
            MediaFileInfoItems.Programs, "-show_programs"
            MediaFileInfoItems.Chapters, "-show_chapters"
            MediaFileInfoItems.ProgramVersion, "-show_program_version"
            MediaFileInfoItems.LibraryVersions, "-show_library_versions"
            MediaFileInfoItems.Frames, "-show_frames"
            MediaFileInfoItems.Packets, "-show_packets"]

let private commandLineArguments flags =
    commandLineFlags.Force()
    |> Map.filter (fun k _ -> k &&& flags = k)
    |> Map.values
    |> Seq.toList


let internal runFFprobe ffprobePath args =
    async {
        let startInfo = ProcessStartInfo(fileName = ffprobePath, arguments = args)

        startInfo.RedirectStandardOutput <- true
        startInfo.RedirectStandardError <- true
        startInfo.UseShellExecute <- false
        startInfo.CreateNoWindow <- false
        startInfo.RedirectStandardInput <- false
        startInfo.StandardOutputEncoding <- UTF8Encoding()
        
        let ffprobeProcess = new Process()
        ffprobeProcess.StartInfo <- startInfo
        if not (ffprobeProcess.Start()) then
            return (Error "Process not started")
        else

            let resultAsync = ffprobeProcess.StandardOutput.ReadToEndAsync() |> Async.AwaitTask

            let errorAsync = ffprobeProcess.StandardError.ReadToEndAsync() |> Async.AwaitTask

            let! outputs = [ resultAsync; errorAsync ] |> Async.Parallel
            ffprobeProcess.WaitForExit()
            
            
            return
                match outputs with
                | [| ""; "" |] -> Error "Invalid ffmpeg output"
                | [| ""; error |] -> Error $"Invalid ffmpeg output \n %s{error}"
                | [| result; _ |] -> Ok result
                | _ -> Error "unknown ffmpeg error"
    }

let private runFFProbeProcess ffprobePath flags filename =
    let args =
        [ "-of"; "json=c=1"; "-v"; "error"; "-private" ]
        @ (commandLineArguments flags)
        @ [ $"\"%s{filename}\"" ]
        |> String.concat " "

    runFFprobe ffprobePath args


let private mapAsync f op =
    async {
        let! x = op
        let value = f x
        return value
    }

let private mapResult (f: 'T -> Result<'K, 'E>) (d: Result<'T, 'E>) : Result<'K, 'E> =
    match d with
    | Error e -> Error e
    | Ok v -> f v

let private pipeProcessAsync d =
    d |> mapAsync (mapResult prepareInfoResult)


let readJsonWithOptionsAsync options (flags: MediaFileInfoItems) filename =
    async {
        match checkFilename filename with
        | Error errorValue -> return Error(errorValue)
        | Ok normalizedFilename ->
            let! res = runFFProbeProcess options.ffprobePath flags normalizedFilename
            return res
    }

let readDataWithOptionsAsync options flags filename =
    readJsonWithOptionsAsync options flags filename |> pipeProcessAsync

let readJsonWithOptions options flags filename =
    readJsonWithOptionsAsync options flags filename |> Async.RunSynchronously

let readDataWithOptions options flags filename =
    readDataWithOptionsAsync options flags filename |> Async.RunSynchronously

let readJsonAsync = readJsonWithOptionsAsync Defaults.defaultConfiguration

let readDataAsync flags filename =
    readJsonAsync flags filename |> pipeProcessAsync

let readJson flags filename =
    readJsonAsync flags filename |> Async.RunSynchronously

let readData flags filename =
    readDataAsync flags filename |> Async.RunSynchronously
