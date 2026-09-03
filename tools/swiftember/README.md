# Swiftember 2026 — Weekly Report & Leaderboard Generator

Automated suite to parse Strava Club Leaderboard data and generate the standardized **Swiftember PDF & Markdown reports** with exact formatting, high-readability typography, fun superlatives, fair % target leaderboards, member shout-outs, and full roster tracking.

---

## 📁 File Structure & Architecture

* **[`generate_report.py`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/generate_report.py)**: Master CLI tool. Parses Strava data, matches against roster & aliases, calculates weekly pro-rata goals, generates HTML with embedded base64 Swifts logo, and exports PDF via Headless Chrome.
* **[`roster.json`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/roster.json)**: Master list of all registered participants and their monthly distance targets (in km). Add new runners or update targets here.
* **[`aliases.json`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/aliases.json)**: Robust Strava-to-Roster alias map handling Strava display names, nicknames, and emoji variations.
* **[`shoutouts.json`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/shoutouts.json)**: Weekly member shout-outs and special event highlights (organized by `week_1`, `week_2`, `week_3`, `week_4`).
* **[`assets/swifts_logo.jpg`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/assets/swifts_logo.jpg)**: Official Birmingham Swifts logo embedded directly into reports.
* **[`mock_strava_data.txt`](file:///Users/bo.wang/.gemini/antigravity-ide/scratch/slot-game-simulators/tools/swiftember/mock_strava_data.txt)**: Active Strava leaderboard input data.

---

## ⚡️ Standardized Report Specification & Typography

All future reports generated with this tool are permanently configured with these exact specifications:

1. **Header Banner**: Official Birmingham Swifts logo + `🏳️‍🌈 SWIFTEMBER 2026` + `Birmingham Swifts — Week {N} Progress & Leaderboard Report`.
2. **⚡️ WEEKLY SWIFTEMBER HEROES** (5 Mutually Exclusive Superlative Cards):
   * 🌟 **Goal Setter** (*Mid & Long Target Distance: $\ge 70\text{ km}$*): Top weekly pro-rata surge among medium/long distance target runners (Winner font: `16px`).
   * 🐣 **Rising Swift** (*Short Target Distance: $\le 50\text{ km}$*): Top weekly pro-rata surge among short distance target runners (Winner font: `16px`).
   * 🔥 **Road Warrior** (*Most Runs Logged*): Runner who logged the most runs during the week.
   * 🏔 **Mountain Goat** (*Most Elevation*): Runner with the most total elevation climbed (m).
   * ⚡️ **Speed Demon** (*Fastest Avg Pace*): Runner with the fastest average pace (min/km).
3. **📊 EXECUTIVE SUMMARY**: 4 summary metric tiles (`24px` metric values).
4. **🎯 WEEKLY ACHIEVEMENT LEADERBOARD**: All active runners ranked fairly by percentage of monthly target completed, featuring `13px` table rows and centered percentage numbers above color-coded progress bars.
5. **📣 SWIFTEMBER SHOUT-OUTS**: Dedicated vertically stacked cards featuring `16px` runner names, `13px` event tags, `13px` descriptions, and a `13px` footer note: *"✨ More shout-outs coming in future weeks!"*.
6. **📋 FULL SWIFTEMBER REPORT**: Complete 56-runner roster sorted by % achievement (active runners) followed by inactive participants ($0\text{ km Logged}$).

---

## 🚀 How to Generate Reports for Real Swiftember Weeks

### When Strava Club Leaderboard Data is Available:

1. Paste the raw Strava club leaderboard table into a file (or into `tools/swiftember/mock_strava_data.txt`).
2. Run:
```bash
python3 tools/swiftember/generate_report.py --week 1 --input tools/swiftember/mock_strava_data.txt
```
3. The report is automatically generated, formatted, and exported to:
   ```
   /Users/bo.wang/Downloads/Swiftember_2026_Week1_Report.pdf
   ```
   *(For Week 2, 3, or 4, simply pass `--week 2`, `--week 3`, or `--week 4`)*.

---

## ⚙️ Command Line Options

* **`-w` / `--week`**: Week number (`1`, `2`, `3`, `4`). Determines expected pro-rata goal ($25\%$, $50\%$, $75\%$, $100\%$) and loads the corresponding week's shout-outs.
* **`-i` / `--input`**: Path to the raw Strava leaderboard text file.
* **`-o` / `--output-pdf`**: Custom output PDF path (defaults to `~/Downloads/Swiftember_2026_Week<N>_Report.pdf`).
* **`-f` / `--font-size`**: Typography scale preset (`large` is the default standard, or `compact` for the smaller layout).
