"""Master generated narration lines: trim silence, slight speed-up, gentle polish, even loudness.

usage: master_voice.py DIR [TEMPO]
Reads DIR/raw_NNN.mp3 and writes DIR/NNN.wav (48 kHz mono, -16 LUFS integrated, -1.5 dBTP).
"""
import glob
import json
import os
import re
import subprocess
import sys

d = sys.argv[1]
tempo = float(sys.argv[2]) if len(sys.argv) > 2 else 1.05

POLISH = ("silenceremove=start_periods=1:start_threshold=-50dB:start_silence=0.05,"
          "areverse,silenceremove=start_periods=1:start_threshold=-50dB:start_silence=0.08,areverse,"
          "atempo=%.3f,highpass=f=70,equalizer=f=160:width_type=o:w=1.2:g=1.5,"
          "equalizer=f=400:width_type=o:w=1:g=-1.5,deesser=i=0.6:m=0.5:f=0.27:s=o,"
          "acompressor=threshold=-22dB:ratio=2.5:attack=8:release=160:makeup=1.5:knee=4" % tempo)

for raw in sorted(glob.glob(os.path.join(d, "raw_*.mp3"))):
    n = re.search(r"raw_(\d+)", raw).group(1)
    out = os.path.join(d, "%s.wav" % n)
    first = subprocess.run(["ffmpeg", "-hide_banner", "-i", raw, "-af", POLISH + ",loudnorm=I=-16:TP=-1.5:LRA=7:print_format=json",
                            "-f", "null", "-"], stderr=subprocess.PIPE, text=True).stderr
    m = json.loads(first[first.rindex("{"):first.rindex("}") + 1])
    second = ("loudnorm=I=-16:TP=-1.5:LRA=7:measured_I=%s:measured_TP=%s:measured_LRA=%s:measured_thresh=%s:offset=%s:linear=true"
              % (m["input_i"], m["input_tp"], m["input_lra"], m["input_thresh"], m["target_offset"]))
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", raw, "-af", POLISH + "," + second + ",aresample=48000",
                    "-ac", "1", "-c:a", "pcm_s16le", out], check=True)
    dur = float(subprocess.check_output(["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", out]))
    print("%s %.2fs" % (out, dur))
