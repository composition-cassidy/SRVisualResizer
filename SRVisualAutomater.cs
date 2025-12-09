/**
 * Sparta Remix Visual Automater
 * Resizes video tracks based on grid layout
 * Multi-track support: processes all selected video tracks
 * WARNING: This will overwrite all existing Track Motion keyframes
 */

using System;
using System.Collections.Generic;
using System.Drawing;
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

    private static int LoadScaleValue()
    {
        try
        {
            if (System.IO.File.Exists(SettingsFilePath))
            {
                string content = System.IO.File.ReadAllText(SettingsFilePath);
                int value;
                if (int.TryParse(content.Trim(), out value))
                {
                    if (value >= 700 && value <= 1000)
                    {
                        return value;
                    }
                }
            }
        }
        catch { }
        return 1000; // Default 100%
    }

    private static void SaveScaleValue(int value)
    {
        try
        {
            System.IO.File.WriteAllText(SettingsFilePath, value.ToString());
        }
        catch { }
    }

    public void FromVegas(Vegas vegas)
    {
        // DPI-aware scaling
        double dpiScale;
        using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
        {
            dpiScale = graphics.DpiX / 96.0;
        }

        Form form = new Form();
        form.Text = "Sparta Remix Visual Automater";
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Font = new Font("Segoe UI", (float)(8.25 * dpiScale));
        form.BackColor = Color.FromArgb(45, 45, 48);
        form.ForeColor = Color.White;
        form.AutoSize = true;
        form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        form.Padding = new Padding((int)(16 * dpiScale));

        FlowLayoutPanel mainLayout = new FlowLayoutPanel();
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.FlowDirection = FlowDirection.TopDown;
        mainLayout.AutoSize = true;
        mainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        mainLayout.WrapContents = false;
        mainLayout.Padding = new Padding(0);
        mainLayout.Margin = new Padding(0);
        form.Controls.Add(mainLayout);

        // Grid size selection
        GroupBox gridGroup = new GroupBox();
        gridGroup.Text = "Grid Layout";
        gridGroup.AutoSize = true;
        gridGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        gridGroup.ForeColor = Color.White;
        gridGroup.BackColor = Color.Transparent;
        gridGroup.Padding = new Padding((int)(10 * dpiScale), (int)(12 * dpiScale), (int)(10 * dpiScale), (int)(10 * dpiScale));

        TableLayoutPanel gridTable = new TableLayoutPanel();
        gridTable.ColumnCount = 2;
        gridTable.RowCount = 3;
        gridTable.AutoSize = true;
        gridTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        gridTable.Dock = DockStyle.Fill;
        gridTable.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        gridTable.BackColor = Color.Transparent;
        for (int i = 0; i < 2; i++)
        {
            gridTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        }
        for (int i = 0; i < 3; i++)
        {
            gridTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        RadioButton grid2x2 = new RadioButton();
        grid2x2.Text = "2×2";
        grid2x2.Checked = true;
        grid2x2.Tag = 2;
        grid2x2.ForeColor = Color.White;
        grid2x2.AutoSize = true;
        grid2x2.Margin = new Padding((int)(4 * dpiScale));

        RadioButton grid3x3 = new RadioButton();
        grid3x3.Text = "3×3";
        grid3x3.Tag = 3;
        grid3x3.ForeColor = Color.White;
        grid3x3.AutoSize = true;
        grid3x3.Margin = new Padding((int)(4 * dpiScale));

        RadioButton grid4x4 = new RadioButton();
        grid4x4.Text = "4×4";
        grid4x4.Tag = 4;
        grid4x4.ForeColor = Color.White;
        grid4x4.AutoSize = true;
        grid4x4.Margin = new Padding((int)(4 * dpiScale));

        RadioButton grid5x5 = new RadioButton();
        grid5x5.Text = "5×5";
        grid5x5.Tag = 5;
        grid5x5.ForeColor = Color.White;
        grid5x5.AutoSize = true;
        grid5x5.Margin = new Padding((int)(4 * dpiScale));

        RadioButton grid6x6 = new RadioButton();
        grid6x6.Text = "6×6";
        grid6x6.Tag = 6;
        grid6x6.ForeColor = Color.White;
        grid6x6.AutoSize = true;
        grid6x6.Margin = new Padding((int)(4 * dpiScale));

        gridTable.Controls.Add(grid2x2, 0, 0);
        gridTable.Controls.Add(grid3x3, 1, 0);
        gridTable.Controls.Add(grid4x4, 0, 1);
        gridTable.Controls.Add(grid5x5, 1, 1);
        gridTable.Controls.Add(grid6x6, 0, 2);

        gridGroup.Controls.Add(gridTable);
        mainLayout.Controls.Add(gridGroup);

        // Size reduction slider
        FlowLayoutPanel scalePanel = new FlowLayoutPanel();
        scalePanel.AutoSize = true;
        scalePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        scalePanel.Margin = new Padding(0, (int)(10 * dpiScale), 0, 0);
        scalePanel.BackColor = Color.Transparent;
        scalePanel.FlowDirection = FlowDirection.TopDown;
        scalePanel.WrapContents = false;

        Label scaleNoteLabel = new Label();
        scaleNoteLabel.Text = "This slider will scale the visual further by your chosen percentage. Leave it at 100% to have perfectly sized boxes.";
        scaleNoteLabel.ForeColor = Color.FromArgb(200, 200, 200);
        scaleNoteLabel.AutoSize = true;
        scaleNoteLabel.MaximumSize = new Size((int)(240 * dpiScale), 0);
        scaleNoteLabel.Margin = new Padding(0, 0, 0, (int)(6 * dpiScale));

        Label scaleLabel = new Label();
        scaleLabel.Text = string.Format("Scale: {0:F1}%", LoadScaleValue() / 10.0);
        scaleLabel.ForeColor = Color.White;
        scaleLabel.AutoSize = true;
        scaleLabel.Margin = new Padding(0, 0, 0, (int)(4 * dpiScale));

        TrackBar scaleSlider = new TrackBar();
        scaleSlider.Minimum = 700; // 70%
        scaleSlider.Maximum = 1000; // 100%
        scaleSlider.Value = LoadScaleValue(); // Load saved value
        scaleSlider.TickFrequency = 25;
        scaleSlider.AutoSize = false;
        scaleSlider.Width = (int)(220 * dpiScale);
        scaleSlider.Height = (int)(28 * dpiScale);
        scaleSlider.BackColor = Color.FromArgb(45, 45, 48);
        scaleSlider.Margin = new Padding(0);
        scaleSlider.ValueChanged += (sender, e) =>
        {
            double percent = scaleSlider.Value / 10.0;
            scaleLabel.Text = string.Format("Scale: {0:F1}%", percent);
            SaveScaleValue(scaleSlider.Value); // Save when changed
        };

        scalePanel.Controls.Add(scaleNoteLabel);
        scalePanel.Controls.Add(scaleLabel);
        scalePanel.Controls.Add(scaleSlider);
        mainLayout.Controls.Add(scalePanel);

        // Warning label
        Label warningLabel = new Label();
        warningLabel.Text = "⚠ WARNING: Deletes all Track Motion keyframes!";
        warningLabel.ForeColor = Color.FromArgb(255, 120, 120);
        warningLabel.AutoSize = true;
        warningLabel.MaximumSize = new Size((int)(240 * dpiScale), 0);
        warningLabel.Margin = new Padding(0, (int)(12 * dpiScale), 0, 0);
        mainLayout.Controls.Add(warningLabel);

        // Apply button
        FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
        buttonPanel.AutoSize = true;
        buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        buttonPanel.Margin = new Padding(0, (int)(12 * dpiScale), 0, 0);
        buttonPanel.BackColor = Color.Transparent;
        buttonPanel.FlowDirection = FlowDirection.LeftToRight;
        buttonPanel.WrapContents = false;
        buttonPanel.Padding = new Padding(0);

        Button applyButton = new Button();
        applyButton.Text = "Apply to Selected Tracks";
        applyButton.AutoSize = true;
        applyButton.DialogResult = DialogResult.OK;
        applyButton.Padding = new Padding((int)(12 * dpiScale), (int)(6 * dpiScale), (int)(12 * dpiScale), (int)(6 * dpiScale));
        applyButton.Margin = new Padding(0);
        applyButton.UseVisualStyleBackColor = false;
        applyButton.FlatStyle = FlatStyle.Flat;
        applyButton.FlatAppearance.BorderSize = 0;
        applyButton.BackColor = Color.FromArgb(0, 122, 204);
        applyButton.ForeColor = Color.White;
        applyButton.Cursor = Cursors.Hand;

        buttonPanel.Controls.Add(applyButton);
        mainLayout.Controls.Add(buttonPanel);

        form.AcceptButton = applyButton;

        // Show form
        if (form.ShowDialog() == DialogResult.OK)
        {
            // Get selected grid size
            int gridSize = 2;
            RadioButton[] radioButtons = { grid2x2, grid3x3, grid4x4, grid5x5, grid6x6 };
            foreach (RadioButton rb in radioButtons)
            {
                if (rb.Checked)
                {
                    gridSize = (int)rb.Tag;
                    break;
                }
            }

            // Get scale factor
            double scaleFactor = scaleSlider.Value / 1000.0;

            // Collect all selected video tracks
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

            // Apply to all selected tracks
            int processedCount = 0;

            foreach (VideoTrack track in selectedTracks)
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

                // Set dimensions (centered position, scaled size)
                keyframe.Width = newWidth;
                keyframe.Height = newHeight;
                keyframe.PositionX = 0.5; // Center X
                keyframe.PositionY = 0.5; // Center Y

                processedCount++;
            }
        }
    }
}
