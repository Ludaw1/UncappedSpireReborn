using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

// Run-scoped state for the chapter-loop system. Lives on the RunState as a ModifierModel
// so it's saved/loaded with the run automatically, the same way Tobiline's UncappedSpireModifier does.
//
// NOTE: ModifierModel / [SavedProperty] / SavedPropertiesTypeCache is the pattern used by the
// reference mod (github.com/Tobiline/UncappedSpire) against an earlier BaseLib build. Verify these
// still exist under MegaCrit.Sts2.Core.Runs / MegaCrit.Sts2.Core.Saves.Runs in BaseLib 3.4.3 -
// if the compiler complains here, that's the first thing to check.
public class UncappedActsModifier : ModifierModel
{
   protected override string IconPath => "res://images/ui/main_menu/patch_notes_icon.png";

    // NOTE: verify this override is required - if ModifierModel.Description isn't abstract/required
    // in 3.4.3, this can be deleted. Chapter is interpolated as {CurrentChapter} in the loc string;
    // add "modifiers.UNCAPPEDACTSMODIFIER.description" (or similar key) to your localization json.
    public override LocString Description
    {
        get
        {
            var baseString = new LocString("modifiers", Id.Entry + ".description");
            baseString.Add("CurrentChapter", CurrentChapter.ToString());
            return baseString;
        }
    }

    [SavedProperty]
    public int CurrentChapter { get; set; } = 1;

    protected override void AfterRunLoaded(RunState runState)
    {
        UncappedActsContext.State = this;
    }

    protected override void AfterRunCreated(RunState runState)
    {
        UncappedActsContext.State = this;
        CurrentChapter = 1;
    }
}
