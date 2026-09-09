using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using RoutineKeeper.Models;
using System;
using System.Collections.Generic;

namespace RoutineKeeper.Services;

public class ThemeService
{
    private const string ThemeKey = "AppThemePref";

    public ThemeType CurrentTheme { get; private set; }

    // Definicje motywów – wszystkie klucze muszą być tutaj
    private static readonly Dictionary<string, string> DefaultColors = new()
    {
        ["PrimaryAccent"]              = "#5E3BEE",
        ["PrimaryAccentDark"]          = "#4925C1",
        ["AppBackground"]              = "#F4F7FC",
        ["CardBackground"]             = "#5E3BEE",
        ["CardBackgroundSecondary"]    = "#FFFFFF",
        ["BorderColor"]                = "#E6E9F0",
        ["CardBorderColor"]            = "#5230CE",
        ["TextPrimary"]                = "#1B1E32",
        ["TextSecondary"]              = "#8F94A6",
        ["TextOnCardPrimary"]          = "#FFFFFF",
        ["TextOnCardSecondary"]        = "#C0AFF9",
        ["DockBackground"]             = "#4925C1",
        ["White"]                      = "#FFFFFF",
        ["Black"]                      = "#000000",
    };

    private static readonly Dictionary<string, string> DarkColors = new()
    {
        ["PrimaryAccent"]              = "#5E3BEE",
        ["PrimaryAccentDark"]          = "#4925C1",
        ["AppBackground"]              = "#0D1117",
        ["CardBackground"]             = "#161B22",
        ["CardBackgroundSecondary"]    = "#161B22",
        ["BorderColor"]                = "#30363D",
        ["CardBorderColor"]            = "#30363D",
        ["TextPrimary"]                = "#E6EDF3",
        ["TextSecondary"]              = "#8B949E",
        ["TextOnCardPrimary"]          = "#FFFFFF",
        ["TextOnCardSecondary"]        = "#C0AFF9",
        ["DockBackground"]             = "#161B22",
        ["White"]                      = "#FFFFFF",
        ["Black"]                      = "#000000",
    };

    private static readonly Dictionary<string, string> CatppuccinColors = new()
    {
        ["PrimaryAccent"]              = "#cba6f7",
        ["PrimaryAccentDark"]          = "#b4befe",
        ["AppBackground"]              = "#1e1e2e",
        ["CardBackground"]             = "#313244",
        ["CardBackgroundSecondary"]    = "#181825",
        ["BorderColor"]                = "#45475a",
        ["CardBorderColor"]            = "#45475a",
        ["TextPrimary"]                = "#cdd6f4",
        ["TextSecondary"]              = "#a6adc8",
        ["TextOnCardPrimary"]          = "#cdd6f4",
        ["TextOnCardSecondary"]        = "#a6adc8",
        ["DockBackground"]             = "#181825",
        ["White"]                      = "#FFFFFF",
        ["Black"]                      = "#000000",
    };

    public ThemeService()
    {
        int savedTheme = Preferences.Default.Get(ThemeKey, (int)ThemeType.Default);
        CurrentTheme = (ThemeType)savedTheme;
    }

    public void InitializeTheme()
    {
        // Przy domyślnym motywie nie robimy nic — XAML już załadował DefaultTheme.xaml
        if (CurrentTheme != ThemeType.Default)
        {
            SetTheme(CurrentTheme);
        }
    }

    public void SetTheme(ThemeType theme)
    {
        CurrentTheme = theme;
        Preferences.Default.Set(ThemeKey, (int)theme);

        var colors = theme switch
        {
            ThemeType.Dark        => DarkColors,
            ThemeType.Catppuccin  => CatppuccinColors,
            _                     => DefaultColors
        };

        var resources = Application.Current?.Resources;
        if (resources == null) return;

        // Nadpisujemy kolory i brushe bezpośrednio — bezpieczna metoda bez swap dict
        foreach (var (key, hex) in colors)
        {
            var color = Color.FromArgb(hex);
            resources[key] = color;

            // Aktualizuj też odpowiadający brush jeśli istnieje
            string brushKey = key + "Brush";
            if (resources.ContainsKey(brushKey))
                resources[brushKey] = new SolidColorBrush(color);
        }
    }
}
