namespace FfmpegFSharp.JsonConvertors

open System
open System.Collections.Generic
open System.Globalization
open System.Text.Json
open System.Text.Json.Serialization

type IMyJsonConvertor<'TSource, 'TTarget> =
    abstract member ConvertFrom: 'TSource -> 'TTarget option
    abstract member ConvertTo: 'TTarget -> 'TSource

type JsonCustomConverter<'TSource, 'TTarget, 'TConverter
    when 'TConverter: (new: unit -> 'TConverter) and 'TConverter :> IMyJsonConvertor<'TSource, 'TTarget>>() =
    inherit JsonConverter<'TTarget>()
    let c = new 'TConverter()

    override this.Read(reader, _, options) =
        let param =
            match reader.TokenType with
            | JsonTokenType.Null -> failwith "needed data not supplied"
            | _ -> (JsonSerializer.Deserialize(&reader, typeof<'TSource>, options) :?> 'TSource)

        match c.ConvertFrom param with
        | None -> failwith $"failed to convert value {param}"
        | Some s -> s


    override this.Write(writer, value, options) =
        JsonSerializer.Serialize(writer, c.ConvertTo value, options)


type JsonCustomOptionConverter<'TSource, 'TTarget, 'TConverter
    when 'TConverter: (new: unit -> 'TConverter) and 'TConverter :> IMyJsonConvertor<'TSource, 'TTarget>>() =
    inherit JsonConverter<'TTarget option>()
    let c = new 'TConverter()


    override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        let param =
            match reader.TokenType with
            | JsonTokenType.Null -> None
            | _ -> Some (JsonSerializer.Deserialize(&reader, typeof<'TSource>, options) :?> 'TSource)

        Option.map c.ConvertFrom param |> Option.flatten


    override this.Write(writer: Utf8JsonWriter, value: 'TTarget option, options: JsonSerializerOptions) =
        match value with
        | None -> ()
        | Some v -> JsonSerializer.Serialize(writer, c.ConvertTo v, options)


type IntBoolOptionConvertor() =
    interface IMyJsonConvertor<int, bool> with
        member this.ConvertFrom(var0) = Some(var0 <> 0)
        member this.ConvertTo(var0) = if var0 then 1 else 0

type StringIntConvertor() =

    let cast (d: string) =
        match Int32.TryParse(d) with
        | false, _ -> None
        | true, d -> Some d

    interface IMyJsonConvertor<string, int> with
        member this.ConvertFrom(var0) = cast var0
        member this.ConvertTo(var0) = $"%i{var0}"

type StringLongConvertor() =
    let cast (d: string) =
        match Int64.TryParse(d) with
        | false, _ -> None
        | true, d -> Some d

    interface IMyJsonConvertor<string, int64> with
        member this.ConvertFrom(var0) = cast var0
        member this.ConvertTo(var0) = $"%i{var0}"



type StringFloatConvertor() =
    let cast (d: string) =
        match Double.TryParse(d) with
        | false, _ -> None
        | true, d -> Some d

    interface IMyJsonConvertor<string, float> with
        member this.ConvertFrom(var0) = cast var0
        member this.ConvertTo(var0) = $"%f{var0}"

type StringTimeSpanConverter() =
    interface IMyJsonConvertor<string, TimeSpan> with
        member this.ConvertFrom(var0) =
            match Decimal.TryParse(var0, NumberStyles.Number, NumberFormatInfo.InvariantInfo) with
            | false, _ -> None
            | true, v ->
                let ticks = (decimal TimeSpan.TicksPerSecond) * v

                Some(TimeSpan.FromTicks(int64 ticks))

        member this.ConvertTo(var0) =
            $"%M{(decimal var0.Ticks) / (decimal TimeSpan.TicksPerSecond)}"



type OptionValueConverter<'T>() =
    inherit JsonConverter<'T option>()

    override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        match reader.TokenType with
        | JsonTokenType.Null -> None
        | _ -> Some <| JsonSerializer.Deserialize<'T>(&reader, options)

    override this.Write(writer: Utf8JsonWriter, value: 'T option, options: JsonSerializerOptions) =
        match value with
        | None -> writer.WriteNullValue()
        | Some value -> JsonSerializer.Serialize(writer, value, options)



type OptionConverter() =
    inherit JsonConverterFactory()

    override this.CanConvert(t: Type) : bool =
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<_>>

    override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
        let typ = typeToConvert.GetGenericArguments() |> Array.head

        let converterType = typedefof<OptionValueConverter<_>>.MakeGenericType(typ)

        Activator.CreateInstance(converterType) :?> JsonConverter

type ListValueConverter<'T>() =
    inherit JsonConverter<'T list>()

    override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        JsonSerializer.Deserialize<'T seq>(&reader, options) |> List.ofSeq

    override this.Write(writer: Utf8JsonWriter, value: 'T list, options: JsonSerializerOptions) =
        JsonSerializer.Serialize(writer, (List.toSeq value), options)

// Instantiates the correct ListValueConverter<T>
type ListConverter() =
    inherit JsonConverterFactory()

    override this.CanConvert(t: Type) : bool =
        t.IsGenericType
        && List.contains (t.GetGenericTypeDefinition()) [ typedefof<list<_>>; typedefof<IReadOnlyCollection<_>> ]

    override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
        let typArgs = typeToConvert.GetGenericArguments()

        let converterType = typedefof<ListValueConverter<_>>.MakeGenericType(typArgs)

        Activator.CreateInstance(converterType) :?> JsonConverter

type MapValueConverter<'K, 'V when 'K: comparison>() =
    inherit JsonConverter<Map<'K, 'V>>()

    override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
        JsonSerializer.Deserialize<Dictionary<'K, 'V>>(&reader, options)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    override this.Write(writer: Utf8JsonWriter, value: Map<'K, 'V>, options: JsonSerializerOptions) =
        let dictionary = Dictionary<'K, 'V>()

        value |> Map.iter (fun k v -> dictionary.Add(k, v))

        JsonSerializer.Serialize(writer, dictionary, options)

// Instantiates the correct MapValueConverter<T>
type MapConverter() =
    inherit JsonConverterFactory()

    override this.CanConvert(t: Type) : bool =
        t.IsGenericType
        && List.contains (t.GetGenericTypeDefinition()) [ typedefof<Map<_, _>>; typedefof<IDictionary<_, _>> ]

    override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
        let typArgs = typeToConvert.GetGenericArguments()

        let converterType = typedefof<MapValueConverter<_, _>>.MakeGenericType(typArgs)

        Activator.CreateInstance(converterType) :?> JsonConverter
