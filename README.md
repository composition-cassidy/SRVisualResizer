# Sparta Remix Visual Automater

A Vegas Pro script for quickly resizing video tracks into grid layouts for Sparta Remixes and YTPMVs.

## What It Does

Automatically resizes *and positions* Track Motion for your selected video tracks into a grid.

Instead of manually dividing sizes and dragging each track into place, you:

1. Select tracks
2. Assign them to grid cells in the UI
3. Click `Apply`

## How to Use

1. **Select your video tracks** in Vegas Pro (one or multiple)
2. **Run the script** from the Script Menu
3. **Choose your grid layout:**
   - **2×2** - Divides dimensions by 2
   - **3×3** - Divides dimensions by 3
   - **4×4** - Divides dimensions by 4
   - **5×5** - Divides dimensions by 5
   - **6×6** - Divides dimensions by 6

4. **Assign tracks to cells:**
   - Pick a track from the **Unassigned Tracks** list
   - Click a grid cell to place it
   - To remove assignments: click an assigned cell while *no track is selected* (removes the last-added track from that cell)

5. **Adjust the Scale Factor** (optional):
   - Range: **70% to 100%**
   - Default: **100%** (fills each cell)
   - Lower values create spacing between visuals
   - The script remembers your Scale Factor and Grid Layout for next time

6. **Click `Apply`**

## Important Notes

- **WARNING:** This script deletes all existing Track Motion keyframes on selected tracks
- Tracks are positioned based on the **project resolution** and the chosen grid cell (centered grid, top-left cell ends up at positive Y / negative X)
- Track sizing is based on the track’s **current Track Motion size** (the first existing keyframe if present, otherwise defaults to `1.0×1.0`), then divided by the grid size and multiplied by the Scale Factor
- You must assign at least one track to a cell, otherwise the script will refuse to apply
- Changing the grid size in the UI clears any existing assignments
- You can assign multiple tracks to the same cell (they will overlap in that cell)
- Settings are saved under `%AppData%\SpartaRemixVisualAutomater\settings.txt`

## Example

If you have a track at 1.0×1.0 (full screen):
- Selecting **2×2** will resize it to **0.5×0.5**
- Selecting **3×3** will resize it to **0.333×0.333**
- With Scale at **87.5%**, a 2×2 becomes **0.4375×0.4375** (slightly smaller, for spacing)

After clicking `Apply`, the script also automatically positions each assigned track into its chosen grid cell.
