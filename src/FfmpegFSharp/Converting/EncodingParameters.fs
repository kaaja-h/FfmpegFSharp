module FfmpegFSharp.Converting.EncodingParameters

open System
open System.Text.RegularExpressions
open FfmpegFSharp
open FfmpegFSharp.Converting



let private validateInputFile (file: string) =
    if (file.StartsWith("http://") || file.StartsWith("https://")) then
        Ok file
    elif not (System.IO.File.Exists file) then
        Error "file not exists"
    else
        Ok file

let private prepareCodec (flags: string) code name =
    { code = code
      name = name
      decodingSupported = flags[0] = 'D'
      encodingSupported = flags[1] = 'E'
      codecType =
        match flags[2] with
        | 'A' -> CodecTypeEnum.AudioCodec
        | 'V' -> CodecTypeEnum.VideoCodec
        | 'S' -> CodecTypeEnum.SubtitleCodec
        | 'D' -> CodecTypeEnum.DataCodec
        | 'T' -> CodecTypeEnum.AttachmentCodec
        | _ -> failwith "uknnown codec"
      intraFrameOnly = flags[3] = 'I'
      lossyCompressionSupport = flags[4] = 'L'
      losslessCompressionSupport = flags[5] = 'S' }


let private parseCodecs (data: string) =
    let r = Regex(@"^ (.{6}) ([^=][^\ ]*)\ *(.+)")

    let res =
        data.Split(Environment.NewLine)
        |> Seq.map r.Match
        |> Seq.filter (fun d -> d.Success)
        |> Seq.map (fun d -> prepareCodec d.Groups[1].Value d.Groups[2].Value d.Groups[3].Value)
        |> Seq.toList

    res

let private parseFormats (data: string) =
    let r = Regex(@"^ (.{2}) ([^=][^\ ]*)\ *(.+)")

    data.Split(Environment.NewLine)
    |> Array.map r.Match
    |> Array.filter (fun d -> d.Success)
    |> Array.map (fun d ->
        { code = d.Groups[2].Value.Trim()
          name = d.Groups[3].Value.Trim()
          encode = d.Groups[1].Value[1..1] = "E"
          decode = d.Groups[1].Value[0..0] = "D" })
    |> Seq.toList

let getCodecs =
    let dict =
        new System.Collections.Concurrent.ConcurrentDictionary<FfmpegOptions, Result<CodecType list, string>>()

    let creator =
        fun (o: FfmpegOptions) ->
            MediaFileInfo.runFFprobe o.ffmpegPath "-codecs"
            |> Async.RunSynchronously
            |> Result.map parseCodecs

    fun (options: FfmpegOptions) -> dict.GetOrAdd(options, creator)


let getFormats =
    let dict =
        new System.Collections.Concurrent.ConcurrentDictionary<FfmpegOptions, Result<FormatType list, string>>()

    let creator =
        fun (o: FfmpegOptions) ->
            MediaFileInfo.runFFprobe o.ffmpegPath "-formats"
            |> Async.RunSynchronously
            |> Result.map parseFormats

    fun (options: FfmpegOptions) -> dict.GetOrAdd(options, creator)




let prepareSessionParametersWithOptions (options: FfmpegOptions) (inputFile: string) (outputFile: string) =
    validateInputFile inputFile
    |> Result.bind (MediaFileInfo.readData MediaFileInfoItems.VersionsAndBasicMedia)
    |> Result.bind (fun mediaFileInfo ->
        getCodecs options
        |> Result.map (fun supportedCodecs -> (supportedCodecs, mediaFileInfo)))
    |> Result.map (fun (supportedCodecs, mediaFileInfo) ->
        { options = options
          intputFile = inputFile
          outputFile = outputFile
          mediaFileInfo = mediaFileInfo
          videoCodec = None
          audioCodec = None
          audioBitrate = None
          videoBitrate = None
          seekTime = None
          duration = None
          supportedCodecs = supportedCodecs
          customCommandlineOptions = None
          overwriteTarget = false
          metadata = Map.empty })

let prepareSessionParameters =
    prepareSessionParametersWithOptions Defaults.defaultConfiguration


let setSeekTime (seek: TimeSpan) (parameters: FfmpegEncodingSessionParameters) =
    Ok { parameters with seekTime = Some seek }

let setDuration (duration: TimeSpan) (parameters: FfmpegEncodingSessionParameters) =
    Ok
        { parameters with
            duration = Some duration }

let setAudioCodec codec (parameters: FfmpegEncodingSessionParameters) =
    if
        codec = "copy"
        || parameters.supportedCodecs
           |> List.exists (fun c -> c.code = codec && c.codecType = CodecTypeEnum.AudioCodec)
    then
        Ok
            { parameters with
                audioCodec = Some codec }
    else
        Error "codec not supported"

let setMetadata metadata (parameters: FfmpegEncodingSessionParameters) =
    Ok { parameters with metadata = metadata }

let setVideoCodec codec (parameters: FfmpegEncodingSessionParameters) =
    if
        codec = "copy"
        || parameters.supportedCodecs
           |> List.exists (fun c -> c.code = codec && c.codecType = CodecTypeEnum.VideoCodec)
    then
        Ok
            { parameters with
                videoCodec = Some codec }
    else
        Error "codec not supported"

let setOverwriteTarget overwrite (parameters: FfmpegEncodingSessionParameters) =
    { parameters with
        overwriteTarget = overwrite }

let setCustomParameters customParameters (parameters: FfmpegEncodingSessionParameters) =
    { parameters with
        customCommandlineOptions = customParameters }

let setVideoBitrateOptions bitrate maxBitrate minBitrate bufsize (parameters: FfmpegEncodingSessionParameters) =
    { parameters with
        videoBitrate =
            Some
                { bitrate = bitrate
                  maxBitrate = maxBitrate
                  bufsize = bufsize
                  minBitrate = minBitrate } }

let setAudioBitrateOptions bitrate maxBitrate minBitrate bufsize (parameters: FfmpegEncodingSessionParameters) =
    { parameters with
        audioBitrate =
            Some
                { bitrate = bitrate
                  maxBitrate = maxBitrate
                  bufsize = bufsize
                  minBitrate = minBitrate } }
