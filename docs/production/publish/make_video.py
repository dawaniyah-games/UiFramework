"""Edit a recorded tutorial episode into a finished, captioned 1080p video.

Usage (from the repository root):
    python3 docs/production/publish/make_video.py KEY RECORDING MARKS OUT_DIR

KEY        01..10 or showcase (picks the script in docs/production/scripts)
RECORDING  the raw screen recording (any size; scaled and padded to 1920x1080)
MARKS      JSON with the beat times, written while recording:
           {"start": <epoch when recording started>,
            "end": <epoch when it stopped>,
            "beats": {"1": <epoch>, "2": <epoch>, ...},
            "inserts": [{"at": <epoch>, "image": "path.png", "dur": 4}],   optional: stills shown over the video
            "sources": {"1": {"file": "clip.mp4", "from": 0, "to": 12}},   optional: a beat taken from another clip
            "speed": {"2": 4}}                                              optional: force a speed-up for a beat
OUT_DIR    where NN.mp4, NN.srt (final timing) and NN.chapters.txt are written

Each beat becomes one segment. Long waits are sped up (at most 4x) and short beats are held on their last
frame until the caption has had time to be read (140 words a minute). The SAY lines are burned in as
captions, ON SCREEN lines appear as a banner, and every episode gets a title card and an end card.
"""
import json
import os
import re
import subprocess
import sys
import tempfile
import textwrap

ROOT = os.getcwd()
SCRIPTS = os.path.join(ROOT, "docs", "production", "scripts")
WPM = 140.0
FPS = 30
W, H = 1920, 1080
NAVY = "0x12142A"
FONT_DIR = "/usr/share/fonts/truetype/google-fonts"

FILES = {
    "01": "01-install-and-config.md", "02": "02-first-element.md", "03": "03-build-the-screen.md",
    "04": "04-animations.md", "05": "05-validate-and-fix.md", "06": "06-canvas-and-addressables.md",
    "07": "07-states.md", "08": "08-build-runtime-config.md", "09": "09-show-from-code.md", "10": "10-popup.md",
    "showcase": "showcase.md",
}
NAMES = {
    "01": "Install and create the config", "02": "Create your first element", "03": "Build a screen under Root",
    "04": "Choose animations", "05": "Validate and fix scenes", "06": "Canvas and Addressables settings",
    "07": "States and shared elements", "08": "Build the runtime config", "09": "Show states from code",
    "10": "Popups and overlays", "showcase": "The showcase app",
}


def read(path):
    with open(path, encoding="utf-8") as handle:
        return handle.read().replace("\r\n", "\n")


def parse_beats(text):
    beats = []
    for block in re.split(r"\n(?=\*\*\d+\. )", text):
        head = re.match(r"\*\*(\d+)\. (.+?)\*\* \((\d+:\d\d)\)", block)
        if not head:
            continue
        says = re.findall(r'^- SAY: "(.*)"\s*$', block, re.M)
        on_screen = []
        for line in re.findall(r"^- ON SCREEN: (.*)$", block, re.M):
            on_screen.extend(re.findall(r"`([^`]+)`", line))
        beats.append({"n": int(head.group(1)), "title": head.group(2), "say": " ".join(says), "on_screen": on_screen})
    return beats


def words(text):
    return len(re.findall(r"[A-Za-z0-9']+", text))


def read_time(text):
    return words(text) / WPM * 60.0


def run(args):
    result = subprocess.run(args, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        sys.stderr.write(result.stderr[-3000:])
        raise SystemExit("ffmpeg failed: " + " ".join(args[:6]))


def ass_time(value):
    centis = int(round(max(value, 0) * 100))
    hours, centis = divmod(centis, 360000)
    minutes, centis = divmod(centis, 6000)
    secs, centis = divmod(centis, 100)
    return "%d:%02d:%02d.%02d" % (hours, minutes, secs, centis)


def srt_time(value):
    millis = int(round(max(value, 0) * 1000))
    hours, millis = divmod(millis, 3600000)
    minutes, millis = divmod(millis, 60000)
    secs, millis = divmod(millis, 1000)
    return "%02d:%02d:%02d,%03d" % (hours, minutes, secs, millis)


def esc(text):
    return text.replace("\\", "\\\\").replace("{", "(").replace("}", ")")


ASS_HEAD = """[Script Info]
ScriptType: v4.00+
PlayResX: 1920
PlayResY: 1080
WrapStyle: 0

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Cap,Poppins Medium,46,&H00FFFFFF,&H00FFFFFF,&HD82A1412,&HD82A1412,0,0,0,0,100,100,0,0,3,14,0,2,160,160,48,1
Style: Banner,Poppins,56,&H002A1412,&H002A1412,&H003DC8FF,&H003DC8FF,1,0,0,0,100,100,0,0,3,22,0,8,160,160,70,1
Style: Kicker,Poppins Medium,40,&H00D6A6A0,&H00D6A6A0,&H00000000,&H00000000,0,0,0,0,100,100,4,0,1,0,0,5,100,100,0,1
Style: Big,Poppins,104,&H003DC8FF,&H003DC8FF,&H00000000,&H00000000,1,0,0,0,100,100,0,0,1,0,0,5,100,100,0,1
Style: Title,Poppins,84,&H00FFFFFF,&H00FFFFFF,&H00000000,&H00000000,1,0,0,0,100,100,0,0,1,0,0,5,100,100,0,1
Style: Small,Poppins Medium,40,&H00FFFFFF,&H00FFFFFF,&H00000000,&H00000000,0,0,0,0,100,100,0,0,1,0,0,5,100,100,0,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
"""


def chunks(text):
    parts = re.split(r"(?<=[.!?])\s+", text.strip())
    out = []
    for part in parts:
        lines = textwrap.wrap(part, 56)
        for index in range(0, len(lines), 2):
            out.append("\\N".join(esc(line) for line in lines[index:index + 2]))
    return [c for c in out if c]


def caption_events(text, start, end):
    """Spread the caption chunks of one beat over [start, end], weighted by length."""
    pieces = chunks(text)
    if not pieces:
        return []
    weights = [len(p) for p in pieces]
    span = end - start
    cursor = start
    events = []
    for piece, weight in zip(pieces, weights):
        length = span * weight / float(sum(weights))
        events.append((cursor, cursor + length, piece))
        cursor += length
    return events


def card(path, lines, duration, tmp):
    ass = os.path.join(tmp, os.path.basename(path) + ".ass")
    with open(ass, "w", encoding="utf-8") as handle:
        handle.write(ASS_HEAD)
        for style, y, text in lines:
            handle.write("Dialogue: 0,%s,%s,%s,,0,0,0,,{\\fad(250,250)\\pos(960,%d)}%s\n"
                         % (ass_time(0), ass_time(duration), style, y, esc(text)))
    run(["ffmpeg", "-v", "error", "-y", "-f", "lavfi", "-i", "color=c=%s:s=%dx%d:r=%d:d=%.3f" % (NAVY, W, H, FPS, duration),
         "-f", "lavfi", "-i", "anullsrc=r=48000:cl=stereo",
         "-vf", "ass=%s:fontsdir=%s" % (ass, FONT_DIR), "-t", "%.3f" % duration, "-shortest",
         "-c:v", "libx264", "-preset", "veryfast", "-crf", "18", "-pix_fmt", "yuv420p", "-c:a", "aac", "-b:a", "128k", path])


def segment(path, source, start, stop, speed, hold, inserts, events, banner, tmp):
    """Cut [start, stop] of source, overlay stills, speed up, hold the last frame, burn captions."""
    raw_len = stop - start
    out_len = raw_len / speed + hold
    ass = path + ".ass"
    with open(ass, "w", encoding="utf-8") as handle:
        handle.write(ASS_HEAD)
        for a, b, text in events:
            handle.write("Dialogue: 0,%s,%s,Cap,,0,0,0,,{\\fad(120,120)}%s\n" % (ass_time(a), ass_time(b), text))
        for index, text in enumerate(banner):
            a = 0.2 + index * 3.5
            handle.write("Dialogue: 1,%s,%s,Banner,,0,0,0,,{\\fad(200,200)}%s\n"
                         % (ass_time(a), ass_time(min(out_len, a + max(3.5, out_len - a if index == len(banner) - 1 else 3.5))), esc(text)))
    inputs = ["-ss", "%.3f" % start, "-t", "%.3f" % raw_len, "-i", source]
    chain = "[0:v]fps=%d,scale=%d:%d:force_original_aspect_ratio=decrease,pad=%d:%d:(ow-iw)/2:(oh-ih)/2:color=%s,setsar=1,setpts=PTS-STARTPTS[base]" % (
        FPS, W, H, W, H, NAVY)
    last = "base"
    for index, insert in enumerate(inserts):
        inputs += ["-loop", "1", "-t", "%.3f" % raw_len, "-i", insert["image"]]
        at = max(0.0, insert["at"])
        chain += ";[%d:v]scale=%d:%d:force_original_aspect_ratio=decrease,format=rgba[img%d]" % (index + 1, int(W * 0.86), int(H * 0.80), index)
        chain += ";[%s][img%d]overlay=(W-w)/2:(H-h)/2-30:enable='between(t,%.3f,%.3f)'[ov%d]" % (last, index, at, at + insert["dur"], index)
        last = "ov%d" % index
    chain += ";[%s]setpts=PTS/%.4f,tpad=stop_mode=clone:stop_duration=%.3f,ass=%s:fontsdir=%s,format=yuv420p[v]" % (
        last, speed, hold, ass, FONT_DIR)
    run(["ffmpeg", "-v", "error", "-y"] + inputs + ["-f", "lavfi", "-i", "anullsrc=r=48000:cl=stereo",
         "-filter_complex", chain, "-map", "[v]", "-map", "%d:a" % (len(inserts) + 1), "-t", "%.3f" % out_len,
         "-r", str(FPS), "-c:v", "libx264", "-preset", "veryfast", "-crf", "18", "-c:a", "aac", "-b:a", "128k", path])
    return out_len


FREEZES = {}


def freezes(recording):
    """Static stretches of the recording (seconds), found with ffmpeg freezedetect."""
    if recording in FREEZES:
        return FREEZES[recording]
    result = subprocess.run(["ffmpeg", "-hide_banner", "-i", recording, "-vf", "fps=10,freezedetect=n=0.00006:d=1.0",
                             "-map", "0:v", "-f", "null", "-"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    starts = [float(x) for x in re.findall(r"freeze_start: ([\d.]+)", result.stderr)]
    ends = [float(x) for x in re.findall(r"freeze_end: ([\d.]+)", result.stderr)]
    while len(ends) < len(starts):
        ends.append(1e9)
    FREEZES[recording] = list(zip(starts, ends))
    return FREEZES[recording]


def keep_ranges(recording, a, b, keep_still=1.2):
    """[a, b] with every still stretch shortened to keep_still seconds (model think time, waits)."""
    ranges = []
    cursor = a
    for fa, fb in freezes(recording):
        if fb <= a or fa >= b:
            continue
        fa, fb = max(fa, a), min(fb, b)
        cut_from = fa + keep_still
        if fb - cut_from > 0.3:
            if cut_from > cursor:
                ranges.append((cursor, cut_from))
            cursor = fb
    if b - cursor > 0.05:
        ranges.append((cursor, b))
    return ranges or [(a, min(b, a + 1.0))]


def condense(path, recording, ranges):
    """Join several [from, to] ranges (seconds into the recording) of one beat into one clip."""
    # One decode with select (trim branches buffer every frame and run out of memory on long beats).
    keep = "+".join("between(t,%.3f,%.3f)" % (a, b) for a, b in ranges)
    chain = "fps=30,select='%s',setpts=N/30/TB" % keep
    run(["ffmpeg", "-v", "error", "-y", "-ss", "%.3f" % max(0.0, ranges[0][0] - 1.0), "-copyts", "-i", recording,
         "-vf", chain, "-an", "-c:v", "libx264", "-preset", "veryfast", "-crf", "16", "-pix_fmt", "yuv420p", path])
    return sum(b - a for a, b in ranges)


def main():
    key, recording, marks_path, out_dir = sys.argv[1:5]
    out_dir = os.path.abspath(out_dir)
    marks = json.load(open(marks_path))
    beats = parse_beats(read(os.path.join(SCRIPTS, FILES[key])))
    text = read(os.path.join(SCRIPTS, FILES[key]))
    next_match = re.search(r"ON SCREEN: `Next: ([^`]+)`", text)
    os.makedirs(out_dir, exist_ok=True)
    tmp = os.path.join(out_dir, "_parts", key)
    os.makedirs(tmp, exist_ok=True)
    parts = []
    srt = []
    chapters = []
    clock = 0.0

    if key == "showcase":
        intro = [("Kicker", 400, "UI FRAMEWORK FOR UNITY"), ("Title", 520, "The showcase app"),
                 ("Small", 640, "Every screen is its own scene")]
    else:
        intro = [("Kicker", 360, "UI FRAMEWORK FOR UNITY  ·  COIN SHOP TUTORIAL"), ("Big", 490, "Episode %d" % int(key)),
                 ("Title", 620, NAMES[key])]
    first = os.path.join(tmp, "000_intro.mp4")
    card(first, intro, 3.0, tmp)
    parts.append(first)
    clock += 3.0

    starts = marks.get("beats", {})
    numbers = [b["n"] for b in beats if str(b["n"]) in starts or str(b["n"]) in marks.get("sources", {})]
    for index, beat in enumerate(beats):
        n = beat["n"]
        source = recording
        windows = str(n) in starts and str(n) not in marks.get("sources", {}) and not marks.get("no_condense")
        if windows:
            a = starts[str(n)] - marks["start"]
            later = [v for k, v in starts.items() if float(k) > n]
            b = (min(later) if later else marks["end"]) - marks["start"]
            ranges = keep_ranges(recording, a, b, float(marks.get("keep_still", 1.2)))
            source = os.path.join(tmp, "%03d_raw.mp4" % n)
            stop = condense(source, recording, ranges)
            start = 0.0
            mapped = []
            for insert in marks.get("inserts", []):
                offset = 0.0
                t = insert["at"] - marks["start"]
                if t < ranges[0][0] - 0.5 or t > ranges[-1][1]:
                    continue
                for a, b in ranges:
                    if t < b:
                        # inside this range, or in the cut gap just before it: show it where the footage resumes
                        mapped.append({"at": offset + max(0.0, t - a), "image": insert["image"], "dur": insert.get("dur", 4)})
                        break
                    offset += b - a
        elif str(n) in marks.get("sources", {}):
            src = marks["sources"][str(n)]
            source, start, stop = src["file"], float(src["from"]), float(src["to"])
        elif str(n) in starts:
            start = starts[str(n)] - marks["start"]
            later = [v for k, v in starts.items() if float(k) > n]
            stop = (min(later) if later else marks["end"]) - marks["start"]
        else:
            continue
        if not windows:
            mapped = None
        raw_len = max(stop - start, 0.5)
        spoken = read_time(beat["say"]) + 1.2 if beat["say"] else 2.0
        speed = float(marks.get("speed", {}).get(str(n), 0) or 0)
        if not speed:
            speed = 1.0
            if raw_len > spoken + 6:
                speed = min(4.0, raw_len / (spoken + 3))
                if speed < 1.3:
                    speed = 1.0
        shown = raw_len / speed
        hold = max(0.0, max(spoken, 3.0) - shown)
        out_len = shown + hold
        inserts = [dict(m, dur=m["dur"] * speed) for m in mapped] if mapped is not None else []
        for insert in ([] if mapped is not None else marks.get("inserts", [])):
            if source is recording and start <= insert["at"] - marks["start"] < stop:
                inserts.append({"at": insert["at"] - marks["start"] - start, "image": insert["image"], "dur": insert.get("dur", 4) * speed})
        events = caption_events(beat["say"], 0.3, out_len - 0.3) if beat["say"] else []
        banner = [b for b in beat["on_screen"] if not b.startswith("Next:")]
        path = os.path.join(tmp, "%03d_beat.mp4" % n)
        segment(path, source, start, start + raw_len, speed, hold, inserts, events, banner, tmp)
        parts.append(path)
        chapters.append((clock, beat["title"]))
        for a, b, cap in events:
            srt.append((clock + a, clock + b, cap.replace("\\N", "\n")))
        clock += out_len
        print("beat %d: raw %.1fs speed %.1fx hold %.1fs -> %.1fs" % (n, raw_len, speed, hold, out_len))

    if key == "showcase":
        outro = [("Title", 470, "Clone it and press Play"), ("Small", 600, "github.com/dawaniyah-games/UiFramework")]
    elif next_match:
        outro = [("Kicker", 420, "NEXT"), ("Title", 530, next_match.group(1)),
                 ("Small", 660, "dawaniyah-games.github.io/UiFramework")]
    else:
        outro = [("Title", 470, "Coin Shop is done"), ("Small", 600, "dawaniyah-games.github.io/UiFramework")]
    last = os.path.join(tmp, "999_outro.mp4")
    card(last, outro, 4.0, tmp)
    parts.append(last)
    clock += 4.0

    listing = os.path.join(tmp, "parts.txt")
    with open(listing, "w") as handle:
        for part in parts:
            handle.write("file '%s'\n" % part)
    name = key if key != "showcase" else "showcase"
    target = os.path.join(out_dir, "%s.mp4" % name)
    run(["ffmpeg", "-v", "error", "-y", "-f", "concat", "-safe", "0", "-i", listing, "-c", "copy", "-movflags", "+faststart", target])
    with open(os.path.join(out_dir, "%s.srt" % name), "w", encoding="utf-8") as handle:
        for index, (a, b, cap) in enumerate(srt, 1):
            handle.write("%d\n%s --> %s\n%s\n\n" % (index, srt_time(a), srt_time(b), cap))
    with open(os.path.join(out_dir, "%s.chapters.txt" % name), "w", encoding="utf-8") as handle:
        handle.write("0:00 Intro\n")
        for at, title in chapters:
            handle.write("%d:%02d %s\n" % (int(at) // 60, int(at) % 60, title))
    with open(os.path.join(out_dir, "%s.parts.txt" % name), "w") as handle:
        handle.write("\n".join(parts[1:-1]))
    print("wrote %s (%.1fs)" % (target, clock))


if __name__ == "__main__":
    main()
