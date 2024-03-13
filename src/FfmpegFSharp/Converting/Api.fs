namespace FfmpegFSharp.Converting

open System
open FfmpegFSharp


type FfmpegEncodingSession = { run: unit -> unit }



type CodecTypeEnum =
    | VideoCodec = 'V'
    | AudioCodec = 'A'
    | SubtitleCodec = 'S'
    | DataCodec = 'D'
    | AttachmentCodec = 'T'

type CodecType =
    { code: string
      name: string
      decodingSupported: bool
      encodingSupported: bool
      codecType: CodecTypeEnum
      intraFrameOnly: bool
      lossyCompressionSupport: bool
      losslessCompressionSupport: bool }

type FormatType =
    { code: string
      name: string
      encode: bool
      decode: bool }

type Prefixes =
    | y = -24
    | z = -21
    | a = -18
    | f = -15
    | p = -12
    | n = -9
    | u = -6
    | m = -3
    | c = -2
    | d = -1
    | x = 1
    | h = 2
    | k = 3
    | K = 3
    | M = 6
    | G = 9
    | T = 12
    | P = 15
    | E = 18
    | Z = 21
    | Y = 24

type decimalUnit = decimal * Prefixes

type BitrateOptionsType =
    { bitrate: decimalUnit option
      maxBitrate: decimalUnit option
      bufsize: decimalUnit option
      minBitrate: decimalUnit option }

type FfmpegEncodingSessionParameters =
    { options: FfmpegOptions
      intputFile: string
      outputFile: string
      mediaFileInfo: MediaFileInfoType
      videoCodec: string option
      videoBitrate: BitrateOptionsType option
      audioCodec: string option
      audioBitrate: BitrateOptionsType option
      seekTime: TimeSpan option
      duration: TimeSpan option
      supportedCodecs: CodecType list
      customCommandlineOptions: string option
      overwriteTarget: bool
      metadata: Map<string, string> }

type ProgressType =
    { frame: int option
      time: string option
      bitrate: string option
      dup: int option
      drop: int option
      speed: string option
      fps: decimal option
      q: decimal option
      size: string option }
