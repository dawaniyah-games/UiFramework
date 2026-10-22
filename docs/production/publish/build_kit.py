"""Build the publishing kit for the UI Framework video series from the scripts in docs/production/scripts.

Run from the repository root: python3 build_kit.py
Writes docs/production/publish/: youtube.md, voiceover.md, captions/*.srt, thumbnails/*.png
"""
import math
import os
import re
import textwrap

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.getcwd()
SCRIPTS = os.path.join(ROOT, "docs", "production", "scripts")
OUT = os.path.join(ROOT, "docs", "production", "publish")
IMAGES = os.path.join(ROOT, "docs", "images")
SRC = os.path.join(OUT, "thumbnails", "src")

DOCS = "https://dawaniyah-games.github.io/UiFramework/"
REPO = "https://github.com/dawaniyah-games/UiFramework"
PACKAGE_URL = "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework"
WPM = 140.0
BEAT_PAUSE = 2.5  # seconds of pause after each beat's voiceover in the full walkthrough
MIN_BEAT = 6.0  # shortest beat in the full walkthrough (beats with little or no voiceover)

NAVY = (18, 20, 42)
PANEL = (32, 36, 74)
ACCENT = (255, 200, 61)
WHITE = (255, 255, 255)
MUTED = (160, 166, 214)

# First font found wins, so the kit also rebuilds on Windows (Segoe UI) or any machine with DejaVu.
FONT_EXTRA = ["/usr/share/fonts/truetype/google-fonts/Poppins-Bold.ttf", "C:/Windows/Fonts/segoeuib.ttf",
              "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", "DejaVuSans-Bold.ttf"]
FONT_SEMI = ["/usr/share/fonts/truetype/google-fonts/Poppins-Medium.ttf", "C:/Windows/Fonts/seguisb.ttf",
             "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", "DejaVuSans.ttf"]

VIDEOS = [
    {"key": "01", "file": "01-install-and-config.md", "name": "Install and create the config", "thumb": "Install and Set Up",
     "anchor": "t-config", "image": ("panel", "images/setup-general.png"),
     "desc": "Start the Coin Shop tutorial: install UI Framework from its git URL, turn on Addressables, and create the config the UI Setup Manager works from. You do this once per project."},
    {"key": "02", "file": "02-first-element.md", "name": "Create your first element", "thumb": "Your First Element",
     "anchor": "t-element", "image": ("panel", "images/setup-new-element.png"),
     "desc": "Every screen, bar and popup is an element: its own small scene with its own script. Create the Main Menu element in one click and see the three files you get."},
    {"key": "03", "file": "03-build-the-screen.md", "name": "Build a screen under Root", "thumb": "Build Under Root",
     "anchor": "t-build", "image": ("phone", "src/coinshop-menu.png"),
     "desc": "Open the Main Menu scene and build it: background, title and two buttons. The one rule: everything goes under Root, because Root is what animates."},
    {"key": "04", "file": "04-animations.md", "name": "Choose animations", "thumb": "Choose Animations",
     "anchor": "t-animate", "image": ("panel", "images/setup-elements.png"),
     "desc": "Pick how the Main Menu appears and leaves from ten tuned presets. No code, and you can still override it for a single call later."},
    {"key": "05", "file": "05-validate-and-fix.md", "name": "Validate and fix scenes", "thumb": "Validate and Fix",
     "anchor": "t-validate", "image": ("dialog", "images/setup-fix-dialog.png"),
     "desc": "Break the menu on purpose and watch the tools catch it: Validate All, the issues filter, and Fix Structure with its before / after preview."},
    {"key": "06", "file": "06-canvas-and-addressables.md", "name": "Canvas and Addressables settings", "thumb": "Bars and Sorting",
     "anchor": "t-canvas", "image": ("phone", "src/coinshop-store.png"),
     "desc": "Add a top bar and a nav bar, and stack everything with sorting orders: screens 0, bars 10, popups 100. See where each scene's Addressables settings live."},
    {"key": "07", "file": "07-states.md", "name": "States and shared elements", "thumb": "States and Shared Bars",
     "anchor": "t-state", "image": ("panel", "images/setup-states.png"),
     "desc": "A state is what the player sees at one moment: a list of elements. Build the Main Menu and Store states, sharing the two bars so they never reload."},
    {"key": "08", "file": "08-build-runtime-config.md", "name": "Build the runtime config", "thumb": "Build the Config",
     "anchor": "t-runtime", "image": ("panel", "images/setup-general.png"),
     "desc": "One button turns your states into the config the game loads and makes every scene Addressable. Rebuild after every change."},
    {"key": "09", "file": "09-show-from-code.md", "name": "Show states from code", "thumb": "Show States from Code",
     "anchor": "t-code", "image": ("phone", "src/coinshop-store.png"),
     "desc": "Two lines start the UI, one line switches screens. Press Play: the store slides in while the shared bars stay put."},
    {"key": "10", "file": "10-popup.md", "name": "Popups and overlays", "thumb": "Popups and Overlays",
     "anchor": "t-popup", "image": ("phone", "src/coinshop-popup.png"),
     "desc": "Finish Coin Shop with a purchase popup: a state shown on top with additive: true, filled with data in Populate, closed with HideUI."},
    {"key": "full", "file": "00-full-walkthrough.md", "name": "The whole tutorial", "thumb": "The Full Tutorial",
     "anchor": "tutorial", "image": ("trio", None),
     "desc": "Build a small game UI in Unity from an empty project in about twelve minutes: screens, shared bars and a popup, each in its own scene, set up in the editor with a few lines of code. Chapters below."},
    {"key": "showcase", "file": "showcase.md", "name": "The showcase app", "thumb": "The Showcase App",
     "anchor": "showcase", "image": ("showcase", None),
     "desc": "A complete mobile game UI built only with UI Framework: an intro, four tabs under shared bars, and reward, purchase and settings popups. Clone the repository and press Play."},
]

TAGS_COMMON = ["Unity", "Unity tutorial", "Unity UI", "game UI", "Unity UI framework", "Addressables", "Unity 6",
               "mobile game UI", "UI architecture", "Unity editor tools", "Coin Shop", "UiManager"]
TAGS_EXTRA = {
    "01": ["Unity package manager", "git URL package", "Addressables setup"],
    "02": ["Unity scenes", "UI element", "code generation"],
    "03": ["TextMeshPro", "Unity canvas", "UI layout"],
    "04": ["UI animation", "tween presets", "screen transitions"],
    "05": ["scene validation", "editor tooling", "hierarchy"],
    "06": ["sorting order", "Canvas Scaler", "nav bar"],
    "07": ["UI states", "shared UI", "state machine"],
    "08": ["Addressables groups", "runtime config"],
    "09": ["async await", "C#", "UI navigation"],
    "10": ["popup", "modal", "overlay"],
    "full": ["full tutorial", "beginner", "step by step"],
    "showcase": ["showcase", "mobile UI", "UI demo"],
}


def read(path):
    with open(path, encoding="utf-8") as handle:
        return handle.read().replace("\r\n", "\n")


def seconds(stamp):
    minutes, secs = stamp.split(":")
    return int(minutes) * 60 + int(secs)


def stamp_of(total):
    total = int(round(total))
    return "%d:%02d" % (total // 60, total % 60)


def target_length(text):
    match = re.search(r"\|\s*Length\s*\|\s*about (\d+) min", text)
    return int(match.group(1)) * 60 if match else None


def parse_beats(text):
    beats = []
    for block in re.split(r"\n(?=\*\*\d+\. )", text):
        head = re.match(r"\*\*(\d+)\. (.+?)\*\* \((\d+:\d\d)\)", block)
        if not head:
            continue
        says = re.findall(r'^- SAY: "(.*)"\s*$', block, re.M)
        on_screen = re.findall(r"^- ON SCREEN: (.*)$", block, re.M)
        beats.append({"n": int(head.group(1)), "title": head.group(2), "start": seconds(head.group(3)),
                      "say": " ".join(says), "on_screen": [s.strip() for s in on_screen]})
    return beats


def words(text):
    return len(re.findall(r"[A-Za-z0-9']+", text))


def beat_ends(beats, total):
    ends = []
    for index, beat in enumerate(beats):
        if index + 1 < len(beats):
            ends.append(beats[index + 1]["start"])
        else:
            spoken = words(beat["say"]) / WPM * 60.0
            ends.append(max(beat["start"] + spoken + 1.5, total or 0))
    return ends


# ------------------------------------------------------------------ captions
def srt_time(value):
    millis = int(round(value * 1000))
    hours, millis = divmod(millis, 3600000)
    minutes, millis = divmod(millis, 60000)
    secs, millis = divmod(millis, 1000)
    return "%02d:%02d:%02d,%03d" % (hours, minutes, secs, millis)


def split_sentences(text):
    parts = re.split(r"(?<=[.!?])\s+", text.strip())
    cues = []
    for part in parts:
        if len(part) <= 84:
            cues.append(part)
            continue
        chunk = ""
        for word in part.split():
            if len(chunk) + len(word) + 1 > 84:
                cues.append(chunk)
                chunk = word
            else:
                chunk = (chunk + " " + word).strip()
        if chunk:
            cues.append(chunk)
    return [cue for cue in cues if cue]


def write_srt(path, beats, total, offset=0.0, skip=()):
    lines = []
    counter = 1
    ends = beat_ends(beats, total)
    for beat, end in zip(beats, ends):
        if beat["n"] in skip or not beat["say"]:
            continue
        start = beat["start"] + 0.5
        spoken = words(beat["say"]) / WPM * 60.0
        stop = min(end - 0.3, start + spoken)
        cues = split_sentences(beat["say"])
        weights = [max(len(cue), 1) for cue in cues]
        span = stop - start
        cursor = start
        for cue, weight in zip(cues, weights):
            length = span * weight / float(sum(weights))
            wrapped = textwrap.wrap(cue, 42)
            lines.append(str(counter))
            lines.append("%s --> %s" % (srt_time(cursor + offset), srt_time(cursor + length + offset)))
            lines.extend(wrapped)
            lines.append("")
            counter += 1
            cursor += length
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write("\n".join(lines))


# ------------------------------------------------------------------ thumbnails
def font(candidates, size):
    for path in candidates:
        try:
            return ImageFont.truetype(path, size)
        except OSError:
            continue
    raise OSError("No usable font found in: " + ", ".join(candidates))


def rounded(image, radius):
    mask = Image.new("L", image.size, 0)
    ImageDraw.Draw(mask).rounded_rectangle([0, 0, image.size[0] - 1, image.size[1] - 1], radius, fill=255)
    return mask


def framed(image, box_w, box_h, mode):
    if mode == "panel":
        crop_h = min(image.height, int(image.width * box_h / float(box_w)))
        image = image.crop((0, 0, image.width, crop_h))
    scale = min(box_w / float(image.width), box_h / float(image.height))
    image = image.resize((int(image.width * scale), int(image.height * scale)), Image.LANCZOS)
    return image


def paste_card(canvas, image, centre_x, centre_y, radius=26, border=6, rotate=0.0):
    w, h = image.size
    card = Image.new("RGBA", (w + border * 2, h + border * 2), (0, 0, 0, 0))
    ImageDraw.Draw(card).rounded_rectangle([0, 0, card.width - 1, card.height - 1], radius + border, fill=PANEL + (255,))
    inner = image.convert("RGBA")
    card.paste(inner, (border, border), rounded(inner, radius))
    if rotate:
        card = card.rotate(rotate, resample=Image.BICUBIC, expand=True)
    shadow = Image.new("RGBA", (card.width + 80, card.height + 80), (0, 0, 0, 0))
    ImageDraw.Draw(shadow).rounded_rectangle([40, 50, card.width + 40, card.height + 50], radius + border, fill=(0, 0, 0, 150))
    shadow = shadow.filter(ImageFilter.GaussianBlur(22))
    canvas.alpha_composite(shadow, (int(centre_x - shadow.width / 2), int(centre_y - shadow.height / 2)))
    canvas.alpha_composite(card, (int(centre_x - card.width / 2), int(centre_y - card.height / 2)))


def background():
    canvas = Image.new("RGBA", (1280, 720), NAVY + (255,))
    glow = Image.new("RGBA", (1280, 720), (0, 0, 0, 0))
    draw = ImageDraw.Draw(glow)
    draw.ellipse([700, -220, 1460, 540], fill=(112, 82, 255, 70))
    draw.ellipse([-260, 380, 420, 980], fill=(255, 200, 61, 34))
    glow = glow.filter(ImageFilter.GaussianBlur(120))
    canvas.alpha_composite(glow)
    return canvas


def draw_text_block(canvas, video):
    draw = ImageDraw.Draw(canvas)
    draw.text((72, 64), "UI FRAMEWORK FOR UNITY", font=font(FONT_SEMI, 28), fill=MUTED)
    key = video["key"]
    if key.isdigit():
        badge = "EP " + str(int(key))
        draw.text((66, 108), badge, font=font(FONT_EXTRA, 150), fill=ACCENT)
        title_y = 300
    else:
        title_y = 170
    title_font = font(FONT_EXTRA, 78)
    lines = textwrap.wrap(video["thumb"], 13)
    for index, line in enumerate(lines[:3]):
        draw.text((72, title_y + index * 92), line, font=title_font, fill=WHITE)
    footer = "Coin Shop tutorial" if key.isdigit() or key == "full" else "Clone and press Play"
    if key == "full":
        footer = "12 minutes · empty project to finished UI"
    draw.rounded_rectangle([72, 616, 72 + int(draw.textlength(footer, font=font(FONT_SEMI, 28))) + 40, 664], 24, fill=PANEL)
    draw.text((92, 620), footer, font=font(FONT_SEMI, 28), fill=ACCENT)


def build_thumbnail(video, path):
    canvas = background()
    mode, source = video["image"]
    if mode == "trio":
        names = ["src/coinshop-menu.png", "src/coinshop-store.png", "src/coinshop-popup.png"]
        offsets = [(880, 380, -6), (1020, 360, 0), (1160, 380, 6)]
        for name, (x, y, angle) in zip(names, offsets):
            image = framed(Image.open(os.path.join(OUT, "thumbnails", name)), 230, 520, "phone")
            paste_card(canvas, image, x, y, radius=18, border=5, rotate=angle)
    elif mode == "showcase":
        names = [os.path.join(IMAGES, "showcase-home.png"), os.path.join(IMAGES, "showcase-store.png")]
        for name, (x, y, angle) in zip(names, [(930, 375, -5), (1110, 370, 4)]):
            image = framed(Image.open(name), 300, 580, "phone")
            paste_card(canvas, image, x, y, radius=22, border=6, rotate=angle)
    else:
        full = os.path.join(OUT, "thumbnails", source) if source.startswith("src/") else os.path.join(ROOT, "docs", source)
        image = Image.open(full)
        if mode == "phone":
            image = framed(image, 330, 600, "phone")
            paste_card(canvas, image, 1030, 370, radius=26, border=7, rotate=3)
        elif mode == "dialog":
            image = framed(image, 560, 420, "dialog")
            paste_card(canvas, image, 960, 380, radius=16, border=6, rotate=2)
        else:
            image = framed(image, 470, 600, "panel")
            paste_card(canvas, image, 1000, 370, radius=18, border=6, rotate=2)
    draw_text_block(canvas, video)
    canvas.convert("RGB").save(path, "PNG", optimize=True)


# ------------------------------------------------------------------ main
def main():
    os.makedirs(os.path.join(OUT, "captions"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "thumbnails"), exist_ok=True)

    parsed = {}
    for video in VIDEOS:
        text = read(os.path.join(SCRIPTS, video["file"]))
        video["length"] = target_length(text)
        video["beats"] = parse_beats(text)
        parsed[video["key"]] = video

    # Full walkthrough: the edit is voice-led (00-full-walkthrough.md), so each kept beat lasts as long as its
    # voiceover plus a short pause, and the building parts are sped up underneath. This gives the chapter times.
    intro_text = read(os.path.join(SCRIPTS, "00-full-walkthrough.md"))
    intro_say = re.search(r'- SAY: "(.*)"', intro_text).group(1)
    intro = max(20.0, words(intro_say) / WPM * 60.0 + BEAT_PAUSE)
    chapters_full = [(0, "Intro")]
    cursor = intro
    full_beats = [{"n": 0, "title": "Intro", "start": 0.0, "say": intro_say, "on_screen": []}]
    for video in VIDEOS[:10]:
        beats = video["beats"]
        kept = [beat for beat in beats if not beat["title"].lower().startswith("wrap up") or video["key"] == "10"]
        if video["key"] == "01":
            kept = kept[1:]
        chapters_full.append((cursor, video["name"]))
        for beat in kept:
            duration = max(MIN_BEAT, words(beat["say"]) / WPM * 60.0 + BEAT_PAUSE)
            full_beats.append({"n": len(full_beats), "title": beat["title"], "start": cursor, "say": beat["say"], "on_screen": []})
            cursor += duration
    full_length = cursor

    # Captions
    for video in VIDEOS:
        if video["key"] == "full":
            continue
        write_srt(os.path.join(OUT, "captions", video["key"] + ".srt"), video["beats"], video["length"])

    write_srt(os.path.join(OUT, "captions", "full.srt"), full_beats, full_length)

    # Thumbnails
    for video in VIDEOS:
        build_thumbnail(video, os.path.join(OUT, "thumbnails", video["key"] + ".png"))

    # Voiceover read sheet
    vo = ["# Voiceover read sheet", "",
          "Every SAY line from the scripts, beat by beat, in recording order. Read at about %d words a minute; "
          "the *fits* column compares the estimated read time with the gap to the next beat. Leave a two-second pause "
          "between beats so they are easy to cut. Generated from `docs/production/scripts/` by "
          "`docs/production/publish/build_kit.py`; edit the scripts, not this file." % WPM, ""]
    for video in VIDEOS:
        beats = video["beats"]
        ends = beat_ends(beats, video["length"])
        title = "Episode %d · %s" % (int(video["key"]), video["name"]) if video["key"].isdigit() else video["name"]
        vo.append("## " + title)
        vo.append("")
        if video["key"] == "full":
            vo.append("Only the intro is new voiceover; the rest is the episodes' audio.")
            vo.append("")
            vo.append("| Beat | Start | Words | Read time | Line |")
            vo.append("| --- | --- | --- | --- | --- |")
            vo.append("| Intro | 0:00 | %d | %.0f s | %s |" % (words(intro_say), words(intro_say) / WPM * 60, intro_say))
            vo.append("")
            continue
        vo.append("| Beat | Start | Gap | Read time | Fits | Line |")
        vo.append("| --- | --- | --- | --- | --- | --- |")
        for beat, end in zip(beats, ends):
            if not beat["say"]:
                continue
            gap = end - beat["start"]
            read_time = words(beat["say"]) / WPM * 60
            fits = "yes" if read_time <= gap else "**%+.0f s**" % (read_time - gap)
            vo.append("| %d. %s | %s | %.0f s | %.0f s | %s | %s |" % (beat["n"], beat["title"], stamp_of(beat["start"]), gap, read_time, fits, beat["say"]))
        vo.append("")
    with open(os.path.join(OUT, "voiceover.md"), "w", encoding="utf-8", newline="\n") as handle:
        handle.write("\n".join(vo))

    # YouTube metadata
    yt = ["# YouTube publishing sheet", "",
          "Everything to paste when uploading each video: title, description (with chapters and links), tags, "
          "thumbnail, captions, playlist and end screen. Generated from the scripts by `build_kit.py`; after the edit, "
          "check every chapter time against the final timeline (YouTube needs the first chapter at 0:00 and each chapter at least 10 s long).",
          "",
          "**Playlist:** *UI Framework for Unity · Coin Shop tutorial* (Episodes 1 to 10 in order, then the full tutorial and the showcase).  ",
          "**Upload as:** Unlisted, check it, then Public. Category *Education*, language English, captions from `captions/`.  ",
          "**Never use `<` or `>` in a title or description:** YouTube rejects them, so code stays in the docs and the pinned comment links there.",
          ""]
    for index, video in enumerate(VIDEOS):
        key = video["key"]
        if key.isdigit():
            number = int(key)
            title = "%s · Unity UI Framework Tutorial %d/10" % (video["name"], number)
            heading = "Episode %d · %s" % (number, video["name"])
            chapters = [(beat["start"], beat["title"]) for beat in video["beats"]]
            step_link = DOCS + "#" + video["anchor"]
        elif key == "full":
            title = "Build a Unity Game UI from Scratch · UI Framework Full Tutorial"
            heading = "The whole tutorial"
            chapters = chapters_full
            step_link = DOCS + "#tutorial"
        else:
            title = "A Complete Mobile Game UI in Unity · UI Framework Showcase"
            heading = "The showcase app"
            chapters = [(beat["start"], beat["title"]) for beat in video["beats"]]
            step_link = DOCS + "#showcase"
        assert len(title) <= 100, title
        description = [video["desc"], ""]
        if key.isdigit():
            description.append("This step in the docs: " + step_link)
            description.append("")
        description.append("Chapters")
        for at, name in chapters:
            description.append("%s %s" % (stamp_of(at), name))
        description.append("")
        if key.isdigit() and int(key) > 1:
            description.append("Previous: Episode %d · %s" % (int(key) - 1, VIDEOS[index - 1]["name"]))
        if key.isdigit() and int(key) < 10:
            description.append("Next: Episode %d · %s" % (int(key) + 1, VIDEOS[index + 1]["name"]))
        if key.isdigit():
            description.append("")
        description.append("Install (Package Manager, Add package from git URL): " + PACKAGE_URL)
        description.append("Source, showcase app and issues: " + REPO)
        description.append("")
        description.append("Docs: " + DOCS + "#tutorial")
        description.append("")
        description.append("#Unity #UnityTutorial #GameUI")
        body = "\n".join(description)
        assert "<" not in body and ">" not in body, key
        assert len(body) <= 5000, key
        tags = TAGS_COMMON + TAGS_EXTRA.get(key, [])
        tag_text = ", ".join(tags)
        assert len(tag_text) <= 500, key
        yt.append("## " + heading)
        yt.append("")
        if key == "full":
            length_note = "about %s (voice-led estimate: voiceover plus %.1f s per beat; the chapter times below use it)" % (stamp_of(full_length), BEAT_PAUSE)
        else:
            length_note = "about %d min" % (video["length"] // 60) if video["length"] else ""
        yt.append("| | |")
        yt.append("| --- | --- |")
        yt.append("| Thumbnail | `thumbnails/%s.png` |" % key)
        yt.append("| Captions | `captions/%s.srt` (timed from the script; nudge after the edit) |" % key)
        yt.append("| Target length | %s |" % length_note)
        yt.append("| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#%s` (`docs/index.html`) |" % video["anchor"])
        if key.isdigit() and int(key) < 10:
            yt.append("| End screen | Next episode (Episode %d) + subscribe |" % (int(key) + 1))
        elif key == "10":
            yt.append("| End screen | The showcase app + the full tutorial |")
        else:
            yt.append("| End screen | Playlist + subscribe |")
        yt.append("")
        yt.append("**Title**")
        yt.append("")
        yt.append("```text")
        yt.append(title)
        yt.append("```")
        yt.append("")
        yt.append("**Description**")
        yt.append("")
        yt.append("```text")
        yt.append(body)
        yt.append("```")
        yt.append("")
        yt.append("**Tags**")
        yt.append("")
        yt.append("```text")
        yt.append(tag_text)
        yt.append("```")
        yt.append("")
        if key in ("09", "10"):
            yt.append("**Pinned comment**")
            yt.append("")
            yt.append("```text")
            yt.append("All the code from this video, ready to copy: " + step_link + "  The element scripts override Awake and call base.Awake() first, so the show and hide animations keep working.")
            yt.append("```")
            yt.append("")
        elif key == "01":
            yt.append("**Pinned comment**")
            yt.append("")
            yt.append("```text")
            yt.append("Package URL for Window → Package Manager → + → Add package from git URL: " + PACKAGE_URL + "  If Unity asks about Legacy Bundles when you create the Addressables settings, press Ignore.")
            yt.append("```")
            yt.append("")
    with open(os.path.join(OUT, "youtube.md"), "w", encoding="utf-8", newline="\n") as handle:
        handle.write("\n".join(yt))

    print("full walkthrough estimate", stamp_of(full_length))


if __name__ == "__main__":
    main()
