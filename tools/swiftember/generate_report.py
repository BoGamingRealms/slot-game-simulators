#!/usr/bin/env python3
"""
Swiftember 2026 - Master Weekly Report & Leaderboard Generator
Generates pixel-perfect HTML/PDF reports and formatted Slack/Markdown summaries
from Strava Club Leaderboard data.
"""

import os
import sys
import json
import argparse
import subprocess
import re
import unicodedata

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
ROSTER_PATH = os.path.join(BASE_DIR, "roster.json")
ALIASES_PATH = os.path.join(BASE_DIR, "aliases.json")

def clean_duplicated_name(name_str):
    """Removes duplicated words/halves from Strava copy-paste."""
    s = name_str.strip()
    words = s.split()
    w_len = len(words)
    if w_len >= 2 and w_len % 2 == 0:
        half = w_len // 2
        if words[:half] == words[half:]:
            return " ".join(words[:half])
    for i in range(1, len(s)):
        left = s[:i].strip()
        right = s[i:].strip()
        if left == right:
            return left
    return s

def normalize(s):
    """Normalize unicode strings for robust fuzzy/alias matching."""
    s = re.sub(r'[^\w\s]', '', s)
    return unicodedata.normalize('NFKD', s).encode('ASCII', 'ignore').decode('utf-8').strip().lower()

def load_roster():
    with open(ROSTER_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)
    return {r["name"]: float(r["target_km"]) for r in data["runners"]}

def load_aliases():
    if not os.path.exists(ALIASES_PATH):
        return {}
    with open(ALIASES_PATH, "r", encoding="utf-8") as f:
        data = json.load(f)
    return data.get("aliases", {})

def parse_strava_table(raw_text):
    """Parses tab-separated or whitespace-separated Strava leaderboard lines."""
    entries = []
    lines = [l.strip() for l in raw_text.strip().split("\n") if l.strip()]
    
    for line in lines:
        if line.lower().startswith("rank") or line.lower().startswith("athlete"):
            continue
            
        parts = line.split("\t")
        if len(parts) >= 6:
            try:
                rank = int(parts[0].strip())
                raw_name = parts[1].strip()
                dist_str = parts[2].replace("km", "").replace(",", "").strip()
                dist = float(dist_str)
                runs = int(parts[3].strip())
                longest_str = parts[4].replace("km", "").replace(",", "").strip()
                longest = float(longest_str)
                pace = parts[5].strip()
                elev = parts[6].strip() if len(parts) > 6 else "--"
                
                clean_name = clean_duplicated_name(raw_name)
                entries.append({
                    "rank": rank,
                    "raw_name": raw_name,
                    "clean_name": clean_name,
                    "distance": dist,
                    "runs": runs,
                    "longest": longest,
                    "pace": pace,
                    "elev": elev
                })
            except Exception:
                continue
    return entries

def process_swiftember_data(strava_entries, roster, aliases, week_num=1):
    matched_runners = []
    unmatched_registered = list(roster.keys())
    expected_week_fraction = week_num / 4.0
    
    # Process Strava athletes
    for s in strava_entries:
        c_name = s["clean_name"]
        norm_c = normalize(c_name)
        
        # 1. Check exact / normalized match in roster
        found = None
        for r in list(unmatched_registered):
            if r.lower() == c_name.lower() or normalize(r) == norm_c:
                found = r
                break
                
        # 2. Check aliases map
        if not found:
            for alias_key, target_name in aliases.items():
                if norm_c == normalize(alias_key) and target_name in unmatched_registered:
                    found = target_name
                    break
                    
        if found:
            unmatched_registered.remove(found)
            monthly_target = roster[found]
            weekly_pro_rata = monthly_target * expected_week_fraction
            pct_monthly = (s["distance"] / monthly_target) * 100.0
            pct_weekly = (s["distance"] / weekly_pro_rata) * 100.0 if weekly_pro_rata > 0 else 0.0
            
            if pct_weekly >= 110.0:
                status = "🟢 Ahead"
            elif pct_weekly >= 90.0:
                status = "🟢 On Track"
            elif pct_weekly >= 60.0:
                status = "🟡 Slightly Behind"
            else:
                status = "🔴 Behind"
                
            matched_runners.append({
                "registered_name": found,
                "strava_name": c_name,
                "monthly_target": monthly_target,
                "weekly_target": weekly_pro_rata,
                "distance": s["distance"],
                "runs": s["runs"],
                "longest": s["longest"],
                "pace": s["pace"],
                "elev": s["elev"],
                "pct_monthly": pct_monthly,
                "pct_weekly": pct_weekly,
                "status": status,
                "strava_rank": s["rank"]
            })
            
    # Process registered runners with 0 km recorded
    for r in unmatched_registered:
        monthly_target = roster[r]
        weekly_pro_rata = monthly_target * expected_week_fraction
        matched_runners.append({
            "registered_name": r,
            "strava_name": "-",
            "monthly_target": monthly_target,
            "weekly_target": weekly_pro_rata,
            "distance": 0.0,
            "runs": 0,
            "longest": 0.0,
            "pace": "-",
            "elev": "-",
            "pct_monthly": 0.0,
            "pct_weekly": 0.0,
            "status": "⚪️ 0 km Logged",
            "strava_rank": "-"
        })
        
    return matched_runners

def generate_html_report(matched_runners, week_num=1, badge_subtitle="Official Swiftember Report"):
    active = [m for m in matched_runners if m["distance"] > 0]
    by_pct = sorted(active, key=lambda x: x["pct_monthly"], reverse=True)
    by_dist = sorted(active, key=lambda x: x["distance"], reverse=True)
    all_sorted = sorted(matched_runners, key=lambda x: (x["pct_monthly"], x["distance"], -x["monthly_target"]), reverse=True)
    
    total_pledge = sum(m["monthly_target"] for m in matched_runners)
    total_logged = sum(m["distance"] for m in active)
    expected_pace_target = total_pledge * (week_num / 4.0)
    pct_total_month = (total_logged / total_pledge) * 100.0 if total_pledge > 0 else 0.0
    pct_pace_rate = (total_logged / expected_pace_target) * 100.0 if expected_pace_target > 0 else 0.0
    total_runs = sum(m["runs"] for m in active)
    
    # Extract superlatives
    pace_setter = by_pct[0] if by_pct else None
    road_warrior = max(active, key=lambda x: (x["runs"], x["distance"])) if active else None
    
    # Low-target category hero (monthly target <= 50 km)
    low_target_active = [m for m in active if m["monthly_target"] <= 50.0]
    pocket_rocket = max(low_target_active, key=lambda x: (x["pct_weekly"], x["distance"])) if low_target_active else None
    
    # Elev climber
    def parse_elev(e_str):
        nums = re.findall(r'\d+', e_str.replace(",", ""))
        return int(nums[0]) if nums else 0
    elev_runner = max(active, key=lambda x: parse_elev(x["elev"])) if active else None
    
    # Speed demon
    def parse_pace(p_str):
        m = re.match(r'(\d+):(\d+)', p_str)
        return int(m.group(1))*60 + int(m.group(2)) if m else 99999
    speed_runner = min([m for m in active if parse_pace(m["pace"]) < 99999], key=lambda x: parse_pace(x["pace"])) if active else None

    # HTML Rows
    target_rows = ""
    for i, r in enumerate(by_pct, 1):
        pct_m = r['pct_monthly']
        pct_w = r['pct_weekly']
        bar_color = "green" if pct_w >= 90 else ("yellow" if pct_w >= 60 else "red")
        badge_cls = "badge-ahead" if "Ahead" in r['status'] else ("badge-track" if "On Track" in r['status'] else ("badge-slight" if "Slightly" in r['status'] else "badge-behind"))
        bar_width = min(100, int(pct_m))
        target_rows += f"""
            <tr>
                <td class="text-center" style="font-weight: 700;">{i}</td>
                <td style="font-weight: 600;">{r['registered_name']}</td>
                <td class="text-right" style="font-weight: 700;">{r['distance']:.1f} km</td>
                <td class="text-right">{r['monthly_target']:.0f} km</td>
                <td class="text-center">
                    <span style="font-weight: 700;">{pct_m:.1f}%</span>
                    <div class="progress-bar-container"><div class="progress-bar {bar_color}" style="width: {bar_width}%;"></div></div>
                </td>
                <td class="text-right" style="font-weight: 700;">{pct_w:.1f}%</td>
                <td class="text-center">{r['runs']}</td>
                <td class="text-right">{r['longest']:.1f} km</td>
                <td class="text-center">{r['pace']}</td>
                <td class="text-right">{r['elev']}</td>
                <td class="text-center"><span class="status-badge {badge_cls}">{r['status']}</span></td>
            </tr>
        """

    dist_rows = ""
    for i, r in enumerate(by_dist[:10], 1):
        badge_cls = "badge-ahead" if "Ahead" in r['status'] else ("badge-track" if "On Track" in r['status'] else ("badge-slight" if "Slightly" in r['status'] else "badge-behind"))
        dist_rows += f"""
            <tr>
                <td class="text-center" style="font-weight: 700;">#{i}</td>
                <td style="font-weight: 700; font-size: 9.5px;">{r['registered_name']}</td>
                <td class="text-right" style="font-weight: 700; color: #1e1b4b; font-size: 10px;">{r['distance']:.1f} km</td>
                <td class="text-center" style="font-weight: 700;">{r['runs']}</td>
                <td class="text-right">{r['longest']:.1f} km</td>
                <td class="text-center">{r['pace']}</td>
                <td class="text-right" style="font-weight: 700;">{r['elev']}</td>
                <td>Goal: {r['monthly_target']:.0f} km <span class="status-badge {badge_cls}" style="margin-left: 4px;">{r['status']}</span></td>
            </tr>
        """

    roster_rows = ""
    for i, r in enumerate(all_sorted, 1):
        pct_m = r['pct_monthly']
        pct_w = r['pct_weekly']
        if r['distance'] == 0:
            badge_cls = "badge-zero"
            status_text = "⚪️ 0 km Logged"
            bar_html = '<span style="color: #94a3b8;">0.0%</span>'
            pace_text = "-"
            runs_text = "0"
        else:
            badge_cls = "badge-ahead" if "Ahead" in r['status'] else ("badge-track" if "On Track" in r['status'] else ("badge-slight" if "Slightly" in r['status'] else "badge-behind"))
            status_text = r['status']
            bar_color = "green" if pct_w >= 90 else ("yellow" if pct_w >= 60 else "red")
            bar_width = min(100, int(pct_m))
            bar_html = f'<span style="font-weight: 600;">{pct_m:.1f}%</span><div class="progress-bar-container"><div class="progress-bar {bar_color}" style="width: {bar_width}%;"></div></div>'
            pace_text = f"{pct_w:.1f}%"
            runs_text = str(r['runs'])

        roster_rows += f"""
            <tr>
                <td class="text-center">{i}</td>
                <td style="font-weight: 600;">{r['registered_name']}</td>
                <td class="text-right">{r['monthly_target']:.0f} km</td>
                <td class="text-right" style="font-weight: 700; color: {'#0f172a' if r['distance'] > 0 else '#94a3b8'};">{r['distance']:.1f} km</td>
                <td class="text-center">{bar_html}</td>
                <td class="text-right">{pace_text}</td>
                <td class="text-center">{runs_text}</td>
                <td class="text-center"><span class="status-badge {badge_cls}">{status_text}</span></td>
            </tr>
        """

    logo_path = os.path.join(BASE_DIR, "assets", "swifts_logo.jpg")
    logo_html = ""
    if os.path.exists(logo_path):
        import base64
        with open(logo_path, "rb") as img_f:
            b64 = base64.b64encode(img_f.read()).decode("utf-8")
            logo_html = f'<img src="data:image/jpeg;base64,{b64}" alt="Swifts Logo" style="height: 46px; max-width: 120px; object-fit: contain; border-radius: 6px; background: #ffffff; padding: 2px 6px; box-shadow: 0 2px 5px rgba(0,0,0,0.15); margin-right: 4px;">'

    html = f"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Swiftember 2026 - Week {week_num} Progress Report</title>
    <style>
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap');
        @page {{ size: A4; margin: 10mm 10mm; }}
        * {{ box-sizing: border-box; -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }}
        body {{
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            color: #1e293b; background-color: #ffffff; line-height: 1.35; font-size: 10px; margin: 0; padding: 0;
        }}
        .header {{
            background: linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%);
            color: #ffffff; padding: 14px 18px; border-radius: 10px; margin-bottom: 12px;
            display: flex; align-items: center; justify-content: flex-start;
        }}
        .header-left {{ display: flex; align-items: center; gap: 14px; width: 100%; }}
        .header-title h1 {{ margin: 0; font-size: 22px; font-weight: 800; letter-spacing: -0.5px; display: flex; align-items: center; gap: 8px; }}
        .header-title p {{ margin: 3px 0 0 0; font-size: 11.5px; color: #cbd5e1; font-weight: 400; }}
        .section-title {{
            font-size: 13px; font-weight: 700; color: #0f172a; margin: 14px 0 8px 0;
            display: flex; align-items: center; gap: 6px; border-bottom: 2px solid #e2e8f0; padding-bottom: 4px;
        }}
        .superlatives-grid {{ display: grid; grid-template-columns: repeat(5, 1fr); gap: 8px; margin-bottom: 12px; }}
        .super-card {{
            background: linear-gradient(180deg, #ffffff 0%, #f8fafc 100%);
            border: 1px solid #e2e8f0; border-top: 3px solid #6366f1; border-radius: 8px; padding: 8px; text-align: center;
        }}
        .super-icon {{ font-size: 16px; margin-bottom: 2px; }}
        .super-award {{ font-size: 8.5px; font-weight: 700; color: #475569; text-transform: uppercase; letter-spacing: 0.3px; margin-bottom: 2px; }}
        .super-winner {{ font-size: 10.5px; font-weight: 700; color: #1e1b4b; margin-bottom: 2px; }}
        .super-stat {{ font-size: 9.5px; font-weight: 600; color: #4338ca; }}
        .metrics-grid {{ display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; margin-bottom: 12px; }}
        .metric-card {{ background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 10px 12px; text-align: center; }}
        .metric-val {{ font-size: 18px; font-weight: 800; color: #0f172a; margin-bottom: 2px; }}
        .metric-label {{ font-size: 9.5px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }}
        .metric-sub {{ font-size: 8.5px; color: #3b82f6; margin-top: 2px; font-weight: 500; }}
        table {{ width: 100%; border-collapse: collapse; font-size: 9px; margin-bottom: 12px; }}
        th {{
            background: #f1f5f9; color: #334155; font-weight: 700; text-transform: uppercase;
            font-size: 8px; letter-spacing: 0.4px; padding: 5px 6px; border-top: 1px solid #cbd5e1; border-bottom: 2px solid #cbd5e1; text-align: left;
        }}
        td {{ padding: 4.5px 6px; border-bottom: 1px solid #f1f5f9; color: #1e293b; vertical-align: middle; }}
        tr:nth-child(even) td {{ background-color: #fafafa; }}
        .text-right {{ text-align: right; }}
        .text-center {{ text-align: center; }}
        .status-badge {{ display: inline-block; padding: 2px 6px; border-radius: 10px; font-size: 8px; font-weight: 700; }}
        .badge-ahead {{ background: #dcfce7; color: #15803d; }}
        .badge-track {{ background: #dbeafe; color: #1d4ed8; }}
        .badge-slight {{ background: #fef9c3; color: #a16207; }}
        .badge-behind {{ background: #fee2e2; color: #b91c1c; }}
        .badge-zero {{ background: #f1f5f9; color: #64748b; }}
        .progress-bar-container {{
            width: 55px; background: #e2e8f0; border-radius: 6px; height: 5px; display: inline-block; vertical-align: middle; margin-left: 4px; overflow: hidden;
        }}
        .progress-bar {{ height: 100%; background: #3b82f6; border-radius: 6px; }}
        .progress-bar.green {{ background: #22c55e; }}
        .progress-bar.blue {{ background: #3b82f6; }}
        .progress-bar.yellow {{ background: #eab308; }}
        .progress-bar.red {{ background: #ef4444; }}
        .page-break {{ page-break-before: always; }}
        .footer {{ font-size: 8px; color: #94a3b8; text-align: center; margin-top: 14px; border-top: 1px solid #e2e8f0; padding-top: 6px; }}
    </style>
</head>
<body>
    <div class="header">
        <div class="header-left">
            {logo_html}
            <div class="header-title">
                <h1>🏃‍♂️ SWIFTEMBER 2026</h1>
                <p>Birmingham Swifts — Week {week_num} Progress & Leaderboard Report</p>
            </div>
        </div>
    </div>

    <div class="section-title">⚡️ WEEKLY SWIFTEMBER HEROES</div>
    <div class="superlatives-grid">
        <div class="super-card">
            <div class="super-icon">🌟</div>
            <div class="super-award">Goal Setter</div>
            <div class="super-winner">{pace_setter['registered_name'] if pace_setter else '-'}</div>
            <div class="super-stat">{f"{pace_setter['pct_weekly']:.1f}% Weekly Goal ({pace_setter['distance']:.1f} km)" if pace_setter else '-'}</div>
        </div>
        <div class="super-card">
            <div class="super-icon">🚀</div>
            <div class="super-award">Pocket Rocket</div>
            <div class="super-winner">{pocket_rocket['registered_name'] if pocket_rocket else '-'}</div>
            <div class="super-stat">{f"{pocket_rocket['pct_weekly']:.1f}% Weekly Goal ({pocket_rocket['distance']:.1f} km)" if pocket_rocket else '-'}</div>
        </div>
        <div class="super-card">
            <div class="super-icon">🔥</div>
            <div class="super-award">Road Warrior</div>
            <div class="super-winner">{road_warrior['registered_name'] if road_warrior else '-'}</div>
            <div class="super-stat">{f"{road_warrior['runs']} Runs ({road_warrior['distance']:.1f} km)" if road_warrior else '-'}</div>
        </div>
        <div class="super-card">
            <div class="super-icon">🏔</div>
            <div class="super-award">Mountain Goat</div>
            <div class="super-winner">{elev_runner['registered_name'] if elev_runner else '-'}</div>
            <div class="super-stat">{f"{elev_runner['elev']} Elevation" if elev_runner else '-'}</div>
        </div>
        <div class="super-card">
            <div class="super-icon">⚡️</div>
            <div class="super-award">Speed Demon</div>
            <div class="super-winner">{speed_runner['registered_name'] if speed_runner else '-'}</div>
            <div class="super-stat">{f"{speed_runner['pace']} ({speed_runner['distance']:.1f} km)" if speed_runner else '-'}</div>
        </div>
    </div>

    <div class="metrics-grid">
        <div class="metric-card">
            <div class="metric-val">{total_pledge:,.0f} km</div>
            <div class="metric-label">Total Month Pledge</div>
            <div class="metric-sub">{len(matched_runners)} Registered Runners</div>
        </div>
        <div class="metric-card">
            <div class="metric-val">{total_logged:,.1f} km</div>
            <div class="metric-label">Distance Logged</div>
            <div class="metric-sub">{pct_total_month:.1f}% of Monthly Goal</div>
        </div>
        <div class="metric-card">
            <div class="metric-val">{pct_pace_rate:.1f}%</div>
            <div class="metric-label">Week {week_num} Goal Progress</div>
            <div class="metric-sub">{total_logged:,.1f} / {expected_pace_target:,.1f} km Target</div>
        </div>
        <div class="metric-card">
            <div class="metric-val">{len(active)} / {len(matched_runners)}</div>
            <div class="metric-label">Active Runners</div>
            <div class="metric-sub">{total_runs} Total Runs</div>
        </div>
    </div>

    <div class="section-title">🎯 WEEKLY ACHIEVEMENT LEADERBOARD</div>
    <table>
        <thead>
            <tr>
                <th class="text-center" style="width: 30px;">Rank</th>
                <th>Runner Name</th>
                <th class="text-right">Logged (km)</th>
                <th class="text-right">Goal (km)</th>
                <th class="text-center">Monthly Progress</th>
                <th class="text-right">Weekly Goal %</th>
                <th class="text-center">Runs</th>
                <th class="text-right">Longest</th>
                <th class="text-center">Avg Pace</th>
                <th class="text-right">Elevation</th>
                <th class="text-center">Status</th>
            </tr>
        </thead>
        <tbody>
            {target_rows}
        </tbody>
    </table>

    <div class="page-break"></div>

    <div class="section-title">📋 FULL SWIFTEMBER REPORT</div>
    <table>
        <thead>
            <tr>
                <th class="text-center" style="width: 25px;">#</th>
                <th>Participant Name</th>
                <th class="text-right">Monthly Goal</th>
                <th class="text-right">Logged This Week</th>
                <th class="text-center">Monthly Progress</th>
                <th class="text-right">Weekly Goal %</th>
                <th class="text-center">Runs</th>
                <th class="text-center">Status / Note</th>
            </tr>
        </thead>
        <tbody>
            {roster_rows}
        </tbody>
    </table>

    <div class="footer">
        Swiftember 2026 Challenge Report • Birmingham Swifts Running Club
    </div>
</body>
</html>
"""
    return html

def main():
    parser = argparse.ArgumentParser(description="Generate Swiftember Weekly PDF & Markdown Report")
    parser.add_argument("-i", "--input", help="Path to raw Strava leaderboard data text file (or stdin if omitted)")
    parser.add_argument("-w", "--week", type=int, default=1, help="Week number (1, 2, 3, 4). Default: 1")
    parser.add_argument("-o", "--output-pdf", help="Output PDF file path (default: ~/Downloads/Swiftember_2026_Week{N}_Report.pdf)")
    parser.add_argument("-t", "--title-sub", default="Official Swiftember Report", help="Subtitle under badge in header")
    args = parser.parse_args()

    roster = load_roster()
    aliases = load_aliases()

    if args.input and os.path.exists(args.input):
        with open(args.input, "r", encoding="utf-8") as f:
            raw_data = f.read()
    else:
        # Default to mock_strava_data.txt
        mock_file = os.path.join(BASE_DIR, "mock_strava_data.txt")
        if os.path.exists(mock_file):
            with open(mock_file, "r", encoding="utf-8") as f:
                raw_data = f.read()
        else:
            print("[ERROR] No input file provided and mock_strava_data.txt not found.")
            sys.exit(1)

    strava_entries = parse_strava_table(raw_data)
    matched_runners = process_swiftember_data(strava_entries, roster, aliases, week_num=args.week)

    # Generate HTML
    html_content = generate_html_report(matched_runners, week_num=args.week, badge_subtitle=args.title_sub)
    
    html_path = os.path.join(BASE_DIR, f"temp_report_week{args.week}.html")
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html_content)

    # Output PDF
    downloads_dir = os.path.expanduser("~/Downloads")
    default_pdf_path = os.path.join(downloads_dir, f"Swiftember_2026_Week{args.week}_Report.pdf")
    pdf_path = args.output_pdf if args.output_pdf else default_pdf_path

    chrome_cmd = [
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
        "--headless",
        "--disable-gpu",
        "--no-pdf-header-footer",
        f"--print-to-pdf={pdf_path}",
        f"file://{os.path.abspath(html_path)}"
    ]

    subprocess.run(chrome_cmd, check=True)
    print(f"\n[SUCCESS] Swiftember Week {args.week} Report successfully generated!")
    print(f"📄 PDF Output: {pdf_path}")
    print(f"📊 Processed: {len(matched_runners)} registered runners ({len([m for m in matched_runners if m['distance'] > 0])} active)")

if __name__ == "__main__":
    main()
