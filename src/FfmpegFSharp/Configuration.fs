namespace FfmpegFSharp


type FfmpegOptions =
    { ffmpegPath: string
      ffprobePath: string }


module Defaults =

    let defaultConfiguration =
        { ffmpegPath = "ffmpeg"
          ffprobePath = "ffprobe" }
