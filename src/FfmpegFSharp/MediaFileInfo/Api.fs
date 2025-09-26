namespace FfmpegFSharp

open System
open System.Text.Json
open System.Text.Json.Serialization
open FfmpegFSharp.JsonConvertors



type ProgramVersionType =
    { version: string
      copyright: string
      [<JsonPropertyName("compiler_ident")>]
      compilerIdent: string
      configuration: string
      [<JsonPropertyName("build_date")>]
      buildDate: string option
      [<JsonPropertyName("build_time")>]
      buildTime: string Option }

type LibraryVersionType =
    { name: string
      major: int
      minor: int
      micro: int
      version: int
      ident: string }

type PixelFormatFlagsType =
    { [<JsonPropertyName("big_endian")>]
      bigEndian: int
      palette: int
      bitstream: int
      hwaccel: int
      planar: int
      rgb: int
      alpha: int }

type PixelFormatComponentType =
    { index: int
      [<JsonPropertyName("bit_depth")>]
      bitDepth: int }


type PixelFormatType =
    { name: string
      [<JsonPropertyName("nb_components")>]
      nbComponents: int
      flags: PixelFormatFlagsType option
      components: PixelFormatComponentType list
      [<JsonPropertyName("log2_chroma_w")>]
      log2ChomaW: int option
      [<JsonPropertyName("log2_chroma_h")>]
      log2ChomaH: int option
      [<JsonPropertyName("bits_per_pixel")>]
      bitsPerPixel: int option }


type TagType = Map<string, string>

type StreamDispositionType =
    { ``default``: int
      dub: int
      original: int
      comment: int
      lyrics: int
      karaoke: int
      forced: int
      [<JsonPropertyName("hearing_impaired")>]
      hearingImpaired: int
      [<JsonPropertyName("visual_impaired")>]
      visualImpaired: int
      [<JsonPropertyName("clean_effects")>]
      cleanEffects: int
      [<JsonPropertyName("attached_pic")>]
      attachedPic: int
      [<JsonPropertyName("timed_thumbnails")>]
      timedThumbnails: int
      captions: int
      descriptions: int
      metadata: int
      dependent: int
      [<JsonPropertyName("still_image")>]
      stillImage: int }

type CommonStreamDataType =
    { index: int
      tags: TagType
      disposition: StreamDispositionType option
      [<JsonPropertyName("codec_type")>]
      codecType: string option
      [<JsonPropertyName("codec_name")>]
      codecName: string option
      [<JsonPropertyName("codec_long_name")>]
      codecLongName: string option
      profile: string option
      [<JsonPropertyName("codec_tag")>]
      codecTag: string
      [<JsonPropertyName("codec_tag_string")>]
      codecTagString: string
      extradata: string option
      [<JsonPropertyName("extradata_size")>]
      extradataSize: int option
      [<JsonPropertyName("extradata_hash")>]
      extradataHash: string option
      id: string option
      [<JsonPropertyName("r_frame_rate")>]
      rFrameRate: string
      [<JsonPropertyName("avg_frame_rate")>]
      avgFrameRate: string
      [<JsonPropertyName("time_base")>]
      timeBase: string option
      [<JsonPropertyName("start_pts")>]
      startPts: int64 option
      [<JsonPropertyName("start_time")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringFloatConvertor>>)>]
      startTime: float option
      [<JsonPropertyName("duration_ts")>]
      durationTs: int64 option
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringFloatConvertor>>)>]
      duration: float option
      [<JsonPropertyName("bit_rate")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringIntConvertor>>)>]
      bitRate: int option
      [<JsonPropertyName("max_bit_rate")>]
      max_bit_rate: int option
      [<JsonPropertyName("bits_per_raw_sample")>]
      bitsPerRawSample: int option
      [<JsonPropertyName("nb_frames")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringIntConvertor>>)>]
      nbFrames: int option
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringIntConvertor>>)>]
      [<JsonPropertyName("nb_read_frames")>]
      nbReadFrames: int option
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringIntConvertor>>)>]
      [<JsonPropertyName("nb_read_packets")>]
      nbReadPackets: int option }

type VideoStreamData =
    { width: int
      height: int
      [<JsonPropertyName("coded_width")>]
      codedWidth: int
      [<JsonPropertyName("coded_height")>]
      codedHeight: int
      [<JsonPropertyName("closed_captions")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<int, bool, IntBoolOptionConvertor>>)>]
      closedCaptions: bool option
      [<JsonPropertyName("film_grain")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<int, bool, IntBoolOptionConvertor>>)>]
      filmGrain: bool option
      [<JsonPropertyName("has_b_frames")>]
      hasBFrames: int option
      [<JsonPropertyName("sample_aspect_ratio")>]
      sampleAspectRatio: string option
      [<JsonPropertyName("display_aspect_ratio")>]
      displayAspectRatio: string option
      [<JsonPropertyName("pix_fmt")>]
      pixFmt: string option
      level: int option
      [<JsonPropertyName("color_range")>]
      colorRange: string option
      [<JsonPropertyName("color_space")>]
      colorSpace: string option
      [<JsonPropertyName("color_transfer")>]
      colorTransfer: string option
      [<JsonPropertyName("color_primaries")>]
      colorPrimaries: string option
      [<JsonPropertyName("chroma_location")>]
      chromaLocation: string option
      [<JsonPropertyName("field_order")>]
      fieldOrder: string option
      refs: int option }

type AudioStreamData =
    { channels: int
      [<JsonPropertyName("sample_fmt")>]
      sampleFmt: string option
      [<JsonPropertyName("sample_rate")>]
      sampleRate: string option
      [<JsonPropertyName("channel_layout")>]
      channelLayout: string option
      [<JsonPropertyName("bits_per_sample")>]
      bitsPerSample: int option
      [<JsonPropertyName("initial_padding")>]
      initialPadding: int option

    }

type StreamData =
    | AudioStream of AudioStreamData
    | VideoStream of VideoStreamData
    | Subtitle
    | Unknown

type StreamType =
    { commonData: CommonStreamDataType
      data: StreamData }


type FormatType =
    { tags: TagType
      filename: string
      [<JsonPropertyName("nb_streams")>]
      nbStreams: int
      [<JsonPropertyName("nb_programs")>]
      nbPrograms: int
      [<JsonPropertyName("format_name")>]
      formatName: string
      [<JsonPropertyName("format_long_name")>]
      formatNameLong: string option
      [<JsonPropertyName("start_time")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringTimeSpanConverter>>)>]
      startTime: TimeSpan option
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringTimeSpanConverter>>)>]
      duration: TimeSpan option
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringLongConvertor>>)>]
      size: int64 option
      [<JsonPropertyName("bit_rate")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringLongConvertor>>)>]
      bitRate: int64 option
      [<JsonPropertyName("probe_score")>]
      probe_score: int option }

type ProgramType =
    { tags: TagType
      streams: StreamType list }


type ChapterType =
    { tags: TagType
      id: int
      start: TimeSpan
      ``end``: TimeSpan }

type ChapterTypeJson =
    { tags: TagType
      id: int
      [<JsonPropertyName("time_base")>]
      timeBase: string
      start: int64
      [<JsonPropertyName("start_time")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringFloatConvertor>>)>]
      startTime: float option
      ``end``: int64
      [<JsonPropertyName("end_time")>]
      [<JsonConverter(typeof<JsonCustomOptionConverter<_, _, StringFloatConvertor>>)>]
      endTime: float option }

type ErrorType = { code: int; string: string }

type MediaFileInfoType =
    { [<JsonPropertyName("program_version")>]
      programVersion: ProgramVersionType option
      [<JsonPropertyName("library_versions")>]
      libraryVersions: LibraryVersionType list
      [<JsonPropertyName("pixel_formats")>]
      pixelFormats: PixelFormatType list
      programs: ProgramType list
      streams: StreamType list
      format: FormatType option
      chapters: ChapterType list
      error: ErrorType option }

type SimpleTypeConverterFactory<'T, 'TConverter
    when 'TConverter: (new: unit -> 'TConverter) and 'TConverter :> JsonConverter<'T>>() =
    inherit JsonConverterFactory()

    override this.CreateConverter(_, _) = new 'TConverter()
    override this.CanConvert(typeToConvert) = typeToConvert = typeof<'T>




type StreamValueConverter() =
    inherit JsonConverter<StreamType>()

    override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        let mutable d = reader

        let baseData =
            JsonSerializer.Deserialize(&reader, typeof<CommonStreamDataType>, options) :?> CommonStreamDataType

        let u =
            match baseData.codecType with
            | Some "video" ->
                VideoStream(JsonSerializer.Deserialize(&d, typeof<VideoStreamData>, options) :?> VideoStreamData)
            | Some "audio" ->
                AudioStream(JsonSerializer.Deserialize(&d, typeof<AudioStreamData>, options) :?> AudioStreamData)
            | Some "subtitle" -> Subtitle
            | _ -> Unknown

        { commonData = baseData; data = u }

    override this.Write(writer: Utf8JsonWriter, value: StreamType, options: JsonSerializerOptions) =
        let opt = JsonSerializerOptions(options)

        let currentConvertor =
            opt.Converters
            |> Seq.filter (fun l -> l.GetType() = typedefof<StreamType>)
            |> Seq.tryHead

        match currentConvertor with
        | None -> ()
        | Some c -> opt.Converters.Remove(c) |> ignore

        JsonSerializer.Serialize(writer, value)





type ChapterTypeValueConverter() =
    inherit JsonConverter<ChapterType>()

    let convertTimebase (s: string) =
        match Decimal.TryParse(s) with
        | true, d -> Some d
        | false, _ ->
            let splited = s.Split('/')

            match splited with
            | [| a; b |] ->
                match Decimal.TryParse(a), Decimal.TryParse(b) with
                | (true, aa), (true, bb) -> Some(aa / bb)
                | _ -> None
            | _ -> None

    override this.Read(reader, _, options) =
        let baseData =
            JsonSerializer.Deserialize(&reader, typeof<ChapterTypeJson>, options) :?> ChapterTypeJson

        let tb = convertTimebase baseData.timeBase |> Option.get

        let startMiliseconds = tb * (decimal baseData.start) * 1000M

        let endMiliseconds = tb * (decimal baseData.``end``) * 1000M

        { id = baseData.id
          tags = baseData.tags
          start = TimeSpan.FromMilliseconds(int startMiliseconds)
          ``end`` = TimeSpan.FromMilliseconds(int endMiliseconds) }

    override this.Write(_, _, _) = failwith "todo"


[<Flags>]
type MediaFileInfoItems =
    | Format = 1
    | Streams = 2
    | Programs = 4
    | Chapters = 8
    | ProgramVersion = 16
    | LibraryVersions = 32
    | Versions = 48
    | Frames = 64
    | Packets = 128
    | BasicMedia = 15
    | VersionsAndBasicMedia = 63
    | All = 255
