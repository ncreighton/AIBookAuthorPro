// =============================================================================
// AI Book Author Pro
// Copyright (c) 2024 Nick Creighton. All rights reserved.
// =============================================================================

// Global using aliases to disambiguate between WPF, WinForms, and other types
// These are intentionally defined here to avoid conflicts with existing local aliases

// WPF Types (preferred for this WPF application)
global using Color = System.Windows.Media.Color;
global using Brush = System.Windows.Media.Brush;
global using ColorConverter = System.Windows.Media.ColorConverter;
global using Application = System.Windows.Application;

// Win32 dialogs (these are better suited for WPF than WinForms dialogs)
global using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
global using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

