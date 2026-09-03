# Swiftember 2026 - Weekly Report & Leaderboard Generator

Automated tool to parse Strava Club Leaderboard data and generate the standardized **Swiftember PDF & Markdown reports** with exact formatting, fun superlatives, fair % target leaderboards, and full roster tracking.

---

## 📁 File Structure
* **`generate_report.py`**: The main CLI reporting tool.
* **`roster.json`**: Master list of all registered participants and their monthly distance targets (in km). Add new runners here whenever someone joins!
* **`aliases.json`**: Name normalization map connecting Strava display names to Swiftember registered names.
* **`mock_strava_data.txt`**: Benchmark test data from August.

---

## 🚀 How to Generate Reports for Real Swiftember Weeks

### Option 1: Using a Data File
1. Copy the weekly Strava club leaderboard text and save it to a text file (e.g., `strava_week1.txt`).
2. Run:
```bash
python3 tools/swiftember/generate_report.py --week 1 --input strava_week1.txt
```
The PDF will be automatically generated into your **`~/Downloads`** folder as `Swiftember_2026_Week1_Report.pdf`.

---

### Option 2: Default Test Run
```bash
python3 tools/swiftember/generate_report.py --week 1
```

---

## ⚙️ Command Line Options
* **`-w` / `--week`**: Week number (`1`, `2`, `3`, `4`). Determines the pro-rata pace rate ($25\%$, $50\%$, $75\%$, $100\%$).
* **`-i` / `--input`**: Path to the raw Strava data text file.
* **`-o` / `--output-pdf`**: Custom path for the output PDF.
* **`-t` / `--title-sub`**: Custom subtitle text under the header badge.
