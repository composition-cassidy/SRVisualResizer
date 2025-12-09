# Sparta Remix Visual Automater

A Vegas Pro script for quickly resizing video tracks into grid layouts for Sparta Remixes and YTPMVs.

## What It Does

Automatically calculates and applies box visual dimensions for grid-based video layouts. Instead of manually dividing track sizes and positioning each visual, this script does the math for you.

## How to Use

1. **Select your video tracks** in Vegas Pro (one or multiple)
2. **Run the script** from the Script Menu
3. **Choose your grid layout:**
   - **2×2** - Divides dimensions by 2
   - **3×3** - Divides dimensions by 3
   - **4×4** - Divides dimensions by 4
   - **5×5** - Divides dimensions by 5
   - **6×6** - Divides dimensions by 6

4. **Adjust the Scale slider** (optional):
   - Default: 100% - Creates perfectly sized boxes that fit the grid
   - Lower values (70%-99%): Makes boxes smaller, creating gaps between visuals
   - The script remembers your scale preference for future use

5. **Click "Apply to Selected Tracks"**

## Important Notes

- **WARNING:** This script deletes all existing Track Motion keyframes on selected tracks
- The script divides your track's **current dimensions**, not project dimensions
- After applying, you'll need to manually position each track in the grid
- Scale preference is automatically saved and remembered between sessions

## Example

If you have a track at 1.0×1.0 (full screen):
- Selecting **2×2** will resize it to **0.5×0.5**
- Selecting **3×3** will resize it to **0.333×0.333**
- With Scale at **87.5%**, a 2×2 becomes **0.4375×0.4375** (slightly smaller, for spacing)

Then you manually drag each track to its position in the grid.
