﻿using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace UnityEditor.Performance.ProfileAnalyzer
{
    internal class ProfileAnalyzerExportWindow : EditorWindow
    {
        internal static class Styles
        {
            public static readonly GUIContent markerTable = new GUIContent("Marker table", "Export data from the single view marker table");
            public static readonly GUIContent singleFrameTimes = new GUIContent("Single Frame Times", "Export frame time data from the single view");
            public static readonly GUIContent comparisonFrameTimes = new GUIContent("Comparison Frame Times", "Export frame time data from the comparison view");
        }

        ProfileDataView m_ProfileDataView;
        ProfileDataView m_LeftDataView;
        ProfileDataView m_RightDataView;

        static public ProfileAnalyzerExportWindow FindOpenWindow()
        {
            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(typeof(ProfileAnalyzerExportWindow));
            if (windows != null && windows.Length > 0)
                return windows[0] as ProfileAnalyzerExportWindow;

            return null;
        }

        static public bool IsOpen()
        {
            if (FindOpenWindow()!=null)
                return true;

            return false;
        }

        static public ProfileAnalyzerExportWindow Open(float screenX, float screenY, ProfileDataView profileSingleView, ProfileDataView profileLeftView, ProfileDataView profileRightView)
        {
            ProfileAnalyzerExportWindow window = GetWindow<ProfileAnalyzerExportWindow>("Export");
            window.minSize = new Vector2(200, 140);
            window.position = new Rect(screenX, screenY, 200, 140);
            window.SetData(profileSingleView, profileLeftView, profileRightView);
            window.Show();

            return window;
        }

        static public void CloseAll()
        {
            ProfileAnalyzerExportWindow window = GetWindow<ProfileAnalyzerExportWindow>("Export");
            window.Close();
        }

        public void SetData(ProfileDataView profileDataView, ProfileDataView leftDataView, ProfileDataView rightDataView)
        {
            m_ProfileDataView = profileDataView;
            m_LeftDataView = leftDataView;
            m_RightDataView = rightDataView;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("Export as CSV:");
            GUILayout.Label("");

            GUILayout.Label("Single View");

            bool enabled = GUI.enabled;
            if (m_ProfileDataView == null || !m_ProfileDataView.IsDataValid())
                GUI.enabled = false;
            if (GUILayout.Button(Styles.markerTable))
                SaveMarkerTableCSV();
            GUI.enabled = enabled;

            if (m_ProfileDataView == null || m_ProfileDataView.analysis == null)
                GUI.enabled = false;
            if (GUILayout.Button(Styles.singleFrameTimes))
                SaveFrameTimesCSV();
            GUI.enabled = enabled;

            GUILayout.Label("Comparison View");

            if (m_LeftDataView == null || !m_LeftDataView.IsDataValid() || m_RightDataView == null || !m_RightDataView.IsDataValid())
                GUI.enabled = false;
            if (GUILayout.Button(Styles.comparisonFrameTimes))
                SaveComparisonFrameTimesCSV();
            GUI.enabled = enabled;

            EditorGUILayout.EndVertical();
        }

        void SaveMarkerTableCSV()
        {
            if (m_ProfileDataView.analysis == null)
                return;

            string path = EditorUtility.SaveFilePanel("Save marker table CSV data", "", "markerTable.csv", "csv");
            if (path.Length != 0)
            {
                var analytic = ProfileAnalyzerAnalytics.BeginAnalytic();
                using (StreamWriter file = new StreamWriter(path))
                {
                    file.Write("Name, ");
                    file.Write("Median Time, Min Time, Max Time, ");
                    file.Write("Median Frame Index, Min Frame Index, Max Frame Index, ");
                    file.Write("Min Depth, Max Depth, ");
                    file.Write("Total Time, ");
                    file.Write("Mean Time, Time Lower Quartile, Time Upper Quartile, ");
                    file.Write("Count Total, Count Median, Count Min, Count Max, ");
                    file.Write("Number of frames containing Marker, ");
                    file.Write("First Frame Index, ");
                    file.Write("Time Min Individual, Time Max Individual, ");
                    file.Write("Min Individual Frame, Max Individual Frame, ");
                    file.WriteLine("Time at Median Frame");

                    List<MarkerData> markerData = m_ProfileDataView.analysis.GetMarkers();
                    markerData.Sort();
                    foreach (MarkerData marker in markerData)
                    {
                        file.Write("{0},", marker.name);
                        file.Write("{0},{1},{2},",
                            marker.msMedian, marker.msMin, marker.msMax);
                        file.Write("{0},{1},{2},",
                            marker.medianFrameIndex, marker.minFrameIndex, marker.maxFrameIndex);
                        file.Write("{0},{1},",
                            marker.minDepth, marker.maxDepth);
                        file.Write("{0},",
                            marker.msTotal);
                        file.Write("{0},{1},{2},",
                            marker.msMean, marker.msLowerQuartile, marker.msUpperQuartile);
                        file.Write("{0},{1},{2},{3},",
                            marker.count, marker.countMedian, marker.countMin, marker.countMax);
                        file.Write("{0},", marker.presentOnFrameCount);
                        file.Write("{0},", marker.firstFrameIndex);
                        file.Write("{0},{1},",
                            marker.msMinIndividual, marker.msMaxIndividual);
                        file.Write("{0},{1},",
                            marker.minIndividualFrameIndex, marker.maxIndividualFrameIndex);
                        file.WriteLine("{0}", marker.msAtMedian);
                    }
                }
                ProfileAnalyzerAnalytics.SendUIButtonEvent(ProfileAnalyzerAnalytics.UIButton.ExportSingleFrames, analytic);
            }
        }

        void SaveFrameTimesCSV()
        {
            if (m_ProfileDataView == null)
                return;
            if (!m_ProfileDataView.IsDataValid())
                return;

            string path = EditorUtility.SaveFilePanel("Save frame time CSV data", "", "frameTime.csv", "csv");
            if (path.Length != 0)
            {
                var analytic = ProfileAnalyzerAnalytics.BeginAnalytic();
                using (StreamWriter file = new StreamWriter(path))
                {
                    file.WriteLine("Frame Offset, Frame Index, Frame Time (ms), Time from first frame (ms)");
                    float maxFrames = m_ProfileDataView.data.GetFrameCount();

                    var frame = m_ProfileDataView.data.GetFrame(0);
                    // msStartTime isn't very accurate so we don't use it

                    double msTimePassed = 0.0;
                    for (int frameOffset = 0; frameOffset < maxFrames; frameOffset++)
                    {
                        frame = m_ProfileDataView.data.GetFrame(frameOffset);
                        int frameIndex = m_ProfileDataView.data.OffsetToDisplayFrame(frameOffset);
                        float msFrame = frame.msFrame;
                        file.WriteLine("{0},{1},{2},{3}",
                            frameOffset, frameIndex, msFrame, msTimePassed);

                        msTimePassed += msFrame;
                    }
                }
                ProfileAnalyzerAnalytics.SendUIButtonEvent(ProfileAnalyzerAnalytics.UIButton.ExportSingleFrames, analytic);
            }
        }

        void SaveComparisonFrameTimesCSV()
        {
            if (m_LeftDataView == null || m_RightDataView == null)
                return;
            if (!m_LeftDataView.IsDataValid() || !m_RightDataView.IsDataValid())
                return;

            string path = EditorUtility.SaveFilePanel("Save comparison frame time CSV data", "", "frameTimeComparison.csv", "csv");
            if (path.Length != 0)
            {
                var analytic = ProfileAnalyzerAnalytics.BeginAnalytic();
                using (StreamWriter file = new StreamWriter(path))
                {
                    file.Write("Frame Offset, ");
                    file.Write("Left Frame Index, Right Frame Index, ");
                    file.Write("Left Frame Time (ms), Left time from first frame (ms), ");
                    file.Write("Right Frame Time (ms), Right time from first frame (ms), ");
                    file.WriteLine("Frame Time Diff (ms)");
                    float maxFrames = Math.Max(m_LeftDataView.data.GetFrameCount(), m_RightDataView.data.GetFrameCount());

                    var leftFrame = m_LeftDataView.data.GetFrame(0);
                    var rightFrame = m_RightDataView.data.GetFrame(0);

                    // msStartTime isn't very accurate so we don't use it

                    double msTimePassedLeft = 0.0;
                    double msTimePassedRight = 0.0;

                    for (int frameOffset = 0; frameOffset < maxFrames; frameOffset++)
                    {
                        leftFrame = m_LeftDataView.data.GetFrame(frameOffset);
                        rightFrame = m_RightDataView.data.GetFrame(frameOffset);
                        int leftFrameIndex = m_LeftDataView.data.OffsetToDisplayFrame(frameOffset);
                        int rightFrameIndex = m_RightDataView.data.OffsetToDisplayFrame(frameOffset);
                        float msFrameLeft = leftFrame != null ? leftFrame.msFrame : 0;
                        float msFrameRight = rightFrame != null ? rightFrame.msFrame : 0;
                        float msFrameDiff = msFrameRight - msFrameLeft;
                        file.Write("{0},", frameOffset);
                        file.Write("{0},{1},",leftFrameIndex, rightFrameIndex);
                        file.Write("{0},{1},", msFrameLeft, msTimePassedLeft);
                        file.Write("{0},{1},", msFrameRight, msTimePassedRight);
                        file.WriteLine("{0}",msFrameDiff);

                        msTimePassedLeft += msFrameLeft;
                        msTimePassedRight += msFrameRight;
                    }
                }
                ProfileAnalyzerAnalytics.SendUIButtonEvent(ProfileAnalyzerAnalytics.UIButton.ExportComparisonFrames, analytic);
            }
        }
    }
}
