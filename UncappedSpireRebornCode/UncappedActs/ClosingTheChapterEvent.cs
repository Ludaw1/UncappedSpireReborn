using System.Reflection;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;

namespace UncappedSpireReborn.UncappedSpireRebornCode.UncappedActs;

// Adapted from Tobiline's ClosingTheChapter.cs. Shown in place of the vanilla post-Act-3-boss
// room. Always offers "loop back to Act 1"; offers "meet the true ending" (TheArchitect) only
// if no Act 4 mod is present; offers a third "enter Act 4" option if Act4Compat found one.
//
// NOTE: replace TheArchitect below with whatever your game version's actual end-game event
// class is named/namespaced as - Tobiline's mod referenced MegaCrit.Sts2.Core.Models.Events.TheArchitect.
public class ClosingTheChapterEvent : CustomEventModel
{
    public override bool IsShared => false;

    public override string CustomInitialPortraitPath => "res://UncappedSpireReborn/images/events/closing_the_chapter.png";

    public override List<(string, string)> Localization => LocManager.Instance.Language switch
    {
        _ => new EventLoc("Closing the Chapter",
            new EventPageLoc("INITIAL", "So close to the peak, you face your final choices...",
                new EventOptionLoc("MEET_THE_ENDING", "Up the Spiral Staircase", "Meet the [sine][red]true ending[/red][/sine]"),
                new EventOptionLoc("ENTER_ACT_4", "Through the Sealed Gate", "Push onward into [orange]Act 4[/orange]"),
                new EventOptionLoc("START_A_NEW_CHAPTER", "Through the Mysterious Door", "Start a new [green]Chapter[/green]"))
        )
    };

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var options = new List<EventOption>
        {
            new(this, StartANewChapter, "UNCAPPEDSPIREREBORN-CLOSING_THE_CHAPTER.pages.INITIAL.options.START_A_NEW_CHAPTER")
        };

        var act4Entry = Act4Compat.TryGetEntryPoint();
        if (act4Entry != null)
        {
            options.Add(new EventOption(this, () => act4Entry(), "UNCAPPEDSPIREREBORN-CLOSING_THE_CHAPTER.pages.INITIAL.options.ENTER_ACT_4"));
        }
        else
        {
            // Only offer the vanilla true ending when there's no Act 4 to push into instead -
            // otherwise the player has three viable "run over" paths, which is one too many.
            options.Add(new EventOption(this, MeetTheEnding, "UNCAPPEDSPIREREBORN-CLOSING_THE_CHAPTER.pages.INITIAL.options.MEET_THE_ENDING"));
        }

        return options;
    }

    public override bool IsAllowed(IRunState runState) => false;

    private static readonly MethodInfo Method_ClearScreens = AccessTools.Method(typeof(RunManager), "ClearScreens");
    private static readonly MethodInfo Method_FadeIn = AccessTools.Method(typeof(RunManager), "FadeIn");

    private Task MeetTheEnding()
    {
        TaskHelper.RunSafely(MeetTheEndingInner());
        return Task.CompletedTask;
    }

    private async Task MeetTheEndingInner()
    {
        var runManager = RunManager.Instance;
        using (new NetLoadingHandle(runManager.NetService))
        {
            if (TestMode.IsOff)
            {
                await NGame.Instance!.Transition.RoomFadeOut();
            }
            Method_ClearScreens.Invoke(runManager, null);
            await runManager.EnterRoom(new EventRoom(ModelDb.Event<TheArchitect>()));
            await (Task)Method_FadeIn.Invoke(runManager, [true])!;
        }
    }

    private static readonly MethodInfo Method_get_State = AccessTools.PropertyGetter(typeof(RunManager), "State");
    private static readonly FieldInfo Field_MapPointHistory = AccessTools.Field(typeof(RunState), "_mapPointHistory");

    private Task StartANewChapter()
    {
        if (!LocalContext.IsMe(Owner))
            return Task.CompletedTask;

        var state = (RunState)Method_get_State.Invoke(RunManager.Instance, null)!;

        // TODO: consider stashing old run history somewhere displayable rather than discarding it.
        Field_MapPointHistory.SetValue(state, new List<List<PlayerMapPointHistoryEntry>>());

        // Re-roll the seed so the new chapter's acts/rooms aren't identical to the last loop.
        // NOTE: singleplayer-only for now - Tobiline's version syncs this seed change to other
        // clients via a ChapterChangeSynchronizer for multiplayer. Worth adding once this works
        // solo; flagging so it's not forgotten rather than silently missing.
        var newSeed = SeedHelper.GetRandomSeed();
        var runRngSet = new RunRngSet(newSeed);
        AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Rng)).Invoke(state, [runRngSet]);
        foreach (var p in state.Players)
        {
            p.InitializeSeed(newSeed);
        }

        var acts = ActModel.GetRandomList(new Rng((uint)StringHelper.GetDeterministicHashCode(newSeed)),
            UnlockState.all, RunManager.Instance.NetService.Type.IsMultiplayer());
        var mutableActs = acts.Select(a => a.ToMutable()).ToList();
        foreach (var act in mutableActs) act.AssertMutable();
        AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Acts)).Invoke(state, [mutableActs]);

        state.CurrentActIndex = -1;

        var actsModifier = state.Modifiers.First(m => m is UncappedActsModifier) as UncappedActsModifier;
        actsModifier!.CurrentChapter++;

        if (UncappedActsConfig.AscensionIncreaseEnabled)
        {
            AscensionIncrease.IncrementAscension(state.GetPlayer(RunManager.Instance.NetService.NetId)!);
        }

        RunManager.Instance.GenerateRooms();
        RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();

        MainFile.Logger.Info($"[UncappedActs] Looped back to Act 1. Now on chapter {actsModifier.CurrentChapter}.");

        return Task.CompletedTask;
    }
}
