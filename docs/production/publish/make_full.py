"""Join the ten edited episodes into the full tutorial (see scripts/00-full-walkthrough.md).

Usage (from the repository root, after make_video.py has built 01..10 into OUT_DIR):
    python3 docs/production/publish/make_full.py OUT_DIR FINALE_CLIP FROM TO

FINALE_CLIP FROM TO   the finished Coin Shop running (seconds), shown under the intro voiceover
Writes OUT_DIR/full.mp4, full.srt and full.chapters.txt.
"""
import os
import re
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import make_video as mv  # noqa: E402

KEYS = ["01", "02", "03", "04", "05", "06", "07", "08", "09", "10"]


def duration(path):
    return float(subprocess.check_output(["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", path]))


def read_srt(path):
    cues = []
    text = mv.read(path)
    for block in text.strip().split("\n\n"):
        lines = block.split("\n")
        if len(lines) < 3:
            continue
        a, b = [x.strip() for x in lines[1].split("-->")]
        cues.append((to_seconds(a), to_seconds(b), "\n".join(lines[2:])))
    return cues


def to_seconds(stamp):
    h, m, rest = stamp.split(":")
    s, ms = rest.split(",")
    return int(h) * 3600 + int(m) * 60 + int(s) + int(ms) / 1000.0


def main():
    out_dir, finale, a, b = sys.argv[1], sys.argv[2], float(sys.argv[3]), float(sys.argv[4])
    tmp = os.path.join(out_dir, "_parts", "full")
    os.makedirs(tmp, exist_ok=True)
    full_text = mv.read(os.path.join(mv.SCRIPTS, "00-full-walkthrough.md"))
    intro_say = re.search(r'## Intro voiceover.*?- SAY: "(.*?)"', full_text, re.S).group(1)

    parts = []
    srt = []
    chapters = [(0.0, "Intro")]
    clock = 0.0

    title = os.path.join(tmp, "000_title.mp4")
    mv.card(title, [("Kicker", 400, "UI FRAMEWORK FOR UNITY"), ("Title", 520, "The full tutorial"),
                    ("Small", 640, "Coin Shop, from an empty project to a finished UI")], 3.0, tmp)
    parts.append(title)
    clock += 3.0

    spoken = mv.read_time(intro_say) + 1.5
    raw = b - a
    speed = 1.0
    hold = max(0.0, spoken - raw)
    events = mv.caption_events(intro_say, 0.3, raw + hold - 0.3)
    intro = os.path.join(tmp, "001_intro.mp4")
    mv.segment(intro, finale, a, b, speed, hold, [], events, ["UI Framework for Unity · the full tutorial"], tmp)
    for x, y, cap in events:
        srt.append((clock + x, clock + y, cap.replace("\\N", "\n")))
    parts.append(intro)
    clock += raw + hold

    for key in KEYS:
        beats = mv.parse_beats(mv.read(os.path.join(mv.SCRIPTS, mv.FILES[key])))
        card = os.path.join(tmp, "%s_card.mp4" % key)
        mv.card(card, [("Kicker", 450, "STEP %d" % int(key)), ("Title", 560, mv.NAMES[key])], 2.0, tmp)
        chapters.append((clock, mv.NAMES[key]))
        parts.append(card)
        clock += 2.0
        for beat in beats:
            if key == "01" and beat["n"] == 1:
                continue
            if key != "10" and beat["title"].lower().startswith("wrap up"):
                continue
            path = os.path.join(out_dir, "_parts", key, "%03d_beat.mp4" % beat["n"])
            if not os.path.exists(path):
                continue
            length = duration(path)
            events = mv.caption_events(beat["say"], 0.3, length - 0.3) if beat["say"] else []
            for x, y, cap in events:
                srt.append((clock + x, clock + y, cap.replace("\\N", "\n")))
            parts.append(path)
            clock += length

    outro = os.path.join(tmp, "999_outro.mp4")
    mv.card(outro, [("Title", 470, "Coin Shop is done"), ("Small", 600, "dawaniyah-games.github.io/UiFramework")], 4.0, tmp)
    parts.append(outro)
    clock += 4.0

    listing = os.path.join(tmp, "parts.txt")
    with open(listing, "w") as handle:
        for part in parts:
            handle.write("file '%s'\n" % part.replace("'", "'\\''"))
    target = os.path.join(out_dir, "full.mp4")
    mv.run(["ffmpeg", "-v", "error", "-y", "-f", "concat", "-safe", "0", "-i", listing, "-c", "copy", "-movflags", "+faststart", target])
    with open(os.path.join(out_dir, "full.srt"), "w", encoding="utf-8") as handle:
        for index, (x, y, cap) in enumerate(srt, 1):
            handle.write("%d\n%s --> %s\n%s\n\n" % (index, mv.srt_time(x), mv.srt_time(y), cap))
    with open(os.path.join(out_dir, "full.chapters.txt"), "w", encoding="utf-8") as handle:
        for at, name in chapters:
            handle.write("%d:%02d %s\n" % (int(at) // 60, int(at) % 60, name))
    print("wrote %s (%d:%02d)" % (target, int(clock) // 60, int(clock) % 60))


if __name__ == "__main__":
    main()
