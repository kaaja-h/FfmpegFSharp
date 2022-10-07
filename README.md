# FfmpegFSharp
F# library for running ffmpeg

# Features


### Media file information reading
Basic ussage:
```f#
// set flags which data read
let flags = MediaFileInfoItems.VersionsAndBasicMedia

let informations = MediaFileInfo.readData flags "path_to_file"
```
Read data async
```f#
async {
    let! informations = MediaFileInfo.readDataAsync flags "path_to_file"
} 
```

Set path to FFProbe and FFMpeg

```f#
let options = {
    ffmpegPath = "path_to_ffmpeg_executable"
    ffprobePath = "path_to_ffprobe_executable"
}
let informations = MediaFileInfo.readDataWithOptions options flags "path_to_file"
```

### File encoding
Runs ffmpeg encoding session
Still in massive development and api changes

1. Setup encoding session parameters
```F#
let sessionParameters = EncodingParameters.prepareSessionParameters "source_file" "target_file"
                        |> Result.bind (EncodingParameters.setSeekTime someTimespanToSeek)
                        |> Result.bind (EncodingParameters.setDuration someTimespandDutarion)
                        |> Result.bind (EncodingParameters.setVideoCodec "someCodecName")
                        ......
                        
```
2. Start encoding session
```F#
    sessionParameters |> Result.bind ((Encoder.encode (someProgressHandlingFuncions)) >> Async.RunSynchronously)
```

## TODO
- stabilize encoding API
- make better Docs
- prepare tests