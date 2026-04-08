using R4PDF.Models;

namespace R4PDF.Fluent.Builders;

/// <summary>
///     Builder for page settings (size, orientation, margins).
/// </summary>
public class SettingsBuilder
{
    internal readonly PageSettings Settings;

    internal SettingsBuilder(PageSettings? baseSettings = null)
    {
        Settings = baseSettings != null
            ? new PageSettings
            {
                PageSize = baseSettings.PageSize,
                Orientation = baseSettings.Orientation,
                Margins = new MarginSettings
                {
                    Top = baseSettings.Margins.Top,
                    Bottom = baseSettings.Margins.Bottom,
                    Left = baseSettings.Margins.Left,
                    Right = baseSettings.Margins.Right
                },
                AutoPagination = new AutoPaginationSettings
                {
                    Enabled = baseSettings.AutoPagination.Enabled,
                    RepeatHeaderOnContinuation = baseSettings.AutoPagination.RepeatHeaderOnContinuation,
                    RepeatFooterOnContinuation = baseSettings.AutoPagination.RepeatFooterOnContinuation,
                    SplitParagraphs = baseSettings.AutoPagination.SplitParagraphs,
                    SplitTables = baseSettings.AutoPagination.SplitTables
                }
            }
            : new PageSettings();
    }

    public SettingsBuilder PageSize(string pageSize)
    {
        Settings.PageSize = pageSize;
        return this;
    }

    public SettingsBuilder Orientation(string orientation)
    {
        Settings.Orientation = orientation;
        return this;
    }

    public SettingsBuilder Margins(string top, string bottom, string left, string right)
    {
        Settings.Margins = new MarginSettings
        {
            Top = top,
            Bottom = bottom,
            Left = left,
            Right = right
        };
        return this;
    }

    public SettingsBuilder Margins(string all)
    {
        Settings.Margins = new MarginSettings
        {
            Top = all,
            Bottom = all,
            Left = all,
            Right = all
        };
        return this;
    }

    public SettingsBuilder AutoPagination(Action<AutoPaginationBuilder> configure)
    {
        var builder = new AutoPaginationBuilder(Settings.AutoPagination);
        configure(builder);
        return this;
    }
}