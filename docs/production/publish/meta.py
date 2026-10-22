"""Build upload metadata (title, description with real chapters, tags) from youtube.md + NN.chapters.txt.
usage: meta.py KEY CHAPTERS_FILE  -> prints JSON
KEY: 01..10, full, showcase
"""
import json, os, re, sys

MD = os.path.join(os.path.dirname(os.path.abspath(__file__)), "youtube.md")
key, chapters_file = sys.argv[1], sys.argv[2]
text = open(MD, encoding="utf-8").read()
sections = re.split(r"\n## ", text)
if key == "full":
    sec = [s for s in sections if s.startswith("The whole tutorial")][0]
elif key == "showcase":
    sec = [s for s in sections if s.startswith("The showcase app")][0]
else:
    sec = [s for s in sections if s.startswith("Episode %d " % int(key))][0]
blocks = re.findall(r"\*\*(Title|Description|Tags)\*\*\s*```text\n(.*?)\n```", sec, re.S)
d = {k: v for k, v in blocks}


def secs(stamp):
    parts = [int(p) for p in stamp.split(":")]
    return sum(p * 60 ** i for i, p in enumerate(reversed(parts)))


def fmt(t):
    return "%d:%02d:%02d" % (t // 3600, t % 3600 // 60, t % 60) if t >= 3600 else "%d:%02d" % (t // 60, t % 60)


rows = []
for line in open(chapters_file, encoding="utf-8"):
    line = line.strip()
    if not line:
        continue
    stamp, name = line.split(" ", 1)
    rows.append([secs(stamp), name])
rows[0][0] = 0
import subprocess, os
video = os.path.join(os.path.dirname(chapters_file), key + ".mp4")
end = float(subprocess.check_output(["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", video])) if os.path.exists(video) else rows[-1][0] + 30
# YouTube: every chapter at least 10 s. A short chapter is absorbed by the next one (which starts earlier);
# a short last chapter is folded into the one before.
merged = [list(r) for r in rows]
changed = True
while changed and len(merged) > 1:
    changed = False
    for i in range(len(merged)):
        stop = merged[i + 1][0] if i + 1 < len(merged) else end
        if stop - merged[i][0] < 10:
            if i + 1 < len(merged) and merged[i + 1][1].lower().startswith("wrap up"):
                del merged[i + 1]
            elif i + 1 < len(merged):
                merged[i + 1][0] = merged[i][0]
                del merged[i]
            else:
                del merged[i]
            changed = True
            break
chap = "Chapters\n" + "\n".join("%s %s" % (fmt(t), n) for t, n in merged)
desc = re.sub(r"Chapters\n(?:\d[\d:]* .*\n?)+", chap + "\n", d["Description"])
if "Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial" not in desc:
    desc += "\n\nDocs: https://dawaniyah-games.github.io/UiFramework/#tutorial"
print(json.dumps({"title": d["Title"].strip(), "description": desc.strip(), "tags": d["Tags"].strip(), "chapters": merged}, indent=1, ensure_ascii=False))
