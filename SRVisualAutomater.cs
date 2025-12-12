/**
 * Sparta Remix Visual Automater
 * Resizes and positions video tracks based on grid layout
 * Multi-track support: processes all selected video tracks
 * WARNING: This will overwrite all existing Track Motion keyframes
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ScriptPortal.Vegas;

public class EntryPoint
{
    private static string SettingsFilePath
    {
        get
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = System.IO.Path.Combine(appData, "SpartaRemixVisualAutomater");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return System.IO.Path.Combine(folder, "settings.txt");
        }
    }

    private static void LoadSettings(out int scaleValue, out int gridSize)
    {
        scaleValue = 1000;
        gridSize = 2;
        try
        {
            if (System.IO.File.Exists(SettingsFilePath))
            {
                string[] lines = System.IO.File.ReadAllLines(SettingsFilePath);
                if (lines.Length >= 1)
                {
                    int value;
                    if (int.TryParse(lines[0].Trim(), out value) && value >= 700 && value <= 1000)
                    {
                        scaleValue = value;
                    }
                }
                if (lines.Length >= 2)
                {
                    int value;
                    if (int.TryParse(lines[1].Trim(), out value) && value >= 2 && value <= 6)
                    {
                        gridSize = value;
                    }
                }
            }
        }
        catch { }
    }

    private static void SaveSettings(int scaleValue, int gridSize)
    {
        try
        {
            System.IO.File.WriteAllText(SettingsFilePath, scaleValue.ToString() + "\n" + gridSize.ToString());
        }
        catch { }
    }

    public void FromVegas(Vegas vegas)
    {
        // Collect all selected video tracks first
        List<VideoTrack> selectedTracks = new List<VideoTrack>();
        foreach (Track track in vegas.Project.Tracks)
        {
            if (track.Selected && track.IsVideo())
            {
                selectedTracks.Add((VideoTrack)track);
            }
        }

        if (selectedTracks.Count == 0)
        {
            MessageBox.Show("No video tracks selected. Please select at least one video track.",
                           "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // DPI-aware scaling
        double dpiScale;
        using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            dpiScale = graphics.DpiX / 96.0;
        }

        // Load saved settings
        int savedScale, savedGridSize;
        LoadSettings(out savedScale, out savedGridSize);

        // Grid cell assignments: Dictionary<cellIndex, List<VideoTrack>>
        Dictionary<int, List<VideoTrack>> cellAssignments = new Dictionary<int, List<VideoTrack>>();
        
        // Track the currently selected track for click-to-place
        VideoTrack selectedTrackForPlacement = null;

        Form form = new Form();
        form.Text = "Sparta Remix Visual Automater";
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Font = new Font("Segoe UI", (float)(9 * dpiScale));
        form.BackColor = Color.FromArgb(45, 45, 48);
        form.ForeColor = Color.White;
        form.Size = new Size((int)(700 * dpiScale), (int)(580 * dpiScale));

        // Main horizontal split
        TableLayoutPanel mainContainer = new TableLayoutPanel();
        mainContainer.Dock = DockStyle.Fill;
        mainContainer.ColumnCount = 2;
        mainContainer.RowCount = 1;
        mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, (int)(220 * dpiScale)));
        mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        mainContainer.Padding = new Padding((int)(12 * dpiScale));
        form.Controls.Add(mainContainer);

        // Left panel - Controls
        FlowLayoutPanel leftPanel = new FlowLayoutPanel();
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.FlowDirection = FlowDirection.TopDown;
        leftPanel.WrapContents = false;
        leftPanel.AutoScroll = true;
        mainContainer.Controls.Add(leftPanel, 0, 0);

        // Grid size selection
        GroupBox gridGroup = new GroupBox();
        gridGroup.Text = "Grid Layout";
        gridGroup.Width = (int)(195 * dpiScale);
        gridGroup.Height = (int)(110 * dpiScale);
        gridGroup.ForeColor = Color.White;
        gridGroup.BackColor = Color.Transparent;

        TableLayoutPanel gridTable = new TableLayoutPanel();
        gridTable.ColumnCount = 2;
        gridTable.RowCount = 3;
        gridTable.Dock = DockStyle.Fill;
        gridTable.BackColor = Color.Transparent;

        RadioButton[] gridRadios = new RadioButton[5];
        int[] gridSizes = { 2, 3, 4, 5, 6 };
        string[] gridLabels = { "2×2", "3×3", "4×4", "5×5", "6×6" };

        for (int i = 0; i < 5; i++)
        {
            gridRadios[i] = new RadioButton();
            gridRadios[i].Text = gridLabels[i];
            gridRadios[i].Tag = gridSizes[i];
            gridRadios[i].ForeColor = Color.White;
            gridRadios[i].AutoSize = true;
            gridRadios[i].Margin = new Padding((int)(4 * dpiScale));
            if (gridSizes[i] == savedGridSize)
            {
                gridRadios[i].Checked = true;
            }
        }

        gridTable.Controls.Add(gridRadios[0], 0, 0);
        gridTable.Controls.Add(gridRadios[1], 1, 0);
        gridTable.Controls.Add(gridRadios[2], 0, 1);
        gridTable.Controls.Add(gridRadios[3], 1, 1);
        gridTable.Controls.Add(gridRadios[4], 0, 2);

        gridGroup.Controls.Add(gridTable);
        leftPanel.Controls.Add(gridGroup);

        // Track list
        GroupBox trackGroup = new GroupBox();
        trackGroup.Text = "Unassigned Tracks";
        trackGroup.Width = (int)(195 * dpiScale);
        trackGroup.Height = (int)(180 * dpiScale);
        trackGroup.ForeColor = Color.White;
        trackGroup.Margin = new Padding(0, (int)(8 * dpiScale), 0, 0);

        ListBox trackListBox = new ListBox();
        trackListBox.Dock = DockStyle.Fill;
        trackListBox.BackColor = Color.FromArgb(30, 30, 30);
        trackListBox.ForeColor = Color.White;
        trackListBox.BorderStyle = BorderStyle.None;
        trackListBox.Font = new Font("Segoe UI", (float)(9 * dpiScale));

        // Function to refresh the track list (show only unassigned tracks)
        Action refreshTrackList = null;
        refreshTrackList = () =>
        {
            trackListBox.Items.Clear();
            foreach (VideoTrack vt in selectedTracks)
            {
                // Check if this track is assigned to any cell
                bool isAssigned = false;
                foreach (var kvp in cellAssignments)
                {
                    if (kvp.Value.Contains(vt))
                    {
                        isAssigned = true;
                        break;
                    }
                }
                if (!isAssigned)
                {
                    string displayName = string.IsNullOrEmpty(vt.Name) ? "Track " + vt.Index : vt.Name;
                    trackListBox.Items.Add(displayName);
                }
            }
            selectedTrackForPlacement = null;
        };

        // Initial population
        refreshTrackList();

        trackGroup.Controls.Add(trackListBox);
        leftPanel.Controls.Add(trackGroup);

        // Instructions label
        Label instructionLabel = new Label();
        instructionLabel.Text = "1. Select a track from the list\n2. Click a grid cell to place it\n\nClick an assigned cell to\nremove the track.";
        instructionLabel.ForeColor = Color.FromArgb(180, 180, 180);
        instructionLabel.AutoSize = true;
        instructionLabel.MaximumSize = new Size((int)(195 * dpiScale), 0);
        instructionLabel.Margin = new Padding(0, (int)(8 * dpiScale), 0, 0);
        leftPanel.Controls.Add(instructionLabel);

        // Scale slider
        GroupBox scaleGroup = new GroupBox();
        scaleGroup.Text = "Scale Factor";
        scaleGroup.Width = (int)(195 * dpiScale);
        scaleGroup.Height = (int)(80 * dpiScale);
        scaleGroup.ForeColor = Color.White;
        scaleGroup.Margin = new Padding(0, (int)(8 * dpiScale), 0, 0);

        FlowLayoutPanel scaleInner = new FlowLayoutPanel();
        scaleInner.Dock = DockStyle.Fill;
        scaleInner.FlowDirection = FlowDirection.TopDown;
        scaleInner.WrapContents = false;

        Label scaleLabel = new Label();
        scaleLabel.Text = string.Format("Scale: {0:F1}%", savedScale / 10.0);
        scaleLabel.ForeColor = Color.White;
        scaleLabel.AutoSize = true;

        TrackBar scaleSlider = new TrackBar();
        scaleSlider.Minimum = 700;
        scaleSlider.Maximum = 1000;
        scaleSlider.Value = savedScale;
        scaleSlider.TickFrequency = 25;
        scaleSlider.Width = (int)(170 * dpiScale);
        scaleSlider.Height = (int)(30 * dpiScale);
        scaleSlider.BackColor = Color.FromArgb(45, 45, 48);

        scaleSlider.ValueChanged += (sender, e) =>
        {
            scaleLabel.Text = string.Format("Scale: {0:F1}%", scaleSlider.Value / 10.0);
        };

        scaleInner.Controls.Add(scaleLabel);
        scaleInner.Controls.Add(scaleSlider);
        scaleGroup.Controls.Add(scaleInner);
        leftPanel.Controls.Add(scaleGroup);

        // Warning and Apply button
        Label warningLabel = new Label();
        warningLabel.Text = "⚠ WARNING: Deletes all Track Motion keyframes!";
        warningLabel.ForeColor = Color.FromArgb(255, 120, 120);
        warningLabel.AutoSize = true;
        warningLabel.MaximumSize = new Size((int)(195 * dpiScale), 0);
        warningLabel.Margin = new Padding(0, (int)(10 * dpiScale), 0, 0);
        leftPanel.Controls.Add(warningLabel);

        Button applyButton = new Button();
        applyButton.Text = "Apply";
        applyButton.Width = (int)(195 * dpiScale);
        applyButton.Height = (int)(35 * dpiScale);
        applyButton.DialogResult = DialogResult.OK;
        applyButton.FlatStyle = FlatStyle.Flat;
        applyButton.FlatAppearance.BorderSize = 0;
        applyButton.BackColor = Color.FromArgb(0, 122, 204);
        applyButton.ForeColor = Color.White;
        applyButton.Cursor = Cursors.Hand;
        applyButton.Margin = new Padding(0, (int)(8 * dpiScale), 0, 0);
        leftPanel.Controls.Add(applyButton);

        // Right panel - Grid visualization
        Panel gridPanel = new Panel();
        gridPanel.Dock = DockStyle.Fill;
        gridPanel.BackColor = Color.FromArgb(60, 60, 65);
        gridPanel.Margin = new Padding((int)(8 * dpiScale), 0, 0, 0);
        mainContainer.Controls.Add(gridPanel, 1, 0);

        // Grid cell buttons container
        TableLayoutPanel gridCellContainer = new TableLayoutPanel();
        gridCellContainer.Dock = DockStyle.Fill;
        gridCellContainer.BackColor = Color.Transparent;
        gridCellContainer.Padding = new Padding((int)(8 * dpiScale));
        gridPanel.Controls.Add(gridCellContainer);

        // Function to rebuild the grid
        Action rebuildGrid = null;
        rebuildGrid = () =>
        {
            gridCellContainer.Controls.Clear();
            gridCellContainer.RowStyles.Clear();
            gridCellContainer.ColumnStyles.Clear();

            int currentGridSize = 2;
            foreach (RadioButton rb in gridRadios)
            {
                if (rb.Checked)
                {
                    currentGridSize = (int)rb.Tag;
                    break;
                }
            }

            gridCellContainer.ColumnCount = currentGridSize;
            gridCellContainer.RowCount = currentGridSize;

            for (int i = 0; i < currentGridSize; i++)
            {
                gridCellContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / currentGridSize));
                gridCellContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / currentGridSize));
            }

            for (int row = 0; row < currentGridSize; row++)
            {
                for (int col = 0; col < currentGridSize; col++)
                {
                    int cellIndex = row * currentGridSize + col;
                    
                    Panel cellPanel = new Panel();
                    cellPanel.Dock = DockStyle.Fill;
                    cellPanel.Margin = new Padding(2);
                    cellPanel.BackColor = Color.FromArgb(80, 80, 85);
                    cellPanel.Tag = cellIndex;
                    cellPanel.Cursor = Cursors.Hand;

                    Label cellLabel = new Label();
                    cellLabel.Dock = DockStyle.Fill;
                    cellLabel.TextAlign = ContentAlignment.MiddleCenter;
                    cellLabel.ForeColor = Color.White;
                    cellLabel.Font = new Font("Segoe UI", (float)(9 * dpiScale));
                    cellLabel.BackColor = Color.Transparent;

                    // Update label based on assignments
                    if (cellAssignments.ContainsKey(cellIndex) && cellAssignments[cellIndex].Count > 0)
                    {
                        List<string> names = new List<string>();
                        foreach (VideoTrack vt in cellAssignments[cellIndex])
                        {
                            string name = string.IsNullOrEmpty(vt.Name) ? "Track " + vt.Index : vt.Name;
                            names.Add("✓ " + name);
                        }
                        cellLabel.Text = string.Join("\n", names.ToArray());
                        cellPanel.BackColor = Color.FromArgb(60, 120, 80);
                    }
                    else
                    {
                        cellLabel.Text = string.Format("Row {0}, Col {1}", row + 1, col + 1);
                        cellLabel.ForeColor = Color.FromArgb(120, 120, 120);
                    }

                    cellPanel.Controls.Add(cellLabel);

                    // Click handler for cell
                    int capturedRow = row;
                    int capturedCol = col;
                    int capturedCellIndex = cellIndex;
                    
                    EventHandler cellClick = (sender, e) =>
                    {
                        // If cell has assignments and no track is selected, remove last track from cell
                        if (selectedTrackForPlacement == null && cellAssignments.ContainsKey(capturedCellIndex) && cellAssignments[capturedCellIndex].Count > 0)
                        {
                            // Remove the last track from this cell
                            cellAssignments[capturedCellIndex].RemoveAt(cellAssignments[capturedCellIndex].Count - 1);
                            if (cellAssignments[capturedCellIndex].Count == 0)
                            {
                                cellAssignments.Remove(capturedCellIndex);
                            }
                            refreshTrackList();
                            rebuildGrid();
                            return;
                        }

                        if (selectedTrackForPlacement != null)
                        {
                            // Remove track from any other cell first
                            List<int> keysToCheck = new List<int>(cellAssignments.Keys);
                            foreach (int key in keysToCheck)
                            {
                                cellAssignments[key].Remove(selectedTrackForPlacement);
                                if (cellAssignments[key].Count == 0)
                                {
                                    cellAssignments.Remove(key);
                                }
                            }

                            // Add to this cell
                            if (!cellAssignments.ContainsKey(capturedCellIndex))
                            {
                                cellAssignments[capturedCellIndex] = new List<VideoTrack>();
                            }
                            cellAssignments[capturedCellIndex].Add(selectedTrackForPlacement);

                            // Clear selection and refresh list
                            selectedTrackForPlacement = null;
                            trackListBox.ClearSelected();
                            refreshTrackList();

                            // Rebuild grid to show updated assignments
                            rebuildGrid();
                        }
                    };

                    cellPanel.Click += cellClick;
                    cellLabel.Click += cellClick;

                    gridCellContainer.Controls.Add(cellPanel, col, row);
                }
            }
        };

        // Handle track selection - map listbox index to actual unassigned track
        trackListBox.SelectedIndexChanged += (sender, e) =>
        {
            if (trackListBox.SelectedIndex >= 0)
            {
                // Find the actual track from the display name
                string selectedName = trackListBox.SelectedItem.ToString();
                foreach (VideoTrack vt in selectedTracks)
                {
                    string displayName = string.IsNullOrEmpty(vt.Name) ? "Track " + vt.Index : vt.Name;
                    if (displayName == selectedName)
                    {
                        selectedTrackForPlacement = vt;
                        break;
                    }
                }
            }
            else
            {
                selectedTrackForPlacement = null;
            }
        };

        // Handle grid size change
        foreach (RadioButton rb in gridRadios)
        {
            rb.CheckedChanged += (sender, e) =>
            {
                if (((RadioButton)sender).Checked)
                {
                    // Clear assignments when grid size changes
                    cellAssignments.Clear();
                    refreshTrackList();
                    rebuildGrid();
                }
            };
        }

        // Initial grid build
        rebuildGrid();

        form.AcceptButton = applyButton;

        // Show form
        if (form.ShowDialog() == DialogResult.OK)
        {
            // Get selected grid size
            int gridSize = 2;
            foreach (RadioButton rb in gridRadios)
            {
                if (rb.Checked)
                {
                    gridSize = (int)rb.Tag;
                    break;
                }
            }

            // Save settings
            SaveSettings(scaleSlider.Value, gridSize);

            // Get scale factor
            double scaleFactor = scaleSlider.Value / 1000.0;

            // Check if any tracks are assigned
            int assignedCount = 0;
            foreach (var kvp in cellAssignments)
            {
                assignedCount += kvp.Value.Count;
            }

            if (assignedCount == 0)
            {
                MessageBox.Show("No tracks assigned to grid cells. Please click on tracks in the list, then click grid cells to assign them.",
                               "No Assignments", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Process each assigned track
            foreach (var kvp in cellAssignments)
            {
                int cellIndex = kvp.Key;
                List<VideoTrack> tracksInCell = kvp.Value;

                // Calculate row and column from cell index
                int row = cellIndex / gridSize;
                int col = cellIndex % gridSize;

                // Calculate position in pixels relative to center (Vegas uses center as 0,0)
                // Project dimensions
                int projectWidth = vegas.Project.Video.Width;
                int projectHeight = vegas.Project.Video.Height;
                
                // Cell size in pixels
                double cellWidth = (double)projectWidth / gridSize;
                double cellHeight = (double)projectHeight / gridSize;
                
                // Position: offset from center
                // For col=0 in 3x3: (0 - 1) * cellWidth = -cellWidth (left of center)
                // For col=1 in 3x3: (1 - 1) * cellWidth = 0 (center)
                // For col=2 in 3x3: (2 - 1) * cellWidth = +cellWidth (right of center)
                // Note: Vegas Y-axis is inverted (positive Y = up), so we negate the row offset
                double centerOffset = (gridSize - 1) / 2.0;
                double posX = (col - centerOffset) * cellWidth;
                double posY = (centerOffset - row) * cellHeight;  // Inverted: top row = positive Y

                foreach (VideoTrack track in tracksInCell)
                {
                    // Get current dimensions (default to 1.0 if no keyframes)
                    double currentWidth = 1.0;
                    double currentHeight = 1.0;

                    if (track.TrackMotion.MotionKeyframes.Count > 0)
                    {
                        currentWidth = track.TrackMotion.MotionKeyframes[0].Width;
                        currentHeight = track.TrackMotion.MotionKeyframes[0].Height;
                    }

                    // Calculate new dimensions
                    double newWidth = (currentWidth / gridSize) * scaleFactor;
                    double newHeight = (currentHeight / gridSize) * scaleFactor;

                    // Clear existing track motion
                    track.TrackMotion.MotionKeyframes.Clear();

                    // Vegas auto-creates a default keyframe, just modify that one
                    TrackMotionKeyframe keyframe;
                    if (track.TrackMotion.MotionKeyframes.Count > 0)
                    {
                        keyframe = track.TrackMotion.MotionKeyframes[0];
                    }
                    else
                    {
                        keyframe = track.TrackMotion.InsertMotionKeyframe(Timecode.FromFrames(0));
                    }

                    // Set dimensions and position
                    keyframe.Width = newWidth;
                    keyframe.Height = newHeight;
                    keyframe.PositionX = posX;
                    keyframe.PositionY = posY;
                }
            }
        }
    }
}
