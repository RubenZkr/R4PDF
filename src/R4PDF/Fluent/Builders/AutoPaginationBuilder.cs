using R4PDF.Models;

namespace R4PDF.Fluent.Builders;

public class AutoPaginationBuilder
{
    internal readonly AutoPaginationSettings Settings;

    internal AutoPaginationBuilder(AutoPaginationSettings settings)
    {
        Settings = settings;
    }

    public AutoPaginationBuilder Enabled(bool enabled = true)
    {
        Settings.Enabled = enabled;
        return this;
    }

    public AutoPaginationBuilder RepeatHeaderOnContinuation(bool enabled = true)
    {
        Settings.RepeatHeaderOnContinuation = enabled;
        return this;
    }

    public AutoPaginationBuilder RepeatFooterOnContinuation(bool enabled = true)
    {
        Settings.RepeatFooterOnContinuation = enabled;
        return this;
    }

    public AutoPaginationBuilder SplitParagraphs(bool enabled = true)
    {
        Settings.SplitParagraphs = enabled;
        return this;
    }

    public AutoPaginationBuilder SplitTables(bool enabled = true)
    {
        Settings.SplitTables = enabled;
        return this;
    }
}