# Swiftember 2026 — Weekly Report & Leaderboard Generator

Automated suite to parse Strava Club Leaderboard data and generate the standardized **Swiftember PDF & Markdown reports** with exact formatting, high-readability typography, fun superlatives, fair % target leaderboards, member shout-outs, and full multi-week cumulative tracking.

---

## 📁 File Structure & Architecture

* **[`generate_report.py`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/generate_report.py)**: Master CLI tool. Supports single-week parsing and automatic multi-week historical accumulation, matches against roster & aliases, calculates pro-rata goals, generates HTML, and exports PDF via Headless Chrome.
* **[`data/`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/data/)**: Permanent archive of weekly inputs and processed runner statistics:
  * `week_1_strava.txt`: Raw Strava club leaderboard data for Week 1 (106 entries, including late registrations).
  * `week_1_stats.json`: Structured archive of all 58 registered runners with Week 1 distance, runs, pace, elev, and % targets.
  * Future weekly archives (`week_2_strava.txt`, `week_2_stats.json`, etc.) are automatically stored here.
* **[`roster.json`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/roster.json)**: Master list of all registered participants and their monthly distance targets (in km).
* **[`aliases.json`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/aliases.json)**: Robust Strava-to-Roster alias map handling Strava display names, nicknames, and emoji variations.
* **[`shoutouts.json`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/shoutouts.json)**: Weekly member shout-outs and special event highlights (organized by `week_1`, `week_2`, `week_3`, `week_4`).
* **[`assets/swifts_logo.jpg`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/assets/swifts_logo.jpg)**: Official Birmingham Swifts logo embedded directly into reports.
* **[`mock_strava_data.txt`](file:///Users/bowang/.gemini/antigravity-ide/scratch/slot-simulator/tools/swiftember/mock_strava_data.txt)**: Fallback raw Strava leaderboard input data.

---

## ⚡️ Multi-Week Cumulative Tracking Logic

Because the Strava club leaderboard automatically resets every Sunday at midnight, weekly exports only contain the mileage logged during that specific week.

Starting from **Week 2**, `generate_report.py` automatically handles multi-week accumulation:
1. **Historical Data Loading**: Automatically loads all prior weekly stats from `data/week_<W>_stats.json` for all $W < N$.
2. **Weekly Performance Isolation**:
   * **Weekly Heroes** (Rising Swift, Goal Setter, Road Warrior, Mountain Goat, Speed Demon) celebrate achievements **in Week $N$**.
   * **🎯 WEEKLY ACHIEVEMENT LEADERBOARD** ranks runners based strictly on **Week $N$ logged distance** against their weekly pro-rata quota ($\text{monthly\_target} / 4.0$).
3. **Month-To-Date (MTD) Cumulative Tracking**:
   * **📋 FULL SWIFTEMBER REPORT (Overall Table)** combines:
     $$\text{Total Logged (MTD)} = \text{Week 1 Distance} + \text{Week 2 Distance} + \dots + \text{Week } N \text{ Distance}$$
     $$\text{Total Runs (MTD)} = \text{Week 1 Runs} + \text{Week 2 Runs} + \dots + \text{Week } N \text{ Runs}$$
   * **Remaining Distance**: $\max(0, \text{Monthly Pledge} - \text{Total Logged MTD})$. If reached, displays `🎉 Done!`.
   * **Monthly Progress**: Progress bar showing percentage of total monthly pledge achieved.
   * **Challenge Status**: Evaluated against the pro-rata milestone at Week $N$ ($\text{Monthly Pledge} \times \frac{N}{4}$):
     * $\ge 110\%$: `🟢 Ahead`
     * $\ge 90\%$: `🟢 On Track`
     * $\ge 60\%$: `🟡 Slightly Behind`
     * $< 60\%$: `🔴 Behind`
     * $0\text{ km logged MTD}$: `⚪️ 0 km Logged`

---

## 🚀 How to Run the Weekly Reports

### Generating Week 1 Report (Archived):
```bash
python3 tools/swiftember/generate_report.py --week 1
```

### Generating Week 2 Report (Next Week):
1. When Week 2 completes, save the Strava leaderboard export as `tools/swiftember/data/week_2_strava.txt` (or pass it via `--input`).
2. Run:
```bash
python3 tools/swiftember/generate_report.py --week 2
```
3. The overall table will automatically combine **Week 1 + Week 2 stats**, while the weekly leaderboard and heroes will feature Week 2.
4. The final PDF is exported directly to:
   ```
   ~/Downloads/Swiftember_2026_Week2_Report.pdf
   ```

---

## ⚙️ Command Line Options

* **`-w` / `--week`**: Week number (`1`, `2`, `3`, `4`). Determines expected pro-rata milestone, loads historical stats for prior weeks, and saves the week's archive.
* **`-i` / `--input`**: Path to the raw Strava leaderboard text file (if omitted, automatically checks `tools/swiftember/data/week_<N>_strava.txt`, then falls back to `mock_strava_data.txt`).
* **`-o` / `--output-pdf`**: Custom output PDF path (defaults to `~/Downloads/Swiftember_2026_Week<N>_Report.pdf`).
* **`-f` / `--font-size`**: Typography scale preset (`large` is the default standard, or `compact` for the smaller layout).
