module FfmpegFSharp.Converting.Encoder

open System
open System.Diagnostics
open System.Globalization
open FfmpegFSharp


type private ChangeController<'T when 'T: equality>(handler: 'T -> unit) =
    let mutable data: 'T option = None

    let notify (v) = async { handler v } |> Async.Start

    member this.next v =
        match data with
        | None ->
            data <- Some v
            notify v
        | Some c when c <> v ->
            data <- Some v
            notify v
        | _ -> ()

let d = Environment.OSVersion.Platform

let private formatTimespan (time: TimeSpan) = time.ToString(@"hh\:mm\:ss\.fff")

let private bitrateCommandlineOptions (t: string) (opts: BitrateOptionsType) =
    seq {
        match opts.bitrate with
        | None -> ()
        | Some (v, u) -> yield $"-b:%s{t} %f{v}%A{u}"

        match opts.bufsize with
        | None -> ()
        | Some (v, u) -> yield $"-bufsize:%s{t} %f{v}%A{u}"

        match opts.maxBitrate with
        | None -> ()
        | Some (v, u) -> yield $"-maxrate:%s{t} %f{v}%A{u}"

        match opts.minBitrate with
        | None -> ()
        | Some (v, u) -> yield $"-minrate:%s{t} %f{v}%A{u}"
    }

let private prepareCommandlineParameters (parameters: FfmpegEncodingSessionParameters) =
    seq {
        if (parameters.seekTime |> Option.isSome) then
            yield
                parameters.seekTime
                |> Option.get
                |> formatTimespan
                |> sprintf "-ss %s"

        yield $"-i \"%s{parameters.intputFile}\""

        if (parameters.customCommandlineOptions
            |> Option.isSome) then
            yield parameters.customCommandlineOptions |> Option.get

        if (parameters.duration |> Option.isSome) then
            yield
                parameters.duration
                |> Option.get
                |> formatTimespan
                |> sprintf "-t %s"

        if parameters.overwriteTarget then
            yield "-y"

        if (parameters.audioCodec |> Option.isSome) then
            yield $"-codec:a %s{parameters.audioCodec.Value}"

        if (parameters.audioBitrate |> Option.isNone) then
            yield! bitrateCommandlineOptions "a" (parameters.audioBitrate |> Option.get)


        if (parameters.videoCodec |> Option.isSome) then
            yield $"-codec:v %s{parameters.videoCodec.Value}"

        if (parameters.videoBitrate |> Option.isNone) then
            yield! bitrateCommandlineOptions "v" (parameters.videoBitrate |> Option.get)

        yield "-hide_banner"
        yield "-loglevel info"

        yield $"\"%s{parameters.outputFile}\""
    }
    |> String.concat " "

let private findErrors (parameters: FfmpegEncodingSessionParameters) =
    seq {
        if not (System.IO.File.Exists parameters.intputFile) then
            yield "Input file not exists"

        if (not parameters.overwriteTarget)
           && (System.IO.File.Exists parameters.outputFile) then
            yield "output file already exists"
    }

let private validate (parameters: FfmpegEncodingSessionParameters) =
    match findErrors parameters |> Seq.tryHead with
    | Some e -> Error e
    | _ -> Ok(parameters: FfmpegEncodingSessionParameters)


let private parseValue name (data: string) =
    let reg = $"%s{name}=\\ *([^\\ ]+)\\ "

    match data with
    | Utils.Regex reg [ res ] -> Some res
    | _ -> None

let private TryOption parser d =
    match parser d with
    | true, l -> Some l
    | _ -> None

let parseDecimal (str: string) =
    match System.Decimal.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture) with
    | (true, result) -> Some result
    | (false, _) -> None



let private parseProgress (data: string) =
    if data.StartsWith("frame=") then
        Some
            { frame =
                parseValue "frame" data
                |> Option.map (TryOption System.Int32.TryParse)
                |> Option.flatten
              fps =
                parseValue "fps" data
                |> Option.map (parseDecimal)
                |> Option.flatten
              q =
                parseValue "q" data
                |> Option.map (parseDecimal)
                |> Option.flatten
              size = parseValue "size" data
              time = parseValue "time" data
              bitrate = parseValue "bitrate" data
              dup =
                parseValue "dup" data
                |> Option.map (TryOption System.Int32.TryParse)
                |> Option.flatten
              drop =
                parseValue "drop" data
                |> Option.map (TryOption System.Int32.TryParse)
                |> Option.flatten
              speed = parseValue "speed" data }
    else
        None



let private run (options: FfmpegOptions) progressHandler commandlineParameters =
    async {
        let startInfo =
            new ProcessStartInfo(fileName = options.ffmpegPath, arguments = commandlineParameters)

        startInfo.RedirectStandardOutput <- false
        startInfo.RedirectStandardError <- true
        startInfo.UseShellExecute <- false
        startInfo.CreateNoWindow <- true
        startInfo.RedirectStandardInput <- false
        let ffmpegProcess = new Process()
        ffmpegProcess.StartInfo <- startInfo

        let change =
            ChangeController(progressHandler)

        ffmpegProcess.ErrorDataReceived.Add(fun data -> data.Data |> parseProgress |> change.next)

        if not (ffmpegProcess.Start()) then
            return Error "Process not started"
        else
            ffmpegProcess.BeginErrorReadLine()

            do!
                ffmpegProcess.WaitForExitAsync()
                |> Async.AwaitTask

            return Ok ffmpegProcess.ExitCode
    }


let asyncResultMap f r =
    async {
        match r with
        | Error e -> return Error e
        | Ok o -> return! (f o)
    }

let encode progressHandler (parameters: FfmpegEncodingSessionParameters) =
    validate parameters
    |> Result.map prepareCommandlineParameters
    |> asyncResultMap (run parameters.options progressHandler)
