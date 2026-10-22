#!/usr/bin/env bash
# Checks the C# rules from CODING_STANDARDS.md that a script can check reliably. Runs in CI (.github/workflows/
# standards.yml) and locally from the repository root:
#
#   bash tools/check-standards.sh            # check every C# file and code template
#   bash tools/check-standards.sh FILE...    # check only these files
#
# Exits 1 when anything fails. A known legacy deviation can be exempted for one file and one rule in
# tools/standards-baseline.txt ("path rule"); new code never goes in there.

set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# File arguments may be relative to wherever the script was started; make them relative to the repository root,
# which is what the baseline uses.
files=()
for argument in "$@"; do
    if [ ! -f "$argument" ]; then
        echo "check-standards: file not found: $argument" >&2
        exit 2
    fi

    absolute="$(cd "$(dirname "$argument")" && pwd)/$(basename "$argument")"
    files+=("${absolute#"$root"/}")
done

cd "$root"

baseline="tools/standards-baseline.txt"

if [ "${#files[@]}" -eq 0 ]; then
    mapfile -t files < <(
        {
            find Packages/com.dawaniyahgames.uiframework Assets/Scripts -name '*.cs' -type f
            find Packages/com.dawaniyahgames.uiframework/Editor/Templates -name '*.txt' -type f
        } | sort
    )
fi

failures=0

for file in "${files[@]}"; do
    exempt=""
    if [ -f "$baseline" ]; then
        exempt="$(awk -v f="$file" '$1 == f { printf "%s ", $2 }' "$baseline")"
    fi

    # LC_ALL=C makes [^ -~] match any non-ASCII byte (emoji in log messages).
    output="$(LC_ALL=C awk -v file="$file" -v exempt=" $exempt" '
        function report(rule, message) {
            if (index(exempt, " " rule " ") > 0) {
                return
            }
            printf "%s:%d: [%s] %s\n", file, FNR, rule, message
            count++
        }

        {
            raw = $0
            sub(/\r$/, "", raw)

            if (raw ~ /\t/) {
                report("tabs", "Indent with 4 spaces, not tabs.")
            }
            if (raw ~ /[ ]+$/) {
                report("trailing-whitespace", "Remove trailing whitespace.")
            }

            # Logs are checked on the raw line, because the message lives in the string.
            if (raw ~ /Debug\.Log[A-Za-z]*\([ ]*\$?"/ && raw !~ /Debug\.Log[A-Za-z]*\([ ]*\$?"\[/) {
                report("log-tag", "Start the message with a [ClassName] tag.")
            }
            if (raw ~ /Debug\.Log/ && raw ~ /[^ -~]/) {
                report("log-ascii", "No emoji or other non-ASCII characters in log messages.")
            }

            # Everything else ignores comments and the contents of string literals.
            code = raw
            if (code ~ /^[ ]*(\/\/|\/\*|\*)/) {
                next
            }
            gsub(/"([^"\\]|\\.)*"/, "\"\"", code)
            sub(/\/\/.*$/, "", code)

            if (code ~ /(^|[^A-Za-z0-9_.])var[ ]+[A-Za-z_][A-Za-z0-9_]*[ ]*(=|in[ ])/) {
                report("no-var", "Write the explicit type instead of var.")
            }
            if (code ~ /(^|[^A-Za-z0-9_])new\(/) {
                report("no-target-typed-new", "Write the type: new List<string>(), not new().")
            }
            if (code ~ /^[ ]*(public|private|protected|internal)[^=;]*=>/ || code ~ /^[ ]*(get|set|init)[ ]*=>/) {
                report("no-expression-body", "Use a block body instead of =>.")
            }
            if (code ~ /^[ ]*(if|else if|for|foreach|while)[ ]*\(.*\)[ ]*[^ {].*;[ ]*$/ || code ~ /^[ ]*else[ ]+[^ {i].*;[ ]*$/) {
                report("braces", "Put the statement in braces on its own lines.")
            }
            if (code ~ /[^ ][ ]*\{[ ]*$/) {
                report("allman", "Put the opening brace on its own line.")
            }
            if ((code ~ /\{[ ]*[^{}]*;[ ]*\}/ && code !~ /\{[ ]*((public|private|protected|internal)[ ]+)?(get|set|init)[ ]*;/) || code ~ /\{[ ]*\}/) {
                report("single-line-block", "Expand the block onto separate lines.")
            }
            if (code ~ /^[ ]+using[ ]+(static[ ]+)?[A-Za-z_][A-Za-z0-9_.]*[ ]*(=[ ]*[A-Za-z_][A-Za-z0-9_.<>]*[ ]*)?;/) {
                report("using-outside-namespace", "Move using directives above the namespace.")
            }
            if (code ~ /^[ ]*((\[SerializeField\]|private|protected|internal|public|static|readonly)[ ]+)+[A-Za-z_][A-Za-z0-9_<>,.\[\]]*[ ]+(_|m_)[A-Za-z0-9_]*[ ]*(=|;)/) {
                report("field-prefix", "No _ or m_ prefix on fields.")
            }
            if (code ~ /(^|[^A-Za-z0-9_])const[ ]+[A-Za-z_][A-Za-z0-9_<>.]*[ ]+[a-z_][A-Za-z0-9_]*[ ]*=/) {
                report("const-case", "Constants are PascalCase.")
            }
            if (code ~ /(class|struct|interface|enum)[ ]+[A-Za-z0-9_]*UI([A-Z]|[^A-Za-z0-9_]|$)/) {
                report("ui-casing", "Type names use Ui, not UI.")
            }
            if (code ~ /\.Wait\(\)|GetAwaiter\(\)\.GetResult\(\)/) {
                report("no-blocking-wait", "Never block on a task; await it.")
            }

            # Methods returning a Task end in Async. The listed public names predate the rule and are kept for consumers.
            if (code ~ /^[ ]*(public|private|protected|internal)[^=(]*[^A-Za-z0-9_]Task(<[^(]*>)?[ ]+[A-Za-z_][A-Za-z0-9_]*[ ]*\(/) {
                declaration = code
                sub(/[ ]*\(.*$/, "", declaration)
                sub(/^.*[ ]/, "", declaration)
                if (declaration !~ /Async$/ && declaration !~ /^(ShowState|ShowStateByKey|HideUI|ClickPop|SettlePop|ShakeLocal)$/) {
                    report("async-suffix", "Methods returning a Task end in Async.")
                }
            }
        }

        END {
            if (count > 0) {
                exit 1
            }
        }
    ' "$file")" || [ -n "$output" ]

    if [ -n "$output" ]; then
        printf '%s\n' "$output"
        failures=$((failures + $(printf '%s\n' "$output" | wc -l)))
    fi
done

if [ "$failures" -gt 0 ]; then
    echo ""
    echo "Coding standards: $failures problem(s). See CODING_STANDARDS.md."
    exit 1
fi

echo "Coding standards: OK (${#files[@]} files)."
