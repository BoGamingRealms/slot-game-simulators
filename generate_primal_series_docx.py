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

def create_primal_series_docx(output_path):
    doc = docx.Document()
    
    # Page setup - 1 inch margins
    for section in doc.sections:
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
    
    # Document Title
    title_p = doc.add_paragraph()
    title_run = title_p.add_run("GAME DESIGN & FRANCHISE FRAMEWORK\nFIRE PRIMAL™ 4-GAME SERIES")
    title_run.font.name = 'Arial'
    title_run.font.size = Pt(22)
    title_run.font.bold = True
    title_run.font.color.rgb = RGBColor(0xB7, 0x1C, 0x1C) # Deep Flame Red
    title_p.paragraph_format.space_after = Pt(4)
    
    # Subtitle
    sub_p = doc.add_paragraph()
    sub_run = sub_p.add_run("Comparative Game Specifications & Feature Differentiators for Elephant, Tiger, Gorilla & Rhino | Version 1.0")
    sub_run.font.size = Pt(10.5)
    sub_run.font.italic = True
    sub_run.font.color.rgb = RGBColor(0x55, 0x55, 0x55)
    sub_p.paragraph_format.space_after = Pt(14)
    
    # Metadata Box (Table)
    meta_table = doc.add_table(rows=4, cols=2)
    meta_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_data = [
        ("Series Franchise Titles", "Fire Primal Elephant, Fire Primal Tiger, Fire Primal Gorilla, Fire Primal Rhino"),
        ("Target Platforms", "Mobile (iOS/Android), Tablet & Desktop (HTML5 / WebGL)"),
        ("Shared Core DNA", "Multi-Stage Base Game, Fire Core Cash & Collector, 4 Feature Bonuses, Jackpot Wheel"),
        ("Engine Architecture", "Unified SlotFramework C# Modular Engine & Parameterized Google Sheets Loader")
    ]
    for i, (k, v) in enumerate(meta_data):
        row = meta_table.rows[i]
        c0, c1 = row.cells[0], row.cells[1]
        c0.text = k
        c0.paragraphs[0].runs[0].font.bold = True
        c0.paragraphs[0].runs[0].font.size = Pt(9.5)
        c0.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xB7, 0x1C, 0x1C)
        c1.text = v
        c1.paragraphs[0].runs[0].font.size = Pt(9.5)
        set_cell_background(c0, "FFEBEE") # Light Red Tint
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
        run.font.color.rgb = RGBColor(0xB7, 0x1C, 0x1C) # Deep Flame Red
        h.paragraph_format.space_before = Pt(16)
        h.paragraph_format.space_after = Pt(6)
        return h

    def add_heading_2(text):
        h = doc.add_paragraph()
        run = h.add_run(text)
        run.font.name = 'Arial'
        run.font.size = Pt(11.5)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0xE6, 0x51, 0x00) # Deep Orange
        h.paragraph_format.space_before = Pt(12)
        h.paragraph_format.space_after = Pt(4)
        return h

    def add_heading_3(text):
        h = doc.add_paragraph()
        run = h.add_run(text)
        run.font.name = 'Arial'
        run.font.size = Pt(10.5)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x2E, 0x7D, 0x32) # Dark Green / Accent
        h.paragraph_format.space_before = Pt(8)
        h.paragraph_format.space_after = Pt(2)
        return h

    # Section 1: Executive Summary
    add_heading_1("1. Executive Summary & Franchise Vision")
    p1 = doc.add_paragraph(
        "The Fire Primal™ series is designed as a premier 4-game slot franchise centered around the raw primal power of nature's "
        "four most iconic apex beasts: the Elephant, the Tiger, the Gorilla, and the Rhino. "
        "Each game builds upon a rock-solid, proven core framework (Multi-Stage progression, Fire Core Cash & Collector, 4 Feature Bonuses, and a Jackpot Bonus Wheel), "
        "ensuring instant familiarity for players across the series."
    )
    p1.paragraph_format.space_after = Pt(6)
    p2 = doc.add_paragraph(
        "To provide distinct player experiences, each title introduces a unique mechanical expression tailored to the natural strengths, "
        "temperament, and hunting/survival behavior of its hero beast. This creates four highly differentiated volatility and gameplay profiles "
        "ranging from balanced cascading collectors to ultra-volatile claw multipliers."
    )
    p2.paragraph_format.space_after = Pt(12)

    # Section 2: Shared Series DNA
    add_heading_1("2. The Shared Series DNA (Common Pillars Across All 4 Games)")
    doc.add_paragraph(
        "Every game in the Fire Primal™ series shares four fundamental structural pillars:"
    )
    dna_points = [
        ("Pillar 1 - Multi-Stage Base Game Progression: ", 
         "Players embark on a journey across 3 evolving stages (Stage 1 -> Stage 2 -> Stage 3). Advancing through stages enriches reelsets with higher densities of high symbols, enhanced Fire Cores, and atmospheric visual transformations."),
        ("Pillar 2 - Fire Core Cash & Collector Mechanics: ", 
         "Fire Core symbols land across the reels carrying direct cash multipliers (e.g. 0.2x to 50x bet). When an animal-specific Collector symbol lands on the same spin, it immediately gathers all active Fire Cores on screen with a unique animal modifier."),
        ("Pillar 3 - 4 Dedicated Bonus Feature Modes: ", 
         "Each game features four high-action bonus rounds: (1) An Animal-Themed Zone / Respin Feature, (2) A Symbol Mutation / Action Feature, (3) An Apex Free Spins Round, and (4) A 5×5 Lock & Slingo™ Hold-and-Spin Feature with a full 12-rank Slingo Ladder."),
        ("Pillar 4 - The Jackpot Bonus Wheel: ", 
         "An iconic multi-tier Jackpot Wheel awarding Mini, Minor, Major, and Grand/Ultra fixed jackpot jackpots, accessible across all 4 titles.")
    ]
    for bold_txt, norm_txt in dna_points:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(4)

    doc.add_paragraph().paragraph_format.space_after = Pt(10)

    # Section 3: Comparative Series Matrix
    add_heading_1("3. Series Comparison & Feature Matrix")
    matrix_table = doc.add_table(rows=1, cols=5)
    matrix_table.alignment = WD_TABLE_ALIGNMENT.CENTER
    m_hdr = matrix_table.rows[0]
    m_hdr_titles = ["Game Title", "Hero Persona", "Unique Collector Action", "Signature Feature Flavor", "Volatility"]
    for idx, text in enumerate(m_hdr_titles):
        cell = m_hdr.cells[idx]
        cell.text = text
        cell.paragraphs[0].runs[0].font.bold = True
        cell.paragraphs[0].runs[0].font.size = Pt(8.5)
        cell.paragraphs[0].runs[0].font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
        set_cell_background(cell, "B71C1C")
        set_cell_margins(cell, 80, 80, 90, 90)

    matrix_data = [
        ("Fire Primal Elephant™", "The Colossal Titan\n(Massive Size & Weight)", "Ground Stomp:\nExpands grid height from 5×3 to 5×4 / 5×5 for that spin", "Roaming 2×2 -> 5×5 Primal Zone & Colossal 3×3/4×4 Blocks", "Medium-High"),
        ("Fire Primal Tiger™", "The Stealth Predator\n(Agility & Claws)", "Claw Slash & Pounce:\nSlashes Fire Cores to apply x2, x3, x5, x10 Multipliers", "Split Symbol Slashes & Predator Low-Symbol Elimination", "Very High"),
        ("Fire Primal Gorilla™", "The Jungle King\n(Brute Strength & Fury)", "Chest-Beat Shockwave:\nUpgrades coin tiers (e.g. 0.5x -> 2x, 2x -> 10x) before collecting", "Cascading Ground Smashes & Rage Meter Respins", "Medium"),
        ("Fire Primal Rhino™", "The Armored Juggernaut\n(Horn Charge & Armor)", "Horn Charge:\nCharges across row, collecting cores & leaving Wilds", "Armored Multi-Hit Cores & Piercing Line Charges", "High")
    ]
    for row_idx, data in enumerate(matrix_data):
        row = matrix_table.add_row()
        bg_col = "FFFFFF" if row_idx % 2 == 0 else "FFF8F8"
        for col_idx, txt in enumerate(data):
            cell = row.cells[col_idx]
            cell.text = txt
            cell.paragraphs[0].runs[0].font.size = Pt(8)
            if col_idx == 0:
                cell.paragraphs[0].runs[0].font.bold = True
            set_cell_background(cell, bg_col)
            set_cell_margins(cell, 60, 60, 70, 70)

    doc.add_paragraph().paragraph_format.space_after = Pt(12)

    # Section 4: Deep Dive Game 1 - Elephant
    add_heading_1("4. Game 1: Fire Primal Elephant™ — 'The Colossal Titan'")
    p_el = doc.add_paragraph(
        "Theme & Story: Set in the ancient savannah, the Elephant represents immense weight, unwavering territory dominance, "
        "and earth-shaking footsteps. Its mechanics focus on heavy impact, expanding grid frames, and colossal symbol blocks."
    )
    p_el.paragraph_format.space_after = Pt(4)

    add_heading_2("Elephant Feature Specifications:")
    el_features = [
        ("Collector Action ('Ground Stomp'): ", 
         "When the Elephant Collector lands on reel 5, it stomps the earth with heavy screenshake, expanding the grid height (from 5×3 to 5×4 or 5×5) for that spin, uncovering hidden subterranean Fire Cores."),
        ("Feature 1 - Primal Zone (Roaming Herd): ", 
         "A glowing roaming zone that moves across the 5×5 grid. Collecting bananas/cores within the zone levels it up from 2×2 -> 3×3 -> 4×4 -> 5×5, collecting all coins inside its borders."),
        ("Feature 2 - Colossal Spins: ", 
         "Reels 2, 3, and 4 merge into a colossal reel landing giant 3×3 Elephant symbols, 3×3 Fire Core blocks, or giant Wilds for guaranteed line connections."),
        ("Feature 3 - Apex Elephant Stampede: ", 
         "All low-paying symbols are replaced with herd symbols. Elephant Collectors lock in place and stomp an incremental +1x cash boost onto all visible coins on every spin."),
        ("Feature 4 - Lock & Slingo™ (Heavyweight Ladder): ", 
         "Standard 5×5 Lock & Slingo round where Slingo ladder milestone prizes deliver heavy global board multipliers (up to 500x Ultra Jackpot for Full House).")
    ]
    for bold_txt, norm_txt in el_features:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 5: Deep Dive Game 2 - Tiger
    add_heading_1("5. Game 2: Fire Primal Tiger™ — 'The Stealth Predator'")
    p_tg = doc.add_paragraph(
        "Theme & Story: Set in the dense mystical bamboo jungle, the Tiger represents silent stealth, razor-sharp lethality, "
        "and explosive ambush strikes. Its mechanics focus on slashing multipliers, splitting symbols, and high-octane volatility."
    )
    p_tg.paragraph_format.space_after = Pt(4)

    add_heading_2("Tiger Feature Specifications:")
    tg_features = [
        ("Collector Action ('Claw Slash & Pounce'): ", 
         "When the Tiger Collector lands, it pounces across the reels and delivers razor claw slashes to all landed Fire Cores, applying random Multipliers (x2, x3, x5, x10) to each core value before vacuuming them into the win meter."),
        ("Feature 1 - Shadow Ambush Respins: ", 
         "The screen dims into nocturnal stealth mode. As Fire Cores land, the Tiger stalks them from the shadows, locking them with persistent claw multiplier marks that increase by +1x every time another core lands on that column."),
        ("Feature 2 - Frenzy Slash Spins (Split Symbols): ", 
         "Fiery tiger claws slash across random reels on every spin, slicing symbols in two. This creates up to 10-of-a-kind paylines and doubles the capacity for Fire Cores on sliced reel positions."),
        ("Feature 3 - Apex Predator Hunt: ", 
         "Every winning spin hunts down and permanently eliminates the lowest paying card suit from the reels for the remainder of the free spins. By spin 5, only Top Tiger symbols, Wilds, and high Fire Cores remain."),
        ("Feature 4 - Lock & Slingo™ (Tiger Strike Ladder): ", 
         "Completed Slingo lines trigger fierce claw slashes across the ladder, applying random x2 to x5 multipliers directly to the ladder jackpot payouts.")
    ]
    for bold_txt, norm_txt in tg_features:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 6: Deep Dive Game 3 - Gorilla
    add_heading_1("6. Game 3: Fire Primal Gorilla™ — 'The Jungle King'")
    p_gr = doc.add_paragraph(
        "Theme & Story: Set in the volcanic mountain rainforest, the Gorilla embodies raw primal fury, destructive chest-beating "
        "shockwaves, and relentless territorial protection. Its mechanics focus on coin value upgrades, cascades, and rage accumulation."
    )
    p_gr.paragraph_format.space_after = Pt(4)

    add_heading_2("Gorilla Feature Specifications:")
    gr_features = [
        ("Collector Action ('Chest-Beat Shockwave'): ", 
         "When the Gorilla Collector lands, it beats its chest with thunderous sound effects, sending a fiery shockwave across the grid that upgrades all low-tier Fire Cores to higher tiers (e.g. 0.5x -> 2x, 2x -> 10x, 5x -> 50x) before collecting."),
        ("Feature 1 - Primal Fury Meter Respins: ", 
         "Non-winning spins and landed banana tokens fill a fiery 'Fury Meter'. When fully charged, the Gorilla goes berserk, smashing all non-core spaces on the board into guaranteed high-value Fire Cores."),
        ("Feature 2 - Ground Smash Cascades (Tumble Reels): ", 
         "The Gorilla slams its fists onto winning spins, smashing low-paying symbols into dust. High-paying symbols and new Fire Cores tumble down from the canopy to fill the vacancies in a cascading win chain."),
        ("Feature 3 - Silverback Apex Stampede: ", 
         "A full-reel Silverback Gorilla Collector Wild lands on reel 5 and walks 1 reel to the left on each spin (Walking Wild Collector), collecting and upgrading all Fire Cores on every step."),
        ("Feature 4 - Lock & Slingo™ (Gorilla Smash Ladder): ", 
         "Completed Slingo lines give the Gorilla energy to smash extra Respin Lives onto the meter (allowing up to 5 max lives instead of 3), greatly increasing Full House completion chances.")
    ]
    for bold_txt, norm_txt in gr_features:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 7: Deep Dive Game 4 - Rhino
    add_heading_1("7. Game 4: Fire Primal Rhino™ — 'The Armored Juggernaut'")
    p_rh = doc.add_paragraph(
        "Theme & Story: Set in the volcanic badlands, the Rhino represents unstoppable linear charge power, impenetrable defense, "
        "and piercing force. Its mechanics focus on row/column line charges, multi-hit armored cores, and unbreakable wilds."
    )
    p_rh.paragraph_format.space_after = Pt(4)

    add_heading_2("Rhino Feature Specifications:")
    rh_features = [
        ("Collector Action ('Horn Charge'): ", 
         "When the Rhino Collector lands, it charges horizontally across its entire row, collecting every Fire Core in its path and leaving a trail of Sticky Wild horns behind it that persist for the next 2 spins."),
        ("Feature 1 - Stampede Line Respins: ", 
         "Charging Rhinos stampede across random rows and columns on each respin, piercing stone barriers to unlock additional reel rows (up to 5×7 layout) and leaving multiplier fire trails."),
        ("Feature 2 - Armored Multi-Hit Cores: ", 
         "Fire Cores land with heavy stone/iron armor plates (2 or 3 lives). When collected by a Rhino, their armor absorbs the collection, meaning the cores stay locked on the reels to be collected again on subsequent spins!"),
        ("Feature 3 - Apex Juggernaut Horn Spins: ", 
         "Full-column Armored Rhino Wilds charge onto the reels with 2x and 3x line multipliers. Whenever two Rhino columns land simultaneously, they smash together and trigger a screen-wide Fire Core collection."),
        ("Feature 4 - Lock & Slingo™ (Horn Pierce Ladder): ", 
         "Completing any horizontal or vertical Slingo line triggers a charging Rhino that pierces through that line, instantly doubling the sum of all cash values along that completed line.")
    ]
    for bold_txt, norm_txt in rh_features:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(3)

    # Section 8: Technical Architecture & Reusability
    add_heading_1("8. Technical Architecture & Engine Reusability Blueprint")
    doc.add_paragraph(
        "Because all 4 games share the exact same structural foundation, the C# simulation engine (SlotFramework & PrimalGame) "
        "and frontend rendering client can be built with maximum modularity and reusability:"
    )
    tech_points = [
        ("Unified Configuration Schema: ", "All 4 games use the identical Google Sheet workbook layout (Reelsets, Stage Weights, Cash Value tables, Jackpot Tables). Switching games requires only loading a different workbook tab/URL."),
        ("Collector Action Interface: ", "The engine implements a standard ICollectorHandler interface where Elephant (ExpandGrid), Tiger (ApplyMultipliers), Gorilla (UpgradeValues), and Rhino (ChargeRow) are isolated plug-and-play strategies."),
        ("Bonus Feature Resolvers: ", "The 4 core bonus modes (Zone, Action, Apex, Slingo) share base classes with customized resolution logic per animal, guaranteeing high code stability and fast turnaround times for future sequels."),
        ("Math Engine Consistency: ", "Each game is calibrated to a standardized 95.50% Total RTP profile with distinct variance distribution, making math certification fast and efficient.")
    ]
    for bold_txt, norm_txt in tech_points:
        p = doc.add_paragraph(style='List Bullet')
        r1 = p.add_run(bold_txt)
        r1.font.bold = True
        r2 = p.add_run(norm_txt)
        p.paragraph_format.space_after = Pt(4)

    # Save Document
    doc.save(output_path)
    print(f"Successfully generated Fire Primal Series Game Design Specification at: {output_path}")

if __name__ == "__main__":
    output_docx = "/Users/bo.wang/Downloads/Fire_Primal_Series_Game_Design_Framework_V1.0.docx"
    create_primal_series_docx(output_docx)
