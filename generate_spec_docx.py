import os
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

def set_cell_background(cell, fill_hex):
    tcPr = cell._element.get_or_add_tcPr()
    shd = OxmlElement('w:shd')
    shd.set(qn('w:val'), 'clear')
    shd.set(qn('w:color'), 'auto')
    shd.set(qn('w:fill'), fill_hex)
    tcPr.append(shd)

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._element.get_or_add_tcPr()
    tcMar = OxmlElement('w:tcMar')
    for m, val in [('top', top), ('bottom', bottom), ('left', left), ('right', right)]:
        node = OxmlElement(f'w:{m}')
        node.set(qn('w:w'), str(val))
        node.set(qn('w:type'), 'dxa')
        tcMar.append(node)
    tcPr.append(tcMar)

def create_game_spec_docx(output_path):
    doc = docx.Document()
    
    # Page setup - Normal Margins (1 inch)
    sections = doc.sections
    for section in sections:
        section.top_margin = Inches(1)
        section.bottom_margin = Inches(1)
        section.left_margin = Inches(1)
        section.right_margin = Inches(1)
        
    # Styles
    styles = doc.styles
    normal_style = styles['Normal']
    normal_style.font.name = 'Arial'
    normal_style.font.size = Pt(10)
    normal_style.font.color.rgb = RGBColor(0x22, 0x22, 0x22)
    
    # Title
    title_p = doc.add_paragraph()
    title_run = title_p.add_run("GAME SPECIFICATION\nCASH VORTEX: TRIPLE POWER™")
    title_run.font.name = 'Arial'
    title_run.font.size = Pt(22)
    title_run.font.bold = True
    title_run.font.color.rgb = RGBColor(0x1A, 0x23, 0x7E) # Indigo
    title_p.paragraph_format.space_after = Pt(4)
    
    # Subtitle
    sub_p = doc.add_paragraph()
    sub_run = sub_p.add_run("Game Specification for Frontend Developers, Game Designers & QA | Version 3.0")
    sub_run.font.size = Pt(10.5)
    sub_run.font.italic = True
    sub_run.font.color.rgb = RGBColor(0x55, 0x55, 0x55)
    sub_p.paragraph_format.space_after = Pt(14)
    
    # Metadata Box (Table)
    meta_table = doc.add_table(rows=4, cols=2)
    meta_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_data = [
        ("Target Platforms", "Mobile (iOS/Android), Tablet & Desktop (HTML5 / WebGL)"),
        ("Reel Layout", "5×5 Matrix (25 Reel Positions) with Central Wild Star at (2,2)"),
        ("Paylines & Mechanics", "12 Slingo Lines, 3-Spin Coin Lifespans, 3-Pot Expired Coins Engine"),
        ("Key Features", "3-Pot X-Wheels (No X Symbol), Center Wild Wheel Bonus, 5×5 Lock & Slingo™ Bonus")
    ]
    for i, (k, v) in enumerate(meta_data):
        row = meta_table.rows[i]
        c0, c1 = row.cells[0], row.cells[1]
        c0.text = k
        c0.paragraphs[0].runs[0].font.bold = True
        c0.paragraphs[0].runs[0].font.size = Pt(9.5)
        c0.paragraphs[0].runs[0].font.color.rgb = RGBColor(0x1A, 0x23, 0x7E)
        c1.text = v
        c1.paragraphs[0].runs[0].font.size = Pt(9.5)
        set_cell_background(c0, "E8EAF6")
        set_cell_background(c1, "F5F5F5")
        set_cell_margins(c0, 80, 80, 120, 120)
        set_cell_margins(c1, 80, 80, 120, 120)
        
    doc.add_paragraph().paragraph_format.space_after = Pt(10)
    
    def add_heading_1(text):
        h = doc.add_paragraph()
        run = h.add_run(text)
        run.font.name = 'Arial'
        run.font.size = Pt(14)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x1A, 0x23, 0x7E)
        h.paragraph_format.space_before = Pt(16)
        h.paragraph_format.space_after = Pt(6)
        return h

    def add_heading_2(text):
        h = doc.add_paragraph()
        run = h.add_run(text)
        run.font.name = 'Arial'
        run.font.size = Pt(12)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x00, 0x79, 0x6B) # Teal
        h.paragraph_format.space_before = Pt(12)
        h.paragraph_format.space_after = Pt(4)
        return h

    # Section 0: Document Revision History & Changelog
    add_heading_1("Document Revision History & Changelog")
    rev_table = doc.add_table(rows=1, cols=4)
    rev_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    r_hdr = rev_table.rows[0]
    r_hdr_titles = ["Version", "Date", "Summary of Changes", "Author / Team"]
    for idx, text in enumerate(r_hdr_titles):
        cell = r_hdr.cells[idx]
        cell.text = text
        cell.paragraphs[0].runs[0].font.bold = True
        cell.paragraphs[0].runs[0].font.size = Pt(9)
        cell.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_background(cell, "1A237E")
        set_cell_margins(cell, 80, 80, 100, 100)

    rev_data = [
        ("v3.0", "Sep 2026", 
         "3-Pot Dynamic Expired Coins Overhaul & X Symbol Removal:\n"
         "• X Symbol Removed: Completely eliminated X symbol from base game and bonus reels.\n"
         "• 3-Pot Expired Coins Engine: At spin start, expired coins fly to 3 top wheel pots (Mini, Mega, Ultra), expanding them visually.\n"
         "• Dynamic Trigger Formula: Single random roll determines if a pot triggers a wheel bonus (at most 1 per spin).\n"
         "• Independent X-Wheels: Standalone prize wheels; all Upgrade slices removed.\n"
         "• Lock & Slingo™ Streamlined: Removed X-Wheels and coin flight from bonus (pure Hold & Spin respin round).\n"
         "• Landing Weight Refinement: Set weight of 2 Cash Coins to 0 for <=5 spaces left to prevent placement overflow.\n"
         "• 95.50% Rebalance: Full mathematical calibration maintaining target RTP of 95.50% ±0.25%.", 
         "Math & Game Design"),
        ("v2.0", "Aug 2026", 
         "Slingo Ladder & Center Wheel Integration:\n"
         "• Lock & Slingo™ Ladder: Added full 12-rank Slingo Ladder prize achievements awarded at bonus conclusion.\n"
         "• Center Wild Wheel Bonus: Introduced center wheel triggered by completed Slingo lines passing through Central Wild Star at (2,2).\n"
         "• Jackpot Isolation Rule: Formulated strict immunity of Mini/Mega/Ultra Jackpots against all modifier boosts, collections, and multipliers.\n"
         "• Live Config Sync: Connected dynamic Google Drive configuration loader for simulation and runtime tuning.", 
         "Math & Engineering"),
        ("v1.0", "Jun 2026", 
         "Initial Game Architecture:\n"
         "• 5×5 Grid Matrix with 12 Slingo Paylines (5 Horizontal, 5 Vertical, 2 Diagonal).\n"
         "• Persistent 3-Spin Coin Lifespan mechanic and line-sharing life reset.\n"
         "• 3 Tiers of Modifiers: Mini, Mega, and Ultra Cash Strikes & Cash Vortexes.\n"
         "• On-reel X-Symbol landing triggering 3-tiered X-Wheels with slice upgrade advancement.\n"
         "• 5×5 Lock & Win Respin Bonus Game.", 
         "Game Design")
    ]
    for row_idx, data in enumerate(rev_data):
        row = rev_table.add_row()
        bg_col = "FFFFFF" if row_idx % 2 == 0 else "F9F9F9"
        for col_idx, txt in enumerate(data):
            cell = row.cells[col_idx]
            cell.text = txt
            cell.paragraphs[0].runs[0].font.size = Pt(8.5)
            if col_idx == 0:
                cell.paragraphs[0].runs[0].font.bold = True
            set_cell_background(cell, bg_col)
            set_cell_margins(cell, 60, 60, 80, 80)

    doc.add_paragraph().paragraph_format.space_after = Pt(10)

    # Section 1
    add_heading_1("1. Executive Summary & High Concept")
    p1 = doc.add_paragraph(
        "Cash Vortex: Triple Power™ combines the excitement of Slingo line completion with persistent locking cash symbols, "
        "explosive modifier mechanics (Strikes and Vortexes), an innovative 3-Pot Dynamic X-Wheel Bonus System driven by flying expired coins, "
        "an independent center-reel wheel bonus, and a dedicated 5×5 Lock & Slingo™ respin bonus feature."
    )
    p1.paragraph_format.space_after = Pt(6)
    p2 = doc.add_paragraph(
        "Unlike traditional slots where symbols vanish on each spin, symbols in Cash Vortex hold an active 3-spin lifespan, staying locked on the reels "
        "to help players complete 5-symbol Slingo Lines (horizontal, vertical, and diagonal). When Slingo lines complete, they award the sum of all cash values along that line. "
        "If a completed line passes through the Central Wild Star, it activates the Center Wild Wheel Bonus."
    )
    p2.paragraph_format.space_after = Pt(6)
    p3 = doc.add_paragraph(
        "At the start of each spin, all expired coins (both un-won coins whose 3-spin lifespan has concluded and coins that won on the previous spin) "
        "fly up to the 3 X-Wheel Pots (Mini, Mega, and Ultra) above the reels. Each expired coin contributes to a pot, visually expanding it and dynamically "
        "increasing the probability of triggering that specific wheel bonus!"
    )
    p3.paragraph_format.space_after = Pt(12)

    # Section 2
    add_heading_1("2. Screen Layout, UI & Visual Hierarchy")
    doc.add_paragraph(
        "The game interface is structured into three primary visual zones:"
    )
    ui_points = [
        ("Top-of-Reels 3-Pot X-Wheels HUD: ", "Displays the 3 interactive wheel pots: Pot 1 (Mini Wheel), Pot 2 (Mega Wheel), and Pot 3 (Ultra Wheel). Expired coins visibly fly into these pots and pulse them."),
        ("5×5 Main Grid Matrix: ", "Contains 25 cell positions indexed from (0,0) at top-left to (4,4) at bottom-right. The central position (2,2) is permanently occupied by the Central Wild Star in the base game."),
        ("Symbol Life Indicators: ", "Every active coin on the reels features an animated visual life badge (countdown: 3 -> 2 -> 1 -> Pop / Fly to Pots)."),
        ("Slingo Payline Overlays: ", "12 predefined winning lines (5 Horizontal Rows, 5 Vertical Columns, and 2 Diagonals).")
    ]
    for bold_txt, norm_txt in ui_points:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 3
    add_heading_1("3. Symbol Catalog & Feature Definitions")
    sym_table = doc.add_table(rows=1, cols=4)
    sym_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    hdr = sym_table.rows[0]
    hdr_titles = ["Symbol Name", "Visual Identifier", "Base Cash Value", "Special Function & Gameplay Behavior"]
    for idx, text in enumerate(hdr_titles):
        cell = hdr.cells[idx]
        cell.text = text
        cell.paragraphs[0].runs[0].font.bold = True
        cell.paragraphs[0].runs[0].font.size = Pt(9)
        cell.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_background(cell, "1A237E")
        set_cell_margins(cell, 80, 80, 100, 100)

    symbols_data = [
        ("Central Wild Star", "Gold Glowing Star at (2,2)", "0.0x (No cash)", "Permanent Wild in base game. Completes any row, column, or diagonal crossing the center. Never expires and cannot be destroyed."),
        ("Blank", "Dark / Transparent Cell", "0.0x", "Empty space where newly spun symbols can land."),
        ("Cash Coin", "Bronze/Silver/Gold Coin", "0.2x – 5.0x Bet", "Standard cash prize coin. Starts with 3 Lives. When expired or won, flies to one of the 3 top wheel pots."),
        ("Jackpot Coin", "Ruby / Sapphire / Diamond", "Mini (5x), Mega (50x), Ultra (500x)", "Fixed Jackpot coin with 3 Lives. Strictly isolated from modifiers (never boosted or collected)."),
        ("Mini Strike", "Blue Lightning Coin", "0.2x – 5.0x Bet", "On landing, adds its cash value to all 4 orthogonal neighboring cells (Up, Down, Left, Right)."),
        ("Mega Strike", "Purple Lightning Coin", "0.2x – 5.0x Bet", "On landing, adds its cash value to all symbols sharing any Slingo line with this cell."),
        ("Ultra Strike", "Gold Lightning Coin", "0.2x – 5.0x Bet", "On landing, adds its cash value to all valuable symbols across the entire 5x5 grid."),
        ("Mini Vortex", "Blue Swirling Portal", "Starts at 1.0x", "On landing, gathers and sums cash values from all 4 orthogonal neighbors into itself."),
        ("Mega Vortex", "Purple Swirling Portal", "Starts at 2.0x", "On landing, gathers and sums cash values from all line-sharing symbols into itself."),
        ("Ultra Vortex", "Gold Swirling Portal", "Starts at 5.0x", "On landing, gathers and sums cash values from all coins on the entire grid into itself.")
    ]
    for row_idx, data in enumerate(symbols_data):
        row = sym_table.add_row()
        bg_col = "FFFFFF" if row_idx % 2 == 0 else "F9F9F9"
        for col_idx, txt in enumerate(data):
            cell = row.cells[col_idx]
            cell.text = txt
            cell.paragraphs[0].runs[0].font.size = Pt(8.5)
            if col_idx == 0:
                cell.paragraphs[0].runs[0].font.bold = True
            set_cell_background(cell, bg_col)
            set_cell_margins(cell, 60, 60, 80, 80)

    p_note = doc.add_paragraph()
    p_note.paragraph_format.space_before = Pt(6)
    p_note_run = p_note.add_run("Important Note: ")
    p_note_run.font.bold = True
    p_note.add_run("The X Symbol has been completely removed from this game. All wheel bonus triggering is driven exclusively by the 3-pot flying expired coins engine.")

    # Section 4
    add_heading_1("4. Base Game Mechanics & Execution Sequence")
    doc.add_paragraph(
        "When the player presses the SPIN button, frontend animations and client state transitions MUST execute in the following exact chronological sequence:"
    )
    steps = [
        ("Step 1: Expired Coins Pot Flight & Dynamic Trigger Phase",
         "1. Identify Expired Coins: Coins that won on the previous spin or reached the end of their 3-spin lifespan (LifeRemaining <= 1) expire.\n"
         "2. Pot Destination Sampling: Each expired coin independently samples which top pot it flies to (Pot 1 Mini, Pot 2 Mega, Pot 3 Ultra).\n"
         "3. Visual Pot Flight: Expired coins lift from the grid and fly in glowing arcs into their respective pots, causing the pots to expand/pulse.\n"
         "4. Dynamic Single-Roll Trigger: Based on coin counts N1, N2, N3, dynamic weights are calculated:\n"
         "   - Weight Pot 1 = N1 * 1000\n"
         "   - Weight Pot 2 = N2 * 100\n"
         "   - Weight Pot 3 = N3 * 20\n"
         "   - Weight No-Trigger = 100000\n"
         "   A single random roll against Total Weight determines if Pot 1, Pot 2, Pot 3, or No Wheel triggers (at most one wheel triggers per spin).\n"
         "5. Grid Cleanup: Expired coins disappear; surviving coins have their life counter decremented by 1 (3 -> 2, or 2 -> 1)."),
        ("Step 2: Symbol Landing & Wheel Execution Phase",
         "Case A (Wheel Bonus Triggered):\n"
         "  1. Special symbol selection is bypassed (no special symbols land on this spin).\n"
         "  2. Empty grid positions fill with Cash Coins and Blanks according to the active table.\n"
         "  3. The triggered wheel (Mini, Mega, or Ultra) immediately spins and awards its prize (Multipliers, Ultra Strikes, Jackpots, or Lock & Slingo).\n"
         "Case B (No Wheel Bonus Triggered):\n"
         "  1. Special symbol selection executes normally (pool includes Jackpot Coins, Strikes, Vortexes).\n"
         "  2. Empty grid positions fill with Cash Coins and Blanks.\n"
         "  3. Modifiers execute in order (Strikes fire lightning boosts first, Vortexes collect values second)."),
        ("Step 3: Symbol Life Cycle Reset Phase",
         "Any existing symbol on the reels sharing any Slingo line with any newly landed symbol has its lifespan reset back to 3 Lives!"),
        ("Step 4: 12 Slingo Lines Evaluation Phase",
         "1. A line completes when all 5 positions contain non-blank symbols.\n"
         "2. Line Payout: Player receives the sum of all cash values along that line.\n"
         "3. Intersection Rule: Coins belonging to multiple winning lines pay out for each completed line.\n"
         "4. Winning coins are highlighted and marked to fly to pots at the start of next spin."),
        ("Step 5: Center Wild Wheel Bonus Trigger Phase",
         "If any completed Slingo line crosses through the Central Wild Star at (2,2) (Center Row, Center Column, Main Diagonal, or Anti-Diagonal), the Center Wild Wheel Bonus is triggered once.")
    ]
    for step_title, step_body in steps:
        add_heading_2(step_title)
        p = doc.add_paragraph(step_body)
        p.paragraph_format.space_after = Pt(6)

    # Section 5
    add_heading_1("5. Center Wild Wheel Bonus")
    doc.add_paragraph(
        "Triggered when a completed winning Slingo line crosses the central wild star at (2,2). "
        "An ornate Bonus Wheel appears as an overlay in the center of the reels and spins once to award:"
    )
    wheel_slices = [
        ("Instant Cash Multipliers (1x, 2x, 3x, 4x, 5x): ", "Directly pays 1x to 5x player's total bet."),
        ("Mini Jackpot: ", "Awards 5x total bet."),
        ("Mega Jackpot: ", "Awards 50x total bet."),
        ("Ultra Jackpot: ", "Awards 500x total bet."),
        ("Lock & Slingo: ", "Launches the 5×5 Lock & Slingo™ Bonus Game!")
    ]
    for bold_txt, norm_txt in wheel_slices:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 6
    add_heading_1("6. Reel-Top X-Wheels (Independent Bonus Wheels)")
    doc.add_paragraph(
        "Each of the 3 top wheels operates as an independent bonus wheel awarded directly from its respective pot (no upgrade slices):"
    )
    xwheel_points = [
        ("Mini Wheel (Wheel 1 - 9 Slices): ", "Contains x2, 5, 1, x3, Mini Jackpot (5x), 3, x2, 2, 4. Multipliers apply to all grid coins; fixed numbers add to all grid coins."),
        ("Mega Wheel (Wheel 2 - 9 Slices): ", "Contains x4, Lock & Slingo, 2, x5, Mini Jackpot (5x), 3, x3, Mega Jackpot (50x), 4."),
        ("Ultra Wheel (Wheel 3 - 10 Slices): ", "Contains x5, Mini Jackpot (5x), x10, Mega Jackpot (50x), x5, Lock & Slingo, x10, Ultra Jackpot (500x), x5, Lock & Slingo.")
    ]
    for bold_txt, norm_txt in xwheel_points:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 7
    add_heading_1("7. Lock & Slingo™ Bonus Game")
    doc.add_paragraph(
        "The Lock & Slingo™ Bonus Game is a 5×5 persistent Lock & Win Hold-and-Spin feature with cascading respins:"
    )
    bonus_rules = [
        ("25 Empty Starting Spaces: ", "The bonus begins with an empty 5×5 board. The Central Wild Star does NOT exist in the bonus round (position (2,2) is a standard empty space)."),
        ("Player Lives (3 Respins): ", "Player begins with 3 Lives. If >=1 symbol lands on a spin, lives reset to 3. A blank spin decrements lives by 1."),
        ("Permanent Symbol Locking: ", "Symbols in the bonus never expire. All landed coins remain permanently locked until the bonus ends (no coins fly to pots)."),
        ("No Wheels / No X Symbols: ", "All wheels and X symbols are removed from the bonus round."),
        ("Active In-Bonus Modifiers: ", "Strikes boost locked coins, and Vortexes gather locked coins according to their standard area of effect."),
        ("Bonus End Conditions: ", "Ends on 3 consecutive blanks (Lives = 0) OR Full House (all 25 positions filled with locked coins).")
    ]
    for bold_txt, norm_txt in bonus_rules:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    add_heading_2("Slingo Pay Ladder Awards (Evaluated at End of Bonus)")
    ladder_table = doc.add_table(rows=1, cols=3)
    ladder_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    l_hdr = ladder_table.rows[0]
    l_hdr.cells[0].text = "Completed Slingo Lines"
    l_hdr.cells[1].text = "Awarded Ladder Prize"
    l_hdr.cells[2].text = "Prize Description & Gameplay Effect"
    for c in l_hdr.cells:
        c.paragraphs[0].runs[0].font.bold = True
        c.paragraphs[0].runs[0].font.size = Pt(9)
        c.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_background(c, "00796B")
        set_cell_margins(c, 80, 80, 100, 100)

    ladder_data = [
        ("0 Lines", "No ladder prize", "No additional prize awarded"),
        ("1 Line", "Mini Strike 1", "Adds +1.0x to 4 orthogonal neighbors"),
        ("2 Lines", "Mini Vortex", "Base 1.0x + gathers 4 orthogonal neighbors"),
        ("3 Lines", "Mini Jackpot", "Awards fixed 5x Total Bet"),
        ("4 Lines", "Mega Vortex", "Base 2.0x + gathers all line-sharing coins"),
        ("5 Lines", "Mega Strike 2", "Adds +2.0x to all line-sharing coins"),
        ("6 Lines", "Multiplier x2", "Multiplies all non-jackpot locked coins by 2x"),
        ("7 Lines", "Ultra Vortex", "Base 5.0x + gathers all coins across entire board"),
        ("8 Lines", "Multiplier x3", "Multiplies all non-jackpot locked coins by 3x"),
        ("9 Lines", "Mega Jackpot", "Awards fixed 50x Total Bet"),
        ("10 Lines", "Ultra Strike 5", "Adds +5.0x to all coins across entire board"),
        ("11 Lines", "Skipped", "Geometry rule - mathematically impossible on 5x5 grid"),
        ("12 Lines (Full House)", "Ultra Jackpot", "Awards fixed 500x Total Bet")
    ]
    for row_idx, (k, v, d) in enumerate(ladder_data):
        row = ladder_table.add_row()
        bg_col = "FFFFFF" if row_idx % 2 == 0 else "F4FBF9"
        row.cells[0].text = k
        row.cells[1].text = v
        row.cells[2].text = d
        for c in row.cells:
            c.paragraphs[0].runs[0].font.size = Pt(8.5)
            set_cell_background(c, bg_col)
            set_cell_margins(c, 60, 60, 80, 80)
        row.cells[0].paragraphs[0].runs[0].font.bold = True

    p_payout = doc.add_paragraph()
    p_payout.paragraph_format.space_before = Pt(8)
    p_payout.add_run("Final Bonus Payout Formula: ").font.bold = True
    p_payout.add_run("Total Win = Sum of all Locked Grid Cash Values + Highest Achieved Slingo Ladder Prize.")

    # Section 8
    add_heading_1("8. Critical Isolation Rules & Strict Edge Cases")
    edge_cases = [
        ("A. The Jackpot Isolation Rule (CRITICAL): ", 
         "Jackpot Coins (Mini, Mega, Ultra) are 100% immune to all game modifiers. Strikes do not add cash to Jackpot coins, Vortexes do not collect Jackpot coins, and Wheel Multipliers do not multiply Jackpot coins."),
        ("B. The Central Wild Star Isolation Rule: ",
         "The star at (2,2) has 0.0 cash value, never expires, cannot be destroyed or multiplied, is not collected as cash by vortexes, but acts as a universal wild completing lines."),
        ("C. The 11-Slingo Geometric Skip Rule: ",
         "Filling 24/25 spaces creates 10 lines. Marking the 25th space simultaneously completes both its row and column, jumping the line count from 10 directly to 12. 11 lines can never mathematically exist."),
        ("D. Multi-Line Coin Stacking: ",
         "Coins belonging to intersecting winning lines pay out for each line they belong to."),
        ("E. Single Center Trigger: ",
         "Multiple center-crossing lines on a single spin activate the Center Wild Wheel Bonus exactly once.")
    ]
    for bold_txt, norm_txt in edge_cases:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(4)

    # Section 9 - Frontend Animation & SFX Table
    add_heading_1("9. Frontend Animation & Audio Choreography")
    vfx_table = doc.add_table(rows=1, cols=3)
    vfx_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    v_hdr = vfx_table.rows[0]
    v_hdr.cells[0].text = "Game Event"
    v_hdr.cells[1].text = "Visual VFX / Animation"
    v_hdr.cells[2].text = "Sound Effect (SFX)"
    for c in v_hdr.cells:
        c.paragraphs[0].runs[0].font.bold = True
        c.paragraphs[0].runs[0].font.size = Pt(9)
        c.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_background(c, "1A237E")
        set_cell_margins(c, 80, 80, 100, 100)

    vfx_data = [
        ("Coin Landing", "Impact slam with gold dust particle burst; numeric life badge appears (3).", "Metallic coin drop / heavy clink."),
        ("Strike Trigger", "Electric arcs shoot from Strike symbol across target cells with glowing impact rings.", "Crackling thunder / electric zap."),
        ("Vortex Trigger", "Swirling gravity well vortex with particle streams flowing into the portal center.", "Deep whoosh / resonant vacuum pulse."),
        ("Expired Coin Pot Flight", "Expired coins lift from grid and fly in glowing arcs into top Mini, Mega, or Ultra pots.", "Whooshing sparkle arc / pot coin clink."),
        ("Pot Growth / Expansion", "Targeted top wheel pot pulses, shakes, and visibly grows larger with aura.", "Rising shimmer tone / resonant hum."),
        ("X-Wheel Pot Trigger", "Triggered top wheel pot bursts with golden fireworks and expands to spin.", "Grand triumphal fanfare / brass swell."),
        ("Life Reset", "Glowing pulse travels along connected Slingo line; coin life counters flash back to 3.", "Magical sparkle chime."),
        ("Slingo Line Win", "Gold line tracing with glowing border; coin numbers fly into win meter.", "Cash register bell / crescendo chords."),
        ("Center Wheel Trigger", "Center Wild Star explodes in golden rays; Center Wheel expands onto screen.", "Dramatic brass fanfare."),
        ("Lock & Slingo Intro", "Base grid flips away into dark galaxy vortex; 3 heart life meters ignite.", "Eerie thunder transition / orchestral swell."),
        ("Full House Win", "Full screen fireworks, gold coin shower, flashing jackpot banner.", "Epic grand jackpot celebratory theme.")
    ]
    for row_idx, (ev, vis, sfx) in enumerate(vfx_data):
        row = vfx_table.add_row()
        bg_col = "FFFFFF" if row_idx % 2 == 0 else "F9F9F9"
        row.cells[0].text = ev
        row.cells[1].text = vis
        row.cells[2].text = sfx
        for c in row.cells:
            c.paragraphs[0].runs[0].font.size = Pt(8.5)
            set_cell_background(c, bg_col)
            set_cell_margins(c, 60, 60, 80, 80)
        row.cells[0].paragraphs[0].runs[0].font.bold = True

    # Save Document
    doc.save(output_path)
    print(f"Successfully generated Google Docs / Word specification at: {output_path}")

if __name__ == "__main__":
    output_docx = "/Users/bo.wang/Downloads/Cash_Vortex_Triple_Power_Game_Spec_V3.0.docx"
    create_game_spec_docx(output_docx)
